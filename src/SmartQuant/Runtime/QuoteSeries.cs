using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{

    public class QuoteSeries : IEnumerable<Quote>, IDataSeries
    {
        public string Name { get; }
        public string Description { get; }
        private List<Quote> quotes = new List<Quote>();

        public int Count => this.quotes.Count;

        long IDataSeries.Count => Count;

        public Quote this[int index] => this.quotes[index];

        DataObject IDataSeries.this[long index] => this[(int)index];

        public DateTime FirstDateTime => this[0].DateTime;

        public DateTime LastDateTime => this[Count - 1].DateTime;

        DateTime IDataSeries.DateTime1 => FirstDateTime;

        DateTime IDataSeries.DateTime2 => LastDateTime;

        public QuoteSeries(string name = "")
        {
            Name = name;
        }

        public void Add(Quote quote) => this.quotes.Add(quote);

        void IDataSeries.Add(DataObject obj) => Add((Quote)obj);

        void IDataSeries.Remove(long index) => this.quotes.RemoveAt((int)index);

        public void Clear() => this.quotes.Clear();

        public bool Contains(DateTime dateTime) => GetIndex(dateTime, IndexOption.Null) != -1;

        public IEnumerator<Quote> GetEnumerator() => this.quotes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int GetIndex(DateTime datetime, IndexOption option)
        {
            throw new NotImplementedException();
        }

        long IDataSeries.GetIndex(DateTime dateTime, SearchOption option)
        {
            switch (option)
            {
                case SearchOption.Next:
                    return GetIndex(dateTime, IndexOption.Next);
                case SearchOption.Prev:
                    return GetIndex(dateTime, IndexOption.Prev);
                default:
                    throw new ApplicationException("Unsupported search option");
            }
        }
    }
}