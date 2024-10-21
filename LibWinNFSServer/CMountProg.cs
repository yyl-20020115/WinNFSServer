using System.Text;

namespace LibWinNFSServer;

public class CMountProg(CFileTable fileTable) : CRPCProg
{
    public const int MOUNT_NUM_MAX = 100;
    public const int MOUNT_PATH_MAX = 100;

    protected int mounts = 0;
    protected string? path_file = null;
    protected Dictionary<string, string>? paths;
    protected List<string> clients = [];
    protected IInputStream? in_stream;
    protected IOutputStream? out_stream;

    private ProcessParam? m_pParam;
    private PRC_STATUS result;
    private readonly CFileTable fileTable = fileTable;

    public bool SetPathFile(string file)
    {
        var formattedFile = FormatPath(file, PathFormats.FORMAT_PATH);

        if (formattedFile == null || File.Exists(formattedFile))
            return false;
        path_file = formattedFile;
        return true;
    }
    public void Export(string path, string pathAlias)
    {
        var formattedPath = FormatPath(path, PathFormats.FORMAT_PATH);
        pathAlias = FormatPath(pathAlias, PathFormats.FORMAT_PATHALIAS);

        if (path != null && pathAlias != null)
        {
            if (!paths.ContainsKey(pathAlias))
            {
                paths[pathAlias] = formattedPath;
                Console.WriteLine($"Path #{paths.Count} is: {path}, path alias is: {pathAlias}");
            }
            else
            {
                Console.WriteLine($"Path {path} with path alias {pathAlias} already known");
            }
        }
    }
    public bool Refresh()
    {
        if (path_file != null)
        {
            ReadPathsFromFile(path_file);
            return true;
        }

        return false;
    }
    public string? GetClientAddr(int nIndex)
    {
        if (nIndex < 0 || nIndex >= mounts) return null;
        for (var i = 0; i < MOUNT_NUM_MAX; i++)
        {
            if (clients[i] != null)
            {
                if (nIndex == 0)
                {
                    return clients[i];  //client address
                }
                else
                {
                    --nIndex;
                }
            }

        }
        return null;
    }
    public int Clients => mounts;  //the number of clients mounted

    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        PPROC[] procs = [
            NULL,
            MNT,
            NOIMP,
            UMNT,
            UMNTALL,
            EXPORT];

        PrintLog("MOUNT ");

        if (pParam.nProc >= procs.Length)
        {
            NOIMP();
            PrintLog("\n");
            return (int)PRC_STATUS.PRC_NOTIMP;
        }

        in_stream = pInStream;
        out_stream = pOutStream;
        m_pParam = pParam;
        result = PRC_STATUS.PRC_OK;
        procs[pParam.nProc]();
        PrintLog("\n");

