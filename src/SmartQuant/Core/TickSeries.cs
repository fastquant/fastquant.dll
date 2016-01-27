using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class TickSeries : IEnumerable<Tick>, IDataSeries, ISeries
    {
        private readonly List<Tick> ticks = new List<Tick>();
        private Tick min;
        private Tick max;

        public string Name { get; }
        public string Description { get; }

        public Tick this[int index] => this.ticks[index];

        public Tick this[DateTime dateTime, SearchOption option = SearchOption.ExactFirst]
        {
            get
            {
                int num = -1;
                switch (option)
                {
                    case SearchOption.Next:
                        num = this.GetIndex(dateTime, IndexOption.Next);
                        break;
                    case SearchOption.Prev:
                        num = this.GetIndex(dateTime, IndexOption.Prev);
                        break;
                }
                if (num >= 0)
                {
                    return this.ticks[num];
                }
                return null;
            }
        }

        double ISeries.this[int index] => this[index].Price;

        double ISeries.this[int index, BarData data] => this[index].Price;

        DataObject IDataSeries.this[long index] => this[(int)index];

        public int Count => this.ticks.Count;

        long IDataSeries.Count => Count;

        DateTime IDataSeries.DateTime1 => FirstDateTime;

        DateTime IDataSeries.DateTime2 => LastDateTime;

        double ISeries.First => this[0].Price;

        double ISeries.Last => this[Count-1].Price;

        public DateTime FirstDateTime => this[0].DateTime;

        public DateTime LastDateTime => this[Count - 1].DateTime;

        public List<Indicator> Indicators => null;

        public TickSeries(string name = "", string description = "")
        {
            Name = name;
            Description = description;
        }

        public void Add(Tick tick)
        {
            this.min = this.min == null || this.min.Price > tick.Price ? tick : this.min;
            this.max = this.max == null || this.max.Price < tick.Price ? tick : this.max;
            throw new NotImplementedException();
        }

        void IDataSeries.Add(DataObject obj) => Add((Tick)obj);

        void IDataSeries.Remove(long index) => this.ticks.RemoveAt((int)index);

        public void Clear()
        {
            this.ticks.Clear();
            this.max = this.min = null;
        }

        public Tick Ago(int n)
        {
            int i = Count - 1 - n;
            if (i < 0)
                throw new ArgumentException($"TickSeries::Ago Can not return tick {n} ticks ago: tick series is too short, count = {Count}");
            return this[i];
        }

        public bool Contains(DateTime dateTime) => GetIndex(dateTime, IndexOption.Null) != -1;

        public DateTime GetDateTime(int index) => this[index].DateTime;

        public IEnumerator<Tick> GetEnumerator() => this.ticks.GetEnumerator();
  
        public int GetIndex(DateTime dateTime, IndexOption option = IndexOption.Null)
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
                    throw new ArgumentException("Unsupported search option");
            }
        }

        public Tick GetMax() => this.max;

        public Tick GetMax(DateTime dateTime1, DateTime dateTime2)
        {
            throw new NotImplementedException();
        }

        public Tick GetMax(int index1, int index2)
        {
            throw new NotImplementedException();
        }

        double ISeries.GetMax(DateTime dateTime1, DateTime dateTime2) => GetMax(dateTime1, dateTime2).Price;

        double ISeries.GetMax(int index1, int index2, BarData barData) => GetMax(index1, index2).Price;

        public Tick GetMin() => this.min;

        public Tick GetMin(int index1, int index2)
        {
            throw new NotImplementedException();
        }

        public Tick GetMin(DateTime dateTime1, DateTime dateTime2)
        {
            throw new NotImplementedException();
        }

        double ISeries.GetMin(DateTime dateTime1, DateTime dateTime2) => GetMin(dateTime1, dateTime2).Price;

        double ISeries.GetMin(int index1, int index2, BarData barData) => GetMin(index1, index2).Price;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}