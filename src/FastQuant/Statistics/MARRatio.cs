using System;
using static System.Math;

namespace FastQuant.Statistics
{
    public class MARRatio : PortfolioStatisticsItem
    {
        protected internal override void OnInit()
        {
            Subscribe(PortfolioStatisticsType.CompoundAnnualGrowthRate);
            Subscribe(PortfolioStatisticsType.MaxDrawdownPercent);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            var changed = false;
            if (statistics.Type == PortfolioStatisticsType.CompoundAnnualGrowthRate)
            {
                this.cagr = statistics;
                changed = true;
            }
            if (statistics.Type == PortfolioStatisticsType.MaxDrawdownPercent)
            {
                this.maxDrawdownPercent = statistics;
                changed = true;
            }
            if (changed && this.cagr != null && this.maxDrawdownPercent != null && this.maxDrawdownPercent.TotalValue != 0)
            {
                this.totalValue = Abs(this.cagr.TotalValue / this.maxDrawdownPercent.TotalValue);
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public override string Category => "Daily / Annual returns";

        public override string Name => "MAR Ratio";

        public override int Type => PortfolioStatisticsType.MARRatio;

        protected PortfolioStatisticsItem cagr;

        protected PortfolioStatisticsItem maxDrawdownPercent;
    }
}
