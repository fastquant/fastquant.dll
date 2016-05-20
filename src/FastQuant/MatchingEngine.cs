using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastQuant
{
    public class MatchingEngine : Provider, IExecutionSimulator
    {
        private IdArray<List<Order>> ordersByInstrumentId = new IdArray<List<Order>>();

        public ICommissionProvider CommissionProvider { get; set; } = new CommissionProvider();

        public ISlippageProvider SlippageProvider { get; set; } = new SlippageProvider();

        public BarFilter BarFilter { get; } = new BarFilter();

        public bool Queued { get; set; } = true;

        public bool AddQueueToOrderText { get; set; }

        public bool FillOnQuote { get; set; } = true;

        public bool FillOnTrade { get; set; } = false;

        public bool FillOnBar { get; set; } = false;

        public bool FillOnBarOpen { get; set; } = false;

        public bool FillOnLevel2 { get; set; } = false;

        public bool FillMarketOnNext { get; set; } = false;

        public bool FillLimitOnNext { get; set; } = false;

        public bool FillStopOnNext { get; set; } = false;

        public bool FillAtStopPrice { get; set; } = false;

        public bool FillStopLimitOnNext { get; set; } = false;

        public bool FillAtLimitPrice { get; set; } = true;

        public bool PartialFills { get; set; } = false;

        public MatchingEngine(Framework framework) : base(framework)
        {
            this.id = ProviderId.MatchingEngine;
            this.name = "MatchingEngine";
        }

        public override void Connect()
        {
            Status = ProviderStatus.Connected;
        }

        public override void Disconnect()
        {
            Status = ProviderStatus.Disconnected;
        }

        protected override void OnConnect()
        {
            Status = ProviderStatus.Connected;
        }

        protected override void OnDisconnect()
        {
            Status = ProviderStatus.Disconnected;
        }

        public void OnBid(Bid bid)
        {
            var orders = this.ordersByInstrumentId[bid.InstrumentId];
            if (orders == null)
                return;

            foreach (var order in orders)
            {
                method_8(order, bid, this.framework.DataManager.GetAsk(order.Instrument));
                if (AddQueueToOrderText)
                    order.Text = order.double_6.ToString();
            }
        }

        public void OnAsk(Ask ask)
        {
            throw new NotImplementedException();
        }

        public void OnLevel2(Level2Snapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public void OnLevel2(Level2Update update)
        {
            throw new NotImplementedException();
        }

        public void OnTrade(Trade trade)
        {
            throw new NotImplementedException();
        }

        public void OnBarOpen(Bar bar)
        {
            throw new NotImplementedException();
        }

        public void OnBar(Bar bar)
        {
            throw new NotImplementedException();
        }

        public override void Send(ExecutionCommand command)
        {
            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    method_5(command.Order);
                    return;
                case ExecutionCommandType.Cancel:
                    method_6(command.Order);
                    return;
                case ExecutionCommandType.Replace:
                    method_7(command);
                    return;
                default:
                    return;
            }
        }

        private void method_5(Order order_0)
        {
        }

        private void method_6(Order order_0)
        {
        }

        private void method_7(ExecutionCommand executionCommand_0)
        {
        }

        private void method_8(Order order_0, Bid bid_0, Ask ask_0)
        {
        }
    }
}
