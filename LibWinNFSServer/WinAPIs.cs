using System.Runtime.InteropServices;

namespace LibWinNFSServer;

public static partial class WinAPIs
{

    [LibraryImport("Kerenl32")]
    public static partial int GetLastError();

    [LibraryImport("Kerenl32")]
    public static partial int SetLastError(int e);

    public const int ERROR_DIR_NOT_EMPTY = 145;

    public const int FILE_EXISTS = 80;

    public const int FILE_NOT_FOUND = 2;
}