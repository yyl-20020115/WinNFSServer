using System.Text;

namespace LibWinNFSServer;

public partial class NFS3Procedure(FileTable? table = null) : RPCProcedure
{
    private uint uid = 0;
    private uint gid = 0;
    private InputStream? ins;
    private OutputStream? outs;
    private ProcessParam? parameter;
    private readonly Dictionary<int, IntPtr> unstable_storage_files = [];
    private readonly FileTable table = table ?? new();
    private PRC_STATUS result;

    public void SetUserID(uint uid, uint gid)
    {
        this.uid = uid;
        this.gid = gid;
    }
    protected delegate NFS3S PROC_INT();
    public override int Process(InputStream ins, OutputStream outs, ProcessParam parameter)
    {
        PROC_INT[] procs = [
            NULL,
            GETATTR,
            SETATTR,
            LOOKUP,
            ACCESS,
            READLINK,
            READ,
            WRITE,
            CREATE,
            MKDIR,
            SYMLINK,
            MKNOD,
            REMOVE,
            RMDIR,
            RENAME,
            LINK,
            READDIR,
            READDIRPLUS,
            FSSTAT,
            FSINFO,
            PATHCONF,
            COMMIT
        ];

        NFS3S? stat = null;

        PrintLog($"[{DateTime.Now}] NFS ");

        if (parameter.Procedure >= procs.Length)
        {
            ProcedureNOIMP();
            PrintLog("\n");

            return (int)PRC_STATUS.PRC_NOTIMP;
        }

        this.ins = ins;
        this.outs = outs;
        this.parameter = parameter;
        result = PRC_STATUS.PRC_OK;

        try
        {
            stat = procs[parameter.Procedure]();
        }
        catch (Exception ex)
        {
            result = PRC_STATUS.PRC_FAIL;
            PrintLog($"Runtime error: {ex.Message}");
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

        PrintLog();

        return (int)result;
    }

    protected NFS3S NULL()
    {
        PrintLog("null");
        return NFS3S.NFS3_OK;
    }
    protected NFS3S GETATTR()
    {
        var path = "";
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
                stat = NFS3S.NFS3ERR_IO;
        }

        Write((int)stat);

        if (stat == NFS3S.NFS3_OK)
            Write(obj_attributes);

        return stat;
    }
    protected NFS3S SETATTR()
    {
        var path = "";
        WccData obj_wcc = new();
        NFS3S stat;
        int nMode;
        FILETIME fileTime;
        //FILE pFile;
        //HANDLE hFile;
        SYSTEMTIME systemTime;

        PrintLog("SETATTR");
        var validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        if (cStr == null) return NFS3S.NFS3ERR_STALE;

        Read(out Sattr3 new_attributes);
        Read(out Sattrguard3 guard);
        stat = CheckFile(cStr);
        obj_wcc.Before.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_wcc.Before.Attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            if (new_attributes.Mode.SetIt) nMode = 0;
            // deliberately not implemented because we cannot reflect uid/gid on windows (easliy)
            if (new_attributes.Uid.SetIt) { }
            if (new_attributes.Gid.SetIt) { }

            // deliberately not implemented
            if (new_attributes.MTime.SetIt == TIME_SETS.SET_TO_CLIENT_TIME) { }
            if (new_attributes.ATime.SetIt == TIME_SETS.SET_TO_CLIENT_TIME) { }

            if (new_attributes.MTime.SetIt == TIME_SETS.SET_TO_SERVER_TIME || new_attributes.ATime.SetIt == TIME_SETS.SET_TO_SERVER_TIME)
            {
                var f = new FileInfo(cStr);
                if (new_attributes.MTime.SetIt == TIME_SETS.SET_TO_SERVER_TIME)
                {
                    //TODO:
                    //SetFileTime(hFile, null, null, ref fileTime);
                }
                if (new_attributes.ATime.SetIt == TIME_SETS.SET_TO_SERVER_TIME)
                {
                    //TODO:
                    //SetFileTime(hFile, null, ref fileTime, null);
                }
            }

            if (new_attributes.Size.SetIt)
            {
                using var fs = new FileStream(cStr, FileMode.Open, FileAccess.ReadWrite);
                fs.SetLength((long)new_attributes.Size.Size);
            }
        }

