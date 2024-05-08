using System.Text;

namespace LibWinNFSServer;

public class CMountProg : CRPCProg
{
    public const int MOUNT_NUM_MAX = 100;
    public const int MOUNT_PATH_MAX = 100;

    protected int m_nMountNum = 0;
    protected string? m_pPathFile = null;
    protected Dictionary<string, string> m_PathMap;
    protected List<string> m_pClientAddr = [];
    protected IInputStream m_pInStream;
    protected IOutputStream m_pOutStream;

    private ProcessParam m_pParam;
    private int m_nResult;
    private CFileTable fileTable;

    public int add { get; private set; }

    public CMountProg(CFileTable fileTable)
    {
        this.fileTable = fileTable;
    }

    public bool SetPathFile(string file)
    {
        var formattedFile = FormatPath(file, PathFormats.FORMAT_PATH);

        if (formattedFile == null|| File.Exists(formattedFile))
        {
            return false;
        }
        m_pPathFile = formattedFile;
        return true;
    }
    public void Export(string path, string pathAlias)
    {
        var formattedPath = FormatPath(path, PathFormats.FORMAT_PATH);
        pathAlias = FormatPath(pathAlias, PathFormats.FORMAT_PATHALIAS);

        if (path != null && pathAlias != null)
        {
            if (!m_PathMap.ContainsKey(pathAlias))
            {
                m_PathMap[pathAlias] = formattedPath;
                Console.WriteLine("Path #{0} is: {1}, path alias is: {2}", m_PathMap.Count, path, pathAlias);
            }
            else
            {
                Console.WriteLine("Path {0} with path alias {1} already known", path, pathAlias);
            }
        }
    }
    public bool Refresh()
    {
        if (m_pPathFile != null)
        {
            ReadPathsFromFile(m_pPathFile);
            return true;
        }

        return false;
    }
    public string? GetClientAddr(int nIndex)
    {
        int i;

        if (nIndex < 0 || nIndex >= m_nMountNum) return null;
        for (i = 0; i < MOUNT_NUM_MAX; i++)
        {
            if (m_pClientAddr[i] != null)
            {
                if (nIndex == 0)
                {
                    return m_pClientAddr[i];  //client address
                }
                else
                {
                    --nIndex;
                }
            }

        }
        return null;
    }
    public int GetMountNumber()
    {
        return m_nMountNum;  //the number of clients mounted
    }
    
    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        PPROC[] pf = [
            ProcedureNULL, 
            ProcedureMNT, 
            ProcedureNOIMP, 
            ProcedureUMNT, 
            ProcedureUMNTALL,
            ProcedureEXPORT];

        PrintLog("MOUNT ");

        if (pParam.nProc >= pf.Length)
        {
            ProcedureNOIMP();
            PrintLog("\n");
            return (int)PRC_STATUS.PRC_NOTIMP;
        }

        m_pInStream = pInStream;
        m_pOutStream = pOutStream;
        m_pParam = pParam;
        m_nResult = (int)PRC_STATUS.PRC_OK;
        pf[pParam.nProc]();
        PrintLog("\n");

