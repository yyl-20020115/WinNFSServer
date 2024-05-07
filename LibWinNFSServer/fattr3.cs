namespace LibWinNFSServer;

public struct fattr3
{
    public uint type;
    public uint mode;
    public uint nlink;
    public uint uid;
    public uint gid;
    public ulong size;
    public ulong used;
    public specdata3 rdev;
    public ulong fsid;
    public ulong fileid;
    public nfstime3 atime;
    public nfstime3 mtime;
    public nfstime3 ctime;
}
