namespace LibWinNFSServer;

public class Fattr3
{
    public uint type;
    public uint mode;
    public uint nlink;
    public uint uid;
    public uint gid;
    public ulong size;
    public ulong used;
    public Specdata3 rdev;
    public ulong fsid;
    public ulong fileid;
    public Nfstime3 atime;
    public Nfstime3 mtime;
    public Nfstime3 ctime;
}
