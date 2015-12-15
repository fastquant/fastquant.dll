using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class FillSeries : IEnumerable<Fill>
    {
        private List<Fill> fills = new List<Fill>();
        private Fill max;
        private Fill min;
        private string name;

        public Fill Max => this.max;

        public Fill Min => this.min;

        public int Count => this.fills.Count;

        public Fill this[int index] => this.fills[index];

        public FillSeries(string name = "")
        {
            this.name = name;
        }

        public void Add(Fill fill)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            this.fills.Clear();
            this.max = this.min = null;
        }

        public int GetIndex(DateTime datetime, IndexOption option)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Fill> GetEnumerator() => this.fills.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}