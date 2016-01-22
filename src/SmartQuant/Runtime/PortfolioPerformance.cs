using System;

namespace SmartQuant
{
    public class PortfolioPerformance
    {
        private Portfolio portfolio;
        private double double_0;
        private double drawdown;
        private double double_2;

        public TimeSeries DrawdownSeries { get; } = new TimeSeries("Drawdown", "Drawdown", -1);
        public TimeSeries EquitySeries { get; } = new TimeSeries("Equity", "Equity", -1);
        public bool UpdateParent { get; set; } = true;

        public event EventHandler Updated;

        public PortfolioPerformance(Portfolio portfolio)
        {
            this.portfolio = portfolio;
        }

        public void Reset()
        {
            this.drawdown = 0;
            this.double_2 = double.MinValue;
        }


        public void Update(bool forceUpdate = false)
        {
            OnEquity(this.portfolio.Value, forceUpdate);
            Updated?.Invoke(this, EventArgs.Empty);

            if (UpdateParent)
                this.portfolio.Parent?.Performance.Update(forceUpdate);
        }

        public void OnEquity(double equity, bool forceUpdate = false) => OnEquity(this.portfolio.framework.Clock.DateTime, equity, forceUpdate);

        public void OnEquity(DateTime dateTime, double equity, bool forceUpdate = false)
        {
            if (this.double_0 == equity && !forceUpdate)
                return;

            this.double_0 = equity;
            if (equity > this.double_2)
            {
                this.double_2 = equity;
                this.drawdown = 0;
            }
            else
            {
                this.drawdown = this.double_2 - equity;
            }
            EquitySeries.Add(dateTime, equity);
            DrawdownSeries.Add(dateTime, this.drawdown);
            this.portfolio.Statistics.OnEquity(equity);
        }

    }
}