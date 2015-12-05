using System;

namespace SmartQuant
{
    public class ExecutionSimulator : Provider, IExecutionSimulator
    {
        public TimeSpan Auction1 { get; set; }
        public TimeSpan Auction2 { get; set; }
        public BarFilter BarFilter { get; } = new BarFilter();
        public ISlippageProvider SlippageProvider { get; set; } = new SlippageProvider();
        public ICommissionProvider CommissionProvider { get; set; } = new CommissionProvider();
        public bool FillAtLimitPrice { get; set; } = true;
        public bool FillAtStopPrice { get; set; }
        public bool FillLimitOnNext { get; set; }
        public bool FillMarketOnNext { get; set; }
        public bool FillOnBar { get; set; } = false;
        public bool FillOnBarOpen { get; set; } = false;
        public bool FillOnLevel2 { get; set; } = true;
        public bool FillOnQuote { get; set; } = true;
        public bool FillOnTrade { get; set; } = true;
        public bool FillStopLimitOnNext { get; set; }
        public bool FillStopOnNext { get; set; }
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
            throw new NotImplementedException();
        }

        public void OnBar(Bar bar)
        {
            throw new NotImplementedException();
        }

        public void OnBarOpen(Bar bar)
        {
            throw new NotImplementedException();
        }

        public void OnBid(Bid bid)
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
            throw new NotImplementedException();
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
    }
}