namespace LibWinNFSServer;

public class CFileTree
{
    private Tree<FILE_ITEM> filesTree = new();
    private Tree<FILE_ITEM>.PreOrderIterator? topNode =null;

    public static bool debug = false;
    public FILE_ITEM AddItem(string absolutePath, byte[] handle)
    {
        FILE_ITEM item = new()
        {
            handle = handle,
            bCached = false
        };

        // If the tree is empty just add the new path as node on the top level.
        if (filesTree.IsEmpty)
        {
            item.path = absolutePath;
            item.nPathLen = absolutePath.Length;

            filesTree.SetHead(item);
            topNode = filesTree.Begin();
        }
        else
        {
            // Check if the requested path belongs to an already registered parent node.
            string sPath = absolutePath;
            TreeNode<FILE_ITEM> parentNode = findParentNodeFromRootForPath(absolutePath);
            string splittedPath = Path.GetFileName(sPath);
            //printf("spl %s %s\n", splittedPath.c_str(), absolutePath);
            item.path = splittedPath;
            // If a parent was found use th parent.
            if (parentNode!=null)
            {
                //printf("parent %s\n", parentNode->data.path);
                filesTree.AppendChild(new Tree<FILE_ITEM>.IteratorBase(parentNode), item);
            }
            else
            {
                // Node wasn't found - most likely a new root - add it to the top level.
                //printf("No parent node found for %s. Adding new sibbling.", absolutePath);
                item.path = absolutePath;
                item.nPathLen = absolutePath.Length;

                filesTree.Insert(new Tree<FILE_ITEM>.PreOrderIterator(topNode.Node), item);
                topNode = filesTree.Begin();
            }
        }

        DisplayTree(topNode.Node, 0);

        return item;
    }
    public void RemoveItem(string absolutePath)
    {
        TreeNode<FILE_ITEM> node = findNodeFromRootWithPath(absolutePath);
        if (node != null)
        {
            filesTree.Erase(new Tree< FILE_ITEM >.IteratorBase(node));
        }
        else
        {
            //printf("Do not find node for path : %s\n", absolutePath);
        }

        DisplayTree(topNode.Node, 0);

    }
    public void RenameItem(string absolutePathFrom, string absolutePathTo)
    {
        TreeNode<FILE_ITEM> node = findNodeFromRootWithPath(absolutePathFrom);
        TreeNode<FILE_ITEM> parentNode = findParentNodeFromRootForPath(absolutePathTo);

        if (parentNode != null && node != null)
        {
            if (parentNode.CountOfChildren < 1)
            {
                FILE_ITEM emptyItem = new();
                emptyItem.nPathLen = 0;
                emptyItem.path = "";
                filesTree.AppendChild(new Tree<FILE_ITEM>.IteratorBase(parentNode), emptyItem);
            }
            Tree<FILE_ITEM>.SiblingIterator firstChild = filesTree.Begin(new Tree<FILE_ITEM>.IteratorBase(parentNode));
            filesTree.MoveAfter(firstChild,new Tree < FILE_ITEM >.IteratorBase(node));

            string sPath=(absolutePathTo);
            string splittedPath = Path.GetFileName( sPath);
            node.Data.path = splittedPath;
        }
        DisplayTree(topNode.Node, 0);
    }

    public TreeNode<FILE_ITEM> FindFileItemForPath(string absolutePath)
    {
        TreeNode<FILE_ITEM> node = findNodeFromRootWithPath(absolutePath);

        return node;
    }

    public void GetNodeFullPath(TreeNode<FILE_ITEM> node, ref string path)
    {
        path += node.Data.path;
        var parentNode = node.Parent;
        while (parentNode != null)
        {
            path = parentNode.Data.path + "\\" + path;
            parentNode = parentNode.Parent;
        }
    }

