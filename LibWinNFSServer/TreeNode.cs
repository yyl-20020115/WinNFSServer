using System.Runtime.CompilerServices;
using System.Xml.Linq;
using static LibWinNFSServer.Tree<T>;

namespace LibWinNFSServer;
public class TreeNode<T>(T? data = null) where T : class
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
            while (tn != null && tn != this.LastChild)
            {
                count++;
                tn = tn?.NextSibling;
            }
            return count;
        }
    }
}

public class Tree<T> where T : class
{
    public TreeNode<T> Head = new();
    public TreeNode<T> Tail = new();
    public bool IsEmpty
    {
        get
        {
            PreOrderIterator it = Begin(), eit = End();
            return (it.Node == eit.Node);
        }
    }

    public Tree()
    {
        this.Init();
    }

    public PreOrderIterator SetHead(T data) => Insert(new PreOrderIterator(Tail), data);
    public PreOrderIterator Begin() => new PreOrderIterator(Head?.NextSibling);
    public PreOrderIterator End() => new PreOrderIterator(Tail);

    public PreOrderIterator Insert(PreOrderIterator iterator, T data)
    {
        if (iterator.Node == null)
        {
            iterator.Node = Tail; // Backward compatibility: when calling insert on a null node,
                                  // insert before the feet.
        }
        TreeNode<T> new_node = new(data);
        //	kp::constructor(&tmp.data, x);
        new_node.FirstChild = null;
        new_node.LastChild = null;

        new_node.Parent = iterator.Node.Parent;
        new_node.NextSibling = iterator.Node;
        new_node.PreviousSibling = iterator.Node.PreviousSibling;
        iterator.Node.PreviousSibling = new_node;

        if (new_node.PreviousSibling == null)
        {
            if (new_node.Parent != null) // when inserting nodes at the head, there is no parent
                new_node.Parent.FirstChild = new_node;
        }
        else
            new_node.PreviousSibling.NextSibling = new_node;
        return new PreOrderIterator(new_node);
    }

    public TreeNode<T> AppendChild(IteratorBase iterator, T data)
    {
        TreeNode<T> node = new()
        {
            FirstChild = null,
            LastChild = null,

            Parent = iterator.Node
        };
        if (iterator?.Node?.LastChild != null)
        {
            iterator.Node.LastChild.NextSibling = node;
        }
        else
        {
            iterator.Node.FirstChild = node;
        }
        node.PreviousSibling = iterator.Node.LastChild;
        iterator.Node.LastChild = node;
        node.NextSibling = null;
        return node;
    }

    public IteratorBase MoveAfter(IteratorBase target, IteratorBase source)
    {
        var dst = target.Node;
        var src = source.Node;

        if (dst == src) return source;
        if (dst.NextSibling!=null)
            if (dst.NextSibling == src) // already in the right spot
                return source;

        // take src out of the tree
        if (src.PreviousSibling != null) src.PreviousSibling.NextSibling = src.NextSibling;
        else src.Parent.FirstChild = src.NextSibling;
        if (src.NextSibling != null) src.NextSibling.PreviousSibling = src.PreviousSibling;
        else src.Parent.LastChild = src.PreviousSibling;

        // connect it to the new point
        if (dst.NextSibling != null) dst.NextSibling.PreviousSibling = src;
        else dst.Parent.LastChild = src;
        src.NextSibling = dst.NextSibling;
        dst.NextSibling = src;
        src.PreviousSibling = dst;
        src.Parent = dst.Parent;
        return new IteratorBase(src);
    }
    public SiblingIterator Begin(IteratorBase iterator)
    {
        if (iterator.Node.FirstChild == null)
        {
            return End(iterator);
        }
        return new SiblingIterator(iterator.Node.FirstChild);
    }

    public SiblingIterator End(IteratorBase iterator)
    {
        SiblingIterator ret = new()
        {
            Parent = iterator.Node
        };
        return ret;
    }

    public void Clear()
    {
        if (this.Head != null)
            while (Head.NextSibling != Tail)
                Erase(new PreOrderIterator(Head.NextSibling));

    }
    public IteratorBase Erase(IteratorBase it)
    {
        TreeNode<T> cur = it.Node;
        IteratorBase ret = it;
        ret.SkipChildren();
        ret.Next();
        //EraseChildren(it);
        if (cur.PreviousSibling == null)
        {
            cur.Parent.FirstChild = cur.NextSibling;
        }
        else
        {
            cur.PreviousSibling.NextSibling = cur.NextSibling;
        }
        if (cur.NextSibling == null)
        {
            cur.Parent.LastChild = cur.PreviousSibling;
        }
        else
        {
            cur.NextSibling.PreviousSibling = cur.PreviousSibling;
        }

        return ret;
    }
    public void EraseChildren(IteratorBase it)
    {
        if (it.Node == null) return;

        var cur = it.Node.FirstChild;
        TreeNode<T>? prev = null;

        while (cur != null)
        {
            prev = cur;
            cur = cur.NextSibling;
            EraseChildren(new(prev));
        }
        it.Node.FirstChild = null;
        it.Node.LastChild = null;
        //	std::cout << "exit" << std::endl;
    }

