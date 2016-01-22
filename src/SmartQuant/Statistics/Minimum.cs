using static System.Math;

namespace SmartQuant.Statistics
{
    public abstract class Minimum : PortfolioStatisticsItem
    {
        protected int type;

        public Minimum(int type)
        {
            this.type = type;
        }

        protected internal override void OnInit()
        {
            Subscribe(this.type);
        }

        protected internal override void OnStatistics(PortfolioStatisticsItem statistics)
        {
            if (statistics.Type == this.type)
            {
                this.longValue = Min(this.longValue, statistics.LongValue);
                this.longValues.Add(Clock.DateTime, this.longValue);
                this.shortValue = Min(this.shortValue, statistics.ShortValue);
                this.shortValues.Add(Clock.DateTime, this.shortValue);
                this.totalValue = Min(this.totalValue, statistics.TotalValue);
                this.totalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }
    }

    public class MaxDrawdown : Minimum
    {
        public MaxDrawdown():base(PortfolioStatisticsType.Drawdown)
        {
        }

        public override string Category => "Summary";

        public override string Name => "Maximum Drawdown";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.MaxDrawdown;
    }

    public class MaxDrawdownPercent : Minimum
    {
        public MaxDrawdownPercent():base(PortfolioStatisticsType.DrawdownPercent)
        {
        }

        public override string Category => "Summary";

        public override string Format => "P2";

        public override string Name => "Maximum Drawdown %";

        public override int Type => PortfolioStatisticsType.MaxDrawdownPercent;
    }

}
