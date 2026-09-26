using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DpiTool;

const int MaxBuffer = 5000;
var buffer = new ConcurrentQueue<string>();   // DNS/SNI olay tamponu (halka)
string logFile = Path.Combine(AppContext.BaseDirectory, "dns-buffer.log");

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine(
        "DpiTool - Windows 11 icin DNS sniffer + DPI atlatma (WinDivert)\n\n" +
        "  DpiTool sniff  [--sni]                       Yalnizca dinler: DNS sorgu/yanitlarini (ve istege bagli TLS SNI) tamponlar\n" +
        "  DpiTool bypass [--dns 1.1.1.1] [--split 2]   DNS'i genel cozumleyiciye yonlendirir, TLS/HTTP ilk paketini bolerek DPI'i sasirtir\n\n" +
        "Yonetici olarak calistirin. Ctrl+C ile cikinca tampon dns-buffer.log dosyasina yazilir.");
    return 0;
}
if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
{ Console.Error.WriteLine("Yonetici olarak calistirmalisiniz."); return 1; }

string? ArgVal(string k) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

bool bypass = args[0] == "bypass";
bool wantSni = args.Contains("--sni");
string dnsAddrStr = ArgVal("--dns") ?? "1.1.1.1";
int split = int.Parse(ArgVal("--split") ?? "2");
if (!IPAddress.TryParse(dnsAddrStr, out var dnsAddr)) { Console.Error.WriteLine("Gecersiz --dns"); return 1; }
byte[] dnsBytes = dnsAddr.GetAddressBytes();

string filter; uint flags;
if (bypass)
{
    filter = $"ip and !loopback and ((outbound and udp.DstPort == 53) or (inbound and udp.SrcPort == 53 and ip.SrcAddr == {dnsAddrStr}) " +
             "or (outbound and (tcp.DstPort == 443 or tcp.DstPort == 80) and tcp.PayloadLength > 0))";
    flags = 0;
}
else
{
    filter = "ip and !loopback and (udp.DstPort == 53 or udp.SrcPort == 53 or tcp.DstPort == 53 or tcp.SrcPort == 53" +
             (wantSni ? " or (outbound and tcp.DstPort == 443 and tcp.PayloadLength > 0)" : "") + ")";
    flags = WinDivert.FlagSniff;
}

IntPtr h;
try { h = WinDivert.WinDivertOpen(filter, 0, 0, flags); }
catch (DllNotFoundException) { Console.Error.WriteLine("WinDivert.dll bulunamadi. WinDivert.dll ve WinDivert64.sys dosyalarini exe'nin yanina koyun."); return 1; }
if (h == new IntPtr(-1))
{ Console.Error.WriteLine($"WinDivertOpen basarisiz (hata {Marshal.GetLastWin32Error()}). 5 = yonetici degil, 577 = imza/surucu sorunu."); return 1; }

Console.CancelKeyPress += (_, e) => { e.Cancel = true; WinDivert.WinDivertClose(h); };
Console.WriteLine($"[{(bypass ? "BYPASS" : "SNIFF")}] filtre: {filter}\nCtrl+C ile durdur.\n");

void Log(string line)
{
    string s = $"{DateTime.Now:HH:mm:ss.fff} {line}";
    Console.WriteLine(s);
    buffer.Enqueue(s);
    while (buffer.Count > MaxBuffer) buffer.TryDequeue(out _);
}

var origDst = new Dictionary<ushort, byte[]>();   // yerel UDP port -> sorgunun asil hedef DNS'i
var buf = new byte[65535];
long dnsCount = 0, splitCount = 0;

