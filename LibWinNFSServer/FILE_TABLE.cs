namespace LibWinNFSServer;

public class FILE_TABLE
{
    public const int TABLE_SIZE = 1024;
    public TreeNode<FILE_ITEM>[] pItems = new TreeNode<FILE_ITEM>[TABLE_SIZE];
    public FILE_TABLE? pNext = null;
}
