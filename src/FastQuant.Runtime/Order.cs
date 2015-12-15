using System;
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

        public override byte TypeId => DataObjectType.Order;

        public string Text { get; set; }

        public double AvgPx { get; set; }
        public int Id { get; internal set; }
        public bool IsDone { get; set; }

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
            // Token: 0x06000F92 RID: 3986 RVA: 0x0000CDBC File Offset: 0x0000AFBC
            get
            {
                return this.qty;
            }
            // Token: 0x06000F93 RID: 3987 RVA: 0x0000CDC4 File Offset: 0x0000AFC4
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
            private set
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
            // Token: 0x06000F9B RID: 3995 RVA: 0x0000CE19 File Offset: 0x0000B019
            get
            {
                return this.routeId;
            }
            // Token: 0x06000F9C RID: 3996 RVA: 0x0000CE21 File Offset: 0x0000B021
            set
            {
                EnsureNotSent();
                this.routeId = value;
            }
        }


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
            if (Status != OrderStatus.NotSent)
                throw new InvalidOperationException("Cannot perform an operation, because order is already sent.");
        }
    }
}