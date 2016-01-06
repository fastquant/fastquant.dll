using System;
using static System.Math;
using System.Linq;

namespace SmartQuant.Statistics
{
    public class CompoundAnnualGrowthRate : PortfolioStatisticsItem
    {
        protected double initial;

        protected bool isSet;

        protected override void OnEquity(double equity)
        {
            if (!this.isSet)
            {
                this.isSet = true;
                this.initial = equity;
            }
        }

        protected override void OnInit()
        {
            Subscribe(PortfolioStatisticsType.AnnualReturn);
        }

        protected override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.AnnualReturn)
            {
                this.totalValue = Pow(1 + Enumerable.Range(0, statistics.TotalValues.Count).Sum(i => statistics.TotalValues[i]) / this.initial, 1.0 / statistics.TotalValues.Count) - 1;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public override string Category => "Daily / Annual returns";

        public override string Format => "P2";

        public override string Name => "Compound Annual Growth Rate";

        public override int Type => PortfolioStatisticsType.CompoundAnnualGrowthRate;
    }
}
