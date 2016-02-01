using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ProviderList : IEnumerable<IProvider>
    {
        private readonly GetByList<IProvider> list = new GetByList<IProvider>("Id", "Name");

        public int Count => this.list.Count;

        public IProvider this[string name] => GetByName(name);

        public void Add(IProvider provider) => this.list.Add(provider);

        public void Remove(IProvider provider) => this.list.Remove(provider);

        public void Clear() => this.list.Clear();

        public bool Contains(IProvider provider) => this.list.Contains(provider);

        public IEnumerator<IProvider> GetEnumerator() => this.list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IProvider GetById(int id) => this.list.GetById(id);

        public IProvider GetByIndex(int index) => this.list.GetByIndex(index);

        public IProvider GetByName(string name) => this.list.GetByName(name);
    }
}