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

        public int GetIndex(DateTime dateTime, IndexOption option)
        {
            return GetIndex_me(dateTime, option);
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
                    throw new ArgumentException("Unsupported search option");
            }
        }

        private int GetIndex_me(DateTime dateTime, IndexOption option)
        {
            var i = this.quotes.BinarySearch(new Quote { DateTime = dateTime }, new DataObjectComparer());
            if (i >= 0)
                return i;
            if (option == IndexOption.Next)
                return ~i;
            if (option == IndexOption.Prev)
                return ~i - 1;
            return -1; // option == IndexOption.Null   
        }

        private int GetIndex_original(DateTime datetime, IndexOption option)
        {
            int num = 0;
            int num2 = 0;
            int num3 = this.quotes.Count - 1;
            bool flag = true;
            while (flag)
            {
                if (num3 < num2)
                {
                    return -1;
                }
                num = (num2 + num3) / 2;
                switch (option)
                {
                    case IndexOption.Null:
                        if (this.quotes[num].dateTime == datetime)
                        {
                            flag = false;
                        }
                        else if (this.quotes[num].dateTime > datetime)
                        {
                            num3 = num - 1;
                        }
                        else if (this.quotes[num].dateTime < datetime)
                        {
                            num2 = num + 1;
                        }
                        break;
                    case IndexOption.Next:
                        if (this.quotes[num].dateTime >= datetime && (num == 0 || this.quotes[num - 1].dateTime < datetime))
                        {
                            flag = false;
                        }
                        else if (this.quotes[num].dateTime < datetime)
                        {
                            num2 = num + 1;
                        }
                        else
                        {
                            num3 = num - 1;
                        }
                        break;
                    case IndexOption.Prev:
                        if (this.quotes[num].dateTime <= datetime && (num == this.quotes.Count - 1 || this.quotes[num + 1].dateTime > datetime))
                        {
                            flag = false;
                        }
                        else if (this.quotes[num].dateTime > datetime)
                        {
                            num3 = num - 1;
                        }
                        else
                        {
                            num2 = num + 1;
                        }
                        break;
                }
            }
            return num;
        }
    }
}