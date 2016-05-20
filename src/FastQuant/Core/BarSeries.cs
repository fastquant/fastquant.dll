using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FastQuant
{
    public class BarSeries : IEnumerable<Bar>, IIdNamedItem, IDataSeries, ISeries
    {
        protected string name = string.Empty;
        protected string description;
        private int maxLength = -1;
        private readonly List<Bar> bars = new List<Bar>();
        private Bar max;
        private Bar min;

        public int Id { get; } = -1;

        public string Name => this.name;

        public string Description => this.description;

        public List<Indicator> Indicators { get; } = new List<Indicator>();
 
        public BarSeries(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public BarSeries(string name = "", string description = "", int maxLength = -1, int id = -1)
        {
            this.name = name;
            this.description = description;
            this.maxLength = maxLength;
            Id = id;
        }

        public int Count => this.bars.Count;

        long IDataSeries.Count => Count;

        DateTime IDataSeries.DateTime1 => FirstDateTime;

        DateTime IDataSeries.DateTime2 => LastDateTime;

        public Bar First => this[0];

        public DateTime FirstDateTime => First.DateTime;

        double ISeries.First => First.Close;

        public Bar Last => this[Count - 1];

        public DateTime LastDateTime => Last.DateTime;

        double ISeries.Last => Last.Close;

        public Bar this[int index] => this.bars[index];

        double ISeries.this[int index] => this[index, BarData.Close];

        DataObject IDataSeries.this[long index] => this[(int)index];

        public Bar this[DateTime dateTime, IndexOption option = IndexOption.Null] => this[GetIndex(dateTime, option)];

        public double this[int index, BarData barData]
        {
            get
            {
                switch (barData)
                {
                    case BarData.Close:
                        return this[index].Close;
                    case BarData.Open:
                        return this[index].Open;
                    case BarData.High:
                        return this[index].High;
                    case BarData.Low:
                        return this[index].Low;
                    case BarData.Median:
                        return this[index].Median;
                    case BarData.Typical:
                        return this[index].Typical;
                    case BarData.Weighted:
                        return this[index].Weighted;
                    case BarData.Average:
                        return this[index].Average;
                    case BarData.Volume:
                        return this[index].Volume;
                    case BarData.OpenInt:
                        return this[index].OpenInt;
                    case BarData.Range:
                        return this[index].Range;
                    default:
                        throw new ArgumentException($"Unknown BarData value {barData}");
                }
            }
        }

        private void method_0()
        {
            this.bars.RemoveAt(0);
            foreach (var indicator in Indicators.Where(i => i.Count > 0))
                indicator.Remove(0);
        }

        private void Add_origin(Bar bar)
        {
            if (this.min == null)
            {
                this.min = bar;
            }
            else if (bar.High < this.min.Low)
            {
                this.min = bar;
            }
            if (this.max == null)
            {
                this.max = bar;
            }
            else if (bar.High > this.max.High)
            {
                this.max = bar;
            }
            if (this.bars.Count != 0 && !(bar.dateTime >= this.bars[this.bars.Count - 1].dateTime))
            {
                this.bars.Insert(this.GetIndex(bar.dateTime, IndexOption.Next), bar);
            }
            else
            {
                this.bars.Add(bar);
            }
            int num = this.bars.Count - 1;
            for (int i = 0; i < this.Indicators.Count; i++)
            {
                this.Indicators[i].Update(num);
            }
            if (this.maxLength != -1 && this.Count > this.maxLength)
            {
                this.method_0();
            }

        }

        public void Add(Bar bar)
        {
            Add_origin(bar);
            //this.min = this.min == null ? bar : bar.High < this.min.Low ? bar : this.min;
            //this.max = this.max == null ? bar : bar.High > this.max.High ? bar : this.max;
        }

        void IDataSeries.Add(DataObject obj) => Add((Bar)obj);

        public void Clear()
        {
            this.bars.Clear();
            this.min = this.max = null;
            Indicators.ForEach(i => i.Clear());
            Indicators.Clear();
        }

        public Bar Ago(int n)
        {
            var i = Count - 1 - n;
            if (i < 0)
                throw new ArgumentException($"BarSeries::Ago Can not return bar {n} bars ago: bar series is too short, count = {Count}");
            return this[i];
        }

        public bool Contains(DateTime dateTime) => GetIndex(dateTime, IndexOption.Null) != -1;

        public DateTime GetDateTime(int index) => this[index].DateTime;

        public Bar GetMin() => this.min;

        public double GetMin(DateTime dateTime1, DateTime dateTime2) => GetMin(GetIndex(dateTime1, IndexOption.Next), GetIndex(dateTime2, IndexOption.Prev), BarData.Low);

        public double GetMin(int index1, int index2, BarData barData) => index1 > index2 || index1 == -1 || index2 == -1 ? double.NaN : Enumerable.Range(index1, index2 - index1 + 1).Min(i => this[i, barData]);
     
        public Bar GetMax() => this.max;

        public double GetMax(DateTime dateTime1, DateTime dateTime2) => GetMax(GetIndex(dateTime1, IndexOption.Next), GetIndex(dateTime2, IndexOption.Prev), BarData.High);
 
        public double GetMax(int index1, int index2, BarData barData) => index1 > index2 || index1 == -1 || index2 == -1 ? double.NaN : Enumerable.Range(index1, index2 - index1 + 1).Max(i => this[i, barData]);

        public Bar HighestHighBar() => GetMax();

        public Bar HighestHighBar(int nBars) => HighestHighBar(Count - nBars, Count - 1);

        public Bar HighestHighBar(DateTime dateTime1, DateTime dateTime2) => HighestHighBar(GetIndex(dateTime1, IndexOption.Next), GetIndex(dateTime2, IndexOption.Prev));

        public Bar HighestHighBar(int index1, int index2)
        {
            if (Count == 0 || !IsIndex1AndIndex2Valid(index1, index2))
                return null;

            var bar = this.bars[index1];
            for (int i = index1 + 1; i <= index2; i++)
                if (this.bars[i].High > bar.High)
                    bar = this.bars[i];
            return bar;
        }

        public Bar LowestLowBar() => GetMin();

        public Bar LowestLowBar(int nBars) => LowestLowBar(Count - nBars, Count - 1);

        public Bar LowestLowBar(DateTime dateTime1, DateTime dateTime2) => LowestLowBar(GetIndex(dateTime1, IndexOption.Next), GetIndex(dateTime2, IndexOption.Prev));

        public Bar LowestLowBar(int index1, int index2)
        {
            if (Count == 0 || !IsIndex1AndIndex2Valid(index1, index2))
                return null;

            var bar = this.bars[index1];
            for (int i = index1 + 1; i <= index2; i++)
                if (this.bars[i].Low < bar.Low)
                    bar = this.bars[i];
            return bar;
        }

        public double HighestHigh() => HighestHighBar().High;

        public double HighestHigh(int nBars) => HighestHighBar(nBars).High;

        public double HighestHigh(int index1, int index2) => HighestHighBar(index1, index2).High;

        public double HighestHigh(DateTime dateTime1, DateTime dateTime2) => HighestHighBar(dateTime1, dateTime2).High;

        public double LowestLow() => LowestLowBar().Low;

        public double LowestLow(int nBars) => LowestLowBar(nBars).Low;

        public double LowestLow(int index1, int index2) => LowestLowBar(index1, index2).Low;

        public double LowestLow(DateTime dateTime1, DateTime dateTime2) => LowestLowBar(dateTime1, dateTime2).Low;

        public Bar HighestLowBar(int nBars) => HighestLowBar(Count - nBars, Count - 1);

        public Bar HighestLowBar(DateTime dateTime1, DateTime dateTime2) => HighestLowBar(GetIndex(dateTime1, IndexOption.Next), GetIndex(dateTime2, IndexOption.Prev));

        public Bar HighestLowBar(int index1, int index2)
        {
            if (Count == 0 || !IsIndex1AndIndex2Valid(index1, index2))
                return null;

            var bar = this.bars[index1];
            for (int i = index1 + 1; i <= index2; i++)
                if (this.bars[i].Low > bar.Low)
                    bar = this.bars[i];
            return bar;
        }

        public double HighestLow(int nBars) => HighestLowBar(nBars).Low;

        public double HighestLow(int index1, int index2) => HighestLowBar(index1, index2).Low;

        public double HighestLow(DateTime dateTime1, DateTime dateTime2) => HighestLowBar(dateTime1, dateTime2).Low;

        public Bar LowestHighBar(int nBars) => LowestHighBar(Count - nBars, Count - 1);

        public Bar LowestHighBar(int index1, int index2)
        {
            if (Count == 0 || !IsIndex1AndIndex2Valid(index1, index2))
                return null;

            var bar = this.bars[index1];
            for (int i = index1 + 1; i <= index2; i++)
                if (this.bars[i].High < bar.High)
                    bar = this.bars[i];
            return bar;
        }

        public Bar LowestHighBar(DateTime dateTime1, DateTime dateTime2) => LowestHighBar(GetIndex(dateTime1, IndexOption.Next), GetIndex(dateTime2, IndexOption.Prev));

        public double LowestHigh(int nBars) => LowestHighBar(nBars).High;

        public double LowestHigh(int index1, int index2) => LowestHighBar(index1, index2).High;

        public double LowestHigh(DateTime dateTime1, DateTime dateTime2) => LowestHighBar(dateTime1, dateTime2).High;
    
        public int GetIndex(DateTime datetime, IndexOption option = IndexOption.Null)
        {
            if (datetime < FirstDateTime)
                return option == IndexOption.Null || option == IndexOption.Prev ? -1 : 0;
            if (datetime > LastDateTime)
                return option == IndexOption.Null || option == IndexOption.Next ? -1 : Count - 1;

            var i = this.bars.BinarySearch(new Bar() { DateTime = datetime }, new DataObjectComparer());
            if (i >= 0)
                return i;
            else if (option == IndexOption.Next)
                return ~i;
            else if (option == IndexOption.Prev)
                return ~i - 1;
            else
                return -1;
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

        void IDataSeries.Remove(long index) => this.bars.RemoveAt((int)index);

        public IEnumerator<Bar> GetEnumerator() => this.bars.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [NotOriginal]
        private bool IsIndex1AndIndex2Valid(int index1, int index2) => index1 >= 0 && index2 >= 0 && index1 <= index2 && index1 <= Count - 1 && index2 <= Count - 1;

        [NotOriginal]
        private bool IsEmpty() => Count == 0;
    }
}