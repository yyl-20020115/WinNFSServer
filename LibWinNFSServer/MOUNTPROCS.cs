namespace LibWinNFSServer;

public enum MOUNTPROCS : int
{
    MOUNTPROC_NULL = 0,
    MOUNTPROC_MNT = 1,
    MOUNTPROC_DUMP = 2,
    MOUNTPROC_UMNT = 3,
    MOUNTPROC_UMNTALL = 4,
    MOUNTPROC_EXPORT = 5
}
