using System.Runtime.InteropServices;
using System.Text;

namespace LibWinNFSServer;

[StructLayout(LayoutKind.Sequential)]
public struct SymbolicLinkReparseBuffer
{
    public ushort SubstituteNameOffset;
    public ushort SubstituteNameLength;
    public ushort PrintNameOffset;
    public ushort PrintNameLength;
    public uint Flags;
    public StringBuilder PathBuffer;
}
[StructLayout(LayoutKind.Sequential)]
public struct MountPointReparseBuffer
{
    public ushort SubstituteNameOffset;
    public ushort SubstituteNameLength;
    public ushort PrintNameOffset;
    public ushort PrintNameLength;
    public StringBuilder PathBuffer;
}
[StructLayout(LayoutKind.Sequential)]
public struct GenericReparseBuffer
{
    public StringBuilder DataBuffer;
}


[StructLayout(LayoutKind.Explicit)]
public struct REPARSE_DATA_BUFFER
{
    [FieldOffset(0)]
    public uint ReparseTag;
    [FieldOffset(4)]
    public ushort ReparseDataLength;
    [FieldOffset(6)]
    public ushort Reserved;
    [FieldOffset(8)]
    public SymbolicLinkReparseBuffer sbuffer;
    [FieldOffset(8)]
    public MountPointReparseBuffer mbuffer;
    [FieldOffset(8)]
    public GenericReparseBuffer gbuffer;
} 
