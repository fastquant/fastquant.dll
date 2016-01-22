namespace SmartQuant.Statistics
{
    public class NumOfTrades : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                LongValues.Add(Clock.DateTime, this.longValue += 1);
            }
            else
            {
                ShortValues.Add(Clock.DateTime, this.shortValue += 1);
            }
            TotalValues.Add(Clock.DateTime, this.totalValue += 1);
            Emit();
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Number of Trades";

        public override int Type => PortfolioStatisticsType.NumOfTrades;
    }

    public class NumOfLossTrades : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (!trade.IsWinning)
            {
                if (trade.IsLong)
                    LongValues.Add(Clock.DateTime, this.longValue += 1);
                else
                    ShortValues.Add(Clock.DateTime, this.shortValue += 1);
                TotalValues.Add(Clock.DateTime, this.totalValue += 1);
                Emit();
            }
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Number of Losing Trades";

        public override int Type => PortfolioStatisticsType.NumOfLossTrades;
    }

    public class NumOfWinTrades : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsWinning)
            {
                if (trade.IsLong)
                    LongValues.Add(Clock.DateTime, this.longValue += 1.0);
                else
                    ShortValues.Add(Clock.DateTime, this.shortValue += 1.0);
                TotalValues.Add(Clock.DateTime, this.totalValue += 1.0);
                Emit();
            }
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Number of Winning Trades";

        public override int Type => PortfolioStatisticsType.NumOfWinTrades;
    }

    public class ConsecutiveLossTrades : PortfolioStatisticsItem
    {
        protected double longLossTrades;

        protected double longWinTrades;

        protected double shortLossTrades;

        protected double shortWinTrades;

        protected double totalLossTrades;

        protected double totalWinTrades;

        protected internal override void OnInit()
        {
            Subscribe(PortfolioStatisticsType.NumOfWinTrades);
            Subscribe(PortfolioStatisticsType.NumOfLossTrades);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.NumOfLossTrades)
            {
                bool changed = false;
                if (statistics.LongValue > this.longLossTrades)
                {
                    this.longLossTrades = statistics.LongValue;
                    LongValues.Add(Clock.DateTime, this.longValue += 1);
                    changed = true;
                }
                if (statistics.ShortValue > this.shortLossTrades)
                {
                    this.shortLossTrades = statistics.ShortValue;
                    ShortValues.Add(Clock.DateTime, this.shortValue += 1.0);
                    changed = true;
                }
                if (statistics.TotalValue > this.totalLossTrades)
                {
                    this.totalLossTrades = statistics.TotalValue;
                    TotalValues.Add(Clock.DateTime, this.totalValue += 1.0);
                    changed = true;
                }
                if (changed)
                {
                    Emit();
                }
            }
            if (statistics.Type == PortfolioStatisticsType.NumOfWinTrades)
            {
                bool changed = false;
                if (statistics.LongValue > this.longWinTrades)
                {
                    this.longWinTrades = statistics.LongValue;
                    this.longValue = 0;
                    LongValues.Add(Clock.DateTime, 0);
                    changed = true;
                }
                if (statistics.ShortValue > this.shortWinTrades)
                {
                    this.shortWinTrades = statistics.ShortValue;
                    this.shortValue = 0;
                    ShortValues.Add(Clock.DateTime, 0);
                    changed = true;
                }
                if (statistics.TotalValue > this.totalWinTrades)
                {
                    this.totalWinTrades = statistics.TotalValue;
                    this.totalValue = 0;
                    TotalValues.Add(Clock.DateTime, 0);
                    changed = true;
                }
                if (changed)
                {
                    Emit();
                }
            }
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Consecutive Losing Trades";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.ConsecutiveLossTrades;
    }

    public class ConsecutiveWinTrades : PortfolioStatisticsItem
    {
        protected double longLossTrades;

        protected double longWinTrades;

        protected double shortLossTrades;

        protected double shortWinTrades;

        protected double totalLossTrades;

        protected double totalWinTrades;

        protected internal override void OnInit()
        {
            Subscribe(PortfolioStatisticsType.NumOfWinTrades);
            Subscribe(PortfolioStatisticsType.NumOfLossTrades);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.NumOfWinTrades)
            {
                bool changed = false;
                if (statistics.LongValue > this.longWinTrades)
                {
                    this.longWinTrades = statistics.LongValue;
                    LongValues.Add(Clock.DateTime, this.longValue += 1.0);
                    changed = true;
                }
                if (statistics.ShortValue > this.shortWinTrades)
                {
                    this.shortWinTrades = statistics.ShortValue;
                    ShortValues.Add(Clock.DateTime, this.shortValue += 1.0);
                    changed = true;
                }
                if (statistics.TotalValue > this.totalWinTrades)
                {
                    this.totalWinTrades = statistics.TotalValue;
                    TotalValues.Add(Clock.DateTime, this.totalValue += 1.0);
                    changed = true;
                }
                if (changed)
                {
                    Emit();
                }
            }
            if (statistics.Type == PortfolioStatisticsType.NumOfLossTrades)
            {
                bool changed = false;
                if (statistics.LongValue > this.longLossTrades)
                {
                    this.longLossTrades = statistics.LongValue;
                    this.longValue = 0;
                    LongValues.Add(Clock.DateTime, 0);
                    changed = true;
                }
                if (statistics.ShortValue > this.shortLossTrades)
                {
                    this.shortLossTrades = statistics.ShortValue;
                    this.shortValue = 0;
                    ShortValues.Add(Clock.DateTime, 0);
                    changed = true;
                }
                if (statistics.TotalValue > this.totalLossTrades)
                {
                    this.totalLossTrades = statistics.TotalValue;
                    this.totalValue = 0;
                    TotalValues.Add(Clock.DateTime, 0);
                    changed = true;
                }
                if (changed)
                {
                    Emit();
                }
            }
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Consecutive Winning Trades";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.ConsecutiveWinTrades;
    }
}
