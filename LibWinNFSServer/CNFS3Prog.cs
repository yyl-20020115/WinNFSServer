using System.Text;

namespace LibWinNFSServer;

public partial class CNFS3Prog : CRPCProg
{
    private uint m_nUID = 0, m_nGID = 0;
    IInputStream? m_pInStream;
    IOutputStream? m_pOutStream;
    ProcessParam? m_pParam;
    Dictionary<int, IntPtr> unstableStorageFile = [];
    private CFileTable fileTable;
    public CNFS3Prog(CFileTable? fileTable = null) => this.fileTable = fileTable ?? new();

    public void SetUserID(uint nUID, uint nGID)
    {
        m_nUID = nUID;
        m_nGID = nGID;
    }
    protected delegate NFS3S PPROC_INT();
    public override int Process(IInputStream pInStream, IOutputStream pOutStream, ProcessParam pParam)
    {
        PPROC_INT[] pf = [
            ProcedureNULL, ProcedureGETATTR, ProcedureSETATTR,
            ProcedureLOOKUP, ProcedureACCESS, ProcedureREADLINK,
            ProcedureREAD, ProcedureWRITE, ProcedureCREATE,
            ProcedureMKDIR, ProcedureSYMLINK, ProcedureMKNOD,
            ProcedureREMOVE, ProcedureRMDIR, ProcedureRENAME,
            ProcedureLINK, ProcedureREADDIR, ProcedureREADDIRPLUS,
            ProcedureFSSTAT, ProcedureFSINFO, ProcedurePATHCONF,
            ProcedureCOMMIT
        ];

        NFS3S? stat = null;

        PrintLog("[{0}] NFS ", DateTime.Now);

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

        try
        {
            stat = pf[pParam.nProc]();
        }
        catch (Exception ex)
        {
            m_nResult = (int)PRC_STATUS.PRC_FAIL;
            PrintLog("Runtime error: {0}", ex.Message);
        }

        PrintLog(" ");

        if (stat.HasValue)
        {
            switch (stat.Value)
            {
                case NFS3S.NFS3_OK:
                    PrintLog("OK");
                    break;
                case NFS3S.NFS3ERR_PERM:
                    PrintLog("PERM");
                    break;
                case NFS3S.NFS3ERR_NOENT:
                    PrintLog("NOENT");
                    break;
                case NFS3S.NFS3ERR_IO:
                    PrintLog("IO");
                    break;
                case NFS3S.NFS3ERR_NXIO:
                    PrintLog("NXIO");
                    break;
                case NFS3S.NFS3ERR_ACCES:
                    PrintLog("ACCESS");
                    break;
                case NFS3S.NFS3ERR_EXIST:
                    PrintLog("EXIST");
                    break;
                case NFS3S.NFS3ERR_XDEV:
                    PrintLog("XDEV");
                    break;
                case NFS3S.NFS3ERR_NODEV:
                    PrintLog("NODEV");
                    break;
                case NFS3S.NFS3ERR_NOTDIR:
                    PrintLog("NOTDIR");
                    break;
                case NFS3S.NFS3ERR_ISDIR:
                    PrintLog("ISDIR");
                    break;
                case NFS3S.NFS3ERR_INVAL:
                    PrintLog("INVAL");
                    break;
                case NFS3S.NFS3ERR_FBIG:
                    PrintLog("FBIG");
                    break;
                case NFS3S.NFS3ERR_NOSPC:
                    PrintLog("NOSPC");
                    break;
                case NFS3S.NFS3ERR_ROFS:
                    PrintLog("ROFS");
                    break;
                case NFS3S.NFS3ERR_MLINK:
                    PrintLog("MLINK");
                    break;
                case NFS3S.NFS3ERR_NAMETOOLONG:
                    PrintLog("NAMETOOLONG");
                    break;
                case NFS3S.NFS3ERR_NOTEMPTY:
                    PrintLog("NOTEMPTY");
                    break;
                case NFS3S.NFS3ERR_DQUOT:
                    PrintLog("DQUOT");
                    break;
                case NFS3S.NFS3ERR_STALE:
                    PrintLog("STALE");
                    break;
                case NFS3S.NFS3ERR_REMOTE:
                    PrintLog("REMOTE");
                    break;
                case NFS3S.NFS3ERR_BADHANDLE:
                    PrintLog("BADHANDLE");
                    break;
                case NFS3S.NFS3ERR_NOT_SYNC:
                    PrintLog("NOT_SYNC");
                    break;
                case NFS3S.NFS3ERR_BAD_COOKIE:
                    PrintLog("BAD_COOKIE");
                    break;
                case NFS3S.NFS3ERR_NOTSUPP:
                    PrintLog("NOTSUPP");
                    break;
                case NFS3S.NFS3ERR_TOOSMALL:
                    PrintLog("TOOSMALL");
                    break;
                case NFS3S.NFS3ERR_SERVERFAULT:
                    PrintLog("SERVERFAULT");
                    break;
                case NFS3S.NFS3ERR_BADTYPE:
                    PrintLog("BADTYPE");
                    break;
                case NFS3S.NFS3ERR_JUKEBOX:
                    PrintLog("JUKEBOX");
                    break;
                default:

                    break;
            }
        }

        PrintLog("");

        return m_nResult;
    }

