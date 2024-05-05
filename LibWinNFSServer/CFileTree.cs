namespace LibWinNFSServer;

public class CFileTree
{
    private Tree<FILE_ITEM> filesTree;
    private TreeNode<FILE_ITEM> topNode;

    public static bool debug = false;
    public FILE_ITEM AddItem(string absolutePath, byte[] handle)
    {
        FILE_ITEM item = new();
        item.handle = handle;
        item.bCached = false;

        // If the tree is empty just add the new path as node on the top level.
        if (filesTree.IsEmpty)
        {
            item.path = absolutePath;

            item.nPathLen = absolutePath.Length;

            filesTree.set_head(item);
            topNode = filesTree.begin();
        }
        else
        {
            // Check if the requested path belongs to an already registered parent node.
            string sPath(absolutePath);
            tree_node_<FILE_ITEM>* parentNode = findParentNodeFromRootForPath(absolutePath);
            string splittedPath = _basename_acp(sPath);
            //printf("spl %s %s\n", splittedPath.c_str(), absolutePath);
            item.path = new char[splittedPath.length() + 1];
            strcpy_s(item.path, (splittedPath.length() + 1), splittedPath.c_str());
            // If a parent was found use th parent.
            if (parentNode)
            {
                //printf("parent %s\n", parentNode->data.path);
                filesTree.append_child(tree < FILE_ITEM >::iterator_base(parentNode), item);
            }
            else
            {
                // Node wasn't found - most likely a new root - add it to the top level.
                //printf("No parent node found for %s. Adding new sibbling.", absolutePath);
                item.path = new char[strlen(absolutePath) + 1];
                strcpy_s(item.path, (strlen(absolutePath) + 1), absolutePath);
                item.nPathLen = (unsigned int)strlen(item.path);

                filesTree.insert(tree < FILE_ITEM >::iterator_base(topNode), item);
                topNode = filesTree.begin();
            }
        }

        DisplayTree(topNode.node, 0);

        return item;
    }
    public void RemoveItem(string absolutePath)
    {
        TreeNode<FILE_ITEM> node = findNodeFromRootWithPath(absolutePath);
        if (node != null)
        {
            filesTree.erase(tree < FILE_ITEM >iterator(node));
        }
        else
        {
            //printf("Do not find node for path : %s\n", absolutePath);
        }

        DisplayTree(topNode, 0);

    }
    public void RenameItem(string absolutePathFrom, string absolutePathTo)
    {
        TreeNode<FILE_ITEM> node = findNodeFromRootWithPath(absolutePathFrom);
        TreeNode<FILE_ITEM> parentNode = findParentNodeFromRootForPath(absolutePathTo);

        if (parentNode != null && node != null)
        {
            if (filesTree.number_of_children(parentNode) < 1)
            {
                FILE_ITEM emptyItem = new();
                emptyItem.nPathLen = 0;
                emptyItem.path = "";
                filesTree.append_child((parentNode), emptyItem);
            }
            tree<FILE_ITEM>::iterator firstChild = filesTree.begin(parentNode);
            filesTree.move_after(firstChild, tree < FILE_ITEM >::iterator(node));

            string sPath(absolutePathTo);
            string splittedPath = sPath.substr(sPath.find_last_of('\\') + 1);
            node->data.path = new char[splittedPath.length() + 1];
            strcpy_s(node->data.path, (splittedPath.length() + 1), splittedPath.c_str());

        }
        DisplayTree(topNode.node, 0);
    }

    public TreeNode<FILE_ITEM> FindFileItemForPath(string absolutePath)
    {
        TreeNode<FILE_ITEM> node = findNodeFromRootWithPath(absolutePath);

        return node;
    }

    public void GetNodeFullPath(TreeNode<FILE_ITEM> node, ref string path)
    {
        path.append(node->data.path);
        tree_node_<FILE_ITEM> parentNode = node->parent;
        while (parentNode != NULL)
        {
            path.insert(0, "\\");
            path.insert(0, parentNode->data.path);
            parentNode = parentNode->parent;
        }
    }

    protected TreeNode<FILE_ITEM> findNodeFromRootWithPath(string path)
    {
        // No topNode - bail out.
        if (topNode.node == NULL)
        {
            return NULL;
        }
        string sPath(path);
        string nPath(topNode->path);
        // topNode path and requested path are the same? Use the node.
        if (sPath == nPath)
        {
            return topNode.node;
        }
        // printf("Did not find node for path : %s\n", path);

        // If the topNode path is part of the requested path this is a subpath.
        // Use the node.
        if (sPath.find(nPath) != stdnpos) {
            // printf("Found %s is part of %s  \n", sPath.c_str(), topNode->path);
            string splittedString = sPath.substr(strlen(topNode->path) + 1);
            return findNodeWithPathFromNode(splittedString.c_str(), topNode.node);
        }

    else
        {
            // If the current topNode isn't related to the requested path
            // iterate over all _top_ level elements in the tree to look for
            // a matching item and register it as current top node.

            // printf("NOT found %s is NOT part of %s  \n", sPath.c_str(), topNode->path);
            tree<FILE_ITEM>::sibling_iterator it;
            for (it = filesTree.begin(); it != filesTree.end(); it++)
            {
                string itPath(it.node->data.path);
                // Current item path matches the requested path - use the item as topNode.
                if (sPath == itPath)
                {
                    // printf("Found parent node %s \n", it.node->data.path);
                    topNode = it;
                    return it.node;
                }
                else if (sPath.find(itPath) != stringnpos)
                {
                    // If the item path is part of the requested path this is a subpath.
                    // Use the the item as topNode and continue analyzing.
                    // printf("Found root node %s \n", it.node->data.path);
                    topNode = it;
                    string splittedString = sPath.substr(itPath.length() + 1);
                    return findNodeWithPathFromNode(splittedString.c_str(), it.node);
                }
            }
        }
        // Nothing found return NULL.
        return null;
    }
    protected TreeNode<FILE_ITEM> findNodeWithPathFromNode(string path, TreeNode<FILE_ITEM> node)
    {
        tree<FILE_ITEM>::sibling_iterator sib = filesTree.begin(node);
        tree<FILE_ITEM>::sibling_iterator end = filesTree.end(node);
        bool currentLevel = true;

        string currentPath = _first_dirname(path);

        size_t position = currentPath.size();
        string followingPath = _following_path(path);
        currentLevel = followingPath.empty();

        while (sib != end)
        {
            // printf("sib->path '%s' lv %d curpath '%s' follow '%s'\n", sib->path, currentLevel, currentPath.c_str(), followingPath.c_str());
            if (strcmp(sib->path, currentPath) == 0)
            {
                if (currentLevel)
                {
                    return sib.node;
                }
                else
                {
                    return findNodeWithPathFromNode(followingPath, sib.node);
                }
            }
            ++sib;
        }
        return NULL;
    }
    protected TreeNode<FILE_ITEM> findParentNodeFromRootForPath(string path)
    {
        string sPath(path);
        string nPath(topNode->path);

        // If the topNode path is not part of the requested path bail out.
        // This avoids also issues with taking substrings of incompatible
        // paths below.
        if (sPath.find(nPath) == stdstringnpos) {
            // printf("Path %s doesn't belong to current topNode %s Found %s is part of %s  \n", sPath.c_str(), topNode->path);
            return NULL;
        }
        string currentPath = sPath.substr(strlen(topNode->path) + 1);
        string followingPath = _dirname_acp(currentPath);
        if (followingPath.empty())
        {
            return topNode.node;
        }
        else
        {
            return findNodeWithPathFromNode(followingPath, topNode.node);
        }
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
}

