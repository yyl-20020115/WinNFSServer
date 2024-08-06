namespace LibWinNFSServer;

public class CFileTree
{
    private readonly Tree<FILE_ITEM> filesTree = new();
    private Tree<FILE_ITEM>.PreOrderIterator? topNode = null;

    public static bool Debug = false;
    public FILE_ITEM AddItem(string absolutePath, byte[] handle)
    {
        FILE_ITEM item = new()
        {
            Handle = handle,
            IsCached = false
        };

        // If the tree is empty just add the new path as node on the top level.
        if (filesTree.IsEmpty)
        {
            item.Path = absolutePath;
            item.PathLength = absolutePath.Length;

            filesTree?.SetHead(item);
            topNode = filesTree?.Begin();
        }
        else
        {
            // Check if the requested path belongs to an already registered parent node.
            var sPath = absolutePath;
            var parentNode = FindParentNodeFromRootForPath(absolutePath);
            var splittedPath = Path.GetFileName(sPath);
            //printf("spl %s %s\n", splittedPath.c_str(), absolutePath);
            item.Path = splittedPath;
            // If a parent was found use th parent.
            if (parentNode != null)
            {
                //printf("parent %s\n", parentNode->data.path);
                filesTree?.AppendChild(new Tree<FILE_ITEM>.IteratorBase(parentNode), item);
            }
            else
            {
                // Node wasn't found - most likely a new root - add it to the top level.
                //printf("No parent node found for %s. Adding new sibbling.", absolutePath);
                item.Path = absolutePath;
                item.PathLength = absolutePath.Length;

                filesTree?.Insert(new Tree<FILE_ITEM>.PreOrderIterator(topNode.Node), item);
                topNode = filesTree?.Begin();
            }
        }

        DisplayTree(topNode?.Node, 0);

        return item;
    }
    public void RemoveItem(string absolutePath)
    {
        var node = FindNodeFromRootWithPath(absolutePath);
        if (node != null)
        {
            filesTree?.Erase(new Tree<FILE_ITEM>.IteratorBase(node));
        }
        else
        {
            //printf("Do not find node for path : %s\n", absolutePath);
        }

        DisplayTree(topNode?.Node, 0);

    }
    public void RenameItem(string absolutePathFrom, string absolutePathTo)
    {
        var node = FindNodeFromRootWithPath(absolutePathFrom);
        var parentNode = FindParentNodeFromRootForPath(absolutePathTo);

        if (parentNode != null && node != null)
        {
            if (parentNode.CountOfChildren < 1)
            {
                FILE_ITEM emptyItem = new()
                {
                    PathLength = 0,
                    Path = ""
                };
                filesTree.AppendChild(new Tree<FILE_ITEM>.IteratorBase(parentNode), emptyItem);
            }
            Tree<FILE_ITEM>.SiblingIterator firstChild = filesTree.Begin(new Tree<FILE_ITEM>.IteratorBase(parentNode));
            filesTree.MoveAfter(firstChild, new Tree<FILE_ITEM>.IteratorBase(node));

            string sPath = (absolutePathTo);
            string splittedPath = Path.GetFileName(sPath);
            node.Data.Path = splittedPath;
        }
        DisplayTree(topNode?.Node, 0);
    }

    public TreeNode<FILE_ITEM>? FindFileItemForPath(string absolutePath)
        => FindNodeFromRootWithPath(absolutePath);

    public static void GetNodeFullPath(TreeNode<FILE_ITEM>? node, ref string path)
    {
        path += node?.Data?.Path;
        var parentNode = node?.Parent;
        while (parentNode != null)
        {
            path = parentNode?.Data?.Path + "\\" + path;
            parentNode = parentNode?.Parent;
        }
    }