    protected void Init()
    {

        Head.Parent = null;
        Head.FirstChild = null;
        Head.LastChild = null;
        Head.PreviousSibling = null;
        Head.NextSibling = Tail;

        Tail.Parent = null;
        Tail.FirstChild = null;
        Tail.LastChild = null;
        Tail.PreviousSibling = Head;
        Tail.NextSibling = null;
    }

    public class IteratorBase
    {
        protected bool Skip = false;
        public TreeNode<T>? Node = null;
        public IteratorBase(TreeNode<T>? Node = null) => this.Node = Node;

        public void SkipChildren(bool skip = true) => this.Skip = skip;
        public int GetChildrenCount()
        {
            var pos = Node?.FirstChild;
            if (pos == null) return 0;

            int ret = 1;
            while (pos != null && pos != Node?.LastChild)
            {
                ++ret;
                pos = pos?.NextSibling;
            }
            return ret;
        }
        public SiblingIterator Begin() => this.Node?.FirstChild == null
                ? this.End()
                : new SiblingIterator(this.Node?.FirstChild)
                {
                    Parent = this.Node
                };
        public SiblingIterator End() => new(null)
        {
            Parent = this.Node
        };
        public IteratorBase Next() => this;
        public IteratorBase Previous() => this;
    }
    public class PreOrderIterator : IteratorBase
    {
        public PreOrderIterator(TreeNode<T>? Node = null) : base(Node)
        {

        }
        public PreOrderIterator Next()
        {
            if (!this.Skip && this.Node?.FirstChild != null)
            {
                this.Node = this.Node.FirstChild;
            }
            else
            {
                this.Skip = false;
                while (this.Node?.NextSibling == null)
                {
                    this.Node = this.Node?.Parent;
                    if (this.Node == null)
                        return this;
                }
                this.Node = this.Node.NextSibling;
            }
            return this;

        }
        public PreOrderIterator Previous()
        {
            if (this.Node?.PreviousSibling != null)
            {
                this.Node = this.Node?.PreviousSibling;
                while (this.Node?.LastChild != null)
                    this.Node = this.Node.LastChild;
            }
            else
            {
                this.Node = this.Node?.Parent;
                if (this.Node == null)
                    return this;
            }
            return this;
        }
    }
    public class PostOrderIterator : IteratorBase
    {
        public PostOrderIterator(TreeNode<T>? Node = null) : base(Node)
        {

        }
        public PostOrderIterator Next()
        {
            if (this.Node?.NextSibling == null)
            {
                this.Node = this.Node?.Parent;
                this.Skip = false;
            }
            else
            {
                this.Node = this.Node?.NextSibling;
                if (this.Skip)
                {
                    this.Skip = false;
                }
                else
                {
                    while (this.Node?.FirstChild != null)
                        this.Node = this.Node.FirstChild;
                }
            }

            return this;
        }
        public PostOrderIterator Previous()
        {
            if (this.Skip || this.Node?.LastChild == null)
            {
                this.Skip = false;
                while (this.Node?.PreviousSibling == null)
                    this.Node = this.Node?.Parent;
                this.Node = this.Node.PreviousSibling;
            }
            else
            {
                this.Node = this.Node.LastChild;
            }

            return this;
        }
    }
    public class SiblingIterator : IteratorBase
    {
        public TreeNode<T>? Parent = null;

        public SiblingIterator(TreeNode<T>? node = null) : base(node)
        {
            this.SetParent();
        }
        public TreeNode<T>? RangeFirst => this.Parent?.FirstChild;
        public TreeNode<T>? RangeLast => this.Parent?.LastChild;
        public SiblingIterator Next()
        {
            if (this.Node != null)
                this.Node = this.Node.NextSibling;
            return this;
        }
        public SiblingIterator Previous()
        {
            if (this.Node != null)
                this.Node = this.Node.PreviousSibling;
            else if (this.Parent != null)
                this.Node = this.Parent.LastChild;
            return this;
        }
        protected void SetParent()
        {
            this.Parent = null;
            if (this.Node == null) return;
            if (this.Node.Parent != null)
                this.Parent = this.Node.Parent;
        }
    }
    public class BridthFirstQueueIterator : IteratorBase
    {
        protected Queue<TreeNode<T>> Queue = [];

