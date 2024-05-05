namespace LibWinNFSServer;

public class CFileTable
{
    public const int FHSIZE = 32;
    public const int NFS3_FHSIZE = 64;

    private readonly CFileTree g_FileTree = new();

    private FILE_TABLE m_pFirstTable, m_pLastTable;
    private uint m_nTableSize;
    private CACHE_LIST? m_pCacheList;
    public CFileTable()
    {
        this.m_pFirstTable = this.m_pLastTable = new FILE_TABLE();
        this.m_nTableSize = 0;
        this.m_pCacheList = null;
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
    public byte[] GetHandleByPath(string path)
    {
        TreeNode<FILE_ITEM> node;

        node = g_FileTree.FindFileItemForPath(path);

        if (node == null)
        {
            //printf("Add file for path %s\n", path);
            AddItem(path);
            node = g_FileTree.FindFileItemForPath(path);
            if (node == null || node?.Data?.handle == null)
            {
                //printf("Missing handle for path %s\n", path);
            }
        }

        return node?.Data?.handle;
    }
    public bool GetPathByHandle(byte[] handle, ref string path)
    {
        uint id;
        TreeNode<FILE_ITEM> node;

        id = BitConverter.ToUInt32(handle);

        node = GetItemByID(id);
        if (node != null)
        {
            g_FileTree.GetNodeFullPath(node, ref path);
            return true;
        }
        else
        {
            return false;
        }
    }
    public TreeNode<FILE_ITEM> FindItemByPath(string path)
    {
        return null;
    }
    public bool RemoveItem(string path)
    {
        TreeNode<FILE_ITEM> foundDeletedItem;
        foundDeletedItem = g_FileTree.FindFileItemForPath(path);
        if (foundDeletedItem != null)
        {
            // Remove from table
            FILE_TABLE pTable;
            uint i;
            uint handle =BitConverter.ToUInt32(foundDeletedItem.Data.handle);

            if (handle >= m_nTableSize)
            {
                //printf("File handle not found to remove : %s", path);
                return false;
            }
            else
            {
                pTable = m_pFirstTable;
                for (i = FILE_TABLE.TABLE_SIZE; i <= handle; i += FILE_TABLE.TABLE_SIZE)
                {
                    pTable = pTable.pNext;
                }

                pTable.pItems[handle + FILE_TABLE.TABLE_SIZE - i] = null;
            }
            // Remove from table end

            string rpath = "";
            g_FileTree.GetNodeFullPath(foundDeletedItem, ref rpath);
            g_FileTree.RemoveItem(rpath);
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
        g_FileTree.RenameItem(pathFrom, pathTo);
    }

    protected TreeNode<FILE_ITEM> AddItem(string path)
    {
        TreeNode<FILE_ITEM> node;
        uint nIndex;

        FILE_ITEM item = new()
        {
            path = path,
            nPathLen = path.Length,
            handle = new byte[NFS3_FHSIZE]
        };
        var ts = BitConverter.GetBytes(m_nTableSize);
        Array.Copy(ts, item.handle, ts.Length);
        //let its handle equal the index
        item.bCached = false;  //not in the cache

        if (m_nTableSize > 0 && (m_nTableSize & (FILE_TABLE.TABLE_SIZE - 1)) == 0)
        {
            m_pLastTable.pNext = new FILE_TABLE();
            m_pLastTable = m_pLastTable.pNext;
            m_pLastTable.pNext = null;
            Array.Clear(m_pLastTable.pItems);
        }

        //printf("\nAdd file %s for handle %i\n", path, (unsigned int )item.handle);

        g_FileTree.AddItem(path, item.handle);
        node = g_FileTree.FindFileItemForPath(path);
        if (node == null)
        {
            //printf("Can't find node just added %s\n", path);
        }

        m_pLastTable.pItems[nIndex = m_nTableSize & (FILE_TABLE.TABLE_SIZE - 1)] = node;  //add the new item in the file table
        ++m_nTableSize;

        return node;  //return the pointer to the new item
    }



    private TreeNode<FILE_ITEM> GetItemByID(uint nID)
    {
        FILE_TABLE pTable;
        uint i;

        if (nID >= m_nTableSize)
        {
            return null;
        }

        pTable = m_pFirstTable;

        for (i = FILE_TABLE.TABLE_SIZE; i <= nID; i += FILE_TABLE.TABLE_SIZE)
        {
            pTable = pTable.pNext;
        }

        return pTable.pItems[nID + FILE_TABLE.TABLE_SIZE - i];

    }
    private void PutItemInCache(FILE_ITEM pItem)
    {
        CACHE_LIST pPrev, pCurr;
        int nCount;

        pPrev = null;
        pCurr = m_pCacheList;

        if (pItem.bCached)
        { //item is already in the cache
            while (pCurr != null)
            {
                if (pItem == pCurr.pItem)
                {
                    if (pCurr == m_pCacheList)
                    {  //at the first
                        return;
                    }
                    else
                    {  //move to the first
                        pPrev.pNext = pCurr.pNext;
                        pCurr.pNext = m_pCacheList;
                        m_pCacheList = pCurr;
                        return;
                    }
                }

                pPrev = pCurr;
                pCurr = pCurr.pNext;
            }
        }
        else
        {
            pItem.bCached = true;

            for (nCount = 0; nCount < 9 && pCurr != null; nCount++)
            { //seek to the end of the cache
                pPrev = pCurr;
                pCurr = pCurr.pNext;
            }

            if (nCount == 9 && pCurr != null)
            { //there are 10 items in the cache
                pPrev.pNext = null;  //remove the last
                pCurr.pItem.bCached = false;
            }
            else
            {
                pCurr = new CACHE_LIST();
            }

            pCurr.pItem = pItem;
            pCurr.pNext = m_pCacheList;
            m_pCacheList = pCurr;  //insert to the first
        }
    }
}