    protected TreeNode<FILE_ITEM>? FindNodeFromRootWithPath(string? path)
    {
        // No topNode - bail out.
        if (path==null || topNode?.Node == null) return null;
        var sPath = path;
        var nPath = topNode?.Node?.Data?.Path;
        // topNode path and requested path are the same? Use the node.
        if (sPath == nPath) return topNode?.Node;
        // printf("Did not find node for path : %s\n", path);

        // If the topNode path is part of the requested path this is a subpath.
        // Use the node.
        if (sPath.Contains(nPath))
        {
            // printf("Found %s is part of %s  \n", sPath.c_str(), topNode->path);
            var splittedString = sPath[(topNode.Node.Data.Path.Length + 1)..];
            return FindNodeWithPathFromNode(splittedString, topNode.Node);
        }

        else
        {
            // If the current topNode isn't related to the requested path
            // iterate over all _top_ level elements in the tree to look for
            // a matching item and register it as current top node.

            // printf("NOT found %s is NOT part of %s  \n", sPath.c_str(), topNode->path);
            for (var it = filesTree.Begin(); it != filesTree.End(); it.Next())
            {
                var itPath = it.Node.Data.Path;
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
                    var splittedString = sPath[(itPath.Length + 1)..];
                    return FindNodeWithPathFromNode(splittedString, it.Node);
                }
            }
        }
        // Nothing found return NULL.
        return null;
    }
    protected TreeNode<FILE_ITEM>? FindNodeWithPathFromNode(string path, TreeNode<FILE_ITEM> node)
    {
        var sib = filesTree.Begin(new Tree<FILE_ITEM>.IteratorBase(node));
        var end = filesTree.End(new Tree<FILE_ITEM>.IteratorBase(node));
        var currentLevel = true;
        var currentPath = Path.GetDirectoryName(path);
        var position = currentPath.Length;
        string followingPath = path.IndexOf('\\') is int p && p >= 0 ? path[(p + 1)..] : "";
        currentLevel = followingPath.Length == 0;

        while (sib != end)
        {
            // printf("sib->path '%s' lv %d curpath '%s' follow '%s'\n", sib->path, currentLevel, currentPath.c_str(), followingPath.c_str());
            if (string.Compare(sib.Node.Data.Path, currentPath) == 0)
            {
                if (currentLevel)
                {
                    return sib.Node;
                }
                else
                {
                    return FindNodeWithPathFromNode(followingPath, sib.Node);
                }
            }
            sib.Next();
        }
        return null;
    }
    protected TreeNode<FILE_ITEM> FindParentNodeFromRootForPath(string path)
    {
        var sPath = path;
        var nPath = topNode?.Node?.Data?.Path;
        // If the topNode path is not part of the requested path bail out.
        // This avoids also issues with taking substrings of incompatible
        // paths below.
        if (!sPath.Contains(nPath)) return null;
        // printf("Path %s doesn't belong to current topNode %s Found %s is part of %s  \n", sPath.c_str(), topNode->path);

        var currentPath = sPath[(topNode.Node.Data.Path.Length + 1)..];//.substr(strlen(topNode->path) + 1);
        var followingPath = Path.GetDirectoryName(currentPath);
        return string.IsNullOrEmpty(followingPath) ? topNode.Node : FindNodeWithPathFromNode(followingPath, topNode.Node);
    }
    public static void DisplayTree(TreeNode<FILE_ITEM>? node, int level)
    {
        if (Debug)
        {
            Console.WriteLine("\n\n\n<<<<<<<<<<<<<<<<<<<<<DISPLAY tree \n\n\n");
            var sib = node?.FirstChild;
            var end = node?.LastChild;
            while (sib != end)
            {
                for (int i = 0; i < level; i++)
                {
                    Console.Write("  ");
                }
                if (sib?.CountOfChildren > 1)
                {
                    DisplayTree(sib, (level + 1));
                }
                sib = sib?.NextSibling;
            }
            Console.WriteLine("\n\n\n<<<<<<<<<<<<<<<<<<<<<End tree \n\n\n");
        }
    }
}

