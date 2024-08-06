namespace LibWinNFSServer;

public class WccAttribute
{
    public ulong size = 0;
    public NfsTime3 mtime = new();
    public NfsTime3 ctime = new();
}
