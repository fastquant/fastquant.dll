// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace SmartQuant
{
    public class Portfolio
    {
        internal Framework framework;

        private Portfolio parent;

        private bool updateParent = true;

        public int Id { get; internal set; } = -1;

        public string Name { get; private set; }

        public string Description { get; set; } = "";

        public bool IsLoaded { get; internal set; }

        [Browsable(false)]
        public Account Account { get; private set; }

        public double AccountValue => Account.Value;

        [Browsable(false)]
        public bool IsOwnerAlgo { get; set; }

        [Browsable(false)]
        public Portfolio Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                if (this.parent == value)
                    return;
                this.parent?.Children.Remove(this);
                this.parent = value;
                if (this.parent != null)
                {
                    Account.Parent = this.parent.Account;
                    this.parent.Children.Add(this);
                }
                else
                    Account.Parent = null;
                this.framework.EventServer.OnPortfolioParentChanged(this, true);
            }
        }

        public List<Portfolio> Children { get; } = new List<Portfolio>();

        [Browsable(false)]
        public List<Position> Positions { get; } = new List<Position>();

        internal IdArray<Position> PositionsByInstrumentId { get; } = new IdArray<Position>(10240);

        [Browsable(false)]
        public List<Transaction> Transactions { get; } = new List<Transaction>();

        internal IdArray<Transaction> TransactionsByOrderId { get; } = new IdArray<Transaction>(102400);

        [Browsable(false)]
        public PortfolioPerformance Performance { get; private set; }

        public double PositionValue => Positions.Sum(p => this.framework.CurrencyConverter.Convert(p.Value, p.Instrument.CurrencyId, Account.CurrencyId));

        public Pricer Pricer { get; set; }

        public double Value => AccountValue + PositionValue;

        public double ActiveOrdersValue
        {
            get
            {
                return this.framework.OrderManager.Orders.TakeWhile(o => !o.IsDone && o.PortfolioId == Id).Sum(o =>
                {
                    var amount = o.Instrument.Factor != 0 ? o.Price * o.Qty * o.Instrument.Factor : o.Price * o.Qty;
                    return this.framework.CurrencyConverter.Convert(amount, o.Instrument.CurrencyId, Account.CurrencyId);
                });
            }
        }

        [Browsable(false)]
        public FillSeries Fills { get; private set; }

        [Browsable(false)]
        public PortfolioStatistics Statistics { get; private set; }

        public bool UpdateParent
        {
            get
            {
                return this.updateParent;
            }
            set
            {
                if (this.updateParent != value)
                {
                    this.updateParent = value;
                    Account.UpdateParent = value;
                }
            }
        }

        public Portfolio(string name)
        {
            Name = name;
        }

        public Portfolio(Framework framework, string name = "") : this(name)
        {
            Init(framework);
        }

        public void Init(Framework framework)
        {
            this.framework = framework;
            Account = new Account(framework);
            Fills = new FillSeries(Name);
            Pricer = this.framework.PortfolioManager.Pricer;
            Performance = new PortfolioPerformance(this);
            Statistics = new PortfolioStatistics(this);
        }

        public void Add(ExecutionReport report)
        {
            OnExecutionReport(report, false);
        }

        public void Add(AccountReport report)
        {
            Account.Add(report);
        }

        public void Add(Fill fill)
        {
            OnFill(fill, false);
        }

        public double GetAccountValue(byte currencyId) => Account.GetValue(currencyId);

        internal Position GetOrCreatePosition(Instrument instrument)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            if (position == null)
            {
                position = new Position(this, instrument);
                PositionsByInstrumentId[instrument.Id] = position;
                Positions.Add(position);
            }
            return position;
        }

        public Position GetPosition(Instrument instrument) => Positions[instrument.Id];

        public double GetPositionValue(byte currencyId) => Positions.TakeWhile(p => p.Instrument.CurrencyId == currencyId).Sum(p => p.Value);

        public double GetValue(byte currencyId) => GetAccountValue(currencyId) + GetPositionValue(currencyId);

        public bool HasPosition(Instrument instrument)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            return position != null && position.Amount != 0;
        }

        public bool HasPosition(Instrument instrument, PositionSide side, double qty)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            return position != null && position.Side == side && position.Qty == qty;
        }

        public bool HasLongPosition(Instrument instrument)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            return position != null && position.Side == PositionSide.Long && position.Qty != 0;
        }

        public bool HasLongPosition(Instrument instrument, double qty)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            return position != null && position.Side == PositionSide.Long && position.Qty == qty;
        }

        public bool HasShortPosition(Instrument instrument)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            return position != null && position.Side == PositionSide.Short && position.Qty != 0;
        }

        public bool HasShortPosition(Instrument instrument, double qty)
        {
            var position = PositionsByInstrumentId[instrument.Id];
            return position != null && position.Side == PositionSide.Short && position.Qty == qty;
        }

        internal void OnExecutionReport(ExecutionReport report, bool queued = true)
        {
            switch (report.ExecType)
            {
                case ExecType.ExecRejected:
                case ExecType.ExecExpired:
                case ExecType.ExecCancelled:
                    {
                        var transaction = TransactionsByOrderId[report.Order.Id];
                        if (transaction != null)
                        {
                            transaction.IsDone = true;
                            TransactionsByOrderId[report.Order.Id] = null;
                            OnTransaction(transaction, queued);
                        }
                        break;
                    }
                case ExecType.ExecTrade:
                    {
                        var transaction = TransactionsByOrderId[report.Order.Id];
                        if (transaction == null)
                        {
                            transaction = new Transaction();
                            this.method_3(transaction, queued);
                            TransactionsByOrderId[report.Order.Id] = transaction;
                        }
                        var fill = new Fill(report);
                        transaction.Add(fill);
                        OnFill(fill, queued);
                        if (report.OrdStatus == OrderStatus.Filled)
                        {
                            transaction.IsDone = true;
                            TransactionsByOrderId[report.Order.Id] = null;
                            OnTransaction(transaction, queued);
                            return;
                        }
                        break;
                    }
                case ExecType.ExecPendingCancel:
                    break;
                default:
                    return;
            }
        }

        // TODO: rewrite it
        internal void OnFill(Fill fill, bool queued = true)
        {
            Fills.Add(fill);
            this.framework.EventServer.OnFill(this, fill, queued);
            var instrument_ = fill.Instrument;
            bool flag = false;
            var position = PositionsByInstrumentId[instrument_.Id];
            if (position == null)
                position = this.GetOrCreatePosition(instrument_);

            if (position.Amount == 0)
                flag = true;

            position.Add(fill);
            Account.Add(fill, false);
            if (flag)
            {
                Statistics.OnPositionChanged(position);
                this.framework.EventServer.OnPositionChanged(this, position, queued);
                Statistics.OnPositionOpened(position);
                this.framework.EventServer.OnPositionOpened(this, position, queued);
            }
            else
            {
                if (fill.Qty > position.Qty && position.Amount != 0.0 && ((fill.Side == OrderSide.Buy && position.Side == PositionSide.Long) || (fill.Side == OrderSide.Sell && position.Side == PositionSide.Short)))
                {
                    Statistics.OnPositionSideChanged(position);
                }
                if (position.Amount != 0.0)
                {
                    Statistics.OnPositionChanged(position);
                }
                this.framework.EventServer.OnPositionChanged(this, position, queued);
                if (position.Amount == 0.0)
                {
                    Statistics.OnPositionClosed(position);
                    this.framework.EventServer.OnPositionClosed(this, position, queued);
                }
            }
            if (UpdateParent)
                Parent?.OnFill(fill, queued);
            Statistics.OnFill(fill);
        }

        // NewTransaction
        internal void method_3(Transaction transaction, bool queued = true)
        {
            Transactions.Add(transaction);
            if (UpdateParent)
                Parent?.method_3(transaction, queued);
        }

        // EmitTransaction?
        internal void OnTransaction(Transaction transaction, bool queued = true)
        {
            this.framework.EventServer.OnTransaction(this, transaction, queued);
            if (UpdateParent)
                Parent?.OnTransaction(transaction, queued);
            Statistics.OnTransaction(transaction);
        }
    }

    public class PortfolioEventArgs : EventArgs
    {
        public Portfolio Portfolio { get; private set; }

        public PortfolioEventArgs(Portfolio portfolio)
        {
            Portfolio = portfolio;
        }
    }

    public delegate void PortfolioEventHandler(object sender, PortfolioEventArgs args);

    public class PortfolioList : IEnumerable<Portfolio>
    {
        private GetByList<Portfolio> list = new GetByList<Portfolio>("Id", "Name");

        public int Count => this.list.Count;

        public Portfolio this[string name] => GetByName(name);

        public PortfolioList()
        {
        }

        public bool Contains(int id) => this.list.Contains(id);

        public bool Contains(string name) => this.list.Contains(name);

        public void Add(Portfolio portfolio) => this.list.Add(portfolio);

        public Portfolio GetByName(string name) => this.list.GetByName(name);

        public Portfolio GetByIndex(int index) => this.list.GetByIndex(index);

        public Portfolio GetById(int id) => this.list.GetById(id);

        internal void Remove(Portfolio portfolio) => this.list.Remove(portfolio);

        public void Clear() => this.list.Clear();

        public IEnumerator<Portfolio> GetEnumerator() => this.list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
