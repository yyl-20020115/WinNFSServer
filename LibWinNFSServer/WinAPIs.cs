using System.Runtime.InteropServices;

namespace LibWinNFSServer;

public static class WinAPIs
{

    [DllImport("Kerenl32")]
    public static extern int GetLastError();

    [DllImport("Kerenl32")]
    public static extern int SetLastError(int e);
}