namespace LibWinNFSServer;

public class Fattr3
{
    public uint type = 0;
    public uint mode = 0;
    public uint nlink = 0;
    public uint uid = 0;
    public uint gid = 0;
    public ulong size = 0;
    public ulong used = 0;
    public Specdata3 rdev = new();
    public ulong fsid = 0;
    public ulong fileid = 0;
    public NfsTime3 atime = new();
    public NfsTime3 mtime = new();
    public NfsTime3 ctime = new();
}