    protected NFS3S ProcedureNULL()
    {
        PrintLog("null");
        return NFS3S.NFS3_OK;
    }
    protected NFS3S ProcedureGETATTR()
    {
        string path = "";
        Fattr3 obj_attributes = new();
        NFS3S stat;

        PrintLog("GETATTR");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        stat = CheckFile(cStr);
        //printf("\nscanned file %s\n", path);
        if (stat == NFS3S.NFS3ERR_NOENT)
        {
            stat = NFS3S.NFS3ERR_STALE;
        }
        else if (stat == NFS3S.NFS3_OK)
        {
            if (!GetFileAttributesForNFS(cStr, out obj_attributes))
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        Write((int)stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj_attributes);
        }

        return stat;
    }
    NFS3S ProcedureSETATTR()
    {
        string path = "";
        Sattr3 new_attributes;
        sattrguard3 guard;
        WccData obj_wcc = new();
        NFS3S stat;
        int nMode;
        FILE* pFile;
        HANDLE hFile;
        FILETIME fileTime;
        SYSTEMTIME systemTime;

        PrintLog("SETATTR");
        bool validHandle = GetPath(ref path);
        string cStr = validHandle ? path : null;
        if (cStr == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }
        Read(out new_attributes);
        Read(out guard);
        stat = CheckFile(cStr);
        obj_wcc.before.attributes_follow = GetFileAttributesForNFS(cStr, out obj_wcc.before.attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            if (new_attributes.mode.set_it)
            {
                nMode = 0;

                if ((new_attributes.mode.mode & 0x100) != 0)
                {
                    nMode |= S_IREAD;
                }

                // Always set read and write permissions (deliberately implemented this way)
                // if ((new_attributes.mode.mode & 0x80) != 0) {
                nMode |= S_IWRITE;
                // }

                // S_IEXEC is not availabile on windows
                // if ((new_attributes.mode.mode & 0x40) != 0) {
                //     nMode |= S_IEXEC;
                // }

                if (_chmod(cStr, nMode) != 0)
                {
                    stat = NFS3ERR_INVAL;
                }
                else
                {

                }
            }

            // deliberately not implemented because we cannot reflect uid/gid on windows (easliy)
            if (new_attributes.uid.set_it) { }
            if (new_attributes.gid.set_it) { }

            // deliberately not implemented
            if (new_attributes.mtime.set_it == TIMESETS.SET_TO_CLIENT_TIME) { }
            if (new_attributes.atime.set_it == TIMESETS.SET_TO_CLIENT_TIME) { }

            if (new_attributes.mtime.set_it == TIMESETS.SET_TO_SERVER_TIME || new_attributes.atime.set_it == TIMESETS.SET_TO_SERVER_TIME)
            {
                hFile = CreateFile(cStr, FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, 0, OPEN_EXISTING, 0, 0);
                if (hFile != INVALID_HANDLE_VALUE)
                {
                    GetSystemTime(ref systemTime);
                    SystemTimeToFileTime(ref systemTime, ref fileTime);
                    if (new_attributes.mtime.set_it == TIMESETS.SET_TO_SERVER_TIME)
                    {
                        SetFileTime(hFile, null, null, ref fileTime);
                    }
                    if (new_attributes.atime.set_it == TIMESETS.SET_TO_SERVER_TIME)
                    {
                        SetFileTime(hFile, null, ref fileTime, null);
                    }
                }
                CloseHandle(hFile);
            }

            if (new_attributes.size.set_it)
            {
                pFile = _fsopen(cStr, "r+b", _SH_DENYWR);
                if (pFile != null)
                {
                    int filedes = _fileno(pFile);
                    _chsize_s(filedes, new_attributes.size.size);
                    fclose(pFile);
                }
            }
        }

        obj_wcc.after.attributes_follow = GetFileAttributesForNFS(cStr, out obj_wcc.after.attributes);

        Write(stat);
        Write(obj_wcc);

        return stat;
    }
    NFS3S ProcedureLOOKUP()
    {
        string path;
        NfsFh3 obj = new();
        PostOpAttr obj_attributes = new();
        PostOpAttr dir_attributes = new();
        NFS3S stat;

        PrintLog("LOOKUP");

        string dirName = "";
        string fileName = "";
        ReadDirectory(ref dirName, ref fileName);

        path = GetFullPath(ref dirName, ref fileName);
        stat = CheckFile((string)dirName, path);

        if (stat == NFS3S.NFS3_OK)
        {
            GetFileHandle(path, obj);
            obj_attributes.attributes_follow = GetFileAttributesForNFS(path, ref obj_attributes.attributes);
        }

        dir_attributes.attributes_follow = GetFileAttributesForNFS(dirName, ref dir_attributes.attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_attributes);

        return stat;
    }
    NFS3S ProcedureACCESS()
    {
        string path = "";
        uint access;
        PostOpAttr obj_attributes = new();
        NFS3S stat;

        PrintLog("ACCESS");
        bool validHandle = GetPath(ref path);
        string cStr = validHandle ? path : null;
        Read(out access);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3ERR_NOENT)
        {
            stat = NFS3S.NFS3ERR_STALE;
        }