        return (int)result;
    }
    public static string? FormatPath(string pPath, PathFormats format)
    {
        pPath = pPath.Trim();
        if (pPath.EndsWith(":\\")) pPath = pPath[..^1];
        pPath = pPath.TrimEnd('/');
        if (pPath.StartsWith('#')) return null;
        if (pPath.StartsWith('"')) pPath = pPath.TrimStart('"');
        if (pPath.EndsWith('"')) pPath = pPath.TrimStart('"');
        if (pPath.Length == 0) return null;

        var result = pPath;

        //Check for right path format
        if (format == PathFormats.FORMAT_PATH)
        {
            if (result[0] == '.')
            {
                if (result.Length == 1)
                {
                    result = Environment.CurrentDirectory;
                }
                else if (result[1] == '\\')
                {
                    result = Environment.CurrentDirectory + result;
                }

            }
            if (result.Length >= 2 && result[1] == ':' && ((result[0] >= 'A' && result[0] <= 'Z') || (result[0] >= 'a' && result[0] <= 'z')))
            { //check path format
                result = "\\\\?\\" + result;
            }

            if ((result.Length < 6) || result[5] != ':' || !char.IsLetter(result[4]))
            { //check path format
                Console.WriteLine($"Path {pPath} format is incorrect.");
                Console.WriteLine("Please use a full path such as C:\\work or \\\\?\\C:\\work");

                return null;
            }
            result = result.Replace('/', '\\');
        }
        else if (format == PathFormats.FORMAT_PATHALIAS)
        {
            if (pPath[1] == ':' && ((pPath[0] >= 'A' && pPath[0] <= 'Z') || (pPath[0] >= 'a' && pPath[0] <= 'z')))
            {
                result = "/" + pPath[1..];

                //transform Windows format to mount path d:\work => /d/work
                result = result.Replace('/', '\\');

            }
            else if (pPath[0] != '/')
            { //check path alias format
                Console.WriteLine("Path alias format is incorrect.");
                Console.WriteLine("Please use a path like /exports");

                return null;
            }
        }
        return result;
    }

    protected void NULL()
    {
        PrintLog("NULL");
    }
    protected void MNT()
    {
        Refresh();
        var path = ""; //MAXPATHLEN+1
        int i;

        PrintLog("MNT");
        PrintLog($" from {m_pParam?.pRemoteAddr ?? ""}\n");

        if (GetPath(ref path))
        {
            out_stream.Write((int)MNTS.MNT_OK); //OK
            var handle = this.fileTable.GetHandleByPath(path);
            if (m_pParam.nVersion == 1)
            {
                var half = new byte[handle.Length >> 1];
                Array.Copy(handle, half, half.Length);
                out_stream.Write(half);  //fhandle
            }
            else
            {
                out_stream.Write(NFS3_FHSIZE);  //length
                out_stream.Write(handle);  //fhandle
                out_stream.Write(0);  //flavor
            }

            ++mounts;

            for (i = 0; i < MOUNT_NUM_MAX; i++)
            {
                if (clients[i] == null)
                { //search an empty space
                    clients[i] = m_pParam.pRemoteAddr;
                    break;
                }
            }
        }
        else
        {
            out_stream.Write((int)MNTS.MNTERR_ACCESS);  //permission denied
        }
    }
    protected void UMNT()
    {
        var path = ""; //MAXPATHLEN+1

        PrintLog("UMNT");
        GetPath(ref path);
        PrintLog($" from {m_pParam?.pRemoteAddr ?? ""}");

        for (var i = 0; i < clients.Count; i++)
        {
            if (clients[i] != null)
            {
                if (string.Compare(m_pParam?.pRemoteAddr, clients[i]) == 0)
                { //address match
                  //remove this address
                    clients[i] = null;
                    --mounts;
                    break;
                }
            }
        }
    }
    protected void UMNTALL()
    {
        PrintLog("UMNTALL NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }
    protected void EXPORT()
    {
        PrintLog("EXPORT");
        //TODO: use encoding utf8?
        foreach (var _path in paths.Keys)
        {
            var buffer = Encoding.UTF8.GetBytes(_path);
            int length = buffer.Length;
            // dirpath
            out_stream.Write(1);
            out_stream.Write(length);
            out_stream.Write(buffer);
            var fillBytes = (length % 4);
            if (fillBytes > 0)
            {
                fillBytes = 4 - fillBytes;
                out_stream.Write(Encoding.ASCII.GetBytes("."));
            }
            // groups
            out_stream.Write(1);
            out_stream.Write(1);
            out_stream.Write(Encoding.ASCII.GetBytes("*"));
            out_stream.Write(Encoding.ASCII.GetBytes("..."));
            out_stream.Write(0);
        }

        out_stream.Write(0);
        out_stream.Write(0);
    }
    protected void NOIMP()
    {
        PrintLog("NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;
    }

    private bool GetPath(ref string returnPath)
    {
        string path;
        string finalPath = "";
        bool foundPath = false;

        in_stream.Read(out uint nSize);

        if (nSize > MAXPATHLEN) nSize = MAXPATHLEN;

        var bytes = new byte[nSize];
        in_stream.Read(bytes);

        path = Encoding.UTF8.GetString(bytes);
        // TODO: this whole method is quite ugly and ripe for refactoring
        // strip slashes
        var _path = path.TrimEnd('\\').TrimEnd('/');

        foreach (var pair in paths)
        {
            // strip slashes
            var pathAliasTemp = pair.Key;
            //pathAliasTemp.erase(pathAliasTemp.find_last_not_of("/\\") + 1);
            var pathAlias = pathAliasTemp;

            // strip slashes
            var windowsPathTemp = pair.Value;
            // if it is a drive letter, e.g. D:\ keep the slash
            //if (windowsPathTemp.substr(windowsPathTemp.size() - 2) != ":\\")
            //{
            //    windowsPathTemp.erase(windowsPathTemp.find_last_not_of("/\\") + 1);
            //}
            var windowsPath = windowsPathTemp;

            //if ((requestedPathSize > aliasPathSize) && (strncmp(path, pathAlias, aliasPathSize) == 0))
            //{
            //    foundPath = true;
            //    //The requested path starts with the alias. Let's replace the alias with the real path
            //    finalPath = windowsPath;
            //    finalPath = finalPath.Replace('/', '\\');
            //}
            //else 
            if (string.Compare(path, pathAlias, true) == 0)
            {
                foundPath = true;
                finalPath = windowsPath;
                //The requested path IS the alias

            }

            if (foundPath) break;
        }

        if (foundPath)
        {
            //The requested path does not start with the alias, let's treat it normally.

            //transform mount path to Windows format. /d/work => d:\work
            //finalPath[0] = finalPath[1];
            //finalPath[1] = ':';
            finalPath = path[1..];
            finalPath = finalPath.Replace('/', '\\');

        }

        PrintLog($"Final local requested path: {finalPath}\n");

        if ((nSize & 3) != 0)
        {
            var buffer = new byte[4];
            //4 - (nSize & 3)
            in_stream.Read(buffer);  //skip opaque bytes
        }

        returnPath = finalPath;
        return foundPath;
    }

    private bool ReadPathsFromFile(string filename)
    {
        var lines = File.ReadAllLines(filename);
        if (File.Exists(filename) && lines.Length > 0)
        {
            foreach (var line in lines)
            {
                if (line.Length == 0) continue;
                var parts = line.Split('>');
                if (parts.Length == 0) continue;
                if (parts.Length == 1)
                {
                    var p1 = parts[0].Trim();
                    if (p1.EndsWith(":\\")) p1 = p1[..^1];
                    this.Export(p1, p1);
                }
                else if (parts.Length == 2)
                {
                    var p1 = parts[0].Trim();
                    if (p1.EndsWith(":\\")) p1 = p1[..^1];
                    var p2 = parts[1].Trim();
                    this.Export(p1, p2);
                }
            }
            return true;
        }
        else
        {
            Console.WriteLine($"Can't open file {filename}.");
            return false;
        }
    }
}