        return m_nResult;
    }
    public string? FormatPath(string pPath, PathFormats format)
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
                Console.WriteLine("Path %s format is incorrect.", pPath);
                Console.WriteLine("Please use a full path such as C:\\work or \\\\?\\C:\\work");
                
                return null;
            }
            result = result.Replace('/', '\\');
        }
        else if (format == PathFormats.FORMAT_PATHALIAS)
        {
            if (pPath[1] == ':' && ((pPath[0] >= 'A' && pPath[0] <= 'Z') || (pPath[0] >= 'a' && pPath[0] <= 'z')))
            {
                result = "/"+pPath[1..];

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

    protected void ProcedureNULL()
    {
        PrintLog("NULL");
    }
    protected void ProcedureMNT()
    {
        Refresh();
        var path =""; //MAXPATHLEN+1
        int i;

        PrintLog("MNT");
        PrintLog(" from {0}\n", m_pParam.pRemoteAddr);

        if (GetPath(ref path))
        {
            m_pOutStream.Write((int)MNTS.MNT_OK); //OK
            var handle = this.fileTable.GetHandleByPath(path);
            if (m_pParam.nVersion == 1)
            {
                var half = new byte[handle.Length >> 1];
                Array.Copy(handle, half, half.Length);
                m_pOutStream.Write(half);  //fhandle
            }
            else
            {
                m_pOutStream.Write(NFS3_FHSIZE);  //length
                m_pOutStream.Write(handle);  //fhandle
                m_pOutStream.Write(0);  //flavor
            }

            ++m_nMountNum;

            for (i = 0; i < MOUNT_NUM_MAX; i++)
            {
                if (m_pClientAddr[i] == null)
                { //search an empty space
                    m_pClientAddr[i] = m_pParam.pRemoteAddr;
                    break;
                }
            }
        }
        else
        {
            m_pOutStream.Write((int)MNTS.MNTERR_ACCESS);  //permission denied
        }
    }
    protected void ProcedureUMNT()
    {
        var path = ""; //MAXPATHLEN+1
        int i;

        PrintLog("UMNT");
        GetPath(ref path);
        PrintLog(" from {0}", m_pParam.pRemoteAddr);

        for (i = 0; i < m_pClientAddr.Count; i++)
        {
            if (m_pClientAddr[i] != null)
            {
                if (String.Compare(m_pParam.pRemoteAddr, m_pClientAddr[i]) == 0)
                { //address match
                     //remove this address
                    m_pClientAddr[i] = null;
                    --m_nMountNum;
                    break;
                }
            }
        }
    }
    protected void ProcedureUMNTALL()
    {
        PrintLog("UMNTALL NOIMP");
        m_nResult = (int)PRC_STATUS.PRC_NOTIMP;
    }
    protected void ProcedureEXPORT()
    {
        PrintLog("EXPORT");
        //TODO: use encoding utf8?
        foreach (var _path in m_PathMap.Keys) {
            var buffer = Encoding.UTF8.GetBytes(_path);
            int length = buffer.Length;
            // dirpath
            m_pOutStream.Write(1);
            m_pOutStream.Write(length);
            m_pOutStream.Write(buffer);
            int fillBytes = (length % 4);
            if (fillBytes > 0)
            {
                fillBytes = 4 - fillBytes;
                m_pOutStream.Write(Encoding.ASCII.GetBytes("."));
            }
            // groups
            m_pOutStream.Write(1);
            m_pOutStream.Write(1);
            m_pOutStream.Write(Encoding.ASCII.GetBytes("*"));
            m_pOutStream.Write(Encoding.ASCII.GetBytes("..."));
            m_pOutStream.Write(0);
        }

        m_pOutStream.Write(0);
        m_pOutStream.Write(0);
    }
    protected void ProcedureNOIMP()
    {
        PrintLog("NOIMP");
        m_nResult = (int)PRC_STATUS.PRC_NOTIMP;
    }

    private bool GetPath(ref string returnPath)
    {
        uint i, nSize = 0;
        string path;
        string finalPath="";
        bool foundPath = false;

        m_pInStream.Read(out nSize);

        if (nSize > MAXPATHLEN) nSize = MAXPATHLEN;

        var bytes = new byte[nSize];
        m_pInStream.Read(bytes);
       
        path = Encoding.UTF8.GetString(bytes);
        // TODO: this whole method is quite ugly and ripe for refactoring
        // strip slashes
        var _path = path.TrimEnd('\\').TrimEnd('/');

        foreach (var pair in m_PathMap)
        {
            // strip slashes
            string pathAliasTemp = pair.Key;
            //pathAliasTemp.erase(pathAliasTemp.find_last_not_of("/\\") + 1);
            string pathAlias = pathAliasTemp;

            // strip slashes
            string windowsPathTemp = pair.Value;
            // if it is a drive letter, e.g. D:\ keep the slash
            //if (windowsPathTemp.substr(windowsPathTemp.size() - 2) != ":\\")
            //{
            //    windowsPathTemp.erase(windowsPathTemp.find_last_not_of("/\\") + 1);
            //}
            string windowsPath = windowsPathTemp;

            //if ((requestedPathSize > aliasPathSize) && (strncmp(path, pathAlias, aliasPathSize) == 0))
            //{
            //    foundPath = true;
            //    //The requested path starts with the alias. Let's replace the alias with the real path
            //    finalPath = windowsPath;
            //    finalPath = finalPath.Replace('/', '\\');
            //}
            //else 
            if (String.Compare(path, pathAlias,true) == 0)
            {
                foundPath = true;
                finalPath = windowsPath;
                //The requested path IS the alias

            }

            if (foundPath == true)
            {
                break;
            }
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

        PrintLog("Final local requested path: {0}\n", finalPath);

        if ((nSize & 3) != 0)
        {
            byte[] buffer = new byte[4];
            //4 - (nSize & 3)
            m_pInStream.Read(buffer);  //skip opaque bytes
        }

        returnPath = finalPath;
        return foundPath;
    }

    private bool ReadPathsFromFile(string sFileName)
    {
        var lines = File.ReadAllLines(sFileName);
        if(File.Exists(sFileName) && lines.Length>0)
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
            Console.WriteLine("Can't open file {0}.", sFileName);
            return false;
        }
    }
}
