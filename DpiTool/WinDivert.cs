using System.Runtime.InteropServices;

namespace DpiTool;

// WinDivert 2.2 P/Invoke (yalnizca network katmani).
[StructLayout(LayoutKind.Sequential, Size = 80)]
public struct WinDivertAddress
{
    public long Timestamp;
    public uint Flags;   // bit0-7 layer, 8-15 event, 16 sniffed, 17 outbound, 18 loopback, 19 impostor, 20 ipv6
    public uint Reserved2;
    public bool Outbound => ((Flags >> 17) & 1) != 0;
}

public static class WinDivert
{
    public const uint FlagSniff = 1;

    [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr WinDivertOpen(string filter, int layer, short priority, ulong flags);

    [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool WinDivertRecv(IntPtr h, byte[] packet, uint packetLen, out uint recvLen, out WinDivertAddress addr);

    [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool WinDivertSend(IntPtr h, byte[] packet, uint packetLen, out uint sendLen, ref WinDivertAddress addr);

    [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool WinDivertClose(IntPtr h);

    [DllImport("WinDivert.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertHelperCalcChecksums(byte[] packet, uint packetLen, ref WinDivertAddress addr, ulong flags);
}
