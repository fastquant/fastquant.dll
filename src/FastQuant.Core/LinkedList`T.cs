// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class LinkedListNode<T>
    {
        public T Data;

        public LinkedListNode<T> Next;

        public LinkedListNode(T data)
        {
            Data = data;
        }
    }

    public class LinkedList<T> : IEnumerable<T>
    {
        public LinkedListNode<T> First;
        public int Count;

        public void Add(T data)
        {
            if (First == null)
            {
                First = new LinkedListNode<T>(data);
                ++Count;
            }
            else
            {
                LinkedListNode<T> node;
                for (node = First; node.Next != null; node = node.Next)
                {
                    if (node.Data.Equals(data))
                        return;
                }
                if (node.Data.Equals(data))
                    return;
                node.Next = new LinkedListNode<T>(data);
                ++Count;
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
                        --this.Count;
                        break;
                    }
                }
            }
        }

        public void Clear()
        {
            First = null;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Helper<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        class Helper<T1> : IEnumerator<T1>, IDisposable
        {
            private LinkedList<T1> list;
            private LinkedListNode<T1> node;
            private int count;

            public T1 Current
            {
                get
                {
                    return this.node.Data;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public Helper(LinkedList<T1> list)
            {
                this.list = list;
                Reset();
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.count >= this.list.Count)
                    return false;
                this.node = this.count == 0 ? this.list.First : this.node.Next;
                ++this.count;
                return true;
            }

            public void Reset()
            {
                this.node = null;
                this.count = 0;
            }
        }
    }
}