        obj_wcc.After.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_wcc.After.Attributes);

        Write(stat);
        Write(obj_wcc);

        return stat;
    }
    protected NFS3S LOOKUP()
    {
        string path;
        NfsFh3 obj = new();
        PostOpAttr obj_attributes = new();
        PostOpAttr dir_attributes = new();
        NFS3S stat;

        PrintLog("LOOKUP");

        var dirName = "";
        var fileName = "";
        ReadDirectory(ref dirName, ref fileName);

        path = GetFullPath(ref dirName, ref fileName);
        stat = CheckFile(dirName, path);

        if (stat == NFS3S.NFS3_OK)
        {
            GetFileHandle(path, obj);
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(path, out obj_attributes.Attributes);
        }

        dir_attributes.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_attributes.Attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_attributes);

        return stat;
    }
    protected NFS3S ACCESS()
    {
        var path = "";
        PostOpAttr obj_attributes = new();
        NFS3S stat;

        PrintLog("ACCESS");
        var validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        Read(out uint access);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3ERR_NOENT)
        {
            stat = NFS3S.NFS3ERR_STALE;
        }

        obj_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_attributes.Attributes);

        Write(stat);
        Write(obj_attributes);

        if (stat == NFS3S.NFS3_OK)
            Write(access);

        return stat;
    }
    protected NFS3S READLINK()
    {
        PrintLog("READLINK");
        var path = string.Empty;
        var pMBBuffer = string.Empty;

        PostOpAttr symlink_attributes = new();
        NfsPath3 data = new();

        //opaque data;
        NFS3S stat;

        REPARSE_DATA_BUFFER lpOutBuffer;
        uint bytesReturned;

        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        if (cStr == null) return NFS3S.NFS3ERR_STALE;

        stat = CheckFile(cStr);
        //if (stat == NFS3S.NFS3_OK)
        //{

        //    hFile = CreateFile(cStr, GENERIC_READ, FILE_SHARE_READ, null, OPEN_EXISTING, FILE_ATTRIBUTE_REPARSE_POINT | FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS, null);

        //    if (hFile == INVALID_HANDLE_VALUE)
        //    {
        //        stat = NFS3S.NFS3ERR_IO;
        //    }
        //    else
        //    {
        //        lpOutBuffer = (REPARSE_DATA_BUFFER)malloc(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
        //        if (!lpOutBuffer)
        //        {
        //            stat = NFS3S.NFS3ERR_IO;
        //        }
        //        else
        //        {
        //            DeviceIoControl(hFile, FSCTL_GET_REPARSE_POINT, null, 0, lpOutBuffer, MAXIMUM_REPARSE_DATA_BUFFER_SIZE, ref bytesReturned, null);
        //            string finalSymlinkPath;
        //            if (lpOutBuffer.ReparseTag == IO_REPARSE_TAG_SYMLINK || lpOutBuffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
        //            {
        //                if (lpOutBuffer.ReparseTag == IO_REPARSE_TAG_SYMLINK)
        //                {
        //                    size_t plen = lpOutBuffer.SymbolicLinkReparseBuffer.PrintNameLength / sizeof(char);
        //                    string szPrintName;//= new WCHAR[plen + 1];
        //                    wcsncpy_s(szPrintName, plen + 1, ref lpOutBuffer.SymbolicLinkReparseBuffer.PathBuffer[lpOutBuffer.SymbolicLinkReparseBuffer.PrintNameOffset / sizeof(WCHAR)], plen);
        //                    szPrintName[plen] = 0;
        //                    string wStringTemp = (szPrintName);
        //                    string cPrintName = wstring_to_dbcs(wStringTemp);

        //                    finalSymlinkPath.assign(cPrintName);
        //                    // TODO: Revisit with cleaner solution
        //                    if (!PathIsRelative(cPrintName))
        //                    {
        //                        string strFromChar;
        //                        strFromChar.append("\\\\?\\");
        //                        strFromChar.append(cPrintName);
        //                        string target = _strdup(strFromChar);
        //                        // remove last folder
        //                        int lastFolderIndex = path.find_last_of('\\');
        //                        if (lastFolderIndex !=npos)
        //                        {
        //                            path = path.substr(0, lastFolderIndex);
        //                        }
        //                        string szOut;// [MAX_PATH] = "";
        //                        PathRelativePathTo(szOut, cStr, FILE_ATTRIBUTE_DIRECTORY, target, FILE_ATTRIBUTE_DIRECTORY);
        //                        string symlinkPath=(szOut);
        //                        finalSymlinkPath.assign(symlinkPath);
        //                    }
        //                }

        //                // TODO: Revisit with cleaner solution
        //                if (lpOutBuffer.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
        //                {
        //                    int slen = lpOutBuffer.MountPointReparseBuffer.SubstituteNameLength / sizeof(char);
        //                    string szSubName;//= new WCHAR[slen + 1];
        //                    wcsncpy_s(szSubName, slen + 1, lpOutBuffer.MountPointReparseBuffer.PathBuffer[lpOutBuffer.MountPointReparseBuffer.SubstituteNameOffset / sizeof(WCHAR)], slen);
        //                    szSubName[slen] = 0;
        //                    string wStringTemp = (szSubName);
        //                    string target = wstring_to_dbcs(wStringTemp);
        //                    target.erase(0, 2);
        //                    target.insert(0, 2, '\\');
        //                    // remove last folder, see above
        //                    int lastFolderIndex = path.find_last_of('\\');
        //                    if (lastFolderIndex != npos)
        //                    {
        //                        path = path.substr(0, lastFolderIndex);
        //                    }
        //                    string szOut = "";// [MAX_PATH] = "";
        //                    PathRelativePathTo(szOut, cStr, FILE_ATTRIBUTE_DIRECTORY, target, FILE_ATTRIBUTE_DIRECTORY);
        //                    string symlinkPath = szOut;
        //                    finalSymlinkPath.assign(symlinkPath);
        //                }

        //                // write path always with / separator, so windows created symlinks work too
        //                replace(finalSymlinkPath.begin(), finalSymlinkPath.end(), '\\', '/');
        //                string result = (finalSymlinkPath);
        //                data.Set(result);
        //            }

        //        }
        //    }
        //}

        symlink_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out symlink_attributes.Attributes);

        Write(stat);
        Write(symlink_attributes);
        if (stat == NFS3S.NFS3_OK) Write(data);

        return stat;
    }
    protected NFS3S READ()
    {
        var path = "";
        PostOpAttr file_attributes = new();
        var eof = false;
        Opaque data = new();
        NFS3S stat;

        PrintLog("READ");
        var validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        if (cStr == null) return NFS3S.NFS3ERR_STALE;
        Read(out long offset);
        Read(out int count);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            data.SetSize((uint)count);

            if (File.Exists(cStr))
            {
                using var fs = new FileStream(cStr, FileMode.Open, FileAccess.Read);
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(data.Contents);
                eof = fs.Position == fs.Length;
            }
            else
            {
                var errorNumber = WinAPIs.GetLastError();
                PrintLog($"{errorNumber}");
                stat = errorNumber == 13 ? NFS3S.NFS3ERR_ACCES : NFS3S.NFS3ERR_IO;
            }
        }

        file_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out file_attributes.Attributes);

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
    protected NFS3S WRITE()
    {
        var path = "";
        WccData file_wcc = new();
        long verf = 0;
        NFS3S stat;

        PrintLog("WRITE");
        var validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        if (cStr == null) return NFS3S.NFS3ERR_STALE;
        Read(out long offset);
        Read(out int count);
        Read(out int stable);
        Read(out Opaque data);
        stat = CheckFile(cStr);

        file_wcc.Before.AttributesFollow = GetFileAttributesForNFS(cStr, out file_wcc.Before.Attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            if (stable == (int)SYNC_MODES.UNSTABLE)
            {
                NfsFh3 handle = new();
                GetFileHandle(cStr, handle);
                //int handleId = *(uint*)handle.contents;

                //if (unstableStorageFile.ContainsKey(handleId))
                //{
                //    pFile = _fsopen(cStr, "r+b", _SH_DENYWR);
                //    if (pFile != null)
                //    {
                //        unstableStorageFile.Add(handleId, pFile);
                //    }
                //}
                //else
                //{
                //    pFile = unstableStorageFile[handleId];
                //}

                //if (pFile != null)
                //{
                //    _fseeki64(pFile, offset, SEEK_SET);
                //    count = (count3_32)fwrite(data.contents, sizeof(char), data.length, pFile);
                //}
                //else
                {
                    int errorNumber = WinAPIs.GetLastError();
                    PrintLog($"{errorNumber}");

                    stat = errorNumber == 13 ? NFS3S.NFS3ERR_ACCES : NFS3S.NFS3ERR_IO;
                }
                // this should not be zero but a timestamp (process start time) instead
                verf = 0;
                // we can reuse this, because no physical write has happend
                file_wcc.After.AttributesFollow = file_wcc.Before.AttributesFollow;
            }
            else
            {
                using var fs = new FileStream(cStr, FileMode.OpenOrCreate, FileAccess.Write);
                if (fs != null)
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(data.Contents);
                    count = data.Contents.Length;
                }
                else
                {
                    var errorNumber = WinAPIs.GetLastError();
                    PrintLog($"{errorNumber}");

                    stat = errorNumber == 13 ? NFS3S.NFS3ERR_ACCES : NFS3S.NFS3ERR_IO;
                }

                stable = (int)SYNC_MODES.FILE_SYNC;
                verf = 0;

                file_wcc.After.AttributesFollow = GetFileAttributesForNFS(
                    cStr, out file_wcc.After.Attributes);
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
    protected NFS3S CREATE()
    {
        string path = null;
        CreateHow3 how = new();
        PostOpFh3 obj = new();
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();
        NFS3S stat;
        var errorNumber = 0;

        PrintLog("CREATE");
        var dirName = "";
        var fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        Read(out how);

        dir_wcc.Before.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.Before.Attributes);

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        if (fs.CanWrite)
        {
            stat = NFS3S.NFS3_OK;
        }
        else
        {
            PrintLog($"{WinAPIs.GetLastError()}");

            stat = errorNumber == 2 ? NFS3S.NFS3ERR_STALE : errorNumber == 13 ? NFS3S.NFS3ERR_ACCES : NFS3S.NFS3ERR_IO;
        }

        if (stat == NFS3S.NFS3_OK)
        {
            obj.HandleFollows = GetFileHandle(path, obj.Handle);
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(path, out obj_attributes.Attributes);
        }

        dir_wcc.After.AttributesFollow = GetFileAttributesForNFS((string)dirName, out dir_wcc.After.Attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_wcc);

        return stat;
    }
    protected NFS3S MKDIR()
    {
        string path;
        Sattr3 attributes = new();
        PostOpFh3 obj = new();
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();
        NFS3S stat;

        PrintLog("MKDIR");

        var dirName = "";
        var fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        Read(out attributes);

        dir_wcc.Before.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.Before.Attributes);

        int result = 0;
        Directory.CreateDirectory(path);
        int e = WinAPIs.GetLastError();
        if (result == 0)
        {
            stat = NFS3S.NFS3_OK;
            obj.HandleFollows = GetFileHandle(path, obj.Handle);
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(path, out obj_attributes.Attributes);
        }
        else if (e == WinAPIs.FILE_EXISTS)
        {
            PrintLog("Directory already exists.");
            stat = NFS3S.NFS3ERR_EXIST;
        }
        else if (e == WinAPIs.FILE_NOT_FOUND)
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

        dir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.After.Attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_wcc);

        return stat;
    }
    protected NFS3S SYMLINK()
    {
        PrintLog("SYMLINK");

        string path;
        PostOpFh3 obj = new();
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();
        NFS3S stat;

        DirOpArgs3 where = new();
        SymLinkData3 symlink = new();

        uint dwFlags = 0;

        var dirName = "";
        var fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);

        Read(out symlink);

        var lpSymlinkFileName = path; // symlink (full path)

        // TODO: Maybe revisit this later for a cleaner solution
        // Convert target path to windows path format, maybe this could also be done
        // in a safer way by a combination of PathRelativePathTo and GetFullPathName.
        // Without this conversion nested folder symlinks do not work cross platform.
        var strFromChar = symlink.Symlink_Data.Path; // target (should be relative path));
        strFromChar = strFromChar.Replace('/', '\\');
        var lpTargetFileName = (strFromChar);

        var fullTargetPath = $"{dirName}{"\\"}{lpTargetFileName}";

        // Relative path do not work with GetFileAttributes (directory are not recognized)
        // so we normalize the path before calling GetFileAttributes
        string fullTargetPathNormalized;// [MAX_PATH];
        string fullTargetPathString = (fullTargetPath);
        fullTargetPathNormalized = Path.GetFullPath(fullTargetPathString);
        var targetFileAttr = File.GetAttributes(fullTargetPathNormalized);

        dwFlags = 0x0;
        if ((targetFileAttr & FileAttributes.Directory) != 0)
        {
            //dwFlags = SYMBOLIC_LINK_FLAG_DIRECTORY;
        }

        var failed = File.CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName/*, dwFlags*/);

        if (failed.Exists)
        {
            stat = NFS3S.NFS3_OK;
            obj.HandleFollows = GetFileHandle(path, obj.Handle);
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(path, out obj_attributes.Attributes);
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

        dir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.After.Attributes);

        Write(stat);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(obj);
            Write(obj_attributes);
        }

        Write(dir_wcc);

        return stat;
    }
    protected NFS3S MKNOD()
    {
        PrintLog("MKNOD");

        return NFS3S.NFS3ERR_NOTSUPP;
    }
    protected NFS3S REMOVE()
    {
        string path;
        WccData dir_wcc = new();
        NFS3S stat;


        PrintLog("REMOVE");

        var dirName = "";
        var fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        stat = CheckFile(dirName, path);

        dir_wcc.Before.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.Before.Attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            var fileAttr = File.GetAttributes(path);
            if ((fileAttr & FileAttributes.Directory) != 0 && (fileAttr & FileAttributes.ReparsePoint) != 0)
            {
                var returnCode = table.RemoveFolder(path);
                if (returnCode != 0)
                    stat = returnCode == WinAPIs.ERROR_DIR_NOT_EMPTY ? NFS3S.NFS3ERR_NOTEMPTY : NFS3S.NFS3ERR_IO;
            }
            else
            {
                if (0 != table.RemoveFile(path))
                {
                    stat = NFS3S.NFS3ERR_IO;
                }
            }
        }

        dir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.After.Attributes);

        Write(stat);
        Write(dir_wcc);

        return stat;
    }
    protected NFS3S RMDIR()
    {
        var path = "";
        WccData dir_wcc = new();
        NFS3S stat;
        int returnCode;

        PrintLog("RMDIR");

        var dirName = "";
        var fileName = "";
        ReadDirectory(ref dirName, ref fileName);
        path = GetFullPath(ref dirName, ref fileName);
        stat = CheckFile(dirName, path);

        dir_wcc.Before.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.Before.Attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            returnCode = table.RemoveFolder(path);
            if (returnCode != 0)
                stat = returnCode == WinAPIs.ERROR_DIR_NOT_EMPTY ? NFS3S.NFS3ERR_NOTEMPTY : NFS3S.NFS3ERR_IO;
        }

        dir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.After.Attributes);

        Write(stat);
        Write(dir_wcc);

        return stat;
    }
    protected NFS3S RENAME()
    {
        string pathFrom, pathTo;
        WccData fromdir_wcc = new(), todir_wcc = new();
        NFS3S stat;
        int returnCode;

        PrintLog("RENAME");

        var dirFromName = "";
        var fileFromName = "";
        ReadDirectory(ref dirFromName, ref fileFromName);
        pathFrom = GetFullPath(ref dirFromName, ref fileFromName);

        var dirToName = "";
        var fileToName = "";
        ReadDirectory(ref dirToName, ref fileToName);
        pathTo = GetFullPath(ref dirToName, ref fileToName);

        stat = CheckFile(dirFromName, pathFrom);

        fromdir_wcc.Before.AttributesFollow = GetFileAttributesForNFS(dirFromName, out fromdir_wcc.Before.Attributes);
        todir_wcc.Before.AttributesFollow = GetFileAttributesForNFS(dirToName, out todir_wcc.Before.Attributes);

        if (File.Exists(pathTo))
        {
            var fileAttr = File.GetAttributes(pathTo);
            if ((fileAttr & FileAttributes.Directory) != 0 && (fileAttr & FileAttributes.ReparsePoint) != 0)
            {
                returnCode = table.RemoveFolder(pathTo);
                if (returnCode != 0)
                    stat = returnCode == WinAPIs.ERROR_DIR_NOT_EMPTY ? NFS3S.NFS3ERR_NOTEMPTY : NFS3S.NFS3ERR_IO;
            }
            else
            {
                if (0 == table.RemoveFile(pathTo))
                    stat = NFS3S.NFS3ERR_IO;
            }
        }

        if (stat == NFS3S.NFS3_OK)
        {
            var errorNumber = table.RenameDirectory(pathFrom, pathTo);

            if (errorNumber != 0)
            {
                PrintLog($"Error {errorNumber}");

                stat = errorNumber == 13 ? NFS3S.NFS3ERR_ACCES : NFS3S.NFS3ERR_IO;
            }
        }

        fromdir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirFromName, out fromdir_wcc.After.Attributes);
        todir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirToName, out todir_wcc.After.Attributes);

        Write(stat);
        Write(fromdir_wcc);
        Write(todir_wcc);

        return stat;
    }
    protected NFS3S LINK()
    {
        PrintLog("LINK");
        var path = "";
        DirOpArgs3 link = new();
        var dirName = "";
        var fileName = "";
        NFS3S stat;
        PostOpAttr obj_attributes = new();
        WccData dir_wcc = new();

        var validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        if (cStr == null) return NFS3S.NFS3ERR_STALE;
        ReadDirectory(ref dirName, ref fileName);

        var linkFullPath = GetFullPath(ref dirName, ref fileName);

        //TODO: Improve checks here, cStr may be null because handle is invalid
        var fi = File.CreateSymbolicLink(linkFullPath, cStr);
        if (!fi.Exists)
        {
            stat = NFS3S.NFS3ERR_IO;
        }
        stat = CheckFile(linkFullPath);
        if (stat == NFS3S.NFS3_OK)
        {
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_attributes.Attributes);

            if (!obj_attributes.AttributesFollow)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        dir_wcc.After.AttributesFollow = GetFileAttributesForNFS(dirName, out dir_wcc.After.Attributes);

        Write(stat);
        Write(obj_attributes);
        Write(dir_wcc);

        return stat;
    }
    protected NFS3S READDIR()
    {
        var path = "";
        PostOpAttr dir_attributes = new();
        long fileid;
        Filename3 name = new();
        bool eof;
        bool bFollows;
        NFS3S stat;
        string filePath;
        int nFound;
        //intptr_t handle;
        //_finddata_t fileinfo;
        uint i, j;

        PrintLog("READDIR");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        Read(out long cookie);
        Read(out long cookieverf);
        Read(out int count);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            dir_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out dir_attributes.Attributes);

            if (!dir_attributes.AttributesFollow)
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
            //handle = _findfirst(filePath, ref fileinfo);
            bFollows = true;

            //if (handle)
            {
                nFound = 0;

                for (i = (uint)cookie; i > 0; i--)
                {
                    //nFound = _findnext(handle, ref fileinfo);
                }

                // TODO: Implement this workaround correctly with the
                // count variable and not a fixed threshold of 10
                if (nFound == 0)
                {
                    j = 10;

                    foreach (var filename in Directory.GetFiles(path, "*.*"))
                    {
                        Write(bFollows); //value follows
                        filePath = $"{cStr}\\{filename}";// fileinfo.name;
                        fileid = table.GetIDByPath(filePath);
                        Write(fileid); //file id
                        name.Set(filename);// fileinfo.name);
                        Write(name); //name
                        ++cookie;
                        Write(cookie); //cookie
                        if (--j == 0)
                        {
                            eof = false;
                            break;
                        }
                    }// while (_findnext(handle, ref fileinfo) == 0);
                }

            }

            bFollows = false;
            Write(bFollows);
            Write(eof); //eof
        }

        return stat;
    }
    protected NFS3S READDIRPLUS()
    {
        var path = "";
        PostOpAttr dir_attributes = new();
        long fileid;
        Filename3 name = new();
        PostOpAttr name_attributes = new();
        PostOpFh3 name_handle = new();
        bool eof;
        NFS3S stat;
        string filePath;// [MAXPATHLEN];
        int nFound;
        uint i, j;
        bool bFollows;

        //intptr_t handle;
        //_finddata_t fileinfo;

        PrintLog("READDIRPLUS");
        bool validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        Read(out long cookie);
        Read(out long cookieverf);
        Read(out int dircount);
        Read(out int maxcount);
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            dir_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out dir_attributes.Attributes);

            if (!dir_attributes.AttributesFollow)
            {
                stat = NFS3S.NFS3ERR_IO;
            }
        }

        Write(stat);
        Write(dir_attributes);

        if (stat == NFS3S.NFS3_OK)
        {
            Write(cookieverf);
            filePath = $"{cStr}\\*";

            //handle = _findfirst(filePath, ref fileinfo);
            eof = true;

            //if (handle)
            {
                nFound = 0;

                for (i = (uint)cookie; i > 0; i--)
                {
                    //nFound = _findnext(handle, ref fileinfo);
                }

                if (nFound == 0)
                {
                    bFollows = true;
                    j = 10;

                    foreach (var filename in Directory.GetFiles(path, "*.*"))
                    {
                        //fileinfo.name = name2;
                        Write(bFollows); //value follows
                        filePath = $"{cStr}\\{filename}";// fileinfo.name;
                        fileid = table.GetIDByPath(filePath);
                        Write(fileid); //file id
                        name.Set(filename);// fileinfo.name);
                        Write(name); //name
                        ++cookie;
                        Write(cookie); //cookie
                        name_attributes.AttributesFollow = GetFileAttributesForNFS(filePath, out name_attributes.Attributes);
                        Write(name_attributes);
                        name_handle.HandleFollows = GetFileHandle(filePath, name_handle.Handle);
                        Write(name_handle);

                        if (--j == 0)
                        {
                            eof = false;
                            break;
                        }
                    } //while (_findnext(handle, ref fileinfo) == 0);
                }
            }

            bFollows = false;
            Write(bFollows); //value follows
            Write(eof); //eof
        }

        return stat;
    }
    protected NFS3S FSSTAT()
    {
        var path = "";
        PostOpAttr obj_attributes = new();
        long tbytes = 0L, fbytes = 0L, abytes = 0L, tfiles = 0L, ffiles = 0L, afiles = 0L;
        uint invarsec = 0;

        NFS3S stat;

        PrintLog("FSSTAT");
        var validHandle = GetPath(ref path);
        var cStr = validHandle ? path : null;
        stat = CheckFile(cStr);

        if (stat == NFS3S.NFS3_OK)
        {
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_attributes.Attributes);

            if (obj_attributes.AttributesFollow)
            {
                DriveInfo driveInfo = new(cStr);
                fbytes = driveInfo.TotalFreeSpace;
                tbytes = driveInfo.TotalSize;
                abytes = driveInfo.AvailableFreeSpace;
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
    protected NFS3S FSINFO()
    {
        var path = "";
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
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_attributes.Attributes);

            if (obj_attributes.AttributesFollow)
            {
                rtmax = 65536;
                rtpref = 32768;
                rtmult = 4096;
                wtmax = 65536;
                wtpref = 32768;
                wtmult = 4096;
                dtpref = 8192;
                maxfilesize = 0x7FFFFFFFFFFFFFFF;
                time_delta.Seconds = 1;
                time_delta.NSeconds = 0;
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
    protected NFS3S PATHCONF()
    {
        var path = "";
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
            obj_attributes.AttributesFollow = GetFileAttributesForNFS(cStr, out obj_attributes.Attributes);

            if (obj_attributes.AttributesFollow)
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
    protected NFS3S COMMIT()
    {
        var path = "";
        int handleId;
        WccData file_wcc = new();
        NFS3S stat;
        long verf;

        PrintLog("COMMIT");
        Read(out NfsFh3 file);
        bool validHandle = table.GetPathByHandle(file.Contents, ref path);
        var cStr = validHandle ? path : null;

        if (validHandle)
            PrintLog($" {path} ");

        // offset and count are unused
        // offset never was anything else than 0 in my tests
        // count does not matter in the way COMMIT is implemented here
        // to fulfill the spec this should be improved
        Read(out long offset);
        Read(out int count);

        file_wcc.Before.AttributesFollow = GetFileAttributesForNFS(cStr, out file_wcc.Before.Attributes);

        handleId = BitConverter.ToInt32(file.Contents);

        if (unstable_storage_files.TryGetValue(handleId, out nint value))
        {
            if (value != 0)
            {
                //CloseHandle(unstableStorageFile[handleId]);
                unstable_storage_files.Remove(handleId);
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

        file_wcc.After.AttributesFollow = GetFileAttributesForNFS(cStr, out file_wcc.After.Attributes);

        Write(stat);
        Write(file_wcc);
        // verf should be the timestamp the server startet to notice reboots
        verf = 0;
        Write(verf);

        return stat;
    }
    protected NFS3S ProcedureNOIMP()
    {
        PrintLog("NOIMP");
        result = PRC_STATUS.PRC_NOTIMP;

        return NFS3S.NFS3_OK;
    }
    protected void Read(byte[] buffer)
    {
        if (ins.Read(buffer) < buffer.Length)
            throw new Exception();
    }
    protected void Read(out bool value)
    {
        if (ins.Read(out uint b) < sizeof(uint))
            throw new Exception();

        value = b == 1;
    }
    protected void Read(out int value)
    {
        if (ins.Read(out value) < sizeof(uint))
            throw new Exception();
    }

    protected void Read(out uint value)
    {
        if (ins.Read(out value) < sizeof(uint))
            throw new Exception();
    }
    protected void Read(out TIME_SETS sets)
    {
        this.Read(out int u);
        sets = (TIME_SETS)u;
    }
    protected void Read(out long value)
    {
        if (ins.Read8(out value) < sizeof(ulong))
            throw new Exception();
    }
    protected void Read(out ulong value)
    {
        if (ins.Read(out value) < sizeof(ulong))
            throw new Exception();
    }
    protected void Read(out STAS stas)
    {
        Read(out int v);
        stas = (STAS)v;
    }
    protected void Read(out Sattr3 attribute)
    {
        attribute = new();

        Read(out attribute.Mode.SetIt);

        if (attribute.Mode.SetIt)
            Read(out attribute.Mode.Mode);

        Read(out attribute.Uid.SetIt);

        if (attribute.Uid.SetIt)
            Read(out attribute.Uid.Uid);

        Read(out attribute.Gid.SetIt);

        if (attribute.Gid.SetIt)
            Read(out attribute.Gid.Gid);

        Read(out attribute.Size.SetIt);

        if (attribute.Size.SetIt)
            Read(out attribute.Size.Size);

        Read(out attribute.ATime.SetIt);

        if (attribute.ATime.SetIt == TIME_SETS.SET_TO_CLIENT_TIME)
            Read(out attribute.ATime.ATime);

        Read(out attribute.MTime.SetIt);

        if (attribute.MTime.SetIt == TIME_SETS.SET_TO_CLIENT_TIME)
            Read(out attribute.MTime.MTime);
    }
    protected void Read(out Sattrguard3 guard)
    {
        guard = new();
        Read(out guard.Check);

        if (guard.Check)
            Read(out guard.Obj_CTime);
    }
    protected void Read(out NfsFh3 fh)
    {
        fh = new();
        Read(out fh.Length);
        Read(fh.Contents = new byte[fh.Length]);
    }
    protected void Read(out Filename3 name)
    {
        name = new Filename3();
        Read(out name.Length);
        Read(name.Contents=new byte[name.Length]);
    }
    protected void Read(out DirOpArgs3 arg)
    {
        arg = new();
        Read(out arg.Dir);
        Read(out arg.Name);

    }
    protected void Read(out Opaque opaque)
    {
        opaque = new();
        Read(out uint len);
        opaque.SetSize(len);

        if (ins.Read(opaque.Contents) < len)
            throw new Exception();

        len = 4 - (len & 3);

        if (len != 4)
        {
            if (ins.Read(out uint b) < len)
                throw new Exception();
        }
    }
    protected void Read(out NfsTime3 time)
    {
        time = new();
        Read(out time.Seconds);
        Read(out time.NSeconds);
    }
    protected void Read(out CreateHow3 how)
    {
        how = new();
        Read(out how.Mode);

        switch (how.Mode)
        {
            case STAS.UNCHECKED or STAS.GUARDED:
                Read(out how.Obj_Attributes);
                break;
            default:
                Read(out how.Verification);
                break;
        }
    }
    protected void Read(out NfsPath3 path)
    {
        path = new();
        Read(out path.Length);
        Read(path.Contents = new byte[path.Length]);
        path.Path = Encoding.UTF8.GetString(path.Contents);
    }
    protected void Read(out SymLinkData3 link)
    {
        link = new();
        Read(out link.Symlink_Attributes);
        Read(out link.Symlink_Data);

    }
    protected void Write(bool value)
    {
        outs?.Write(value ? 1 : 0);
    }
    protected void Write(NFS3S status)
    {
        this.Write((int)status);
    }
    protected void Write(int value)
    {
        outs?.Write(value);
    }
    protected void Write(uint value)
    {
        outs?.Write(value);
    }
    protected void Write(long value)
    {
        outs?.Write8(value);
    }
    protected void Write(ulong value)
    {
        outs?.Write8(value);
    }
    protected void Write(Fattr3 attribute)
    {
        Write(attribute.Type);
        Write(attribute.Mode);
        Write(attribute.NLink);
        Write(attribute.Uid);
        Write(attribute.Gid);
        Write(attribute.Size);
        Write(attribute.Used);
        Write(attribute.Rdev);
        Write(attribute.Fsid);
        Write(attribute.FileId);
        Write(attribute.ATime);
        Write(attribute.MTime);
        Write(attribute.CTime);
    }
    protected void Write(Opaque opaque)
    {
        uint len, b;

        Write(opaque.Length);
        outs?.Write(opaque.Contents);
        len = opaque.Length & 3;

        if (len != 0)
        {
            b = 0;
            outs.Write(BitConverter.GetBytes(b));//, 4 - len);
        }
    }
    protected void Write(WccData data)
    {
        Write(data.Before);
        Write(data.After);
    }
    protected void Write(PostOpAttr attribute)
    {
        Write(attribute.AttributesFollow);
        if (attribute.AttributesFollow)
            Write(attribute.Attributes);
    }
    protected void Write(PreOpAttr attribute)
    {
        Write(attribute.AttributesFollow);
        if (attribute.AttributesFollow)
            Write(attribute.Attributes);
    }
    protected void Write(PostOpFh3 fh)
    {
        Write(fh.HandleFollows);
        if (fh.HandleFollows)
            Write(fh.Handle);
    }
    protected void Write(NfsTime3 time)
    {
        Write(time.Seconds);
        Write(time.NSeconds);
    }
    protected void Write(Specdata3 spec)
    {
        Write(spec.SpecData1);
        Write(spec.SpecData2);
    }
    protected void Write(WccAttribute attribute)
    {
        Write(attribute.Size);
        Write(attribute.MTime);
        Write(attribute.CTime);
    }

    protected bool GetPath(ref string path)
    {
        Read(out NfsFh3 obj);
        bool valid = table.GetPathByHandle(obj.Contents, ref path);
        if (valid)
        {
            PrintLog($" {path} ");
        }
        else
        {
            PrintLog(" File handle is invalid ");
        }

        return valid;
    }
    protected bool ReadDirectory(ref string dirName, ref string fileName)
    {
        DirOpArgs3 fileRequest = new();
        Read(out fileRequest);

        if (table.GetPathByHandle(fileRequest.Dir.Contents, ref dirName))
        {
            fileName = fileRequest.Name.Name;
            return true;
        }
        else
        {
            return false;
        }
    }
    protected string GetFullPath(ref string dirName, ref string fileName)
    {
        if (dirName.Length + 1 + fileName.Length > MAXPATHLEN)
            return null;

        var fullPath = $"{dirName}\\{fileName}";
        PrintLog($" {fullPath} ");

        return fullPath;
    }
    protected static NFS3S CheckFile(string? fullPath) => fullPath switch
    {
        null => NFS3S.NFS3ERR_STALE,
        _ => !File.Exists(fullPath) ? NFS3S.NFS3ERR_NOENT : NFS3S.NFS3_OK
    };
    protected static NFS3S CheckFile(string directory, string fullPath) =>
        // FileExists will not work for the root of a drive, e.g. \\?\D:\, therefore check if it is a drive root with GetDriveType
        directory == null || (!Directory.Exists(directory)
            && Directory.GetDirectoryRoot(directory) == directory) || fullPath == null
            ? NFS3S.NFS3ERR_STALE
            : !File.Exists(fullPath) ? NFS3S.NFS3ERR_NOENT 
        : NFS3S.NFS3_OK
        ;
    protected bool GetFileHandle(string path, NfsFh3 fh)
    {
        var handle = table.GetHandleByPath(path);
        if (handle == null)
        {
            PrintLog("no filehandle(path %s)", path);
            return false;
        }
        fh.Contents = handle;
        return true;
    }

    protected static bool GetFileAttributesForNFS(string path, out WccAttribute attribute)
    {
        attribute = new();
        if (path == null || !File.Exists(path)) return false;
        FileInfo f = new(path);
        attribute.Size = (ulong)f.Length;
        attribute.MTime.Seconds = (uint)f.LastWriteTime.Second;
        attribute.MTime.NSeconds = 0;
        // TODO: This needs to be tested (not called on my setup)
        // This seems to be the changed time, not creation time.
        //pAttr.ctime.seconds = data.st_ctime;
        attribute.CTime.Seconds = (uint)f.CreationTime.Second;
        attribute.CTime.NSeconds = 0;

        return true;
    }
    protected bool GetFileAttributesForNFS(string path, out Fattr3 attribute)
    {
        var fileAttr = File.GetAttributes(path);
        attribute = new();

        if (path == null || fileAttr == FileAttributes.None) return false;

        if ((fileAttr & FileAttributes.Directory) != 0)
        {
            attribute.Type = (uint)NF3S.NF3DIR;
        }
        else if ((fileAttr & FileAttributes.Archive) != 0)
        {
            attribute.Type = (uint)NF3S.NF3REG;
        }
        else if ((fileAttr & FileAttributes.Normal) != 0)
        {
            attribute.Type = (uint)NF3S.NF3REG;
        }
        else
        {
            attribute.Type = 0;
        }

        if ((fileAttr & FileAttributes.ReparsePoint) != 0)
            attribute.Type = (uint)NF3S.NF3LNK;

        attribute.Mode = 0;
        // Set execution right for all
        attribute.Mode |= 0x49;
        // Set read right for all
        attribute.Mode |= 0x124;

        //if ((lpFileInformation.dwFileAttributes ref  FILE_ATTRIBUTE_READONLY) == 0) {
        if ((fileAttr & FileAttributes.ReadOnly) != 0)
            attribute.Mode |= 0x92;
        //}
        var fi = new FileInfo(path);
        attribute.NLink = fi.LinkTarget != null ? 1u : 0u;
        attribute.Uid = uid;
        attribute.Gid = gid;
        attribute.Size = (ulong)fi.Length;
        attribute.Used = attribute.Size;
        attribute.Rdev.SpecData1 = 0;
        attribute.Rdev.SpecData2 = 0;
        attribute.Fsid = 7; //NTFS //4; 
        attribute.FileId = table.GetIDByPath(path);
        attribute.ATime.Seconds = (uint)fi.LastAccessTime.Second;// FileTimeToPOSIX(lpFileInformation.ftLastAccessTime);
        attribute.ATime.NSeconds = 0;
        attribute.MTime.Seconds = (uint)fi.LastWriteTime.Second;// FileTimeToPOSIX(lpFileInformation.ftLastWriteTime);
        attribute.MTime.NSeconds = 0;
        // This seems to be the changed time, not creation time
        attribute.CTime.Seconds = (uint)fi.CreationTime.Second;// FileTimeToPOSIX(lpFileInformation.ftLastWriteTime);
        attribute.CTime.NSeconds = 0;

        return true;
    }
    public static uint FileTimeToPOSIX(FILETIME ft)
    {
        // takes the last modified date
        long date = ft.HighDateTime << 32 | ft.LowDateTime;
        // 100-nanoseconds = milliseconds * 10000
        long adjust = 11644473600000 * 10000;

        // removes the diff between 1970 and 1601
        date -= adjust;

        // converts back from 100-nanoseconds to seconds
        return (uint)(date / 10000000);
    }
}
