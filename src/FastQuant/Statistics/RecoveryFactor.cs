using System;
using static System.Math;

namespace FastQuant.Statistics
{
    public class RecoveryFactor : PortfolioStatisticsItem
    {
        protected internal override void OnInit()
        {
            this.shortValue = 1;
            this.longValue = 1;
            this.totalValue = 1;
            LongValues.Add(Clock.DateTime, this.longValue);
            ShortValues.Add(Clock.DateTime, this.shortValue);
            TotalValues.Add(Clock.DateTime, this.totalValue);
            Subscribe(PortfolioStatisticsType.NetProfit);
            Subscribe(PortfolioStatisticsType.MaxDrawdown);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            bool changed = false;
            if (statistics.Type == PortfolioStatisticsType.NetProfit)
            {
                this.netProfit = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.MaxDrawdown)
            {
                this.maxDrawdown = statistics;
                changed = true;
            }
            if (changed && this.netProfit != null && this.maxDrawdown != null)
            {
                bool updated = false;
                if (this.maxDrawdown.LongValue != 0)
                {
                    this.longValue = Abs(this.netProfit.LongValue / this.maxDrawdown.LongValue);
                    LongValues.Add(Clock.DateTime, this.longValue);
                    updated = true;
                }
                if (this.maxDrawdown.ShortValue != 0)
                {
                    this.shortValue = Abs(this.netProfit.ShortValue / this.maxDrawdown.ShortValue);
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                    updated = true;
                }
                if (this.maxDrawdown.TotalValue != 0)
                {
                    this.totalValue = Math.Abs(this.netProfit.TotalValue / this.maxDrawdown.TotalValue);
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    updated = true;
                }
                if (updated)
                {
                    Emit();
                }
            }
        }

        public override string Category => "Summary";

        public override string Name => "Recovery Factor";

        public override int Type => PortfolioStatisticsType.RecoveryFactor;

        protected PortfolioStatisticsItem maxDrawdown;

        protected PortfolioStatisticsItem netProfit;
    }
}