        obj_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out obj_attributes.attributes);

        Write(stat);
        Write(obj_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(access);
        }

        return stat;
    }
    NFS3S ProcedureREADLINK()
    {
        PrintLog("READLINK");
        string path;
        string pMBBuffer = "";

        PostOpAttr symlink_attributes = new();
        NfsPath3 data = new();

        //opaque data;
        NFS3S stat;

        HANDLE hFile;
        REPARSE_DATA_BUFFER* lpOutBuffer;
        lpOutBuffer = (REPARSE_DATA_BUFFER*)malloc(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
        uint bytesReturned;

        bool validHandle = GetPath(path);
        string cStr = validHandle ? path : null;
        if (cStr == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }
        stat = CheckFile(cStr);
        if (stat == NFS3S.NFS3_OK)
        {

            hFile = CreateFile(cStr, GENERIC_READ, FILE_SHARE_READ, null, OPEN_EXISTING, FILE_ATTRIBUTE_REPARSE_POINT | FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS, null);

            if (hFile == INVALID_HANDLE_VALUE)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
            else
            {
                lpOutBuffer = (REPARSE_DATA_BUFFER*)malloc(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
                if (!lpOutBuffer)
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
                else
                {
                    DeviceIoControl(hFile, FSCTL_GET_REPARSE_POINT, null, 0, lpOutBuffer, MAXIMUM_REPARSE_DATA_BUFFER_SIZE, ref bytesReturned, null);
                    string finalSymlinkPath;
                    if (lpOutBuffer.ReparseTag == IO_REPARSE_TAG_SYMLINK || lpOutBuffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
                    {
                        if (lpOutBuffer.ReparseTag == IO_REPARSE_TAG_SYMLINK)
                        {
                            size_t plen = lpOutBuffer.SymbolicLinkReparseBuffer.PrintNameLength / sizeof(WCHAR);
                            WCHAR* szPrintName = new WCHAR[plen + 1];
                            wcsncpy_s(szPrintName, plen + 1, ref lpOutBuffer.SymbolicLinkReparseBuffer.PathBuffer[lpOutBuffer.SymbolicLinkReparseBuffer.PrintNameOffset / sizeof(WCHAR)], plen);
                            szPrintName[plen] = 0;
                            string wStringTemp = (szPrintName);
                            string cPrintName = wstring_to_dbcs(wStringTemp);

                            finalSymlinkPath.assign(cPrintName);
                            // TODO: Revisit with cleaner solution
                            if (!PathIsRelative(cPrintName))
                            {
                                string strFromChar;
                                strFromChar.append("\\\\?\\");
                                strFromChar.append(cPrintName);
                                string target = _strdup(strFromChar);
                                // remove last folder
                                size_t lastFolderIndex = path.find_last_of('\\');
                                if (lastFolderIndex != string.npos)
                                {
                                    path = path.substr(0, lastFolderIndex);
                                }
                                char szOut[MAX_PATH] = "";
                                PathRelativePathTo(szOut, cStr, FILE_ATTRIBUTE_DIRECTORY, target, FILE_ATTRIBUTE_DIRECTORY);
                                string symlinkPath=(szOut);
                                finalSymlinkPath.assign(symlinkPath);
                            }
                        }

                        // TODO: Revisit with cleaner solution
                        if (lpOutBuffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
                        {
                            size_t slen = lpOutBuffer.MountPointReparseBuffer.SubstituteNameLength / sizeof(WCHAR);
                            WCHAR* szSubName = new WCHAR[slen + 1];
                            wcsncpy_s(szSubName, slen + 1, ref lpOutBuffer.MountPointReparseBuffer.PathBuffer[lpOutBuffer.MountPointReparseBuffer.SubstituteNameOffset / sizeof(WCHAR)], slen);
                            szSubName[slen] = 0;
                            string wStringTemp = (szSubName);
                            string target = wstring_to_dbcs(wStringTemp);
                            target.erase(0, 2);
                            target.insert(0, 2, '\\');
                            // remove last folder, see above
                            size_t lastFolderIndex = path.find_last_of('\\');
                            if (lastFolderIndex != string.npos)
                            {
                                path = path.substr(0, lastFolderIndex);
                            }
                            char szOut[MAX_PATH] = "";
                            PathRelativePathTo(szOut, cStr, FILE_ATTRIBUTE_DIRECTORY, target, FILE_ATTRIBUTE_DIRECTORY);
                            string symlinkPath = szOut;
                            finalSymlinkPath.assign(symlinkPath);
                        }

                        // write path always with / separator, so windows created symlinks work too
                        replace(finalSymlinkPath.begin(), finalSymlinkPath.end(), '\\', '/');
                        string result = _strdup(finalSymlinkPath);
                        data.Set(result);
                    }
                    free(lpOutBuffer);
                }
            }
            CloseHandle(hFile);
        }

        symlink_attributes.attributes_follow = GetFileAttributesForNFS(cStr, ref symlink_attributes.attributes);

        Write(stat);
        Write(symlink_attributes);
        if (stat == NFS3S.NFS3_OK)
        {
            Write(data);
        }

        return stat;
    }
    NFS3S ProcedureREAD()
    {
        string path = "";
        long offset;
        int count;
        PostOpAttr file_attributes = new();
        bool eof = false;
        Opaque data = new();
        NFS3S stat;
        FILE* pFile;

        PrintLog("READ");
        bool validHandle = GetPath(ref path);
        string cStr = validHandle ? path : null;
        if (cStr == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }
        Read(out offset);
        Read(out count);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            data.SetSize(count);
            pFile = _fsopen(cStr, "rb", _SH_DENYWR);

            if (pFile != null)
            {
                _fseeki64(pFile, offset, SEEK_SET);
                count = (count3_32)fread(data.contents, sizeof(char), count, pFile);
                eof = fgetc(pFile) == EOF;
                fclose(pFile);
            }
            else
            {
                char buffer[BUFFER_SIZE];
                errno_t errorNumber = errno;
                strerror_s(buffer, BUFFER_SIZE, errorNumber);
                PrintLog(buffer);

                if (errorNumber == 13)
                {
                    stat = NFS3S.NFS3ERR_ACCES;
                }
                else
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
            }
        }

        file_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out file_attributes.attributes);

        Write(stat);
        Write(file_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(count);
            Write(eof);
            Write(data);
        }

        return stat;
    }
    NFS3S ProcedureWRITE()
    {
        string path = "";
        long offset;
        int count;
        int stable;
        Opaque data;
        WccData file_wcc = new();
        long verf = 0;
        NFS3S stat;
        FILE* pFile;

        PrintLog("WRITE");
        bool validHandle = GetPath(ref path);
        string cStr = validHandle ? path : null;
        if (cStr == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }
        Read(out offset);
        Read(out count);
        Read(out stable);
        Read(out data);
        stat = CheckFile(cStr);

        file_wcc.before.attributes_follow = GetFileAttributesForNFS(cStr, out file_wcc.before.attributes);

        if (stat == NFS3S.NFS3_OK)
        {

            if (stable == UNSTABLE)
            {
                NfsFh3 handle;
                GetFileHandle(cStr, ref handle);
                int handleId = *(uint*)handle.contents;

                if (unstableStorageFile.count(handleId) == 0)
                {
                    pFile = _fsopen(cStr, "r+b", _SH_DENYWR);
                    if (pFile != null)
                    {
                        unstableStorageFile.insert(make_pair(handleId, pFile));
                    }
                }
                else
                {
                    pFile = unstableStorageFile[handleId];
                }

                if (pFile != null)
                {
                    _fseeki64(pFile, offset, SEEK_SET);
                    count = (count3_32)fwrite(data.contents, sizeof(char), data.length, pFile);
                }
                else
                {
                    char buffer[BUFFER_SIZE];
                    errno_t errorNumber = errno;
                    strerror_s(buffer, BUFFER_SIZE, errorNumber);
                    PrintLog(buffer);

                    if (errorNumber == 13)
                    {
                        stat = NFS3S.NFS3ERR_ACCES;
                    }
                    else
                    {
                        stat = NFS3S.NFS3ERR_IO;
                    }
                }
                // this should not be zero but a timestamp (process start time) instead
                verf = 0;
                // we can reuse this, because no physical write has happend
                file_wcc.after.attributes_follow = file_wcc.before.attributes_follow;
            }
            else
            {

                pFile = _fsopen(cStr, "r+b", _SH_DENYWR);

                if (pFile != null)
                {
                    _fseeki64(pFile, offset, SEEK_SET);
                    count = (count3_32)fwrite(data.contents, sizeof(char), data.length, pFile);
                    fclose(pFile);
                }
                else
                {
                    char buffer[BUFFER_SIZE];
                    errno_t errorNumber = errno;
                    strerror_s(buffer, BUFFER_SIZE, errorNumber);
                    PrintLog(buffer);

                    if (errorNumber == 13)
                    {
                        stat = NFS3S.NFS3ERR_ACCES;
                    }
                    else
                    {
                        stat = NFS3S.NFS3ERR_IO;
                    }
                }

                stable = FILE_SYNC;
                verf = 0;

                file_wcc.after.attributes_follow = GetFileAttributesForNFS(cStr, ref file_wcc.after.attributes);
            }
        }

        Write(stat);
        Write(file_wcc);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(count);
            Write(stable);
            Write(verf);
        }

        return stat;
    }
    NFS3S ProcedureCREATE()
    {
        string path = null;
        Createhow3 how = new();
        PostOpFh3 obj = new();
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();
        NFS3S stat = 0;
        FILE* pFile = null;

        PrintLog("CREATE");
        string dirName = "";
        string fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        Read(out how);

        dir_wcc.before.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.before.attributes);

        pFile = _fsopen(path, "wb", _SH_DENYWR);

        if (pFile != null)
        {
            fclose(pFile);
            stat = NFS3_OK;
        }
        else
        {
            char buffer[BUFFER_SIZE];
            errno_t errorNumber = errno;
            strerror_s(buffer, BUFFER_SIZE, errorNumber);
            PrintLog(buffer);

            if (errorNumber == 2)
            {
                stat = NFS3S.NFS3ERR_STALE;
            }
            else if (errorNumber == 13)
            {
                stat = NFS3S.NFS3ERR_ACCES;
            }
            else
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        if (stat == NFS3S.NFS3_OK)
        {
            obj.handle_follows = GetFileHandle(path, obj.handle);
            obj_attributes.attributes_follow = GetFileAttributesForNFS(path, out obj_attributes.attributes);
        }

        dir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.after.attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_wcc);

        return stat;
    }
    NFS3S ProcedureMKDIR()
    {
        string path;
        Sattr3 attributes = new();
        PostOpFh3 obj = new();
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();
        NFS3S stat;

        PrintLog("MKDIR");

        string dirName = "";
        string fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        Read(out attributes);

        dir_wcc.before.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.before.attributes);

        int result = 0;
        Directory.CreateDirectory(path);

        if (result == 0)
        {
            stat = NFS3S.NFS3_OK;
            obj.handle_follows = GetFileHandle(path, obj.handle);
            obj_attributes.attributes_follow = GetFileAttributesForNFS(path, out obj_attributes.attributes);
        }
        else if (errno == EEXIST)
        {
            PrintLog("Directory already exists.");
            stat = NFS3ERR_EXIST;
        }
        else if (errno == ENOENT)
        {
            stat = NFS3S.NFS3ERR_NOENT;
        }
        else
        {
            stat = CheckFile(path);

            if (stat != NFS3S.NFS3_OK)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        dir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.after.attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_wcc);

        return stat;
    }
    NFS3S ProcedureSYMLINK()
    {
        PrintLog("SYMLINK");

        string path;
        PostOpFh3 obj = new();
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();
        NFS3S stat;

        Diropargs3 where = new();
        SymLinkData3 symlink = new();

        uint targetFileAttr;
        uint dwFlags;

        string dirName = "";
        string fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);

        Read(out symlink);

        var lpSymlinkFileName = path; // symlink (full path)

        // TODO: Maybe revisit this later for a cleaner solution
        // Convert target path to windows path format, maybe this could also be done
        // in a safer way by a combination of PathRelativePathTo and GetFullPathName.
        // Without this conversion nested folder symlinks do not work cross platform.
        string strFromChar;
        strFromChar.append(symlink.symlink_data.path); // target (should be relative path));
        replace(strFromChar.begin(), strFromChar.end(), '/', '\\');
         LPTSTR lpTargetFileName = const_cast<LPSTR>(strFromChar);

        string fullTargetPath = dirName + ("\\") + (lpTargetFileName);

        // Relative path do not work with GetFileAttributes (directory are not recognized)
        // so we normalize the path before calling GetFileAttributes
        TCHAR fullTargetPathNormalized[MAX_PATH];
         LPTSTR fullTargetPathString = const_cast<LPSTR>(fullTargetPath);
        GetFullPathName(fullTargetPathString, MAX_PATH, fullTargetPathNormalized, null);
        targetFileAttr = GetFileAttributes(fullTargetPathNormalized);

        dwFlags = 0x0;
        if (targetFileAttr & FILE_ATTRIBUTE_DIRECTORY)
        {
            dwFlags = SYMBOLIC_LINK_FLAG_DIRECTORY;
        }

        var failed = CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName, dwFlags);

        if (failed != 0)
        {
            stat = NFS3S.NFS3_OK;
            obj.handle_follows = GetFileHandle(path, obj.handle);
            obj_attributes.attributes_follow = GetFileAttributesForNFS(path, out obj_attributes.attributes);
        }
        else
        {
            stat = NFS3S.NFS3ERR_IO;
            PrintLog("An error occurs or file already exists.");
            stat = CheckFile(path);
            if (stat != NFS3S.NFS3_OK)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        dir_wcc.after.attributes_follow = GetFileAttributesForNFS(dirName, out dir_wcc.after.attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_wcc);

        return stat;
    }
    NFS3S ProcedureMKNOD()
    {
        PrintLog("MKNOD");

        return NFS3S.NFS3ERR_NOTSUPP;
    }
    NFS3S ProcedureREMOVE()
    {
        string path;
        WccData dir_wcc = new();
        NFS3S stat;
      

        PrintLog("REMOVE");

        string dirName = "";
        string fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        stat = CheckFile((string)dirName, path);

        dir_wcc.before.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.before.attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            var fileAttr = File.GetAttributes(path);
            if ((fileAttr & FILE_ATTRIBUTE_DIRECTORY) && (fileAttr & FILE_ATTRIBUTE_REPARSE_POINT))
            {
                var returnCode = CFileTable.RemoveFolder(path);
                if (returnCode)
                {
                    if (returnCode == ERROR_DIR_NOT_EMPTY)
                    {
                        stat = NFS3S.NFS3ERR_NOTEMPTY;
                    }
                    else
                    {
                        stat = NFS3S.NFS3ERR_IO;
                    }
                }
            }
            else
            {
                if (!fileTable.RemoveFile(path))
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
            }
        }

        dir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.after.attributes);

        Write(stat);
        Write(dir_wcc);

        return stat;
    }
    NFS3S ProcedureRMDIR()
    {
        string path ="";
        WccData dir_wcc = new();
        NFS3S stat;
        int returnCode;

        PrintLog("RMDIR");

        string dirName = "";
        string fileName ="";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        stat = CheckFile(dirName, path);

        dir_wcc.before.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.before.attributes);

        if (stat ==NFS3S.NFS3_OK)
        {
            returnCode = fileTable.RemoveFolder(path);
            if (returnCode != 0)
            {
                if (returnCode == ERROR_DIR_NOT_EMPTY)
                {
                    stat = NFS3S.NFS3ERR_NOTEMPTY;
                }
                else
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
            }
        }

        dir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.after.attributes);

        Write(stat);
        Write(dir_wcc);

        return stat;
    }
    NFS3S ProcedureRENAME()
    {
        string pathFrom, pathTo;
        WccData fromdir_wcc = new(), todir_wcc = new();
        NFS3S stat;
        uint returnCode;

        PrintLog("RENAME");

        string dirFromName = "";
        string fileFromName ="";
        ReadDirectory(ref dirFromName, ref fileFromName);
        pathFrom = GetFullPath(ref dirFromName, ref fileFromName);

        string dirToName = "";
        string fileToName = "";
        ReadDirectory(ref dirToName, ref fileToName);
        pathTo = GetFullPath(ref dirToName,ref fileToName);

        stat = CheckFile((string)dirFromName, pathFrom);

        fromdir_wcc.before.attributes_follow = GetFileAttributesForNFS((string)dirFromName, out fromdir_wcc.before.attributes);
        todir_wcc.before.attributes_follow = GetFileAttributesForNFS((string)dirToName, out todir_wcc.before.attributes);

        if (File.Exists(pathTo))
        {
            var fileAttr = File.GetAttributes(pathTo);
            if ((fileAttr & FILE_ATTRIBUTE_DIRECTORY) && (fileAttr & FILE_ATTRIBUTE_REPARSE_POINT))
            {
                returnCode = CFileTable.RemoveFolder(pathTo);
                if (returnCode != 0)
                {
                    if (returnCode == ERROR_DIR_NOT_EMPTY)
                    {
                        stat = NFS3S.NFS3ERR_NOTEMPTY;
                    }
                    else
                    {
                        stat = NFS3S.NFS3ERR_IO;
                    }
                }
            }
            else
            {
                if (0==fileTable.RemoveFile(pathTo))
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
            }
        }

        if (stat == NFS3S.NFS3_OK)
        {
            var errorNumber = fileTable.RenameDirectory(pathFrom, pathTo);

            if (errorNumber != 0)
            {
                PrintLog("Error {0}",errorNumber);

                if (errorNumber == 13)
                {
                    stat = NFS3S.NFS3ERR_ACCES;
                }
                else
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
            }
        }

        fromdir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirFromName, out fromdir_wcc.after.attributes);
        todir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirToName, out todir_wcc.after.attributes);

        Write(stat);
        Write(fromdir_wcc);
        Write(todir_wcc);

        return stat;
    }
    NFS3S ProcedureLINK()
    {
        PrintLog("LINK");
        string path = "";
        Diropargs3 link;
        string dirName = "";
        string fileName ="";
        NFS3S stat;
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();

        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        if (cStr == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }
        ReadDirectory(ref dirName, ref fileName);

        string linkFullPath = GetFullPath(ref dirName, ref fileName);

        //TODO: Improve checks here, cStr may be null because handle is invalid
        if (CreateHardLink(linkFullPath, cStr, null) == 0)
        {
            stat = NFS3S.NFS3ERR_IO;
        }
        stat = CheckFile(linkFullPath);
        if (stat == NFS3S.NFS3_OK)
        {
            obj_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out obj_attributes.attributes);

            if (!obj_attributes.attributes_follow)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        dir_wcc.after.attributes_follow = GetFileAttributesForNFS((string)dirName, out dir_wcc.after.attributes);

        Write(stat);
        Write(obj_attributes);
        Write(dir_wcc);

        return stat;
    }
    NFS3S ProcedureREADDIR()
    {
        string path ="";
        long cookie;
        long cookieverf;
        int count;
        PostOpAttr dir_attributes = new();
        long fileid;
        Filename3 name=new();
        bool eof;
        bool bFollows;
        NFS3S stat;
        string filePath;
        intptr_t handle;
        int nFound;
        _finddata_t fileinfo;
        uint i, j;

        PrintLog("READDIR");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        Read(out cookie);
        Read(out cookieverf);
        Read(out count);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            dir_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out dir_attributes.attributes);

            if (!dir_attributes.attributes_follow)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        Write(stat);
        Write(dir_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(cookieverf);
            filePath = cStr + "\\*";
            eof = true;
            handle = _findfirst(filePath, ref fileinfo);
            bFollows = true;

            if (handle)
            {
                nFound = 0;

                for (i = (uint)cookie; i > 0; i--)
                {
                    nFound = _findnext(handle, ref fileinfo);
                }

                // TODO: Implement this workaround correctly with the
                // count variable and not a fixed threshold of 10
                if (nFound == 0)
                {
                    j = 10;

                    do
                    {
                        Write(bFollows); //value follows
                        sprintf_s(filePath, "%s\\%s", cStr, fileinfo.name);
                        fileid = CFileTable.GetFileID(filePath);
                        Write(fileid); //file id
                        name.Set(fileinfo.name);
                        Write(name); //name
                        ++cookie;
                        Write(cookie); //cookie
                        if (--j == 0)
                        {
                            eof = false;
                            break;
                        }
                    } while (_findnext(handle, ref fileinfo) == 0);
                }

                _findclose(handle);
            }

            bFollows = false;
            Write(bFollows);
            Write(eof); //eof
        }

        return stat;
    }
    NFS3S ProcedureREADDIRPLUS()
    {
        string path = "";
        long cookie;
        long cookieverf;
        int dircount, maxcount;
        PostOpAttr dir_attributes = new();
        long fileid;
        Filename3 name;
        PostOpAttr name_attributes = new();
        PostOpFh3 name_handle = new();
        bool eof;
        NFS3S stat;
        string filePath;// [MAXPATHLEN];
        intptr_t handle;
        int nFound;
        _finddata_t fileinfo;
        uint i, j;
        bool bFollows;

        PrintLog("READDIRPLUS");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        Read(out cookie);
        Read(out cookieverf);
        Read(out dircount);
        Read(out maxcount);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            dir_attributes.attributes_follow = GetFileAttributesForNFS(cStr, ref dir_attributes.attributes);

            if (!dir_attributes.attributes_follow)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        Write(stat);
        Write(dir_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(cookieverf);
            filePath = cStr + "\\*";

            handle = _findfirst(filePath, ref fileinfo);
            eof = true;

            if (handle)
            {
                nFound = 0;

                for (i = (uint)cookie; i > 0; i--)
                {
                    nFound = _findnext(handle, ref fileinfo);
                }

                if (nFound == 0)
                {
                    bFollows = true;
                    j = 10;

                    do
                    {
                        Write(bFollows); //value follows
                        sprintf_s(filePath, "%s\\%s", cStr, fileinfo.name);
                        fileid = CFileTable.GetFileID(filePath);
                        Write(fileid); //file id
                        name.Set(fileinfo.name);
                        Write(name); //name
                        ++cookie;
                        Write(cookie); //cookie
                        name_attributes.attributes_follow = GetFileAttributesForNFS(filePath, ref name_attributes.attributes);
                        Write(name_attributes);
                        name_handle.handle_follows = GetFileHandle(filePath, ref name_handle.handle);
                        Write(name_handle);

                        if (--j == 0)
                        {
                            eof = false;
                            break;
                        }
                    } while (_findnext(handle, ref fileinfo) == 0);
                }

                _findclose(handle);
            }

            bFollows = false;
            Write(bFollows); //value follows
            Write(eof); //eof
        }

        return stat;
    }
    NFS3S ProcedureFSSTAT()
    {
        string path="";
        PostOpAttr obj_attributes = new();
        long tbytes = 0L, fbytes = 0L, abytes = 0L, tfiles = 0L, ffiles = 0L, afiles = 0L;
        uint invarsec = 0;

        NFS3S stat;

        PrintLog("FSSTAT");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            obj_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out obj_attributes.attributes);

            if (obj_attributes.attributes_follow
                && GetDiskFreeSpaceEx(cStr, 
                (PULARGE_INTEGER) & fbytes, 
                (PULARGE_INTEGER) & tbytes,
                (PULARGE_INTEGER) & abytes)
                )
            {
                //tfiles = 99999999999;
                //ffiles = 99999999999;
                //afiles = 99999999999;
                invarsec = 0;
            }
            else
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        Write(stat);
        Write(obj_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(tbytes);
            Write(fbytes);
            Write(abytes);
            Write(tfiles);
            Write(ffiles);
            Write(afiles);
            Write(invarsec);
        }

        return stat;
    }
    NFS3S ProcedureFSINFO()
    {
        string path = "";
        PostOpAttr obj_attributes = new();
        uint rtmax = 0, rtpref = 0, rtmult = 0, wtmax = 0, wtpref = 0, wtmult = 0, dtpref = 0;
        long maxfilesize = 0;
        NfsTime3 time_delta = new();
        uint properties = 0;
        NFS3S stat;

        PrintLog("FSINFO");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            obj_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out obj_attributes.attributes);

            if (obj_attributes.attributes_follow)
            {
                rtmax = 65536;
                rtpref = 32768;
                rtmult = 4096;
                wtmax = 65536;
                wtpref = 32768;
                wtmult = 4096;
                dtpref = 8192;
                maxfilesize = 0x7FFFFFFFFFFFFFFF;
                time_delta.seconds = 1;
                time_delta.nseconds = 0;
                properties = (uint)(FSF3S.FSF3_LINK | FSF3S.FSF3_SYMLINK | FSF3S.FSF3_CANSETTIME);
            }
            else
            {
                stat = NFS3S.NFS3ERR_SERVERFAULT;
            }
        }

        Write(stat);
        Write(obj_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(rtmax);
            Write(rtpref);
            Write(rtmult);
            Write(wtmax);
            Write(wtpref);
            Write(wtmult);
            Write(dtpref);
            Write(maxfilesize);
            Write(time_delta);
            Write(properties);
        }

        return stat;
    }
    NFS3S ProcedurePATHCONF()
    {
        string path = "";
        PostOpAttr obj_attributes = new();
        NFS3S stat;
        uint linkmax = 0, name_max = 0;
        bool no_trunc = false, chown_restricted = false, case_insensitive = false, case_preserving = false;

        PrintLog("PATHCONF");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            obj_attributes.attributes_follow = GetFileAttributesForNFS(cStr, out obj_attributes.attributes);

            if (obj_attributes.attributes_follow)
            {
                linkmax = 1023;
                name_max = 255;
                no_trunc = true;
                chown_restricted = true;
                case_insensitive = true;
                case_preserving = true;
            }
            else
            {
                stat = NFS3S.NFS3ERR_SERVERFAULT;
            }
        }

        Write(stat);
        Write(obj_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(linkmax);
            Write(name_max);
            Write(no_trunc);
            Write(chown_restricted);
            Write(case_insensitive);
            Write(case_preserving);
        }

        return stat;
    }
    NFS3S ProcedureCOMMIT()
    {
        string path = "";
        int handleId;
        long offset;
        int count;
        WccData file_wcc = new();
        NFS3S stat;
        NfsFh3 file;
        long verf;

        PrintLog("COMMIT");
        Read(out file);
        bool validHandle = fileTable.GetPathByHandle(file.contents, ref path);
        var cStr = validHandle ? path : null;

        if (validHandle)
        {
            PrintLog(" {0} ", path);
        }

        // offset and count are unused
        // offset never was anything else than 0 in my tests
        // count does not matter in the way COMMIT is implemented here
        // to fulfill the spec this should be improved
        Read(out offset);
        Read(out count);

        file_wcc.before.attributes_follow = GetFileAttributesForNFS(cStr, out file_wcc.before.attributes);

        handleId = BitConverter.ToInt32(file.contents);

        if (unstableStorageFile.ContainsKey(handleId))
        {
            if (unstableStorageFile[handleId] != 0)
            {
                //CloseHandle(unstableStorageFile[handleId]);
                unstableStorageFile.Remove(handleId);
                stat = NFS3S.NFS3_OK;
            }
            else
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }
        else
        {
            stat = NFS3S.NFS3_OK;
        }

        file_wcc.after.attributes_follow = GetFileAttributesForNFS(cStr, out file_wcc.after.attributes);

        Write(stat);
        Write(file_wcc);
        // verf should be the timestamp the server startet to notice reboots
        verf = 0;
        Write(verf);

        return stat;
    }
    NFS3S ProcedureNOIMP()
    {
        PrintLog("NOIMP");
        m_nResult = (int)PRC_STATUS.PRC_NOTIMP;

        return NFS3S.NFS3_OK;
    }
    void Read(byte[] buffer)
    {
        if (m_pInStream.Read(buffer) < buffer.Length)
        {
            throw new Exception();
        }
    }
    void Read(out bool pBool)
    {
        uint b;

        if (m_pInStream.Read(out b) < sizeof(uint))
        {
            throw new Exception();
        }

        pBool = b == 1;
    }
    void Read(out int pUint32)
    {
        if (m_pInStream.Read(out pUint32) < sizeof(uint))
        {
            throw new Exception();
        }
    }

    void Read(out uint pUint32)
    {
        if (m_pInStream.Read(out pUint32) < sizeof(uint))
        {
            throw new Exception();
        }
    }
    void Read(out TIMESETS ts)
    {
        this.Read(out int u);
        ts = (TIMESETS)u;
    }
    void Read(out long pUint64)
    {
        if (m_pInStream.Read8(out pUint64) < sizeof(ulong))
        {
            throw new Exception();
        }
    }
    void Read(out ulong pUint64)
    {
        if (m_pInStream.Read8(out pUint64) < sizeof(ulong))
        {
            throw new Exception();
        }
    }
    void Read(out STAS stas)
    {
        Read(out int v);
        stas = (STAS)v;
    }
    void Read(out Sattr3 pAttr)
    {
        pAttr = new();

        Read(out pAttr.mode.set_it);

        if (pAttr.mode.set_it)
        {
            Read(out pAttr.mode.mode);
        }

        Read(out pAttr.uid.set_it);

        if (pAttr.uid.set_it)
        {
            Read(out pAttr.uid.uid);
        }

        Read(out pAttr.gid.set_it);

        if (pAttr.gid.set_it)
        {
            Read(out pAttr.gid.gid);
        }

        Read(out pAttr.size.set_it);

        if (pAttr.size.set_it)
        {
            Read(out pAttr.size.size);
        }

        Read(out pAttr.atime.set_it);

        if (pAttr.atime.set_it == TIMESETS.SET_TO_CLIENT_TIME)
        {
            Read(out pAttr.atime.atime);
        }

        Read(out pAttr.mtime.set_it);

        if (pAttr.mtime.set_it == TIMESETS.SET_TO_CLIENT_TIME)
        {
            Read(out pAttr.mtime.mtime);
        }
    }
    void Read(out sattrguard3 pGuard)
    {
        pGuard = new();
        Read(out pGuard.check);

        if (pGuard.check)
        {
            Read(out pGuard.obj_ctime);
        }
    }
    void Read(out NfsFh3 pPath)
    {
        pPath = new();
        Read(out pPath.length);
        Read(pPath.contents);
    }
    void Read(out Filename3 pfn)
    {
        pfn = new Filename3();
        Read(out pfn.length);
        Read(pfn.contents);
    }
    void Read(out Diropargs3 pDir)
    {
        pDir = new();
        Read(out pDir.dir);
        Read(out pDir.name);

    }
    void Read(out Opaque pOpaque)
    {
        pOpaque = new();
        uint len, b;

        Read(out len);
        pOpaque.SetSize(len);

        if (m_pInStream.Read(pOpaque.contents) < len)
        {
            throw new Exception();
        }

        len = 4 - (len & 3);

        if (len != 4)
        {
            if (m_pInStream.Read(out b) < len)
            {
                throw new Exception();
            }
        }
    }
    void Read(out NfsTime3 pTime)
    {
        pTime = new();
        Read(out pTime.seconds);
        Read(out pTime.nseconds);

    }
    void Read(out Createhow3 pHow)
    {
        pHow = new();
        Read(out pHow.mode);

        if (pHow.mode == STAS.UNCHECKED || pHow.mode == STAS.GUARDED)
        {
            Read(out pHow.obj_attributes);
        }
        else
        {
            Read(out pHow.verf);
        }

    }
    void Read(out NfsPath3 pPath)
    {
        pPath = new();
        Read(out pPath.length);
        Read(pPath.contents);
        pPath.path = Encoding.UTF8.GetString(pPath.contents);
    }
    void Read(out SymLinkData3 pSymlink)
    {
        pSymlink = new();
        Read(out pSymlink.symlink_attributes);
        Read(out pSymlink.symlink_data);

    }
    void Write(bool pBool)
    {
        m_pOutStream?.Write(pBool ? 1 : 0);

    }
    void Write(NFS3S status)
    {
        this.Write((int)status);
    }
    void Write(int pUint32)
    {
        m_pOutStream?.Write(pUint32);

    }
    void Write(uint pUint32)
    {
        m_pOutStream?.Write(pUint32);

    }
    void Write(long pUint64)
    {
        m_pOutStream?.Write8(pUint64);

    }
    void Write(ulong pUint64)
    {
        m_pOutStream?.Write8(pUint64);

    }
    void Write(Fattr3 pAttr)
    {
        Write(pAttr.type);
        Write(pAttr.mode);
        Write(pAttr.nlink);
        Write(pAttr.uid);
        Write(pAttr.gid);
        Write(pAttr.size);
        Write(pAttr.used);
        Write(pAttr.rdev);
        Write(pAttr.fsid);
        Write(pAttr.fileid);
        Write(pAttr.atime);
        Write(pAttr.mtime);
        Write(pAttr.ctime);

    }
    void Write(Opaque pOpaque)
    {
        uint len, b;

        Write(pOpaque.length);
        m_pOutStream?.Write(pOpaque.contents);
        len = pOpaque.length & 3;

        if (len != 0)
        {
            b = 0;
            m_pOutStream.Write(BitConverter.GetBytes(b));//, 4 - len);
        }

    }
    void Write(WccData pWcc)
    {
        Write(pWcc.before);
        Write(pWcc.after);

    }
    void Write(PostOpAttr pAttr)
    {
        Write(pAttr.attributes_follow);

        if (pAttr.attributes_follow)
        {
            Write(pAttr.attributes);
        }

    }
    void Write(PreOpAttr pAttr)
    {
        Write(pAttr.attributes_follow);

        if (pAttr.attributes_follow)
        {
            Write(pAttr.attributes);
        }

    }
    void Write(PostOpFh3 pObj)
    {
        Write(pObj.handle_follows);

        if (pObj.handle_follows)
        {
            Write(pObj.handle);
        }

    }
    void Write(NfsTime3 pTime)
    {
        Write(pTime.seconds);
        Write(pTime.nseconds);

    }
    void Write(Specdata3 pSpec)
    {
        Write(pSpec.specdata1);
        Write(pSpec.specdata2);

    }
    void Write(WccAttr pAttr)
    {
        Write(pAttr.size);
        Write(pAttr.mtime);
        Write(pAttr.ctime);

    }

    int m_nResult;

    bool GetPath(ref string path)
    {
        NfsFh3 obj = new();

        Read(out obj);
        bool valid = fileTable.GetPathByHandle(obj.contents, ref path);
        if (valid)
        {
            PrintLog(" {0} ", path);
        }
        else
        {
            PrintLog(" File handle is invalid ");
        }

        return valid;
    }
    bool ReadDirectory(ref string dirName, ref string fileName)
    {
        Diropargs3 fileRequest = new();
        Read(out fileRequest);

        if (fileTable.GetPathByHandle(fileRequest.dir.contents, ref dirName))
        {
            fileName = fileRequest.name.name;
            return true;
        }
        else
        {
            return false;
        }
    }
    string GetFullPath(ref string dirName, ref string fileName)
    {
        //TODO: Return string
        // char [MAXPATHLEN + 1];
        string fullPath;

        if (dirName.Length + 1 + fileName.Length > MAXPATHLEN)
        {
            return null;
        }

        fullPath = dirName + "\\" + fileName;
        PrintLog(" {0} ", fullPath);

        return fullPath;
    }
    NFS3S CheckFile(string? fullPath)
    {
        if (fullPath == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }

        //if (!FileExists(fullPath)) {
        if (!File.Exists(fullPath))
        {
            return NFS3S.NFS3ERR_NOENT;
        }

        return NFS3S.NFS3_OK;
    }
    NFS3S CheckFile(string directory, string fullPath)
    {
        // FileExists will not work for the root of a drive, e.g. \\?\D:\, therefore check if it is a drive root with GetDriveType
        if (directory == null || (!Directory.Exists(directory)
            && Directory.GetDirectoryRoot(directory) == directory) || fullPath == null)
        {
            return NFS3S.NFS3ERR_STALE;
        }

        if (!File.Exists(fullPath))
        {
            return NFS3S.NFS3ERR_NOENT;
        }

        return NFS3S.NFS3_OK;
    }
    bool GetFileHandle(string path, NfsFh3 pObject)
    {
        var handle = fileTable.GetHandleByPath(path);
        if (handle == null)
        {
            PrintLog("no filehandle(path %s)", path);
            return false;
        }
        pObject.contents = handle;
        return true;
    }
    bool GetFileAttributesForNFS(string path, out WccAttr pAttr)
    {
        pAttr = new();
        if (path == null || !File.Exists(path))
        {
            return false;
        }
        FileInfo f = new(path);

        pAttr.size = (ulong)f.Length;
        pAttr.mtime.seconds = (uint)f.LastWriteTime.Second;
        pAttr.mtime.nseconds = 0;
        // TODO: This needs to be tested (not called on my setup)
        // This seems to be the changed time, not creation time.
        //pAttr.ctime.seconds = data.st_ctime;
        pAttr.ctime.seconds = (uint)f.CreationTime.Second;
        pAttr.ctime.nseconds = 0;

        return true;
    }
    bool GetFileAttributesForNFS(string path, out Fattr3 pAttr)
    {
        var fileAttr = File.GetAttributes(path);


        FileAttributes dwFlagsAndAttributes;

        pAttr = new();


        if (path == null || fileAttr == FileAttributes.None)
        {
            return false;
        }

        dwFlagsAndAttributes = 0;
        if ((fileAttr & FileAttributes.Directory) != 0)
        {
            pAttr.type = (uint)NF3S.NF3DIR;
            dwFlagsAndAttributes = FileAttributes.Directory | FileAttributes.Archive;
        }
        else if ((fileAttr & FileAttributes.Archive) != 0)
        {
            pAttr.type = (uint)NF3S.NF3REG;
            dwFlagsAndAttributes = FileAttributes.Archive | FileAttributes;
        }
        else if ((fileAttr & FileAttributes.Normal) != 0)
        {
            pAttr.type = (uint)NF3S.NF3REG;
            dwFlagsAndAttributes = FileAttributes.Normal | FILE_FLAG_OVERLAPPED;
        }
        else
        {
            pAttr.type = 0;
        }

        if (fileAttr & FileAttributes.ReparsePoint)
        {
            pAttr.type = (uint)NF3S.NF3LNK;
            dwFlagsAndAttributes = FileAttributes.ReparsePoint | FileAttributes.Archive;
        }

        pAttr.mode = 0;

        // Set execution right for all
        pAttr.mode |= 0x49;

        // Set read right for all
        pAttr.mode |= 0x124;

        //if ((lpFileInformation.dwFileAttributes ref  FILE_ATTRIBUTE_READONLY) == 0) {
        pAttr.mode |= 0x92;
        //}

        ulong fileSize = lpFileInformation.nFileSizeHigh;
        fileSize <<= sizeof(lpFileInformation.nFileSizeHigh) * 8;
        fileSize |= lpFileInformation.nFileSizeLow;
        pAttr.nlink = lpFileInformation.nNumberOfLinks;
        pAttr.uid = m_nUID;
        pAttr.gid = m_nGID;
        pAttr.size = fileSize;
        pAttr.used = pAttr.size;
        pAttr.rdev.specdata1 = 0;
        pAttr.rdev.specdata2 = 0;
        pAttr.fsid = 7; //NTFS //4; 
        pAttr.fileid = fileTable.GetIDByPath(path);
        pAttr.atime.seconds = FileTimeToPOSIX(lpFileInformation.ftLastAccessTime);
        pAttr.atime.nseconds = 0;
        pAttr.mtime.seconds = FileTimeToPOSIX(lpFileInformation.ftLastWriteTime);
        pAttr.mtime.nseconds = 0;
        // This seems to be the changed time, not creation time
        pAttr.ctime.seconds = FileTimeToPOSIX(lpFileInformation.ftLastWriteTime);
        pAttr.ctime.nseconds = 0;

        return true;
    }
    uint FileTimeToPOSIX(FILETIME ft)
    {
        // takes the last modified date
        long date = ft.dwHighDateTime << 32 | ft.dwLowDateTime;
        // 100-nanoseconds = milliseconds * 10000
        long adjust = 11644473600000 * 10000;

        // removes the diff between 1970 and 1601
        date -= adjust;

        // converts back from 100-nanoseconds to seconds
        return (uint)(date / 10000000);
    }
}
