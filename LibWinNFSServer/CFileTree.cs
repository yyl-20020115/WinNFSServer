using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibWinNFSServer;

public class CFileTree
{
    public
		static bool debug = false;
    FILE_ITEM AddItem(string absolutePath, byte[] handle);
    void RemoveItem(string absolutePath);
    void RenameItem(string absolutePathFrom, string absolutePathTo);

    TreeNode<FILE_ITEM> FindFileItemForPath(string absolutePath);

    string GetNodeFullPath(TreeNode<FILE_ITEM> node);

    protected
		TreeNode<FILE_ITEM> findNodeFromRootWithPath(string path);
    TreeNode<FILE_ITEM> findNodeWithPathFromNode(string path, TreeNode<FILE_ITEM> node);
		TreeNode<FILE_ITEM> findParentNodeFromRootForPath(string path);
    void DisplayTree(TreeNode<FILE_ITEM> node, int level);
};

