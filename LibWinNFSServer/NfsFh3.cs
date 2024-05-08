namespace LibWinNFSServer;

public class NfsFh3 : Opaque
{
    public NfsFh3():base(CFileTable.NFS3_FHSIZE) { }
    ~NfsFh3() { }
}
