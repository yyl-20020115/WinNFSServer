namespace LibWinNFSServer;
public class TreeNode<T>(T? data = null) where T:class
{
    public TreeNode<T>? Parent;
    public TreeNode<T>? FirstChild;
    public TreeNode<T>? LastChild;
    public TreeNode<T>? PreviousSibling;
    public TreeNode<T>? NextSibling;
	public T? Data = data;
}
