using System.Runtime.InteropServices;

namespace LibWinNFSServer;

internal static partial class WinAPIs
{

    [LibraryImport("Kerenl32")]
    internal static partial int GetLastError();

    [LibraryImport("Kerenl32")]
    internal static partial int SetLastError(int e);

    internal const int ERROR_DIR_NOT_EMPTY = 145;

    internal const int FILE_EXISTS = 80;

    internal const int FILE_NOT_FOUND = 2;
}