while (WinDivert.WinDivertRecv(h, buf, (uint)buf.Length, out uint len, out var addr))
{
    int L = (int)len;
    bool modified = false, handled = false;
    int ihl = (buf[0] & 15) * 4;
    byte proto = buf[9];

    if (L >= ihl + 8 && proto == 17) // UDP
    {
        ushort sp = (ushort)((buf[ihl] << 8) | buf[ihl + 1]), dp = (ushort)((buf[ihl + 2] << 8) | buf[ihl + 3]);
        var info = DnsParser.Parse(buf, ihl + 8, L);
        if (info != null)
        {
            dnsCount++;
            string src = new IPAddress(buf.AsSpan(12, 4)).ToString(), dst = new IPAddress(buf.AsSpan(16, 4)).ToString();
            if (!info.IsResponse)
                Log($"DNS ? {DnsParser.TypeName(info.QType),-5} {info.Name}  ({src}:{sp} -> {dst})");
            else
                Log($"DNS ! {DnsParser.TypeName(info.QType),-5} {info.Name} = {string.Join(", ", info.Answers)}{(info.Suspicious ? "  [SUPHELI: ozel/bos IP]" : "")}  (from {src})");
        }
        if (bypass)
        {
            if (addr.Outbound && dp == 53 && !buf.AsSpan(16, 4).SequenceEqual(dnsBytes))
            {
                if (origDst.Count > 4096) origDst.Clear();
                origDst[sp] = buf.AsSpan(16, 4).ToArray();
                dnsBytes.CopyTo(buf, 16);
                modified = true;
            }
            else if (!addr.Outbound && sp == 53 && origDst.TryGetValue(dp, out var od))
            {
                od.CopyTo(buf, 12);   // yaniti sanki asil DNS'ten gelmis gibi goster
                modified = true;
            }
        }
    }
    else if (L >= ihl + 20 && proto == 6) // TCP
    {
        int dp = (buf[ihl + 2] << 8) | buf[ihl + 3];
        int thl = (buf[ihl + 12] >> 4) * 4;
        int pay = ihl + thl, plen = L - pay;
        if (dp == 53 && plen > 2)
        {
            var info = DnsParser.Parse(buf, pay + 2, L);   // TCP DNS: 2 bayt uzunluk oneki
            if (info != null && !info.IsResponse) { dnsCount++; Log($"DNS ? (tcp) {DnsParser.TypeName(info.QType),-5} {info.Name}"); }
        }
        else if ((dp == 443 || dp == 80) && plen > 0)
        {
            string? sni = dp == 443 ? Tls.ReadSni(buf, pay, L) : null;
            if (sni != null) Log($"TLS SNI {sni}");
            if (bypass && addr.Outbound && plen > split + 1)
            {
                bool first = dp == 443 ? buf[pay] == 0x16 : (buf[pay] is (byte)'G' or (byte)'P' or (byte)'H');
                if (first)
                {
                    SendSplit(h, buf, L, ihl, thl, split, addr);
                    splitCount++;
                    handled = true;
                }
            }
        }
    }

    if (!bypass || handled) continue;   // sniff modunda kopya alinir, yeniden enjekte edilmez
    if (modified) WinDivert.WinDivertHelperCalcChecksums(buf, (uint)L, ref addr, 0);
    WinDivert.WinDivertSend(h, buf, (uint)L, out _, ref addr);
}

File.WriteAllLines(logFile, buffer);
Console.WriteLine($"\nKapandi. {dnsCount} DNS paketi, {splitCount} bolunmus el sikisma. Tampon ({buffer.Count} satir): {logFile}");
return 0;

// Ilk veri paketini iki TCP segmentine boler (ikincisinin seq'i kaydirilir) -> DPI SNI/Host'u tek pakette goremez.
static void SendSplit(IntPtr h, byte[] buf, int L, int ihl, int thl, int split, WinDivertAddress addr)
{
    int pay = ihl + thl;
    var p1 = new byte[pay + split];
    Array.Copy(buf, p1, p1.Length);
    p1[ihl + 13] &= 0xF6;                 // PSH ve FIN'i ilk parcadan kaldir
    p1[2] = (byte)(p1.Length >> 8); p1[3] = (byte)p1.Length;

    int rest = L - pay - split;
    var p2 = new byte[pay + rest];
    Array.Copy(buf, p2, pay);
    Array.Copy(buf, pay + split, p2, pay, rest);
    uint seq = (uint)((buf[ihl + 4] << 24) | (buf[ihl + 5] << 16) | (buf[ihl + 6] << 8) | buf[ihl + 7]) + (uint)split;
    p2[ihl + 4] = (byte)(seq >> 24); p2[ihl + 5] = (byte)(seq >> 16); p2[ihl + 6] = (byte)(seq >> 8); p2[ihl + 7] = (byte)seq;
    p2[2] = (byte)(p2.Length >> 8); p2[3] = (byte)p2.Length;

    foreach (var p in new[] { p1, p2 })
    {
        var a = addr;
        WinDivert.WinDivertHelperCalcChecksums(p, (uint)p.Length, ref a, 0);
        WinDivert.WinDivertSend(h, p, (uint)p.Length, out _, ref a);
    }
}
