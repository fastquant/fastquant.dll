// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace SmartQuant
{
    public class ReportItem
    {
        protected internal int id;
        protected internal string name;
        protected internal string description;

        public string Name => this.name;

        public string Description => this.description;

        public ReportItem()
        {
        }

        protected virtual void OnBid(Bid bid)
        {
        }

        protected virtual void OnAsk(Ask Ask)
        {
        }

        protected virtual void OnTrade(Trade trade)
        {
        }

        protected virtual void OnBar(Bar bar)
        {
        }

        protected virtual void OnExecutionReport(ExecutionReport report)
        {
        }

        protected internal virtual void Clear()
        {
        }
    }

    public class Report
    {
        private List<ReportItem> reports = new List<ReportItem>();

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
}
