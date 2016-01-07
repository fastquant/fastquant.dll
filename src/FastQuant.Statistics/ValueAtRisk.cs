using System.Collections.Generic;

namespace SmartQuant.Statistics
{
    public class ValueAtRisk : PortfolioStatisticsItem
    {
        protected List<double> pnls;

        public override string Category => "Daily / Annual returns";

        public double Level { get; set; }

        public override string Name => "Value at Risk";

        public override int Type => PortfolioStatisticsType.ValueAtRisk;

        private bool method_0(double double_1)
        {
            for (int i = 0; i < this.pnls.Count; i++)
            {
                if (this.pnls[i] >= double_1)
                {
                    this.pnls.Insert(i, double_1);
                    return true;
                }
            }
            return false;
        }

        public override void OnInit()
        {
            this.pnls = new List<double>();
            Level = 99;
            Subscribe(PortfolioStatisticsType.NetProfit);
        }

        public override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == PortfolioStatisticsType.NetProfit)
            {
                if (!this.method_0(statistics.TotalValue))
                {
                    this.pnls.Add(statistics.TotalValue);
                }
                int num = this.pnls.Count - (int)(this.Level * (double)this.pnls.Count / 100.0) - 1;
                if (num >= 0 && num <= this.pnls.Count - 1)
                {
                    this.totalValue = this.pnls[num];
                    this.totalValues.Add(Clock.DateTime, this.totalValue);
                    Emit();
                }
            }
        }
    }
}
