namespace LibWinNFSServer;
public class TreeNode<T>(T? data = null) where T:class
{
    public TreeNode<T>? Parent;
    public TreeNode<T>? FirstChild;
    public TreeNode<T>? LastChild;
    public TreeNode<T>? PreviousSibling;
    public TreeNode<T>? NextSibling;
	public T? Data = data;
    
    public int CountOfChildren
    {
        get
        {
            var count = 0;
            var tn = this.FirstChild;
            while (tn!=null && tn != this.LastChild)
            {
                count++;
                tn = tn?.NextSibling;
            }
            return count;
        }
    }
}

public class Tree<T>(TreeNode<T> Root) where T: class
{
    public TreeNode<T> Root = Root;
    public bool IsEmpty => this.Root == null;
}