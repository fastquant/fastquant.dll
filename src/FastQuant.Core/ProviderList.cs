using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ProviderList : IEnumerable<IProvider>
    {
        public IEnumerator<IProvider> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}