    protected TreeNode<FILE_ITEM> findNodeFromRootWithPath(string path)
    {
        // No topNode - bail out.
        if (topNode.Node == null)
        {
            return null;
        }
        string sPath=(path);
        string nPath=(topNode.Node.Data.path);
        // topNode path and requested path are the same? Use the node.
        if (sPath == nPath)
        {
            return topNode.Node;
        }
        // printf("Did not find node for path : %s\n", path);

        // If the topNode path is part of the requested path this is a subpath.
        // Use the node.
        if (sPath.Contains(nPath)) {
            // printf("Found %s is part of %s  \n", sPath.c_str(), topNode->path);
            string splittedString = sPath[(topNode.Node.Data.path.Length + 1) ..];
            return findNodeWithPathFromNode(splittedString, topNode.Node);
        }

    else
        {
            // If the current topNode isn't related to the requested path
            // iterate over all _top_ level elements in the tree to look for
            // a matching item and register it as current top node.

            // printf("NOT found %s is NOT part of %s  \n", sPath.c_str(), topNode->path);
            Tree<FILE_ITEM>.PreOrderIterator it;
            for (it = filesTree.Begin(); it != filesTree.End(); )
            {
                string itPath = (it.Node.Data.path);
                // Current item path matches the requested path - use the item as topNode.
                if (sPath == itPath)
                {
                    // printf("Found parent node %s \n", it.node->data.path);
                    topNode = it;
                    return it.Node;
                }
                else if (sPath.Contains(itPath))
                {
                    // If the item path is part of the requested path this is a subpath.
                    // Use the the item as topNode and continue analyzing.
                    // printf("Found root node %s \n", it.node->data.path);
                    topNode = it;
                    string splittedString = sPath[(itPath.Length + 1)..];
                    return findNodeWithPathFromNode(splittedString, it.Node);
                }
                it.Next();
            }
        }
        // Nothing found return NULL.
        return null;
    }
    protected TreeNode<FILE_ITEM> findNodeWithPathFromNode(string path, TreeNode<FILE_ITEM> node)
    {
        Tree<FILE_ITEM>.SiblingIterator sib = filesTree.Begin(new Tree<FILE_ITEM>.IteratorBase(node));
        Tree<FILE_ITEM>.SiblingIterator end = filesTree.End(new Tree<FILE_ITEM>.IteratorBase(node));
        bool currentLevel = true;

        string currentPath = Path.GetDirectoryName(path);

        var position = currentPath.Length;
        string followingPath = path.IndexOf('\\') is int p && p>=0 ? path[(p+1)..]:"";
        currentLevel = followingPath.Length==0;

        while (sib != end)
        {
            // printf("sib->path '%s' lv %d curpath '%s' follow '%s'\n", sib->path, currentLevel, currentPath.c_str(), followingPath.c_str());
            if (String.Compare(sib.Node.Data.path, currentPath) == 0)
            {
                if (currentLevel)
                {
                    return sib.Node;
                }
                else
                {
                    return findNodeWithPathFromNode(followingPath, sib.Node);
                }
            }
            sib.Next();
        }
        return null;
    }
    protected TreeNode<FILE_ITEM> findParentNodeFromRootForPath(string path)
    {
        string sPath=(path);
        string nPath=topNode.Node.Data.path;

        // If the topNode path is not part of the requested path bail out.
        // This avoids also issues with taking substrings of incompatible
        // paths below.
        if (!sPath.Contains(nPath)) {
            // printf("Path %s doesn't belong to current topNode %s Found %s is part of %s  \n", sPath.c_str(), topNode->path);
            return null;
        }
        string currentPath = sPath[(topNode.Node.Data.path.Length + 1)..];//.substr(strlen(topNode->path) + 1);
        string followingPath = Path.GetDirectoryName(currentPath);
        if (string.IsNullOrEmpty( followingPath))
        {
            return topNode.Node;
        }
        else
        {
            return findNodeWithPathFromNode(followingPath, topNode.Node);
        }
    }
    protected void DisplayTree(TreeNode<FILE_ITEM> node, int level)
    {
        if (debug)
        {
            Console.WriteLine("\n\n\n<<<<<<<<<<<<<<<<<<<<<DISPLAY tree \n\n\n");
            var sib2 = node.FirstChild;
            var end2 = node.LastChild;
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
}

