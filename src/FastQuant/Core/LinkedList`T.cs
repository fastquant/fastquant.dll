// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace FastQuant
{
    public class LinkedListNode<T>
    {
        public T Data { get; set; }

        public LinkedListNode<T> Next { get; set; }

        public LinkedListNode(T data)
        {
            Data = data;
        }
    }

    public class LinkedList<T> : IEnumerable<T>
    {
        public LinkedListNode<T> First { get; set; }
        public int Count { get; set; }

        public void Clear()
        {
            First = null;
            Count = 0;
        }

        public void Add(T data)
        {
            if (First == null)
            {
                First = new LinkedListNode<T>(data);
                Count++;
                return;
            }

            var n = First;
            while (!n.Data.Equals(data))
            {
                if (n.Next == null)
                {
                    n.Next = new LinkedListNode<T>(data);
                    Count++;
                    break;
                }
                n = n.Next;
            }
        }

        public void Remove(T data)
        {
            if (First == null)
                return;
            if (First.Data.Equals(data))
            {
                First = First.Next;
                --Count;
            }
            else
            {
                var lastNode = First;
                for (var node = First.Next; node != null; node = node.Next)
                {
                    if (!node.Data.Equals(data))
                    {
                        lastNode = node;
                    }
                    else
                    {
                        lastNode.Next = node.Next;
                        --Count;
                        break;
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Helper<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        class Helper<T1> : IEnumerator<T1>
        {
            private LinkedList<T1> list;
            private LinkedListNode<T1> node;
            private int pos;

            public T1 Current => this.node.Data;

            object IEnumerator.Current => Current;

            public Helper(LinkedList<T1> list)
            {
                this.list = list;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.pos >= this.list.Count)
                    return false;
                this.node = this.pos == 0 ? this.list.First : this.node.Next;
                ++this.pos;
                return true;
            }

            public void Reset()
            {
                this.node = null;
                this.pos = 0;
            }
        }
    }
}
