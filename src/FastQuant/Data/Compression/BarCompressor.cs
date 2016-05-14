using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastQuant.Data.Compression
{
    public class CompressedBarEventArgs : EventArgs
    {
        public Bar Bar { get; }

        public CompressedBarEventArgs(Bar bar)
        {
            Bar = bar;
        }
    }

    public enum InputType
    {
        Trade,
        Bid,
        Ask,
        BidAsk,
        Middle,
        Spread,
        Bar
    }

    public class PriceSizeItem
    {
        public double Price { get; }

        public int Size { get; }

        public PriceSizeItem(double price, int size)
        {
            Price = price;
            Size = size;
        }
    }


    public class DataEntry
    {
        public DateTime DateTime { get; }

        public PriceSizeItem[] Items { get; }

        public DataEntry(DateTime datetime, PriceSizeItem[] items)
        {
            DateTime = datetime;
            Items = items;
        }
    }

    public abstract class BarCompressor
    {
        protected BarCompressor()
        {
            this.bar = null;
        }

        public abstract void Add(DataEntry entry);

        protected void AddItemsToBar(PriceSizeItem[] items)
        {
            foreach (var item in items)
                this.method_0(item);
        }

        public BarSeries Compress(DataEntryEnumerator enumerator)
        {
            var series = new BarSeries();
            NewCompressedBar += (sender, e) => series.Add(e.Bar);
            while (enumerator.MoveNext())
                Add(enumerator.Current);

            this.method_1();
            return series;
        }

        protected void CreateNewBar(BarType barType, DateTime beginTime, DateTime endTime, double price)
        {
            this.bar = new Bar(beginTime, endTime, this.instrumentId, barType, this.newBarSize, price, price, price, price, 0L, 0L);
        }

        protected void EmitNewCompressedBar()
        {
            NewCompressedBar?.Invoke(this, new CompressedBarEventArgs(this.bar));
        }

        public static BarCompressor GetCompressor(int instrumentId, BarType barType, long oldBarSize, long newBarSize, TimeSpan time1 = default(TimeSpan), TimeSpan time2 = default(TimeSpan))
        {
            BarCompressor barCompressor;
            switch (barType)
            {
                case BarType.Time:
                    barCompressor = new TimeBarCompressor();
                    break;
                case BarType.Tick:
                    barCompressor = new TickBarCompressor();
                    break;
                case BarType.Volume:
                    barCompressor = new VolumeBarCompressor();
                    break;
                case BarType.Session:
                    barCompressor = new SessionBarCompressor(time1, time2);
                    break;
                default:
                    throw new ArgumentException($"Unknown bar type - {barType}");
            }

            barCompressor.instrumentId = instrumentId;
            barCompressor.oldBarSize = oldBarSize;
            barCompressor.newBarSize = newBarSize;
            return barCompressor;
        }

        private void method_0(PriceSizeItem item)
        {
            if (item.Price < this.bar.Low)
                this.bar.Low = item.Price;
            if (item.Price > this.bar.High)
                this.bar.High = item.Price;
            this.bar.Close = item.Price;
            this.bar.Volume += item.Size;
        }

        private void method_1()
        {
            if (this.bar != null)
                EmitNewCompressedBar();
        }

        public event EventHandler<CompressedBarEventArgs> NewCompressedBar;

        protected Bar bar;

        protected int instrumentId;

        protected long newBarSize;

        protected long oldBarSize;

        private BarSeries series;
    }

    internal class TimeBarCompressor : BarCompressor
    {
        public override void Add(DataEntry entry)
        {
            if (this.bar == null || this.bar.DateTime <= entry.DateTime)
            {
                if (this.bar != null)
                    EmitNewCompressedBar();

                var dateTime = this.method_2(entry.DateTime);
                var endTime = this.method_3(dateTime);
                CreateNewBar(BarType.Time, dateTime, endTime, entry.Items[0].Price);
            }
            AddItemsToBar(entry.Items);
        }

        private DateTime method_2(DateTime dateTime_0)
        {
            if (this.newBarSize == 60 * 60 * 168)
            {
                var num = dateTime_0.DayOfWeek >= DayOfWeek.Sunday ? dateTime_0.DayOfWeek - DayOfWeek.Monday : 6;
                return dateTime_0.Date.AddDays(-num);
            }
            if (this.newBarSize == 60 * 60 * 72)
            {
                return dateTime_0.Date.AddDays(-(double)dateTime_0.Day + 1);
            }
            var num2 = (long)dateTime_0.TimeOfDay.TotalSeconds / this.newBarSize * this.newBarSize;
            return dateTime_0.Date.AddSeconds(num2);
        }

        private DateTime method_3(DateTime dateTime_0)
        {
            return this.newBarSize == 60 * 60 * 72 ? dateTime_0.Date.AddMonths(1) : dateTime_0.AddSeconds(this.newBarSize);
        }
    }

    internal class TickBarCompressor : BarCompressor
    {
        public TickBarCompressor()
        {
            this.long_0 = 0;
        }

        public override void Add(DataEntry entry)
        {
            if (this.bar == null)
                CreateNewBar(BarType.Tick, entry.DateTime, entry.DateTime, entry.Items[0].Price);
            AddItemsToBar(entry.Items);

            this.bar.DateTime = entry.DateTime;
            this.long_0 += this.oldBarSize;
            if (this.long_0 == this.newBarSize)
            {
                EmitNewCompressedBar();
                this.bar = null;
                this.long_0 = 0;
            }
        }

        private long long_0;

    }

    internal class VolumeBarCompressor : BarCompressor
    {
        public override void Add(DataEntry entry)
        {
            if (this.bar == null)
                CreateNewBar(BarType.Volume, entry.DateTime, entry.DateTime, entry.Items[0].Price);

            AddItemsToBar(entry.Items);
            this.bar.DateTime = entry.DateTime;

            if (this.bar.Volume >= this.newBarSize)
            {
                EmitNewCompressedBar();
                this.bar = null;
            }
        }
    }

    internal class SessionBarCompressor : BarCompressor
    {
        public SessionBarCompressor(TimeSpan time1, TimeSpan time2)
        {
            this.time1 = time1;
            this.time2 = time2;
        }

        public override void Add(DataEntry entry)
        {
            if (this.time1 <= entry.DateTime.TimeOfDay && entry.DateTime.TimeOfDay < this.time2)
            {
                if (this.bar == null || this.bar.dateTime.Date != entry.DateTime.Date)
                {
                    if (this.bar != null)
                        EmitNewCompressedBar();
                    CreateNewBar(BarType.Session, entry.DateTime.Date.Add(this.time1), entry.DateTime.Date.Add(this.time2), entry.Items[0].Price);
                }
                AddItemsToBar(entry.Items);
            }
        }

        private TimeSpan time1;

        private TimeSpan time2;
    }
}
