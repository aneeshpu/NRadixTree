using System;
using System.Collections.Generic;

namespace RadixTree
{
    public class Node<T> where T : class
    {
        private readonly string key;
        private T value;
        private readonly List<Node<T>> children = new List<Node<T>>();

        protected Node(string key, T value)
        {
            this.key = key;
            this.value = value;
        }

        public Node(){}

        public void Insert(string key, T value)
        {
            if (Contains(key)) throw new DuplicateKeyException(string.Format("Duplicate key: '{0}'", key));

            var potentialChild = new Node<T>(key, value);
            var foundParent = children.Exists(existingChild =>
                                             {
                                                 if (existingChild.IsTheSameAs(key) && existingChild.IsMarkedForDeletion())
                                                 {
                                                     existingChild.MergeWith(value);
                                                     return true;
                                                 }
                                                 if (existingChild.IsReallyMyChild(potentialChild))
                                                 {
                                                     existingChild.Insert(key, value);
                                                     return true;
                                                 }

                                                 if (existingChild.IsMySibling(potentialChild))
                                                 {
                                                     ForkANewChildAndAddChildren(existingChild, potentialChild);
                                                     return true;
                                                 }

                                                 return false;
                                             });

            if(foundParent) return;
            AcceptAsOwnChild(potentialChild);
        }

        private void MergeWith(T value)
        {
            this.value = value;
        }

        private void Disown(Node<T> existingChild)
        {
            children.Remove(existingChild);
        }

        private void AcceptAsOwnChild(Node<T> child)
        {
            if(NotItself(child)) children.Add(child);
        }

        private bool NotItself(Node<T> child)
        {
            return !Equals(child);
        }

        private void ForkANewChildAndAddChildren(Node<T> existingChild, Node<T> newChild)
        {
            var keyForNewParent = existingChild.CommonBeginningInKeys(newChild);
            keyForNewParent = keyForNewParent.Trim();
            var surrogateParent = new Node<T>(keyForNewParent, default(T));

            if(IsAlreadyAddedUnderTheCorrectParent(surrogateParent))
            {
                AcceptAsOwnChild(newChild);
                return;
            }

            if(newChild.IsTheSameAs(surrogateParent))
            {
                surrogateParent = newChild;
            }

            surrogateParent.AcceptAsOwnChild(existingChild);
            surrogateParent.AcceptAsOwnChild(newChild);

            AcceptAsOwnChild(surrogateParent);
            Disown(existingChild);
        }

        private bool IsTheSameAs(Node<T> parent)
        {
            return Equals(parent);
        }

        private bool IsAlreadyAddedUnderTheCorrectParent(Node<T> surrogateParent)
        {
            return Equals(surrogateParent);
        }

        private bool IsMySibling(Node<T> potentialSibling)
        {
            return CommonBeginningInKeys(potentialSibling).Length > 0;
        }

        private string CommonBeginningInKeys(Node<T> potentialSibling)
        {
            String commonStart = String.Empty;
            foreach (var character in key)
            {
                if (!potentialSibling.key.StartsWith(commonStart + character)) break;
                commonStart += character;
            }
            return commonStart;
        }

        private bool IsReallyMyChild(Node<T> potentialChild)
        {
            return potentialChild.key.StartsWith(key);
        }

        public bool Delete(string key)
        {
            Node<T> nodeToBeDeleted = children.Find(child => child.Find(key) != null);
            if(nodeToBeDeleted == null) return false;

            if(nodeToBeDeleted.HasChildren)
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

        protected bool HasChildren
        {
            get { return children.Count > 0; }
        }

        public T Find(string key)
        {
            if (this.key == key) return value;
            var node = default(T);
            children.Find(child =>
                                         {
                                             node= child.Find(key);
                                             return node!= null;
                                         });
            if(node == null) return default(T);
            return node;
        }

        public bool Contains(string key)
        {
            if (this.key == key && IsMarkedForDeletion()) return false;

            if(this.key == key) return true;

            return children.Exists(node => node.Contains(key));
        }

        private bool IsMarkedForDeletion()
        {
            return value == null;
        }

        public List<T> Search(string keyPrefix)
        {
            if(IsTheSameAs(keyPrefix))
            {
                return MeAndMyDescendants();
            }

            return SearchInMyChildren(keyPrefix);
        }

        private List<T> SearchInMyChildren(string keyPrefix)
        {
            var searchResults = new List<T>();
            var tempNode = new Node<T>(keyPrefix,null);

            children.Exists(node =>
                                {
                                    if(tempNode.IsReallyMyChild(node))
                                    {
                                        searchResults = node.MeAndMyDescendants();
                                        return true;
                                    }   
                 
                                    if(node.IsReallyMyChild(keyPrefix))
                                    {
                                        searchResults = node.Search(keyPrefix);
                                        return true;
                                    }
                                    return false;
                                });

            return searchResults;
        }

        private bool IsReallyMyChild(string keyPrefix)
        {
            return keyPrefix.StartsWith(key);
        }

        private List<T> MeAndMyDescendants()
        {
            var meAndMyDescendants = new List<T>();
            if(!IsMarkedForDeletion())
                meAndMyDescendants.Add(value);

            children.ForEach(child => meAndMyDescendants.AddRange(child.MeAndMyDescendants()));
            return meAndMyDescendants;
        }

        private bool IsTheSameAs(string keyPrefix)
        {
            return key == keyPrefix;
        }

        public long Size()
        {
            long size = 0;
            children.ForEach(node =>
                                 {
                                     if (!node.IsMarkedForDeletion())
                                         size++;
                                     size += node.Size();
                                 });
            return size;
        }

        public string Complete(string prefix)
        {
            throw new NotImplementedException();
        }

        public void PrintString(int depth)
        {
            Console.WriteLine(string.Format("{0}{1}",depth,key));
            children.ForEach(node => node.PrintString(++depth));
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
    }    

}