// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace FastQuant
{
    public class ReportItem
    {
        protected internal int id;
        protected internal string name;
        protected internal string description;

        public string Name => this.name;

        public string Description => this.description;

        protected virtual void OnBid(Bid bid)
        {
            // noop
        }

        protected virtual void OnAsk(Ask Ask)
        {
            // noop
        }

        protected virtual void OnTrade(Trade trade)
        {
            // noop
        }

        protected virtual void OnBar(Bar bar)
        {
            // noop
        }

        protected virtual void OnExecutionReport(ExecutionReport report)
        {
            // noop
        }

        protected internal virtual void Clear()
        {
            // noop
        }
    }

    public class Report
    {
        private readonly List<ReportItem> reports = new List<ReportItem>();

        public void Add(ReportItem item)
        {
            this.reports.Add(item);
        }

        public void Clear()
        {
            foreach (var report in this.reports)
                report.Clear();
            this.reports.Clear();
        }
    }

    public class ReportManager
    {
    }

    public class RiskReport
    {
        public RiskReport(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }

    public class RiskManager
    {
        protected Framework framework;

        public RiskManager(Framework framework)
        {
            this.framework = framework;
        }

        public virtual RiskReport OnExecutionCommand(ExecutionCommand command)
        {
            return null;
        }

        public virtual void PropertyChanged(OnPropertyChanged onPropertyChanged)
        {
            // noop
        }
    }
}
