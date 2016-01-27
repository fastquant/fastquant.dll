// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class PortfolioPerformance
    {
        private Portfolio portfolio;
        private double equity;
        private double drawdown;
        private double maxEquity;

        public TimeSeries DrawdownSeries { get; } = new TimeSeries("Drawdown", "Drawdown");

        public TimeSeries EquitySeries { get; } = new TimeSeries("Equity", "Equity");

        public bool UpdateParent { get; set; } = true;

        public event EventHandler Updated;

        public PortfolioPerformance(Portfolio portfolio)
        {
            this.portfolio = portfolio;
        }

        public void Reset()
        {
            this.drawdown = 0;
            this.maxEquity = double.MinValue;
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
            if (this.equity == equity && !forceUpdate)
                return;

            this.equity = equity;
            this.maxEquity = Math.Max(this.maxEquity, this.equity);
            this.drawdown = this.maxEquity - equity;
            EquitySeries.Add(dateTime, this.equity);
            DrawdownSeries.Add(dateTime, this.drawdown);
            this.portfolio.Statistics.OnEquity(equity);
        }
    }
}