using System.Collections.Generic;

namespace RadixTree
{
    public class Node<T>
    {
        private readonly List<Node<T>> children = new List<Node<T>>();
        private readonly string key;
        private T value;

        private Node(string key, T value)
        {
            this.key = key;
            this.value = value;
        }

        private Node()
        {
        }

        protected bool HasChildren
        {
            get { return children.Count > 0; }
        }

        public void Insert(string key, T value)
        {
            var potentialChild = new Node<T>(key, value);
            Add(potentialChild);
        }

        private bool Add(Node<T> theNewChild)
        {
            if (Contains(theNewChild))
                throw new DuplicateKeyException(string.Format("Duplicate key: '{0}'", theNewChild.key));

            if (!IsParentOf(theNewChild))
                return false;

            bool childrenObligedRequest = RequestChildrenToOwn(theNewChild);
            if (childrenObligedRequest) return true;

            AcceptAsOwnChild(theNewChild);
            return true;
        }

        private bool RequestChildrenToOwn(Node<T> newChild)
        {
            return
                children.Exists(
                    existingChild =>
                    existingChild.MergeIfSameAs(newChild) || existingChild.Add(newChild) ||
                    ForkANewChildAndAddChildren(existingChild, newChild));
        }

        private bool MergeIfSameAs(Node<T> potentialChild)
        {
            if (!IsTheSameAs(potentialChild) || IsNotUnrealNode())
                return false;

            value = potentialChild.value;
            return true;
        }

        private bool IsNotUnrealNode()
        {
            return !IsUnrealNode();
        }

        private void Disown(Node<T> existingChild)
        {
            children.Remove(existingChild);
        }

        private Node<T> AcceptAsOwnChild(Node<T> child)
        {
            if (NotItself(child)) children.Add(child);
            return this;
        }

        private bool NotItself(Node<T> child)
        {
            return !Equals(child);
        }

        private bool ForkANewChildAndAddChildren(Node<T> existingChild, Node<T> newChild)
        {
            if (existingChild.IsNotMySibling(newChild))
                return false;

            var surrogateParent = MakeASurrogateParent(existingChild, newChild);
            if (surrogateParent.IsTheSameAs(this))
                return false;

            SwapChildren(existingChild, newChild, surrogateParent);
            return true;
        }

        private bool IsNotMySibling(Node<T> newChild)
        {
            return !IsMySibling(newChild);
        }

        private void SwapChildren(Node<T> existingChild, Node<T> newChild, Node<T> surrogateParent)
        {
            surrogateParent.AcceptAsOwnChild(existingChild)
                .AcceptAsOwnChild(newChild);

            AcceptAsOwnChild(surrogateParent);
            Disown(existingChild);
        }

        private Node<T> MakeASurrogateParent(Node<T> existingChild, Node<T> newChild)
        {
            string keyForNewParent = existingChild.CommonBeginningInKeys(newChild);
            keyForNewParent = keyForNewParent.Trim();
            var surrogateParent = new Node<T>(keyForNewParent, default(T));

            return surrogateParent.IsTheSameAs(newChild) ? newChild : surrogateParent;
        }

        private bool IsTheSameAs(Node<T> parent)
        {
            return Equals(parent);
        }

        private bool IsMySibling(Node<T> potentialSibling)
        {
            return CommonBeginningInKeys(potentialSibling).Length > 0;
        }

        private string CommonBeginningInKeys(Node<T> potentialSibling)
        {
            return key.CommonBeginningWith(potentialSibling.key);
        }

        internal virtual bool IsParentOf(Node<T> potentialChild)
        {
            return potentialChild.key.StartsWith(key);
        }

        public bool Delete(string key)
        {
            Node<T> nodeToBeDeleted = children.Find(child => child.Find(key) != null);
            if (nodeToBeDeleted == null) return false;

            if (nodeToBeDeleted.HasChildren)
            {
                nodeToBeDeleted.MarkAsDeleted();
                return true;
            }

            children.Remove(nodeToBeDeleted);
            return true;
        }

        private void MarkAsDeleted()
        {
            value = default(T);
        }

        public T Find(string key)
        {
            var childBeingSearchedFor = new Node<T>(key, default(T));
            return Find(childBeingSearchedFor);
        }

        private T Find(Node<T> childBeingSearchedFor)
        {
            if (Equals(childBeingSearchedFor)) return value;
            T node = default(T);
            children.Find(child =>
                              {
                                  node = child.Find(childBeingSearchedFor);
                                  return node != null;
                              });
            if (node == null) return default(T);
            return node;
        }

        public bool Contains(string key)
        {
            return Contains(new Node<T>(key, default(T)));
        }

        private bool Contains(Node<T> child)
        {
            if (Equals(child) && IsUnrealNode()) return false;

            if (Equals(child)) return true;

            return children.Exists(node => node.Contains(child));
        }

        private bool IsUnrealNode()
        {
            return value == null;
        }

        public List<T> Search(string keyPrefix)
        {
            var nodeBeingSearchedFor = new Node<T>(keyPrefix, default(T));
            return Search(nodeBeingSearchedFor);
        }

        private List<T> Search(Node<T> nodeBeingSearchedFor)
        {
            if (IsTheSameAs(nodeBeingSearchedFor))
                return MeAndMyDescendants();

            return SearchInMyChildren(nodeBeingSearchedFor);
        }

        private List<T> SearchInMyChildren(Node<T> nodeBeingSearchedFor)
        {
            List<T> searchResults = null;

            children.Exists(existingChild => (searchResults = existingChild.SearchUpAndDown(nodeBeingSearchedFor)).Count > 0);

            return searchResults;
        }

        private List<T> SearchUpAndDown(Node<T> node)
        {
            if (node.IsParentOf(this))
                return MeAndMyDescendants();

            return IsParentOf(node) ? Search(node) : new List<T>();

        }

        private List<T> MeAndMyDescendants()
        {
            var meAndMyDescendants = new List<T>();
            if (!IsUnrealNode())
                meAndMyDescendants.Add(value);

            children.ForEach(child => meAndMyDescendants.AddRange(child.MeAndMyDescendants()));
            return meAndMyDescendants;
        }

        public long Size()
        {
            const long size = 0;
            return Size(size);
        }

        private long Size(long size)
        {
            if (!IsUnrealNode())
                size++;

            children.ForEach(node => size += node.Size());
            return size;
        }

        public override string ToString()
        {
            return key;
        }

        public bool Equals(Node<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.key, key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Node<T>)) return false;
            return Equals((Node<T>) obj);
        }

        public override int GetHashCode()
        {
            return (key != null ? key.GetHashCode() : 0);
        }

        public static Node<T> Root()
        {
            return new RootNode<T>();
        }

        private class RootNode<T> : Node<T>
        {
            internal override bool IsParentOf(Node<T> potentialChild)
            {
                return true;
            }
        }

    }
}