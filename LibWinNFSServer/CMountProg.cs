using System.Collections.Generic;
using System.Linq;

namespace LibWinNFSServer;

public class CMountProg : CRPCProg
{
    public const int MOUNT_NUM_MAX = 100;
    public const int MOUNT_PATH_MAX = 100;

    protected int m_nMountNum = 0;
    protected string m_pPathFile = null;
    protected Dictionary<string, string> m_PathMap;
    protected string[] m_pClientAddr = new string[MOUNT_NUM_MAX];
    protected IInputStream m_pInStream;
    protected IOutputStream m_pOutStream;

    private ProcessParam m_pParam;
    private int m_nResult;

    public CMountProg()
    {

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
    public string GetClientAddr(int nIndex)
    {
        int i;

        if (nIndex < 0 || nIndex >= m_nMountNum)
        {
            return null;
        }

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
    public int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        PPROC pf[] = { ProcedureNULL, ProcedureMNT, ProcedureNOIMP, ProcedureUMNT, ProcedureUMNTALL, ProcedureEXPORT };

        PrintLog("MOUNT ");

        if (pParam->nProc >= sizeof(pf) / sizeof(PPROC))
        {
            ProcedureNOIMP();
            PrintLog("\n");
            return PRC_STATUS.PRC_NOTIMP;
        }

        m_pInStream = pInStream;
        m_pOutStream = pOutStream;
        m_pParam = pParam;
        m_nResult = PRC_OK;
        (this->* pf[pParam->nProc])();
        PrintLog("\n");

        return m_nResult;
    }
    public string FormatPath(string pPath, PathFormats format)
    {
        size_t len = strlen(pPath);

        //Remove head spaces
        while (*pPath == ' ')
        {
            ++pPath;
            len--;
        }

        //Remove tail spaces
        while (len > 0 && *(pPath + len - 1) == ' ')
        {
            len--;
        }

        //Remove windows tail slashes (except when its only a drive letter)
        while (len > 0 && *(pPath + len - 2) != ':' && *(pPath + len - 1) == '\\')
        {
            len--;
        }

        //Remove unix tail slashes
        while (len > 1 && *(pPath + len - 1) == '/')
        {
            len--;
        }

        //Is comment?
        if (*pPath == '#')
        {
            return NULL;
        }

        //Remove head "
        if (*pPath == '"')
        {
            ++pPath;
            len--;
        }

        //Remove tail "
        if (len > 0 && *(pPath + len - 1) == '"')
        {
            len--;
        }

        if (len < 1)
        {
            return NULL;
        }
        char* result = (char*)malloc(len + 1);
        if (result == NULL) return NULL;
        strncpy_s(result, len + 1, pPath, len);

        //Check for right path format
        if (format == FORMAT_PATH)
        {
            if (result[0] == '.')
            {
                static char path1[MAXPATHLEN];
                char* p = _getcwd(path1, MAXPATHLEN);

                if (result[1] == '\0')
                {
                    len = strlen(path1);
                    result = (char*)realloc(result, len + 1);
                    if (result == NULL) return NULL;
                    strcpy_s(result, len + 1, path1);
                }
                else if (result[1] == '\\')
                {
                    strcat_s(path1, result + 1);
                    len = strlen(path1);
                    result = (char*)realloc(result, len + 1);
                    if (result == NULL) return NULL;
                    strcpy_s(result, len + 1, path1);
                }

            }
            if (len >= 2 && result[1] == ':' && ((result[0] >= 'A' && result[0] <= 'Z') || (result[0] >= 'a' && result[0] <= 'z')))
            { //check path format
                char tempPath[MAXPATHLEN] = "\\\\?\\";
                strcat_s(tempPath, result);
                len = strlen(tempPath);
                result = (char*)realloc(result, len + 1);
                if (result == NULL) return NULL;
                strcpy_s(result, len + 1, tempPath);
            }

            if ((len < 6) || result[5] != ':' || !isalpha(result[4]))
            { //check path format
                Console.WriteLine("Path %s format is incorrect.", pPath);
                Console.WriteLine("Please use a full path such as C:\\work or \\\\?\\C:\\work");
                free(result);
                return NULL;
            }

            for (size_t i = 0; i < len; i++)
            {
                if (result[i] == '/')
                {
                    result[i] = '\\';
                }
            }
        }
        else if (format == FORMAT_PATHALIAS)
        {
            if (pPath[1] == ':' && ((pPath[0] >= 'A' && pPath[0] <= 'Z') || (pPath[0] >= 'a' && pPath[0] <= 'z')))
            {
                strncpy_s(result, len + 1, pPath, len);
                //transform Windows format to mount path d:\work => /d/work
                result[1] = result[0];
                result[0] = '/';
                size_t len = strlen(result);
                for (size_t i = 0; i < len; i++)
                {
                    if (i >= 2 && result[i] == '\\')
                    {
                        result[i] = '/';
                    }
                }
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
        string path =""; //MAXPATHLEN+1
        int i;

        PrintLog("MNT");
        PrintLog(" from {0}\n", m_pParam.pRemoteAddr);

        if (GetPath(ref path))
        {
            m_pOutStream->Write(MNT_OK); //OK

            if (m_pParam->nVersion == 1)
            {
                m_pOutStream->Write(CFileTable::GetFileHandle(path), FHSIZE);  //fhandle
            }
            else
            {
                m_pOutStream->Write(NFS3_FHSIZE);  //length
                m_pOutStream->Write(CFileTable::GetFileHandle(path), NFS3_FHSIZE);  //fhandle
                m_pOutStream->Write(0);  //flavor
            }

            ++m_nMountNum;

            for (i = 0; i < MOUNT_NUM_MAX; i++)
            {
                if (m_pClientAddr[i] == NULL)
                { //search an empty space
                    m_pClientAddr[i] = new char[strlen(m_pParam->pRemoteAddr) + 1];
                    strcpy_s(m_pClientAddr[i], (strlen(m_pParam->pRemoteAddr) + 1), m_pParam->pRemoteAddr);  //remember the client address
                    break;
                }
            }
        }
        else
        {
            m_pOutStream->Write(MNTERR_ACCESS);  //permission denied
        }
    }
    protected void ProcedureUMNT()
    {
        string path = ""; //MAXPATHLEN+1
        int i;

        PrintLog("UMNT");
        GetPath(ref path);
        PrintLog(" from {0}", m_pParam.pRemoteAddr);

        for (i = 0; i < MOUNT_NUM_MAX; i++)
        {
            if (m_pClientAddr[i] != null)
            {
                if (strcmp(m_pParam->pRemoteAddr, m_pClientAddr[i]) == 0)
                { //address match
                    delete[] m_pClientAddr[i];  //remove this address
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
        m_nResult = PRC_NOTIMP;

    }
    protected void ProcedureEXPORT()
    {
        PrintLog("EXPORT");

        for (auto const &exportedPath : m_PathMap) {
            const char* path = exportedPath.first.c_str();
            unsigned int length = (unsigned int)strlen(path);
            // dirpath
            m_pOutStream->Write(1);
            m_pOutStream->Write(length);
            m_pOutStream->Write(const_cast<char*>(path), length);
            int fillBytes = (length % 4);
            if (fillBytes > 0)
            {
                fillBytes = 4 - fillBytes;
                m_pOutStream->Write(".", fillBytes);
            }
            // groups
            m_pOutStream->Write(1);
            m_pOutStream->Write(1);
            m_pOutStream->Write("*", 1);
            m_pOutStream->Write("...", 3);
            m_pOutStream->Write(0);
        }

        m_pOutStream->Write(0);
        m_pOutStream->Write(0);
    }
    protected void ProcedureNOIMP()
    {
        PrintLog("NOIMP");
        m_nResult = PRC_STATUS.PRC_NOTIMP;

    }

    private bool GetPath(ref string returnPath)
    {
        unsigned long i, nSize;
        static char path[MAXPATHLEN + 1];
        static char finalPath[MAXPATHLEN + 1];
        bool foundPath = false;

        m_pInStream->Read(&nSize);

        if (nSize > MAXPATHLEN)
        {
            nSize = MAXPATHLEN;
        }

        typedef std::map < std::string, std::string>::iterator it_type;
        m_pInStream->Read(path, nSize);
        path[nSize] = '\0';

        // TODO: this whole method is quite ugly and ripe for refactoring
        // strip slashes
        std::string pathTemp(path);
        pathTemp.erase(pathTemp.find_last_not_of("/\\") + 1);
        std::copy(pathTemp.begin(), pathTemp.end(), path);
        path[pathTemp.size()] = '\0';

        for (it_type iterator = m_PathMap.begin(); iterator != m_PathMap.end(); iterator++)
        {

            // strip slashes
            std::string pathAliasTemp(iterator->first.c_str());
            pathAliasTemp.erase(pathAliasTemp.find_last_not_of("/\\") + 1);
            char* pathAlias = const_cast<char*>(pathAliasTemp.c_str());

            // strip slashes
            std::string windowsPathTemp(iterator->second.c_str());
            // if it is a drive letter, e.g. D:\ keep the slash
            if (windowsPathTemp.substr(windowsPathTemp.size() - 2) != ":\\")
            {
                windowsPathTemp.erase(windowsPathTemp.find_last_not_of("/\\") + 1);
            }
            char* windowsPath = const_cast<char*>(windowsPathTemp.c_str());

            size_t aliasPathSize = strlen(pathAlias);
            size_t windowsPathSize = strlen(windowsPath);
            size_t requestedPathSize = pathTemp.size();

            if ((requestedPathSize > aliasPathSize) && (strncmp(path, pathAlias, aliasPathSize) == 0))
            {
                foundPath = true;
                //The requested path starts with the alias. Let's replace the alias with the real path
                strncpy_s(finalPath, MAXPATHLEN, windowsPath, windowsPathSize);
                strncpy_s(finalPath + windowsPathSize, MAXPATHLEN - windowsPathSize, (path + aliasPathSize), requestedPathSize - aliasPathSize);
                finalPath[windowsPathSize + requestedPathSize - aliasPathSize] = '\0';

                for (i = 0; i < requestedPathSize - aliasPathSize; i++)
                {
                    //transform path to Windows format
                    if (finalPath[windowsPathSize + i] == '/')
                    {
                        finalPath[windowsPathSize + i] = '\\';
                    }
                }
            }
            else if ((requestedPathSize == aliasPathSize) && (strncmp(path, pathAlias, aliasPathSize) == 0))
            {
                foundPath = true;
                //The requested path IS the alias
                strncpy_s(finalPath, MAXPATHLEN, windowsPath, windowsPathSize);
                finalPath[windowsPathSize] = '\0';
            }

            if (foundPath == true)
            {
                break;
            }
        }

        if (foundPath != true)
        {
            //The requested path does not start with the alias, let's treat it normally.
            strncpy_s(finalPath, MAXPATHLEN, path, nSize);
            //transform mount path to Windows format. /d/work => d:\work
            finalPath[0] = finalPath[1];
            finalPath[1] = ':';

            for (i = 2; i < nSize; i++)
            {
                if (finalPath[i] == '/')
                {
                    finalPath[i] = '\\';
                }
            }

            finalPath[nSize] = '\0';
        }

        PrintLog("Final local requested path: %s\n", finalPath);

        if ((nSize & 3) != 0)
        {
            m_pInStream->Read(&i, 4 - (nSize & 3));  //skip opaque bytes
        }

        *returnPath = finalPath;
        return foundPath;
    }

    private bool ReadPathsFromFile(string sFileName)
    {
        std::ifstream pathFile(sFileName);

        if (pathFile.is_open())
        {
            std::string line, path;
            std::vector < std::string> paths;
            std::istringstream ss;

            while (std::getline(pathFile, line))
            {
                ss.clear();
                paths.clear();
                ss.str(line);

                // split path and alias separated by '>'
                while (std::getline(ss, path, '>'))
                {
                    paths.push_back(path);
                }
                if (paths.size() < 1)
                {
                    continue;
                }
                if (paths.size() < 2)
                {
                    paths.push_back(paths[0]);
                }

                // clean path, trim spaces and slashes (except drive letter)
                paths[0].erase(paths[0].find_last_not_of(" ") + 1);
                if (paths[0].substr(paths[0].size() - 2) != ":\\")
                {
                    paths[0].erase(paths[0].find_last_not_of("/\\ ") + 1);
                }

                char* pCurPath = (char*)malloc(paths[0].size() + 1);
                pCurPath = (char*)paths[0].c_str();

                if (pCurPath != NULL)
                {
                    char* pCurPathAlias = (char*)malloc(paths[1].size() + 1);
                    pCurPathAlias = (char*)paths[1].c_str();
                    Export(pCurPath, pCurPathAlias);
                }
            }
        }
        else
        {
            Console.WriteLine("Can't open file {0}.", sFileName);
            return false;
        }

        return true;
    }
}
