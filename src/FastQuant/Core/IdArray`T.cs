// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FastQuant
{
    public class IdArray<T>
    {
        private T[] array;
        private int size;
        private readonly int reserved;

        public int Size => this.size;

        public T this[int id]
        {
            get
            {
                EnsureSize(id);
                return this.array[id];
            }
            set
            {
                Add(id, value);
            }
        }

        public IdArray(int size = 1024)
        {
            this.size = size;
            this.reserved = size;
            this.array = new T[size];
        }

        public void Clear()
        {
            for (int i = 0; i < this.array.Length; i++)
                this.array[i] = default(T);
        }

        public void Add(int id, T value)
        {
            EnsureSize(id);
            this.array[id] = value;
        }

        public void Remove(int id)
        {
            Add(id, default(T));
        }

        private void Resize(int id)
        {
            Console.WriteLine($"IdArray::Resize index = {id}");
            var length = id + this.reserved;
            Array.Resize(ref this.array, length);
            this.size = length;
        }

        public void CopyTo(IdArray<T> array)
        {
            for (int i = 0; i < array.Size; i++)
                array[i] = i < Size ? this.array[i] : default(T);
        }

        private void EnsureSize(int id)
        {
            if (id >= Size)
                Resize(id);
        }
    }
}