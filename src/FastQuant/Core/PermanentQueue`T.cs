// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace FastQuant
{
    public class PermanentQueue<T>
    {
        private readonly Dictionary<object, int> readers = new Dictionary<object, int>();
        private readonly List<T> items = new List<T>();

        public void AddReader(object reader)
        {
            lock (this.items)
                this.readers[reader] = 0;
        }

        public void Clear()
        {
            lock (this.items)
            {
                foreach (var key in this.readers.Keys)
                    this.readers[key] = 0;
                this.items.Clear();
            }
        }

        public int Count(int startIndex)
        {
            lock (this.items)
                return this.items.Count - startIndex;
        }

        public T[] DequeueAll(object reader)
        {
            lock (this.items)
            {
                int count = this.readers[reader];
                if (this.items.Count >= count + 1)
                {
                    var result = new T[this.items.Count - count];
                    this.items.CopyTo(count, result, 0, result.Length);
                    this.readers[reader] = count + result.Length;
                    return result;
                }
                else
                    return null;
            }
        }

        public void Enqueue(T item)
        {
            lock (this.items)
                this.items.Add(item);
        }

        public void RemoveReader(object reader)
        {
            lock (this.items)
                this.readers.Remove(reader);
        }
    }
}