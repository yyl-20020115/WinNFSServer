namespace LibWinNFSServer;

public class CFileTable
{
    public const int FHSIZE = 32;
    public const int NFS3_FHSIZE = 64;

    private readonly CFileTree fileTree = new();

    private FILE_TABLE m_pFirstTable;
    private FILE_TABLE m_pLastTable;
    private uint m_nTableSize = 0;
    public CFileTable()
    {
        this.m_pFirstTable
            = this.m_pLastTable
            = new();
        this.m_nTableSize = 0;
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
        var node = fileTree.FindFileItemForPath(path);
        if (node == null)
        {
            //printf("Add file for path %s\n", path);
            AddItem(path);
            node = fileTree.FindFileItemForPath(path);
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
    public TreeNode<FILE_ITEM>? FindItemByPath(string? path) => null;
    public bool RemoveItem(string path)
    {
        var foundDeletedItem = fileTree.FindFileItemForPath(path);
        if (foundDeletedItem != null)
        {
            // Remove from table
            uint handle = BitConverter.ToUInt32(foundDeletedItem.Data.Handle);

            if (handle >= m_nTableSize)
            {
                //printf("File handle not found to remove : %s", path);
                return false;
            }
            else
            {
                uint i;
                var pTable = m_pFirstTable;
                for (i = FILE_TABLE.TABLE_SIZE; i <= handle; i += FILE_TABLE.TABLE_SIZE)
                {
                    pTable = pTable?.pNext;
                }

                pTable.pItems[handle + FILE_TABLE.TABLE_SIZE - i] = null;
            }
            // Remove from table end

            var rpath = "";
            CFileTree.GetNodeFullPath(foundDeletedItem, ref rpath);
            fileTree.RemoveItem(rpath);
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
        fileTree.RenameItem(pathFrom, pathTo);
    }

    protected TreeNode<FILE_ITEM>? AddItem(string path)
    {
        FILE_ITEM item = new()
        {
            Path = path,
            PathLength = path.Length,
            Handle = new byte[NFS3_FHSIZE]
        };
        var ts = BitConverter.GetBytes(m_nTableSize);
        Array.Copy(ts, item.Handle, ts.Length);
        //let its handle equal the index
        item.IsCached = false;  //not in the cache

        if (m_nTableSize > 0 && (m_nTableSize & (FILE_TABLE.TABLE_SIZE - 1)) == 0)
        {
            m_pLastTable.pNext = new ();
            m_pLastTable = m_pLastTable.pNext;
            m_pLastTable.pNext = null;
            Array.Clear(m_pLastTable.pItems);
        }

        //printf("\nAdd file %s for handle %i\n", path, (unsigned int )item.handle);
        fileTree.AddItem(path, item.Handle);
        var node = fileTree.FindFileItemForPath(path);
        if (node == null)
        {
            //printf("Can't find node just added %s\n", path);
        }

        m_pLastTable.pItems[m_nTableSize & (FILE_TABLE.TABLE_SIZE - 1)] = node;  //add the new item in the file table
        ++m_nTableSize;

        return node;  //return the pointer to the new item
    }

    private TreeNode<FILE_ITEM>? GetItemByID(uint nID)
    {
        if (nID >= m_nTableSize) return null;
        var pTable = m_pFirstTable;
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
        int e;
        try
        {
            File.Delete(path);
            e = WinAPIs.GetLastError();
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
        int e;
        try
        {
            Directory.Delete(path, true);
            e = WinAPIs.GetLastError();

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
