using System;
using System.Threading.Tasks;

namespace SmartQuant
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
            Parallel.ForEach(this.array, elem => elem = default(T));
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
            Console.WriteLine("IdArray::Resize index = {0}", id);
            var length = id + this.reserved;
            Array.Resize(ref this.array, length);
            this.size = length;
        }

        public void CopyTo(IdArray<T> array)
        {
            Parallel.For(0, array.Size, i => array[i] = i > Size - 1 ? default(T) : this.array[i]);
        }

        private void EnsureSize(int id)
        {
            if (id >= Size)
                Resize(id);
        }
    }

}