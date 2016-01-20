using System;
using System.Collections.Generic;
using SmartQuant.Providers;
using System.Linq;

namespace SmartQuant
{
    public class OrderBookAggr
    {
        public enum AggregationMode
        {
            TotalValue,
            MaxValue
        }

        private readonly object lck = new object();
        private readonly Dictionary<int, Tuple<DateTime, List<Tick>, List<Tick>>> GonEpGhAb = new Dictionary<int, Tuple<DateTime, List<Tick>, List<Tick>>>();
        private bool bool_0;
        private DateTime dateTime_0;
        private readonly int instrumentId;
        private List<Tick> bids;
        private List<Tick> asks;

        public AggregationMode Mode { get; set; } = AggregationMode.TotalValue;

        public TimeSpan Timeout { get; }= TimeSpan.FromSeconds(15);

        public OrderBookAggr(int instrumentId)
        {
            this.instrumentId = instrumentId;
        }

        public int GetAskVolume()
        {
            lock (this.lck)
            {
                if (!this.bool_0)
                    this.method_3();
                return GetTickVolume(this.asks);
            }
        }

        public double GetAvgAskPrice()
        {
            lock (this.lck)
            {
                if (!this.bool_0)
                    this.method_3();
                return GetAvgTickPrice(this.asks);
            }
        }

        public double GetAvgBidPrice()
        {
            lock (this.lck)
            {
                if (!this.bool_0)
                    this.method_3();
                return GetAvgTickPrice(this.bids);
            }
        }

        public int GetBidVolume()
        {
            lock (this.lck)
            {
                if (!this.bool_0)
                    this.method_3();
                return GetTickVolume(this.bids);
            }
        }

        public Level2Snapshot GetLevel2Snapshot()
        {
            lock (this.lck)
            {
                if (!this.bool_0)
                    this.method_3();

                var bids = new List<Bid>();
                var asks = new List<Ask>();
                foreach (var tick in this.bids)
                    if (tick is Bid)
                        bids.Add(tick as Bid);

                foreach (var tick in this.asks)
                    if (tick is Ask)
                        asks.Add(tick as Ask);

                return new Level2Snapshot(this.dateTime_0, 0, this.instrumentId, bids.ToArray(), asks.ToArray());
            }
        }

        public Quote GetQuote(int level)
        {
            lock (this.lck)
            {
                if (!this.bool_0)
                    this.method_3();

                var bid = new Bid();
                var ask = new Ask();
                if (this.bids.Count > level)
                {
                    var tick = this.bids[level];
                    bid = new Bid(tick.dateTime, tick.ProviderId, tick.InstrumentId, tick.Price, tick.Size);
                }
                if (this.asks.Count > level)
                {
                    var tick = this.asks[level];
                    ask = new Ask(tick.dateTime, tick.ProviderId, tick.InstrumentId, tick.Price, tick.Size);
                }
                return new Quote(bid, ask);
            }
        }

        private int GetTickVolume(IEnumerable<Tick> ticks) => ticks.Sum(t => t.Size);

        internal void method_0(Level2Snapshot l2s)
        {
            lock (this.lck)
            {
                this.bids.Clear();
                this.asks.Clear();
                this.bool_0 = false;
                this.GonEpGhAb[l2s.ProviderId] = new Tuple<DateTime, List<Tick>, List<Tick>>(l2s.DateTime, new List<Tick>(l2s.Bids), new List<Tick>(l2s.Asks));
            }
        }

