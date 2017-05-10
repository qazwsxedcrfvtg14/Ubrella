using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cut
{
    public class Treap<T, S> where T : IComparable
    {
        Func<T, T, int> comp;
        public Treap() {
            comp = Comparer<T>.Default.Compare;
        }
        public Treap(Func<T, T, int> _comp)
        {
            comp = _comp;
        }
        public class Node {
            public T Key;
            public S Value;
            public Node l, r, fa;
            public UInt32 Count;
            public Node(T _k, S _v, Node _fa = null) {
                Key = _k;
                Value = _v;
                fa = _fa;
            }
            public void push() {
                fa = null;
            }
            public void pull() {
                Count = 1;
                if (l != null)
                {
                    Count += l.Count;
                    l.fa = this;
                }
                if (r != null)
                {
                    Count += r.Count;
                    r.fa = this;
                }
            }
            Node top() {
                Node now = this;
                if (fa != null) now = fa.top();
                return now;
            }
            public Node left() {
                if (l != null) return l.left();
                else return this;
            }
            public Node right() {
                if (r != null) return r.right();
                else return this;
            }
            public Node next() {
                if (r != null) return r.left();
                Node now = this;
                while (now.fa != null)
                    if (now.fa.l == now) return now.fa;
                    else now = now.fa;
                return null;
            }
            public Node pre() {
                if (l != null) return l.right();
                Node now = this;
                while (now.fa != null)
                    if (now.fa.r == now) return now.fa;
                    else now = now.fa;
                return null;
            }
        }
        UInt32 rand = (UInt32)(new Random()).Next();
        Node Merge(Node a, Node b) {
            if (a == null) return b;
            if (b == null) return a;
            if ((rand++) % (a.Count + b.Count) < a.Count)
            {
                a.push();
                a.r = Merge(a.r, b);
                a.pull();
                return a;
            }
            else {
                b.push();
                b.l = Merge(a, b.l);
                b.pull();
                return b;
            }
        }
        Tuple<Node, Node> Split(Node now, T k)
        {
            if (now == null) return Tuple.Create<Node, Node>(null, null);
            else if (comp(now.Key,k) < 0)
            {
                now.push();
                var sp = Split(now.r, k);
                now.r = sp.Item1;
                now.pull();
                return Tuple.Create(now, sp.Item2);
            }
            else
            {
                now.push();
                var sp = Split(now.l, k);
                now.l = sp.Item2;
                now.pull();
                return Tuple.Create(sp.Item1, now);
            }
        }
        Tuple<Node, Node> Split2(Node now, T k)
        {
            if (now == null) return Tuple.Create<Node, Node>(null, null);
            else if (comp(now.Key,k)<=0)
            {
                now.push();
                var sp = Split(now.r, k);
                now.r = sp.Item1;
                now.pull();
                return Tuple.Create(now, sp.Item2);
            }
            else
            {
                now.push();
                var sp = Split(now.l, k);
                now.l = sp.Item2;
                now.pull();
                return Tuple.Create(sp.Item1, now);
            }
        }
        Node root;
        public UInt32 Count
        {
            get
            {
                if (root == null) return 0;
                else return root.Count;
            }
        }
        public Node begin()
        {
            if (root == null) return null;
            return root.left();
        }
        public void add(T key, S value) {
            var now = find(key);
            if (now != null)
            {
                now.Value = value;
            }
            else
            {
                var sp = Split(root, key);
                root = Merge(Merge(sp.Item1, new Node(key, value)), sp.Item2);
            }
        }
        public void remove(T key) {
            var sp1 = Split(root, key);
            var sp2 = Split2(sp1.Item2, key);
            //delete sp2.Item2;
            root = Merge(sp1.Item1, sp2.Item2);
        }
        public Node lower_bound(T key)
        {
            Node now = root, pri = null;
            while (now != null)
                if (comp(key,now.Key) <= 0)
                {
                    pri = now;
                    now = now.l;
                }
                else now = now.r;
            return pri;
        }
        public Node upper_bound(T key)
        {
            Node now = root, pri = null;
            while (now != null)
                if (comp(key,now.Key) < 0)
                {
                    pri = now;
                    now = now.l;
                }
                else now = now.r;
            return pri;
        }
        public Node find(T key) {
            Node now = lower_bound(key);
            if (now != null && comp(now.Key,key)!=0)
                now = null;
            return now;
        }
    }
}