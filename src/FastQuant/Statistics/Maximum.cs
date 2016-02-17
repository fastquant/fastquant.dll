using static System.Math;

namespace FastQuant.Statistics
{
    public abstract class Maximum : PortfolioStatisticsItem
    {
        protected int type;

        public Maximum(int type)
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
                this.longValue = Max(this.longValue, statistics.LongValue);
                LongValues.Add(Clock.DateTime, this.longValue);
                this.shortValue = Max(this.shortValue, statistics.ShortValue);
                ShortValues.Add(Clock.DateTime, this.shortValue);
                this.totalValue = Max(this.totalValue, statistics.TotalValue);
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }
    }

    public class MaxConsecutiveLossTrades : Maximum
    {
        public MaxConsecutiveLossTrades() : base(PortfolioStatisticsType.ConsecutiveLossTrades)
        {
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Maximum Consecutive Losing Trades";

        public override int Type => PortfolioStatisticsType.MaxConsecutiveLossTrades;
    }

    public class MaxConsecutiveWinTrades : Maximum
    {
        public MaxConsecutiveWinTrades() : base(PortfolioStatisticsType.ConsecutiveWinTrades)
        {
        }

        public override string Category => "Trades";

        public override string Format => "F0";

        public override string Name => "Maximum Consecutive Winning Trades";

        public override int Type => PortfolioStatisticsType.MaxConsecutiveWinTrades;
    }
}