        internal void method_1(Level2Update l2u)
        {
            //lock (this.lck)
            //{
            //    this.bool_0 = false;
            //    Tuple<DateTime, List<Tick>, List<Tick>> tuple;
            //    if (this.GonEpGhAb.TryGetValue(l2u.ProviderId, out tuple))
            //    {
            //        tuple = new Tuple<DateTime, List<Tick>, List<Tick>>(l2u.DateTime, tuple.Item2, tuple.Item3);
            //    }
            //    else
            //    {
            //        tuple = new Tuple<DateTime, List<Tick>, List<Tick>>(l2u.DateTime, new List<Tick>(), new List<Tick>());
            //    }
            //    this.GonEpGhAb[l2u.ProviderId] = tuple;
            //    Level2[] level2_ = l2u.Entries;
            //    int i = 0;
            //    while (i < level2_.Length)
            //    {
            //        Level2 level = level2_[i];
            //        switch (level.Side)
            //        {
            //            case Level2Side.Bid:
            //                {
            //                    List<Tick> list = tuple.Item2;
            //                    goto IL_B2;
            //                }
            //            case Level2Side.Ask:
            //                {
            //                    List<Tick> list = tuple.Item3;
            //                    goto IL_B2;
            //                }
            //        }
            //        IL_1B4:
            //        i++;
            //        continue;
            //        IL_B2:
            //        switch (level.Action)
            //        {
            //            case Level2UpdateAction.New:
            //                {
            //                    List<Tick> list;
            //                    list.Insert(level.Position, new Tick(level));
            //                    goto IL_1B4;
            //                }
            //            case Level2UpdateAction.Change:
            //                {
            //                    List<Tick> list;
            //                    list[level.Position].size = level.Size;
            //                    goto IL_1B4;
            //                }
            //            case Level2UpdateAction.Delete:
            //                {
            //                    List<Tick> list;
            //                    if (level.Position >= list.Count)
            //                    {
            //                        Console.WriteLine($"OrderBook:: {level.Side} Delete warning at index:  {level.Position}, max index: {list.Count - 1}, InstrumentId: {level.InstrumentId}");
                                
            //                        list.RemoveAt(list.Count - 1);
            //                        goto IL_1B4;
            //                    }
            //                    list.RemoveAt(level.Position);
            //                    goto IL_1B4;
            //                }
            //            case Level2UpdateAction.Reset:
            //                {
            //                    List<Tick> list;
            //                    list.Clear();
            //                    goto IL_1B4;
            //                }
            //            default:
            //                goto IL_1B4;
            //        }
            //    }
            //}
        }

        private double GetAvgTickPrice(IEnumerable<Tick> ticks)
        {
            double val = 0;
            double total = 0;
            foreach (var t in ticks)
            {
                val += t.Price * t.Size;
                total += t.Size;
            }
            return val / total;
        }

        private void method_3()
        {
            SortedList<double, Tick> sortedList = new SortedList<double, Tick>(new PriceComparer(PriceSortOrder.Descending));
            SortedList<double, Tick> sortedList2 = new SortedList<double, Tick>(new PriceComparer(PriceSortOrder.Ascending));
            this.dateTime_0 = default(DateTime);
            foreach (Tuple<DateTime, List<Tick>, List<Tick>> current in this.GonEpGhAb.Values)
            {
                if (current.Item1 > this.dateTime_0)
                {
                    this.dateTime_0 = current.Item1;
                }
            }
            List<Tuple<DateTime, List<Tick>, List<Tick>>> list = new List<Tuple<DateTime, List<Tick>, List<Tick>>>();
            foreach (Tuple<DateTime, List<Tick>, List<Tick>> current2 in this.GonEpGhAb.Values)
            {
                if (current2.Item1 + this.Timeout >= this.dateTime_0)
                {
                    list.Add(current2);
                }
            }
            foreach (Tuple<DateTime, List<Tick>, List<Tick>> current3 in list)
            {
                foreach (Tick current4 in current3.Item2)
                {
                    if (!sortedList.ContainsKey(current4.Price))
                    {
                        Tick tick = new Tick(current4);
                        if (Mode == AggregationMode.TotalValue)
                        {
                            tick.ProviderId = 0;
                        }
                        sortedList.Add(current4.Price, tick);
                    }
                    else if (Mode == AggregationMode.TotalValue)
                    {
                        sortedList[current4.Price].Size += current4.Size;
                    }
                    else if (Mode == AggregationMode.MaxValue && current4.Size > sortedList[current4.Price].Size)
                    {
                        sortedList[current4.Price] = new Tick(current4);
                    }
                }
                foreach (Tick current5 in current3.Item3)
                {
                    if (!sortedList2.ContainsKey(current5.Price))
                    {
                        Tick tick2 = new Tick(current5);
                        if (this.Mode == AggregationMode.TotalValue)
                        {
                            tick2.ProviderId = 0;
                        }
                        sortedList2.Add(current5.Price, tick2);
                    }
                    else if (this.Mode == AggregationMode.TotalValue)
                    {
                        sortedList2[current5.Price].Size += current5.Size;
                    }
                    else if (this.Mode == AggregationMode.MaxValue && current5.Size > sortedList2[current5.Price].Size)
                    {
                        sortedList2[current5.Price] = new Tick(current5);
                    }
                }
            }
            this.bids = new List<Tick>(sortedList.Values);
            this.asks = new List<Tick>(sortedList2.Values);
            this.bool_0 = true;
        }

        public List<Tick> Asks
        {
            get
            {
                lock (this.lck)
                {
                    if (!this.bool_0)
                        this.method_3();
                    return new List<Tick>(this.asks);
                }
            }
        }

        public List<Tick> Bids
        {
            get
            {
                lock (this.lck)
                {
                    if (!this.bool_0)
                        this.method_3();
                    return new List<Tick>(this.bids);
                }
            }
        }
    }
}
