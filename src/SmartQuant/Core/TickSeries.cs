using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                        num = GetIndex(dateTime, IndexOption.Next);
                        break;
                    case SearchOption.Prev:
                        num = GetIndex(dateTime, IndexOption.Prev);
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
            this.min = this.min == null ? tick : tick.Price < this.min.Price ? tick : this.min;
            this.max = this.max == null ? tick : tick.Price > this.min.Price ? tick : this.max;

            if (this.ticks.Count == 0 || tick.DateTime >= this.ticks[this.ticks.Count - 1].DateTime)
                this.ticks.Add(tick);
            else
                this.ticks.Insert(GetIndex(tick.DateTime, IndexOption.Next), tick);
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
            var i = Count - 1 - n;
            if (i < 0)
                throw new ArgumentException($"TickSeries::Ago Can not return tick {n} ticks ago: tick series is too short, count = {Count}");
            return this[i];
        }

        public bool Contains(DateTime dateTime) => GetIndex(dateTime, IndexOption.Null) != -1;

        public DateTime GetDateTime(int index) => this[index].DateTime;

        public IEnumerator<Tick> GetEnumerator() => this.ticks.GetEnumerator();
  
        public int GetIndex(DateTime dateTime, IndexOption option = IndexOption.Null)
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

        public Tick GetMax() => this.max;

        public Tick GetMax(DateTime dateTime1, DateTime dateTime2)
        {
            Tick tick = null;
            foreach (var t in this.ticks)
            {
                if (dateTime1 <= t.DateTime)
                {
                    if (t.DateTime > dateTime2)
                        return tick;
                    tick = tick == null ? t : t.Price > tick.Price ? t : tick;
                }
            }
            return tick;
        }

        public Tick GetMax(int index1, int index2)
        {
            var tick = this[index1];
            for (var i = index1 + 1; i <= index2; i++)
            {
                var t = this[i];
                if (t.Price > tick.Price)
                    tick = t;
            }
            return tick;
        }

        double ISeries.GetMax(DateTime dateTime1, DateTime dateTime2) => GetMax(dateTime1, dateTime2).Price;

        double ISeries.GetMax(int index1, int index2, BarData barData) => GetMax(index1, index2).Price;

        public Tick GetMin() => this.min;

        public Tick GetMin(int index1, int index2)
        {
            var tick = this[index1];
            for (var i = index1 + 1; i <= index2; i++)
            {
                var t = this[i];
                if (t.Price < tick.Price)
                    tick = t;
            }
            return tick;
        }

        public Tick GetMin(DateTime dateTime1, DateTime dateTime2)
        {
            Tick tick = null;
            foreach (var t in this.ticks)
            {
                if (dateTime1 <= t.dateTime)
                {
                    if (t.dateTime > dateTime2)
                        return tick;
                    tick = tick == null ? t : t.Price < tick.Price ? t : tick;
                }
            }
            return tick;
        }

        double ISeries.GetMin(DateTime dateTime1, DateTime dateTime2) => GetMin(dateTime1, dateTime2).Price;

        double ISeries.GetMin(int index1, int index2, BarData barData) => GetMin(index1, index2).Price;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int GetIndex_me(DateTime dateTime, IndexOption option = IndexOption.Null)
        {
            var i = this.ticks.BinarySearch(new Tick() { DateTime = dateTime }, new DataObjectComparer());
            if (i >= 0)
                return i;
            else if (option == IndexOption.Next)
                return ~i;
            else if (option == IndexOption.Prev)
                return ~i - 1;
            return -1; // option == IndexOption.Null
        }

        private int GetIndex_original(DateTime dateTime, IndexOption option = IndexOption.Null)
        {
            int num = 0;
            int num2 = 0;
            int num3 = this.ticks.Count - 1;
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
                        if (this.ticks[num].dateTime == dateTime)
                        {
                            flag = false;
                        }
                        else if (this.ticks[num].dateTime > dateTime)
                        {
                            num3 = num - 1;
                        }
                        else if (this.ticks[num].dateTime < dateTime)
                        {
                            num2 = num + 1;
                        }
                        break;
                    case IndexOption.Next:
                        if (this.ticks[num].dateTime >= dateTime && (num == 0 || this.ticks[num - 1].dateTime < dateTime))
                        {
                            flag = false;
                        }
                        else if (this.ticks[num].dateTime < dateTime)
                        {
                            num2 = num + 1;
                        }
                        else
                        {
                            num3 = num - 1;
                        }
                        break;
                    case IndexOption.Prev:
                        if (this.ticks[num].dateTime <= dateTime && (num == this.ticks.Count - 1 || this.ticks[num + 1].dateTime > dateTime))
                        {
                            flag = false;
                        }
                        else if (this.ticks[num].dateTime > dateTime)
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