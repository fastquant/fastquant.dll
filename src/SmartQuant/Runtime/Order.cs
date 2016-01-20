using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SmartQuant
{
    public enum OrderType : byte
    {
        Market,
        Stop,
        Limit,
        StopLimit,
        MarketOnClose,
        Pegged,
        TrailingStop,
        TrailingStopLimit
    }

    public enum OrderStatus : byte
    {
        NotSent,
        PendingNew,
        New,
        Rejected,
        PartiallyFilled,
        Filled,
        PendingCancel,
        Cancelled,
        Expired,
        PendingReplace,
        Replaced
    }

    public enum OrderSide : byte
    {
        Buy,
        Sell
    }

    public enum TimeInForce : byte
    {
        ATC,
        Day,
        GTC,
        IOC,
        OPG,
        OC,
        FOK,
        GTX,
        GTD,
        GFS,
        AUC
    }

    public enum ExecType
    {
        ExecNew,
        ExecRejected,
        ExecTrade,
        ExecPendingCancel,
        ExecCancelled,
        ExecCancelReject,
        ExecPendingReplace,
        ExecReplace,
        ExecReplaceReject
    }

    public class Order : DataObject
    {
        private OrderSide side;
        private OrderStatus status;
        private OrderType type;
        private TimeInForce timeInForce;
        private double qty;
        private byte routeId;
        private double price;
        private double stopPx;
        private string account;
        private string clientId;

        private IExecutionProvider provider;
        private Portfolio portfolio;

        private Instrument instrument;
        internal int InstrumentId { get; set; }

        public string ClOrderId { get; set; }

        public override byte TypeId => DataObjectType.Order;

        public string Account
        {
            get
            {
                return this.account;
            }
            set
            {
                EnsureNotSent();
                this.account = value;
            }
        }

        public string ClientID
        {
            get
            {
                return this.clientId;
            }
            set
            {
                EnsureNotSent();
                this.clientId = value;
            }
        }

        public string Text { get; set; }

        public double AvgPx { get; set; }

        public double LeavesQty { get; }

        public double CumQty { get; }

        public string ProviderOrderId { get; set; }

        public Instrument Instrument
        {
            get
            {
                return this.instrument;
            }
            set
            {
                this.instrument = value;
                InstrumentId = this.instrument?.Id ?? -1;
            }
        }


        public int Id { get; internal set; }

        [ReadOnly(true)]
        public double Price
        {
            get
            {
                return this.price;
            }
            set
            {
                EnsureNotSent();
                this.price = value;
            }
        }

        [ReadOnly(true)]
        public double Qty
        {
            get
            {
                return this.qty;
            }
            set
            {
                EnsureNotSent();
                this.qty = value;
            }
        }

        [ReadOnly(true)]
        public double StopPx
        {
            get
            {
                return this.stopPx;
            }
            set
            {
                EnsureNotSent();
                this.stopPx = value;
            }
        }
        public string OCA { get; set; }
        public int StrategyId { get; set; }

        public int ClientId { get; set; }

        [ReadOnly(true)]
        public OrderSide Side
        {
            get
            {
                return this.side;
            }
            set
            {
                EnsureNotSent();
                this.side = value;
            }
        }

        [ReadOnly(true)]
        public OrderType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                EnsureNotSent();
                this.type = value;
            }
        }

        public OrderStatus Status
        {
            get
            {
                return this.status;
            }
            internal set
            {
                this.status = value;
            }
        }

        [ReadOnly(true)]
        public TimeInForce TimeInForce
        {
            get
            {
                return this.timeInForce;
            }
            set
            {
                EnsureNotSent();
                this.timeInForce = value;
            }
        }

        [ReadOnly(true)]
        public byte RouteId
        {
            get
            {
                return this.routeId;
            }
            set
            {
                EnsureNotSent();
                this.routeId = value;
            }
        }

        public byte ProviderId { get; set; }

        public IExecutionProvider Provider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
                ProviderId = this.provider?.Id ?? 0;
            }
        }

        public int PortfolioId { get; set; }

        [Browsable(false)]
        public Portfolio Portfolio
        {
            get
            {
                return this.portfolio;
            }
            set
            {
                this.portfolio = value;
                PortfolioId = this.portfolio?.Id ?? -1;
            }
        }

        [Category("Message"), Description("Messages")]
        public List<ExecutionMessage> Messages { get; set; }


        [Browsable(false)]
        public bool IsFilled => Status == OrderStatus.Filled;

        [Browsable(false)]
        public bool IsCancelled => Status == OrderStatus.Cancelled;

        [Browsable(false)]
        public bool IsRejected => Status == OrderStatus.Rejected;

        [Browsable(false)]
        public bool IsExpired => Status == OrderStatus.Expired;

        [Browsable(false)]
        public bool IsNew => Status == OrderStatus.New;

        [Browsable(false)]
        public bool IsPartiallyFilled => Status == OrderStatus.PartiallyFilled;

        [Browsable(false)]
        public bool IsNotSent => Status == OrderStatus.NotSent;

        [Browsable(false)]
        public bool IsPendingCancel => Status == OrderStatus.PendingCancel;

        [Browsable(false)]
        public bool IsPendingNew => Status == OrderStatus.PendingNew;

        [Browsable(false)]
        public bool IsPendingReplace => Status == OrderStatus.PendingReplace;

        [Browsable(false)]
        public bool IsReplaced => Status == OrderStatus.Replaced;

        [Browsable(false)]
        public bool IsDone => IsFilled || IsCancelled || IsRejected || IsExpired;

        public Order()
        {

        }

        public Order(ExecutionCommand command)
        {

        }

        public Order(Order order)
        {
        }

        public Order(IExecutionProvider provider, Instrument instrument, OrderType type, OrderSide side, double qty,
            double price = 0.0, double stopPx = 0.0, TimeInForce timeInForce = TimeInForce.Day, string text = "")
        {
        }

        public Order(IExecutionProvider provider, Portfolio portfolio, Instrument instrument, OrderType type, OrderSide side,
            double qty, double price = 0.0, double stopPx = 0.0, TimeInForce timeInForce = TimeInForce.Day,
            byte routeId = 0, string text = "")
        {
        }

        public string GetSideAsString() => Side == OrderSide.Buy ? "Buy" : Side == OrderSide.Sell ? "Sell" : "Undefined";

        public string GetStatusAsString()
        {
            switch (Status)
            {
                case OrderStatus.NotSent:
                    return "NotSent";
                case OrderStatus.PendingNew:
                    return "PendingNew";
                case OrderStatus.New:
                    return "New";
                case OrderStatus.Rejected:
                    return "Rejected";
                case OrderStatus.PartiallyFilled:
                    return "PartiallyFilled";
                case OrderStatus.Filled:
                    return "Filled";
                case OrderStatus.PendingCancel:
                    return "PendingCancel";
                case OrderStatus.Cancelled:
                    return "Cancelled";
                case OrderStatus.Expired:
                    return "Expired";
                case OrderStatus.PendingReplace:
                    return "PendingReplace";
                case OrderStatus.Replaced:
                    return "Replaced";
                default:
                    return "Undefined";
            }
        }

        public void OnExecutionCommand(ExecutionCommand command)
        {
            throw new NotImplementedException();
        }

        public void OnExecutionReport(ExecutionReport report)
        {
            throw new NotImplementedException();
        }

        public string GetTypeAsString()
        {
            switch (Type)
            {
                case OrderType.Market:
                    return "Market";
                case OrderType.Stop:
                    return "Stop";
                case OrderType.Limit:
                    return "Limit";
                case OrderType.StopLimit:
                    return "StopLimit";
                default:
                    return "Undefined";
            }
        }


        private void EnsureNotSent()
        {
            if (!IsNotSent)
                throw new InvalidOperationException("Cannot perform an operation, because order is already sent.");
        }
    }
}