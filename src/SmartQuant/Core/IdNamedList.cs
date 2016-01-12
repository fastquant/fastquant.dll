using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public interface IIdNamedItem
    {
        int Id { get; }
        string Name { get; }
    }

    public class IdNamedItem
    {
        protected int id = -1;
        protected string name = string.Empty;

        public int Id { get { return this.id; } }

        public string Name { get { return this.name; } }
    }

    public class IdNamedList : IEnumerable<IIdNamedItem>
    {
        private Dictionary<string, IIdNamedItem> dictionary;
        private IdArray<IIdNamedItem> array;
        private List<IIdNamedItem> list;

        public IdNamedList(int size = 10000)
        {
            this.dictionary = new Dictionary<string, IIdNamedItem>();
            this.array = new IdArray<IIdNamedItem>(size);
            this.list = new List<IIdNamedItem>();
        }

        public IEnumerator<IIdNamedItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}