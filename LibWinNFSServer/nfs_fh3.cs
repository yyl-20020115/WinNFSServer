namespace LibWinNFSServer;

public class nfs_fh3 : opaque
{
    public nfs_fh3():base(CFileTable.NFS3_FHSIZE) { }
    ~nfs_fh3() { }
}
