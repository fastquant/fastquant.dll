using static System.Math;

namespace SmartQuant.Statistics
{
    public abstract class Average : PortfolioStatisticsItem
    {
        protected int type;

        public Average(int type)
        {
            this.type = type;
        }

        protected override void OnInit()
        {
            Subscribe(this.type);
        }

        protected override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == this.type)
            {
                bool changed = false;
                if (statistics.LongValues.Count > this.longValues.Count)
                {
                    this.longValue = (this.longValue * this.longValues.Count + statistics.LongValue) / (this.longValues.Count + 1);
                    this.longValues.Add(Clock.DateTime, this.longValue);
                    changed = true;
                }
                if (statistics.ShortValues.Count > this.shortValues.Count)
                {
                    this.shortValue = (this.shortValue * this.shortValues.Count + statistics.ShortValue) / (this.shortValues.Count + 1);
                    this.shortValues.Add(Clock.DateTime, this.shortValue);
                    changed = true;
                }
                if (statistics.TotalValues.Count > this.totalValues.Count)
                {
                    this.totalValue = (this.totalValue * this.totalValues.Count + statistics.TotalValue) / (this.totalValues.Count + 1);
                    this.totalValues.Add(base.Clock.DateTime, this.totalValue);
                    changed = true;
                }
                if (changed)
                {
                    Emit();
                }
            }
        }
    }

    public class AvgDrawdown : Average
    {
        public AvgDrawdown() : base(PortfolioStatisticsType.Drawdown)
        {
        }

        public override string Category => "Summary";

        public override string Name => "Average Drawdown";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.AvgDrawdown;
    }

    public class AvgDailyReturnPercent : Average
    {
        public AvgDailyReturnPercent() : base(PortfolioStatisticsType.DailyReturnPercent)
        {
        }

        public override string Category => "Daily / Annual returns";

        public override string Format => "P2";

        public override string Name => "Average Daily Return %";

        public override int Type => PortfolioStatisticsType.AvgDailyReturnPercent;
    }

    public class AvgDrawdownPercent : Average
    {
        public AvgDrawdownPercent() : base(PortfolioStatisticsType.DrawdownPercent)
        {
        }

        public override string Category => "Summary";

        public override string Format => "P2";

        public override string Name => "Average Drawdown %";

        public override int Type => PortfolioStatisticsType.AvgDrawdownPercent;
    }

    public class AvgLossTrade : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average Losing Trade";

        public override int Type => PortfolioStatisticsType.AvgLossTrade;

        public AvgLossTrade() : base(PortfolioStatisticsType.LossTradesPnL)
        {
        }
    }

    public class AvgMaxAdverseExcursion : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average Maximum Adverse Excursion";

        public override int Type => PortfolioStatisticsType.AvgMaxAdverseExcursion;

        public AvgMaxAdverseExcursion() : base(PortfolioStatisticsType.MaxAdverseExcursion)
        {
        }
    }

    public class AvgMaxFavorableExcursion : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average Maximum Favorable Excursion";

        public override int Type => PortfolioStatisticsType.AvgMaxFavorableExcursion;

        public AvgMaxFavorableExcursion() : base(PortfolioStatisticsType.MaxFavorableExcursion)
        {
        }
    }

    public class AvgTrade : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average Trade";

        public override int Type => PortfolioStatisticsType.AvgTrade;

        public AvgTrade() : base(PortfolioStatisticsType.TradesPnL)
        {
        }
    }

    public class AvgTradesDuration : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average Trades Duration";

        public override int Type => PortfolioStatisticsType.AvgTradesDuration;

        public AvgTradesDuration() : base(PortfolioStatisticsType.TradesDuration)
        {
        }
    }

    public class AvgWinTrade : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average Winning Trade";

        public override int Type => PortfolioStatisticsType.AvgWinTrade;

        public AvgWinTrade() : base(PortfolioStatisticsType.WinTradesPnL)
        {
        }
    }

    public class AvgEndOfTradeDrawdown : Average
    {
        public override string Category => "Trades";

        public override string Name => "Average End of Trade Drawdown";

        public override int Type => PortfolioStatisticsType.AvgEndOfTradeDrawdown;
        public AvgEndOfTradeDrawdown() : base(PortfolioStatisticsType.EndOfTradeDrawdown)
        {
        }
    }
}
