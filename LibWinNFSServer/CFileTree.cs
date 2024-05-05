namespace LibWinNFSServer;

public class CFileTree
{
    public static bool debug = false;
    public FILE_ITEM AddItem(string absolutePath, byte[] handle);
    public void RemoveItem(string absolutePath);
    public void RenameItem(string absolutePathFrom, string absolutePathTo);

    public TreeNode<FILE_ITEM> FindFileItemForPath(string absolutePath);

    public string GetNodeFullPath(TreeNode<FILE_ITEM> node);

    protected TreeNode<FILE_ITEM> findNodeFromRootWithPath(string path);
    protected TreeNode<FILE_ITEM> findNodeWithPathFromNode(string path, TreeNode<FILE_ITEM> node);
	protected TreeNode<FILE_ITEM> findParentNodeFromRootForPath(string path);
    protected void DisplayTree(TreeNode<FILE_ITEM> node, int level);
};

