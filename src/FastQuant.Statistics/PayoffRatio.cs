using System;
using static System.Math;

namespace SmartQuant.Statistics
{
    public class PayoffRatio : PortfolioStatisticsItem
    {
        public override void OnInit()
        {
            this.shortValue = 1;
            this.longValue = 1;
            this.totalValue = 1;
            LongValues.Add(Clock.DateTime, this.longValue);
            ShortValues.Add(Clock.DateTime, this.shortValue);
            TotalValues.Add(Clock.DateTime, this.totalValue);
            Subscribe(PortfolioStatisticsType.AvgWinTrade);
            Subscribe(PortfolioStatisticsType.AvgLossTrade);
        }

        public override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            bool changed = false;
            if (statistics.Type == PortfolioStatisticsType.AvgWinTrade)
            {
                this.avgWinTrades = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.AvgLossTrade)
            {
                this.avgLossTrades = statistics;
                changed = true;
            }
            if (changed && this.avgWinTrades != null && this.avgLossTrades != null)
            {
                bool updated = false;
                if (this.avgLossTrades.LongValue != 0)
                {
                    this.longValue = Abs(this.avgWinTrades.LongValue / this.avgLossTrades.LongValue);
                    LongValues.Add(Clock.DateTime, this.longValue);
                    updated = true;
                }
                if (this.avgLossTrades.ShortValue != 0.0)
                {
                    this.shortValue = Math.Abs(this.avgWinTrades.ShortValue / this.avgLossTrades.ShortValue);
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                    updated = true;
                }
                if (this.avgLossTrades.TotalValue != 0.0)
                {
                    this.totalValue = Abs(this.avgWinTrades.TotalValue / this.avgLossTrades.TotalValue);
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    updated = true;
                }
                if (updated)
                {
                    Emit();
                }
            }
        }

        public override string Category => "Trades";

        public override string Name => "Payoff Ratio";

        public override int Type => PortfolioStatisticsType.PayoffRatio;

        protected PortfolioStatisticsItem avgLossTrades;

        protected PortfolioStatisticsItem avgWinTrades;
    }
}
