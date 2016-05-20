using System;
using System.Collections;
using System.Collections.Generic;

namespace FastQuant.Data.Compression
{
    public abstract class DataEntryEnumerator : IEnumerator<DataEntry>
    {
        protected DataEntryEnumerator(int count)
        {
            this.count = count;
            Reset();
        }

        public void Dispose()
        {
            // noop
        }

        public bool MoveNext() => ++this.index < this.count;

        public void Reset() => this.index = -1;

        public abstract DataEntry Current { get; }

        object IEnumerator.Current { get { throw new NotImplementedException(); } }

        protected int index;

        private int count;
    }

    public class TickDataEnumerator : DataEntryEnumerator
    {
        public TickDataEnumerator(TickSeries series) : base(series.Count)
        {
            this.series = series;
        }

        public override DataEntry Current
        {
            get
            {
                var tick = this.series[this.index];
                return new DataEntry(tick.DateTime, new[] { new PriceSizeItem(tick.Price, tick.Size) });
            }
        }

        private TickSeries series;
    }

    public class QuoteDataEnumerator : DataEntryEnumerator
    {
        public QuoteDataEnumerator(QuoteSeries series, InputType inputType): base(series.Count)
        {
            this.series = series;
            this.inputType = inputType;
            this.int_1 = inputType == InputType.BidAsk ? 2 : 1;
        }

        public override DataEntry Current
        {
            get
            {
                var quote = this.series[this.index];
                var datetime = quote.Ask.DateTime.Ticks > quote.Bid.DateTime.Ticks ? quote.Ask.DateTime : quote.Bid.DateTime;
                var array = new PriceSizeItem[this.int_1];
                switch (this.inputType)
                {
                    case InputType.Bid:
                        datetime = quote.Bid.dateTime;
                        array[0] = new PriceSizeItem(quote.Bid.Price, quote.Bid.Size);
                        break;
                    case InputType.Ask:
                        datetime = quote.Ask.dateTime;
                        array[0] = new PriceSizeItem(quote.Ask.Price, quote.Ask.Size);
                        break;
                    case InputType.BidAsk:
                        array[0] = new PriceSizeItem(quote.Bid.Price, quote.Bid.Size);
                        array[1] = new PriceSizeItem(quote.Ask.Price, quote.Ask.Size);
                        break;
                    case InputType.Middle:
                        array[0] = new PriceSizeItem((quote.Bid.Price + quote.Ask.Price) / 2.0, (quote.Bid.Size + quote.Ask.Size) / 2);
                        break;
                    case InputType.Spread:
                        array[0] = new PriceSizeItem(quote.Ask.Price - quote.Bid.Price, 0);
                        break;
                }
                return new DataEntry(datetime, array);
            }
        }

        private InputType inputType;

        private int int_1;

        private QuoteSeries series;
    }

    public class BarDataEnumerator : DataEntryEnumerator
    {
        public BarDataEnumerator(BarSeries series): base(series.Count)
        {
            this.series = series;
        }

        public override DataEntry Current
        {
            get
            {
                var bar = this.series[this.index];
                return new DataEntry(bar.OpenDateTime, new[]
                {
                    new PriceSizeItem(bar.Open, (int)bar.Volume),
                    new PriceSizeItem(bar.High, 0),
                    new PriceSizeItem(bar.Low, 0),
                    new PriceSizeItem(bar.Close, 0)
                });
            }
        }

        private BarSeries series;
    }
}