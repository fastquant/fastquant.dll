using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ExecutionSimulator : Provider, IExecutionSimulator
    {
        private class Class43
        {
            public Class43(ExecutionReport report)
            {
                OrdStatus = report.OrdStatus;
                AvgPx = report.AvgPx;
                CumQty = report.CumQty;
                LeavesQty = report.LeavesQty;
                Commission = report.Commission;
            }

            public double AvgPx { get; }

            public double LeavesQty { get; }

            public double Commission { get; }

            public OrderStatus OrdStatus { get; }

            public double CumQty { get; }
        }

        private List<Order> list_0 = new List<Order>();

        private List<Order> list_1 = new List<Order>();

        private IdArray<List<Order>> idArray_0= new IdArray<List<Order>>(10240);

        private IdArray<ExecutionSimulator.Class43> idArray_1 = new IdArray<ExecutionSimulator.Class43>(10240);

        public TimeSpan Auction1 { get; set; }

        public TimeSpan Auction2 { get; set; }

        public BarFilter BarFilter { get; } = new BarFilter();

        public ISlippageProvider SlippageProvider { get; set; } = new SlippageProvider();

        public ICommissionProvider CommissionProvider { get; set; } = new CommissionProvider();

        public bool FillOnQuote { get; set; } = true;
        public bool FillOnTrade { get; set; } = true;
        public bool FillOnLevel2 { get; set; } = true;
        public bool FillLimitOnNext { get; set; } = true;
        public bool FillStopOnNext { get; set; } = true;
        public bool FillStopLimitOnNext { get; set; } = true;
        public bool FillAtLimitPrice { get; set; } = true;
        public bool FillAtStopPrice { get; set; }
        public bool FillMarketOnNext { get; set; }
        public bool FillOnBar { get; set; }
        public bool FillOnBarOpen { get; set; }

        public bool Queued { get; set; } = true;
        public bool PartialFills { get; set; }

        public ExecutionSimulator(Framework framework):base(framework)
        {
            this.framework = framework;
            this.id = ProviderId.ExecutionSimulator;
            this.name = "ExecutionSimulator";
            this.description = "Default execution simulator";
            this.url = "fastquant.org";
        }

        public override void Clear()
        {
            this.idArray_0.Clear();
            this.idArray_1.Clear();
        }

        public override void Send(ExecutionCommand command)
        {
            if (IsDisconnected)
                Connect();
            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    Send(command.Order);
                    return;
                case ExecutionCommandType.Cancel:
                    Cancel(command.Order);
                    return;
                case ExecutionCommandType.Replace:
                    Replace(command);
                    return;
                default:
                    return;
            }
        }

        public void Fill(Order order, double price, int size)
        {
        }

        public void OnAsk(Ask ask)
        {
        //    throw new NotImplementedException();
        }

        public void OnBar(Bar bar)
        {
         //   throw new NotImplementedException();
        }

        public void OnBarOpen(Bar bar)
        {
        //    throw new NotImplementedException();
        }

        public void OnBid(Bid bid)
        {
            var iId = bid.InstrumentId;
            if (this.idArray_0[iId] == null)
                return;

            if (FillOnQuote)
            {
                for (int i = 0; i < this.idArray_0[iId].Count; i++)
                {
                    var order = this.idArray_0[iId][i];
                    this.method_12(order, bid);
                }
                this.method_11();
            }
        }

        private void method_12(Order order, Bid bid)
        {
            throw new NotImplementedException();
        }

        public void OnLevel2(Level2Update update)
        {
            throw new NotImplementedException();
        }

        public void OnLevel2(Level2Snapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public void OnTrade(Trade trade)
        {
        //    throw new NotImplementedException();
        }

        private void Send(Order order)
        {
        }

        private void Cancel(Order order)
        {
        }

        private void Replace(ExecutionCommand command)
        {
        }

        private void method_11()
        {
            if (this.list_0.Count > 0)
            {
                foreach (var order in this.list_0)
                    this.idArray_0[order.InstrumentId].Remove(order);
                this.list_0.Clear();
            }
        }
    }
}