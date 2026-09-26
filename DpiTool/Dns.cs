using System.Net;
using System.Text;

namespace DpiTool;

public record DnsInfo(ushort Id, bool IsResponse, string Name, ushort QType, List<string> Answers, bool Suspicious);

public static class DnsParser
{
    static readonly Dictionary<ushort, string> Types = new() { [1] = "A", [28] = "AAAA", [5] = "CNAME", [15] = "MX", [16] = "TXT", [65] = "HTTPS", [2] = "NS" };
    public static string TypeName(ushort t) => Types.TryGetValue(t, out var n) ? n : "T" + t;

    static ushort U16(byte[] b, int o) => (ushort)((b[o] << 8) | b[o + 1]);

    // Sıkıştırılmış isimleri de çözer; bitiş konumunu pos'a yazar.
    static string ReadName(byte[] b, int end, ref int pos)
    {
        var sb = new StringBuilder();
        int p = pos, hops = 0;
        bool jumped = false;
        while (p < end)
        {
            int len = b[p];
            if (len == 0) { p++; break; }
            if ((len & 0xC0) == 0xC0)
            {
                if (p + 1 >= end || hops++ > 10) break;
                int ptr = ((len & 0x3F) << 8) | b[p + 1];
                if (!jumped) pos = p + 2;
                jumped = true;
                p = ptr;
                continue;
            }
            p++;
            if (p + len > end) break;
            if (sb.Length > 0) sb.Append('.');
            sb.Append(Encoding.ASCII.GetString(b, p, len));
            p += len;
        }
        if (!jumped) pos = p;
        return sb.ToString();
    }

    public static DnsInfo? Parse(byte[] b, int start, int end)
    {
        if (end - start < 12) return null;
        var d = new byte[end - start];
        Array.Copy(b, start, d, 0, d.Length);
        int n = d.Length;
        ushort id = U16(d, 0), flags = U16(d, 2), qd = U16(d, 4), an = U16(d, 6);
        int pos = 12;
        string name = ""; ushort qtype = 0;
        if (qd > 0)
        {
            name = ReadName(d, n, ref pos);
            if (pos + 4 > n) return null;
            qtype = U16(d, pos);
            pos += 4;
        }
        var answers = new List<string>();
        bool suspicious = false;
        for (int i = 0; i < an && pos < n; i++)
        {
            ReadName(d, n, ref pos);
            if (pos + 10 > n) break;
            ushort type = U16(d, pos), rdlen = U16(d, pos + 8);
            pos += 10;
            if (pos + rdlen > n) break;
            if (type == 1 && rdlen == 4)
            {
                var ip = new IPAddress(d.AsSpan(pos, 4));
                answers.Add(ip.ToString());
                var a = ip.GetAddressBytes();
                // 0.0.0.0, loopback, özel ağ adresleri: sahte/engelleme yanıtı olabilir
                if (a[0] == 0 || a[0] == 127 || a[0] == 10 || (a[0] == 192 && a[1] == 168) || (a[0] == 172 && a[1] >= 16 && a[1] <= 31))
                    suspicious = true;
            }
            else if (type == 28 && rdlen == 16)
                answers.Add(new IPAddress(d.AsSpan(pos, 16)).ToString());
            else if (type == 5)
            {
                int p2 = pos;
                answers.Add("CNAME " + ReadName(d, n, ref p2));
            }
            pos += rdlen;
        }
        return new DnsInfo(id, (flags & 0x8000) != 0, name, qtype, answers, suspicious);
    }
}

public static class Tls
{
    // TLS ClientHello içinden SNI (server_name) okur.
    public static string? ReadSni(byte[] b, int start, int end)
    {
        if (end - start < 48 || b[start] != 0x16 || b[start + 5] != 0x01) return null;
        int p = start + 43;
        if (p >= end) return null;
        p += 1 + b[p];                                    // session id
        if (p + 2 > end) return null;
        p += 2 + ((b[p] << 8) | b[p + 1]);                // cipher suites
        if (p + 1 > end) return null;
        p += 1 + b[p];                                    // compression
        if (p + 2 > end) return null;
        int extEnd = Math.Min(end, p + 2 + ((b[p] << 8) | b[p + 1]));
        p += 2;
        while (p + 4 <= extEnd)
        {
            int type = (b[p] << 8) | b[p + 1], len = (b[p + 2] << 8) | b[p + 3];
            p += 4;
            if (type == 0 && p + 5 <= extEnd)
            {
                int nl = (b[p + 3] << 8) | b[p + 4];
                return p + 5 + nl <= end ? Encoding.ASCII.GetString(b, p + 5, nl) : null;
            }
            p += len;
        }
        return null;
    }
}
