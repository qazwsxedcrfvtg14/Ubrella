using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cut
{
    public class Trie<T>:IEnumerable<KeyValuePair<string, T>>
    {
        public class Node {
            public char Key;
            public KeyValuePair<string, T> val;
            public bool real=false;
            public Treap<char, Node> children=new Treap<char, Node>(
                (char a,char b) => { return Char.ToLower(a).CompareTo(Char.ToLower(b)); }
                );
            public Node father;
            public Node next() {
                var ch = children.begin();
                if (ch != null)
                {
                    if (ch.Value.real)
                        return ch.Value;
                    else
                        return ch.Value.next();
                }
                var now = this;
                Treap<char, Node>.Node brother = null;
                while (brother == null) {
                    if (now.father == null) return null;
                    brother = now.father.children.upper_bound(now.Key);
                    now = now.father;
                }
                if (brother.Value.real)
                    return brother.Value;
                else
                    return brother.Value.next();
            }
        }
        Node root=new Node();
        UInt32 count=0;
        public UInt32 Count
        {
            get
            {
                return count;
            }
        }
        public void add(string s, T value)
        {
            if (s == null) return;
            var now = root;
            foreach (char c in s)
            {
                var nx = now.children.find(c);
                if (nx == null)
                {
                    now.children.add(c, new Node { Key = c, father = now });
                    nx = now.children.find(c);
                }
                now = nx.Value;
            }
            if (now.real) count--;
            now.val = new KeyValuePair<string,T>(s,value);
            now.real = true;
            count++;
        }

        public void remove(string s)
        {
            if (s == null) return ;
            var now = root;
            foreach (char c in s)
            {
                var nx = now.children.find(c);
                if (nx == null)
                    return;
                now = nx.Value;
            }
            if (now.real) count--;
            now.val = new KeyValuePair<string, T>();
            //now.val.Value = default(T);
            now.real = false;
        }

        public Node find(string s)
        {
            if (s == null) return null;
            var now = root;
            foreach (char c in s)
            {
                var nx = now.children.find(c);
                if (nx == null)
                    return null;
                now = nx.Value;
            }
            if (now.real)
                return now;
            else
                return null;
        }
        public Node upper_bound(string s)
        {
            if (s == null) return null;
            var now = root;
            foreach (char c in s)
            {
                var nx = now.children.find(c);
                if (nx == null)
                {
                    now.children.add(c, new Node { Key = c, father = now });
                    nx = now.children.find(c);
                    return nx.Value.next();
                }
                now = nx.Value;
            }
            return now.next();
        }
        public Node lower_bound(string s)
        {
            if (s == null) return null;
            var now = find(s);
            if (now != null) return now;
            return upper_bound(s);
        }
        public bool exists(string s)
        {
            if (s == null) return false;
            var now = root;
            foreach (char c in s)
            {
                var nx = now.children.find(c);
                if (nx == null)
                    return false;
                now = nx.Value;
            }
            if (now.real)
                return true;
            else
                return false;
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return new TreiEnumerator(root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TreiEnumerator(root);
        }

        public class TreiEnumerator : IEnumerator<KeyValuePair<string, T>>
        {
            private Node now;
            public TreiEnumerator(Node _now)
            {
                now = _now;
            }
            public void Reset()
            {
                //now = null;
            }
            public KeyValuePair<string, T> Current
            {
                get
                {
                    return now.val;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool MoveNext()
            {
                if (now == null)
                    return false;
                now = now.next();
                if (now == null)
                    return false;
                return
                    true;
            }

            public void Dispose()
            {
                now = null;
            }
        }
    }
}
