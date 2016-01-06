using System;
using static System.Math;
using System.Linq;

namespace SmartQuant.Statistics
{
    public abstract class StandardDeviation : PortfolioStatisticsItem
    {
        protected double longAvg;
        protected double shortAvg;
        protected double totalAvg;
        protected int type;

        public StandardDeviation(int type)
        {
            this.type = type;
        }

        protected double GetStdDev(TimeSeries ts, double avg)
        {
            return ts.Count > 1 ? Sqrt(Enumerable.Range(0, ts.Count).Sum(i => Pow(ts[i] - avg, 2)) / (ts.Count - 1)) : 0;
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
                if (statistics.LongValues.Count > LongValues.Count)
                {
                    this.longAvg = (this.longAvg * LongValues.Count + statistics.LongValue) / (LongValues.Count + 1);
                    this.longValue = GetStdDev(statistics.LongValues, this.longAvg);
                    LongValues.Add(Clock.DateTime, LongValue);
                    changed = true;
                }
                if (statistics.ShortValues.Count > this.shortValues.Count)
                {
                    this.shortAvg = (this.shortAvg * this.shortValues.Count + statistics.ShortValue) / (ShortValues.Count + 1);
                    this.shortValue = GetStdDev(statistics.ShortValues, this.shortAvg);
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                    changed = true;
                }
                if (statistics.TotalValues.Count > this.totalValues.Count)
                {
                    this.totalAvg = (this.totalAvg * this.totalValues.Count + statistics.TotalValue) / (TotalValues.Count + 1);
                    this.totalValue = GetStdDev(statistics.TotalValues, this.totalAvg);
                    TotalValues.Add(Clock.DateTime, this.totalValue);
                    changed = true;
                }
                if (changed)
                {
                    Emit();
                }
            }
        }
    }
}
