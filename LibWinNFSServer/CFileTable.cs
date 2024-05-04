using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;

public class CFileTable
{
    CFileTable();
    public uint GetIDByPath(string path);
    byte[] GetHandleByPath(string path);
    bool GetPathByHandle(byte[] handle, string path);
    TreeNode<FILE_ITEM> FindItemByPath(string path);
    bool RemoveItem(string path);
    void RenameFile(string pathFrom, string pathTo);

    protected
		TreeNode<FILE_ITEM> AddItem(string path);

    private
    FILE_TABLE m_pFirstTable, m_pLastTable;
    uint m_nTableSize;
    CACHE_LIST m_pCacheList;

    TreeNode<FILE_ITEM> GetItemByID(uint nID)
    {

    }
    void PutItemInCache(FILE_ITEM pItem)
    {

    }

}