        public BridthFirstQueueIterator(TreeNode<T>? Node = null)
        {
            if (Node != null)
                this.Queue.Enqueue(Node);
        }

        public BridthFirstQueueIterator Next()
        {
            // Add child nodes and pop current node
            SiblingIterator sib = this.Begin();
            while (sib != this.End())
            {
                Queue.Enqueue(sib.Node);
                sib.Next();
            }
            Queue.Dequeue();

            if (Queue.Count > 0)
                this.Node = Queue.Peek();
            else
                this.Node = null;

            return this;
        }
    }
    public class FixedDepthIterator : IteratorBase
    {
        public TreeNode<T>? TopNode = null;
        public FixedDepthIterator(TreeNode<T>? Node = null) : base(Node)
        {

        }
        public FixedDepthIterator Next()
        {
            if (this.Node?.NextSibling != null)
            {
                this.Node = this.Node.NextSibling;
            }
            else
            {
                int relative_depth = 0;
            upper:
                do
                {
                    if (this.Node == this.TopNode)
                    {
                        this.Node = null; // FIXME: return a proper fixed_depth end iterator once implemented
                        return this;
                    }
                    this.Node = this.Node?.Parent;
                    if (this.Node == null) return this;
                    --relative_depth;
                } while (this.Node.NextSibling == null);
            lower:
                this.Node = this.Node?.NextSibling;
                while (this.Node?.FirstChild == null)
                {
                    if (this.Node?.NextSibling == null)
                        goto upper;
                    this.Node = this.Node?.NextSibling;
                    if (this.Node == null) return this;
                }
                while (relative_depth < 0 && this.Node?.FirstChild != null)
                {
                    this.Node = this.Node.FirstChild;
                    ++relative_depth;
                }
                if (relative_depth < 0)
                {
                    if (this.Node?.NextSibling == null) goto upper;
                    else goto lower;
                }
            }

            return this;
        }

        public FixedDepthIterator Previous()
        {
            if (this.Node?.PreviousSibling != null)
            {
                this.Node = this.Node.PreviousSibling;
            }
            else
            {
                int relative_depth = 0;
            upper:
                do
                {
                    if (this.Node == this.TopNode)
                    {
                        this.Node = null;
                        return this;
                    }
                    this.Node = this.Node?.Parent;
                    if (this.Node == null) return this;
                    --relative_depth;
                } while (this.Node.PreviousSibling == null);
            lower:
                this.Node = this.Node?.PreviousSibling;
                while (this.Node?.LastChild == null)
                {
                    if (this.Node?.PreviousSibling == null)
                        goto upper;
                    this.Node = this.Node.PreviousSibling;
                    if (this.Node == null) return this;
                }
                while (relative_depth < 0 && this.Node.LastChild != null)
                {
                    this.Node = this.Node?.LastChild;
                    ++relative_depth;
                }
                if (relative_depth < 0)
                {
                    if (this.Node?.PreviousSibling == null) goto upper;
                    else goto lower;
                }
            }
            return this;

        }
    }
    public class LeafIterator : IteratorBase
    {
        public TreeNode<T>? TopNode = null;
        public LeafIterator(TreeNode<T>? Node = null) : base(Node)
        {

        }
        public LeafIterator Next()
        {
            if (this.Node?.FirstChild != null)
            { // current node is no longer leaf (children got added)
                while (this.Node?.FirstChild != null)
                    this.Node = this.Node?.FirstChild;
            }
            else
            {
                while (this.Node?.NextSibling == null)
                {
                    if (this.Node?.Parent == null) return this;
                    this.Node = this.Node.Parent;
                    if (TopNode != null && this.Node == TopNode) return this;
                }
                this.Node = this.Node?.NextSibling;
                while (this.Node?.FirstChild != null)
                    this.Node = this.Node?.FirstChild;
            }

            return this;
        }
        public LeafIterator Previous()
        {
            while (this.Node?.PreviousSibling == null)
            {
                if (this.Node?.Parent == null) return this;
                this.Node = this.Node?.Parent;
                if (TopNode != null && this.Node == TopNode) return this;
            }
            this.Node = this.Node.PreviousSibling;
            while (this.Node.LastChild != null)
                this.Node = this.Node.LastChild;

            return this;
        }
    }
}