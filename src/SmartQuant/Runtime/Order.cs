// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

    public enum ExecType : byte
    {
        ExecNew,
        ExecStopped,
        ExecRejected,
        ExecExpired,
        ExecTrade,
        ExecPendingCancel,
        ExecCancelled,
        ExecCancelReject,
        ExecPendingReplace,
        ExecReplace,
        ExecReplaceReject,
        ExecTradeCorrect,
        ExecTradeCancel,
        ExecOrderStatus,
        ExecPendingNew,
        ExecClearingHold
    }


    public class Order : DataObject
    {
        private OrderSide side;
        private OrderStatus status;
        private OrderType type;
        private TimeInForce timeInForce;
        private DateTime expireTime;
        private double qty;
        private byte routeId;
        private double price;
        private double stopPx;
        private string account;
        private string clientId;
        private IExecutionProvider provider;
        private Portfolio portfolio;
        private Instrument instrument;
        internal ObjectTable fields;

        internal double double_6;

        public int Id { get; internal set; } = -1;

        public int ClId { get; set; } = -1;

        public int AlgoId { get; set; } = -1;

        public int ClientId { get; set; } = -1;

        public string ProviderOrderId { get; set; } = string.Empty;

        public string ClOrderId { get; set; } = string.Empty;

        internal int InstrumentId { get; set; } = -1;

        public int PortfolioId { get; set; } = -1;

        public int StrategyId { get; set; } = -1;

        public string OCA { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

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

        [Category("Message"), Description("Commands")]
        public List<ExecutionCommand> Commands { get; set; } = new List<ExecutionCommand>();

        [Category("Message"), Description("Messages")]
        public List<ExecutionMessage> Messages { get; set; } = new List<ExecutionMessage>();

        [Category("Message"), Description("Reports")]
        public List<ExecutionReport> Reports { get; set; } = new List<ExecutionReport>();

        public ObjectTable Fields => this.fields = this.fields ?? new ObjectTable();

        public object this[int index]
        {
            get
            {
                return Fields[index];
            }
            set
            {
                Fields[index] = value;
            }
        }


        public double AvgPx { get; set; }

        public double LeavesQty { get; internal set; }

        public double CumQty { get; internal set; }

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
        public DateTime ExpireTime
        {
            get
            {
                return this.expireTime;
            }
            set
            {
                EnsureNotSent();
                this.expireTime = value;
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

        [Browsable(false)]
        public bool IsOCA => !string.IsNullOrEmpty(OCA);

        public DateTime TransactTime { get; set; }

        public Order()
        {
        }

        public Order(ExecutionCommand command)
        {
            if (command.Type != ExecutionCommandType.Send)
                throw new Exception($"Order::Order Can not create order from execution command of type different than Send : {command.Type}");
            Id = command.OrderId;
            ClOrderId= command.ClOrderId;
            AlgoId = command.AlgoId;
            ProviderOrderId = command.ProviderOrderId;
            ProviderId = command.ProviderId;
            RouteId = command.RouteId;
            PortfolioId = command.PortfolioId;
            StrategyId = command.StrategyId;
            InstrumentId = command.InstrumentId;
            TransactTime = command.TransactTime;
            DateTime = command.DateTime;
            Side = command.Side;
            Type = command.OrdType;
            TimeInForce = command.TimeInForce;
            ExpireTime = command.ExpireTime;
            Price = command.Price;
            StopPx = command.StopPx;
            Qty = command.Qty;
            Text = command.Text;
            Account = command.Account;
            ClientID = command.ClientID;
            ClientId = command.ClientId;
        }

        public Order(Order order)
        {
            Id = order.Id;
            ClId = order.ClId;
            AlgoId = order.AlgoId;
            ProviderOrderId = order.ProviderOrderId;
            ProviderId = order.ProviderId;
            PortfolioId = order.PortfolioId;
            StrategyId = order.StrategyId;
            InstrumentId = order.InstrumentId;
            TransactTime = order.TransactTime;
            DateTime = order.DateTime;
            Instrument = order.Instrument;
            provider = order.Provider;
            Portfolio = order.Portfolio;
            Status = order.Status;
            Side = order.Side;
            Type = order.Type;
            TimeInForce = order.TimeInForce;
            ExpireTime = order.ExpireTime;
            Price = order.Price;
            StopPx = order.StopPx;
            AvgPx = order.AvgPx;
            Qty = order.Qty;
            CumQty = order.CumQty;
            Text = order.Text;
            Account = order.Account;
            ClientID = order.ClientID;
        }

        public Order(IExecutionProvider provider, Instrument instrument, OrderType type, OrderSide side, double qty, double price = 0, double stopPx = 0, TimeInForce timeInForce = TimeInForce.Day, string text = "")
            : this()
        {
            Provider = provider;
            Instrument = instrument;
            Type = type;
            Side = side;
            Qty = qty;
            Price = price;
            StopPx = stopPx;
            TimeInForce = timeInForce;
            Text = text;
        }

        public Order(IExecutionProvider provider, Portfolio portfolio, Instrument instrument, OrderType type, OrderSide side, double qty, double price = 0, double stopPx = 0, TimeInForce timeInForce = TimeInForce.Day, byte routeId = 0, string text = "")
           : this(provider, instrument, type, side, qty, price, stopPx, timeInForce, text = "")
        {
            Portfolio = portfolio;
            RouteId = routeId;
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
            Commands.Add(command);
            Messages.Add(command);
        }

        public void OnExecutionReport(ExecutionReport report)
        {
            Status = report.OrdStatus;
            if (report.ExecType == ExecType.ExecTrade)
                AvgPx = (AvgPx  * CumQty + report.LastPx * report.LastQty) / (CumQty + report.LastQty);

            CumQty = report.CumQty;
            LeavesQty = report.LeavesQty;

            if (report.ExecType == ExecType.ExecNew)
                ProviderOrderId = report.ProviderOrderId;

            if (report.ExecType == ExecType.ExecReplace)
            {
                Type = report.OrdType;
                Price = report.Price;
                StopPx = report.StopPx;
                Qty = report.OrdQty;
            }

            Reports.Add(report);
            Messages.Add(report);
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