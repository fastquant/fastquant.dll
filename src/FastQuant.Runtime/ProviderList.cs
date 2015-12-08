using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ProviderList : IEnumerable<IProvider>
    {
        public int Count { get; }

        public IProvider this[string name] => GetByName(name);

        public void Add(IProvider provider)
        {
            throw new NotImplementedException();
        }
        public void Remove(IProvider provider)
        {
            throw new NotImplementedException();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IProvider provider)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IProvider> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IProvider GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IProvider GetByIndex(int index)
        {
            throw new NotImplementedException();
        }
        public IProvider GetByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}