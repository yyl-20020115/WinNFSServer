namespace LibWinNFSServer;

public class CFileTable
{
    public const int FHSIZE = 32;
    public const int NFS3_FHSIZE = 64;

    private readonly CFileTree tree = new();

    private readonly FILE_TABLE first;
    private FILE_TABLE last;
    private uint size = 0;
    public CFileTable()
    {
        this.first
            = this.last
            = new();
        this.size = 0;
    }
    public uint GetIDByPath(string path)
    {
        var handle = GetHandleByPath(path);
        return handle switch
        {
            null => 0,
            _ => BitConverter.ToUInt32(handle, 0)
        };
    }
    public byte[]? GetHandleByPath(string path)
    {
        var node = tree.FindFileItemForPath(path);
        if (node == null)
        {
            //printf("Add file for path %s\n", path);
            AddItem(path);
            node = tree.FindFileItemForPath(path);
            if (node == null || node?.Data?.Handle == null)
            {
                //printf("Missing handle for path %s\n", path);
            }
        }
        return node?.Data?.Handle;
    }
    public bool GetPathByHandle(byte[]? handle, ref string path)
    {
        var id = BitConverter.ToUInt32(handle);
        var node = GetItemByID(id);
        if (node != null)
        {
            CFileTree.GetNodeFullPath(node, ref path);
            return true;
        }
        return false;
    }
    public bool RemoveItem(string path)
    {
        var foundDeletedItem = tree.FindFileItemForPath(path);
        if (foundDeletedItem != null && foundDeletedItem.Data is not null)
        {
            // Remove from table
            uint handle = BitConverter.ToUInt32(foundDeletedItem.Data.Handle);

            if (handle >= size)
            {
                //printf("File handle not found to remove : %s", path);
                return false;
            }
            else
            {
                uint i;
                var pTable = first;
                for (i = FILE_TABLE.TABLE_SIZE; i <= handle; i += FILE_TABLE.TABLE_SIZE)
                {
                    pTable = pTable?.pNext;
                }

                pTable.pItems[handle + FILE_TABLE.TABLE_SIZE - i] = null;
            }
            // Remove from table end

            var rpath = "";
            CFileTree.GetNodeFullPath(foundDeletedItem, ref rpath);
            tree.RemoveItem(rpath);
            return true;
        }
        else
        {
            //printf("File not found to remove : %s", path);
        }
        return false;
    }
    public void RenameFile(string pathFrom, string pathTo)
    {
        tree.RenameItem(pathFrom, pathTo);
    }

    protected TreeNode<FILE_ITEM>? AddItem(string path)
    {
        FILE_ITEM item = new()
        {
            Path = path,
            PathLength = path.Length,
            Handle = new byte[NFS3_FHSIZE]
        };
        var ts = BitConverter.GetBytes(size);
        Array.Copy(ts, item.Handle, ts.Length);
        //let its handle equal the index
        item.IsCached = false;  //not in the cache

        if (size > 0 && (size & (FILE_TABLE.TABLE_SIZE - 1)) == 0)
        {
            last.pNext = new ();
            last = last.pNext;
            last.pNext = null;
            Array.Clear(last.pItems);
        }

        //printf("\nAdd file %s for handle %i\n", path, (unsigned int )item.handle);
        tree.AddItem(path, item.Handle);
        var node = tree.FindFileItemForPath(path);
        if (node == null)
        {
            //printf("Can't find node just added %s\n", path);
        }

        last.pItems[size & (FILE_TABLE.TABLE_SIZE - 1)] = node;  //add the new item in the file table
        ++size;

        return node;  //return the pointer to the new item
    }

    private TreeNode<FILE_ITEM>? GetItemByID(uint nID)
    {
        if (nID >= size) return null;
        var pTable = first;
        uint i;
        for (i = FILE_TABLE.TABLE_SIZE; i <= nID; i += FILE_TABLE.TABLE_SIZE)
            pTable = pTable?.pNext;

        return pTable?.pItems[nID + FILE_TABLE.TABLE_SIZE - i];
    }
    public int SRenameFile(string pathFrom, string pathTo)
    {
        int e = -1;
        try
        {
            File.Move(pathFrom, pathTo);
            e = WinAPIs.GetLastError();
            this.RenameFile(pathFrom, pathTo);
            return e;
        }
        catch
        {
            return e;
        }
    }

    public int RenameDirectory(string pathFrom, string pathTo)
    {
        var e = SRenameFile(pathFrom, pathTo);
        var dotFile = "\\.";
        var dotDirectoryPathFrom = pathFrom + dotFile;
        var dotDirectoryPathTo = pathTo + dotFile;
        var backDirectoryPathFrom = pathFrom + dotFile;
        var backDirectoryPathTo = pathTo + dotFile;
        if (e == 0)
        {
            this.RenameFile(dotDirectoryPathFrom, dotDirectoryPathTo);
            this.RenameFile(backDirectoryPathFrom, backDirectoryPathTo);
        }
        return e;
    }

    public int RemoveFile(string path)
    {
        try
        {
            File.Delete(path);
            int e = WinAPIs.GetLastError();
            this.RemoveItem(path);
            return e;
        }
        catch
        {
            return WinAPIs.GetLastError();
        }
    }

    public int RemoveFolder(string path)
    {
        try
        {
            Directory.Delete(path, true);
            int e = WinAPIs.GetLastError();

            var dotFile = "\\.";
            var backFile = "\\..";

            var dotDirectoryPath = path + dotFile;
            var backDirectoryPath = path + backFile;

            this.RemoveItem(dotDirectoryPath);
            this.RemoveItem(backDirectoryPath);
            this.RemoveItem(path);

            return e;
        }
        catch
        {
            return WinAPIs.GetLastError();
        }
    }
}
