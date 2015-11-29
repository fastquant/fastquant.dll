using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class FillSeries : IEnumerable<Fill>
    {
        public FillSeries(string name = "")
        {
            
        }

        public void Add(Fill fill)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Fill> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}