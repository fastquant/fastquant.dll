// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace SmartQuant
{
    public class PortfolioStatisticsCategory
    {
        public const string Summary = "Summary";
        public const string Trades = "Trades";
        public const string DailyAnnual = "Daily / Annual returns";
        public const string UpsideDownside = "Upside / Downside returns";
    }

    public class PortfolioStatisticsType
    {
        public const int NetProfit = 1;
        public const int GrossProfit = 2;
        public const int GrossLoss = 3;
        public const int Drawdown = 4;
        public const int DrawdownPercent = 5;
        public const int AvgDrawdown = 6;
        public const int AvgDrawdownPercent = 7;
        public const int MaxDrawdown = 8;
        public const int MaxDrawdownPercent = 9;
        public const int ProfitFactor = 10;
        public const int RecoveryFactor = 11;

        public const int NumOfTrades = 21;
        public const int NumOfWinTrades = 22;
        public const int NumOfLossTrades = 23;
        public const int ProfitablePercent = 24;
        public const int TradesPnL = 25;
        public const int WinTradesPnL = 26;
        public const int LossTradesPnL = 27;
        public const int AvgTrade = 28;
        public const int AvgWinTrade = 29;
        public const int AvgLossTrade = 30;
        public const int PayoffRatio = 31;
        public const int MaxAdverseExcursion = 32;
        public const int MaxFavorableExcursion = 33;
        public const int EndOfTradeDrawdown = 34;
        public const int AvgMaxAdverseExcursion = 35;
        public const int AvgMaxFavorableExcursion = 36;
        public const int AvgEndOfTradeDrawdown = 37;
        public const int ConsecutiveWinTrades = 38;
        public const int ConsecutiveLossTrades = 39;
        public const int MaxConsecutiveWinTrades = 40;
        public const int MaxConsecutiveLossTrades = 41;
        public const int TradesDuration = 42;
        public const int AvgTradesDuration = 43;

        public const int AnnualReturn = 63;
        public const int DailyReturnPercent = 64;
        public const int AnnualReturnPercent = 65;
        public const int AvgDailyReturnPercent = 66;
        public const int AvgAnnualReturnPercent = 67;
        public const int DailyReturnPercentStdDev = 68;
        public const int AnnualReturnPercentStdDev = 69;
        public const int DailyDownsideRisk = 70;
        public const int AnnualDownsideRisk = 71;
        public const int SharpeRatio = 72;
        public const int SortinoRatio = 73;
        public const int CompoundAnnualGrowthRate = 74;
        public const int MARRatio = 75;
        public const int ValueAtRisk = 76;
    }

    public class PortfolioStatisticsItem
    {
        protected internal double totalValue;
        protected internal double longValue;
        protected internal double shortValue;

        protected internal TimeSeries totalValues;
        protected internal TimeSeries longValues;
        protected internal TimeSeries shortValues;

        protected internal Portfolio portfolio;
        protected internal PortfolioStatistics statistics;

        public virtual int Type => 0;

        public virtual string Name => "Unknown";

        public virtual string Format => "F2";

        public virtual string Description => "Unknown";

        public virtual string Category => "Unknown";

        public virtual bool Show => true;

        public double TotalValue => this.totalValue;

        public double LongValue => this.longValue;

        public double ShortValue => this.shortValue;

        public TimeSeries TotalValues => this.totalValues;

        public TimeSeries LongValues => this.longValues;

        public TimeSeries ShortValues => this.shortValues;

        public Clock Clock => this.portfolio.framework.Clock;

        public int EmitId { get; private set; }

        public PortfolioStatisticsItem()
        {
            this.totalValues = new TimeSeries(Name, "");
            this.longValues = new TimeSeries($"{Name} Long", "");
            this.shortValues = new TimeSeries($"{Name} Short", "");
        }

        public void Subscribe(int itemType) => this.statistics.Subscribe(this, itemType);

        public void Unsubscribe(int itemType) => this.statistics.Unsubscribe(this, itemType);

        protected internal void Emit()
        {
            if (this.statistics != null)
            {
                EmitId++;
                this.statistics.OnStatistics(this);
                if (this.portfolio.Parent != null)
                    this.statistics.OnStatistics(this.portfolio, this);
            }
        }

        protected internal virtual void OnInit()
        {
            // noop
        }

        protected internal virtual void OnEquity(double equity)
        {
            // noop
        }

        protected internal virtual void OnFill(Fill fill)
        {
            // noop
        }

        protected internal virtual void OnTransaction(Transaction transaction)
        {
            // noop
        }

        protected internal virtual void OnPositionOpened(Position position)
        {
            // noop
        }

        protected internal virtual void OnPositionClosed(Position position)
        {
            // noop
        }

        protected internal virtual void OnPositionChanged(Position position)
        {
            // noop
        }

        protected internal virtual void OnPositionSideChanged(Position position)
        {
            // noop
        }

        protected internal virtual void OnRoundTrip(TradeInfo trade)
        {
            // noop
        }

        protected internal virtual void OnStatistics(PortfolioStatisticsItem statistics)
        {
            // noop
        }

        protected internal virtual void OnStatistics(Portfolio portfolio, PortfolioStatisticsItem statistics)
        {
            // noop
        }

        protected internal virtual void OnClear()
        {
            // noop
        }
    }

    public class PortfolioStatisticsItemList : IEnumerable<PortfolioStatisticsItem>
    {
        private readonly GetByList<PortfolioStatisticsItem> items = new GetByList<PortfolioStatisticsItem>("Type", "Name");

        public int Count => this.items.Count;

        public PortfolioStatisticsItem this[int index] => this.items.GetByIndex(index);

        public bool Contains(int type) => this.items.Contains(type);

        public void Add(PortfolioStatisticsItem item)
        {
            if (Contains(item.Type))
                Remove(item.Type);
            this.items.Add(item);
        }

        public void Remove(int type) => this.items.Remove(type);

        public PortfolioStatisticsItem GetByType(int type) => this.items.GetById(type);

        public PortfolioStatisticsItem GetByIndex(int index) => this.items.GetByIndex(index);

        public void Clear() => this.items.Clear();

        public IEnumerator<PortfolioStatisticsItem> GetEnumerator() => this.items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class PortfolioStatistics
    {
        private Portfolio portfolio;

        internal IdArray<TradeDetector> detectors = new IdArray<TradeDetector>(10240);

        internal IdArray<List<int>> subscriptions = new IdArray<List<int>>();

        public PortfolioStatisticsItemList Items { get; } = new PortfolioStatisticsItemList();

        public PortfolioStatistics(Portfolio portfolio)
        {
            this.portfolio = portfolio;
            var list = portfolio.framework.StatisticsManager.CloneAll();
            foreach (var item in list)
                Add(item);
        }

        public void Add(PortfolioStatisticsItem item)
        {
            if (item.statistics == null)
            {
                item.statistics = this;
                item.portfolio = this.portfolio;
                Items.Add(item);
                item.OnInit();
            }
            else
                Console.WriteLine($"PortfolioStatistics::Add Error. Item already belongs to other statistics {item}");
        }

        public PortfolioStatisticsItem Get(int type) => Items.GetByType(type);

        internal void Subscribe(PortfolioStatisticsItem item, int type)
        {
            if (Items.GetByType(type) == null)
                Add(this.portfolio.framework.StatisticsManager.Clone(type));
            var subscription = this.subscriptions[type] = this.subscriptions[type] ?? new List<int>();
            if (subscription.Contains(item.Type))
                Console.WriteLine($"PortfolioStatistics::Subscribe Item {item.Type} is already subscribed for item {type}");
            else
                subscription.Add(item.Type);
        }

        internal void Unsubscribe(PortfolioStatisticsItem item, int type)
        {
            if (this.subscriptions[type] != null && this.subscriptions[type].Contains(item.Type))
                this.subscriptions[type].Remove(item.Type);
            else
                Console.WriteLine($"PortfolioStatistics::Unsubscribe Item {item.Type} is not subscribed for item {type}");
        }

        internal void DetectTrade(Fill fill)
        {
            var id = fill.Instrument.Id;
            if (this.detectors[id] == null)
            {
                var detector = new TradeDetector(TradeDetectionType.FIFO, this.portfolio);
                detector.TradeDetected += (sender, e) =>
                {
                    var info = e.TradeInfo;
                    foreach (var item in Items)
                        item.OnRoundTrip(info);
                };
                this.detectors[id] = detector;
            }
            this.detectors[id].Add(fill);
        }

        internal void OnFill(Fill fill)
        {
            DetectTrade(fill);
            foreach (var item in Items)
                item.OnFill(fill);
        }

        internal void OnTransaction(Transaction transaction)
        {
            foreach (var item in Items)
                item.OnTransaction(transaction);
        }

        internal void OnPositionOpened(Position position)
        {
            foreach (var item in Items)
                item.OnPositionOpened(position);
        }

        internal void OnPositionClosed(Position position)
        {
            foreach (var item in Items)
                item.OnPositionClosed(position);
        }

        internal void OnPositionChanged(Position position)
        {
            foreach (var item in Items)
                item.OnPositionChanged(position);
        }

        internal void OnPositionSideChanged(Position position)
        {
            foreach (var item in Items)
                item.OnPositionSideChanged(position);
        }

        internal void OnStatistics(PortfolioStatisticsItem item)
        {
            var subscription = this.subscriptions[item.Type];
            if (subscription != null)
                foreach (var type in subscription)
                    Items.GetByType(type).OnStatistics(item);
        }

        internal void OnStatistics(Portfolio portfolio, PortfolioStatisticsItem item)
        {
            foreach (var i in Items.Where(i => i != item))
                i.OnStatistics(portfolio, item);
        }

        internal void OnClear()
        {
            foreach (var item in Items)
                item.OnClear();
        }

        internal void OnEquity(double value)
        {
            for (var i = 0; i < this.detectors.Size; i++)
                this.detectors[i]?.OnEquity(value);
     
            foreach (var item in Items)
                item.OnEquity(value);
        }
    }
}
