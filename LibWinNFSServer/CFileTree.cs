namespace LibWinNFSServer;

public class CFileTree
{
    private TreeNode<FILE_ITEM> filesTree;
    private TreeNode<FILE_ITEM> topNode;

    public static bool debug = false;
    public FILE_ITEM AddItem(string absolutePath, byte[] handle)
    {

    }
    public void RemoveItem(string absolutePath)
    {

    }
    public void RenameItem(string absolutePathFrom, string absolutePathTo)
    {

    }

    public TreeNode<FILE_ITEM> FindFileItemForPath(string absolutePath)
    {

    }

    public string GetNodeFullPath(TreeNode<FILE_ITEM> node)
    {

    }

    protected TreeNode<FILE_ITEM> findNodeFromRootWithPath(string path)
    {

    }
    protected TreeNode<FILE_ITEM> findNodeWithPathFromNode(string path, TreeNode<FILE_ITEM> node)
    {

    }
	protected TreeNode<FILE_ITEM> findParentNodeFromRootForPath(string path)
    {

    }
    protected void DisplayTree(TreeNode<FILE_ITEM> node, int level)
    {
        if (debug)
        {
            Console.WriteLine("\n\n\n<<<<<<<<<<<<<<<<<<<<<DISPLAY tree \n\n\n");
            TreeNode<FILE_ITEM> sib2 = node.FirstChild;
            TreeNode<FILE_ITEM> end2 = node.LastChild;
            while (sib2 != end2)
            {
                for (int i = 0; i < level; i++)
                {
                    Console.Write("  ");
                }
                if (sib2.CountOfChildren > 1)
                {
                    DisplayTree(sib2, (level + 1));
                }
                sib2 = sib2.NextSibling;
            }
            Console.WriteLine("\n\n\n<<<<<<<<<<<<<<<<<<<<<End tree \n\n\n");
        }
    }
};

