namespace FastQuant.Statistics
{
    public class TradesPnL : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsLong)
            {
                this.longValue = trade.NetPnL;
                LongValues.Add(Clock.DateTime, this.longValue);
            }
            else
            {
                this.shortValue = trade.NetPnL;
                ShortValues.Add(Clock.DateTime, this.shortValue);
            }
            this.totalValue = trade.NetPnL;
            TotalValues.Add(Clock.DateTime, this.totalValue);
            Emit();
        }

        public override string Category => "Trades";

        public override string Name => "Trades PnL";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.TradesPnL;
    }

    public class WinTradesPnL : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (trade.IsWinning)
            {
                if (trade.IsLong)
                {
                    this.longValue = trade.NetPnL;
                    LongValues.Add(Clock.DateTime, this.longValue);
                }
                else
                {
                    this.shortValue = trade.NetPnL;
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                }
                this.totalValue = trade.NetPnL;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public override string Category => "Trades";

        public override string Name => "Winning Trades PnL";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.WinTradesPnL;
    }

    public class LossTradesPnL : PortfolioStatisticsItem
    {
        protected internal override void OnRoundTrip(TradeInfo trade)
        {
            if (!trade.IsWinning)
            {
                if (trade.IsLong)
                {
                    this.longValue = trade.NetPnL;
                    LongValues.Add(Clock.DateTime, this.longValue);
                }
                else
                {
                    this.shortValue = trade.NetPnL;
                    ShortValues.Add(Clock.DateTime, this.shortValue);
                }
                this.totalValue = trade.NetPnL;
                TotalValues.Add(Clock.DateTime, this.totalValue);
                Emit();
            }
        }

        public override string Category => "Trades";

        public override string Name => "Losing Trades PnL";

        public override bool Show => false;

        public override int Type => PortfolioStatisticsType.LossTradesPnL;
    }
}
