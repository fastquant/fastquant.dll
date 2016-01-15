// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;

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

        [Browsable(false)]
        public Account Account { get; private set; }

        public List<Portfolio> Children { get; } = new List<Portfolio>();

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
                if (this.parent != null)
                    this.parent.Children.Remove(this);
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

        public Pricer Pricer { get; set; }

        [Browsable(false)]
        public List<Transaction> Transactions { get; } = new List<Transaction>();

        internal IdArray<Transaction> TransactionsByOrderId { get; } = new IdArray<Transaction>(102400);

        [Browsable(false)]
        public PortfolioPerformance Performance { get; private set; }

        [Browsable(false)]
        public List<Position> Positions { get; } = new List<Position>();

        internal IdArray<Position> PositionsByInstrumentId { get; } = new IdArray<Position>(10240);

        public double Value => AccountValue + PositionValue;

        public double AccountValue => Account.Value;

        public double PositionValue
        {
            get
            {
                double num = 0;
                //                for (int index = 0; index < this.list_1.Count; ++index)
                //                    num += this.framework_0.ginterface4_0.Convert(this.list_1[index].Value, this.list_1[index].instrument_0.byte_0, this.account_0.byte_0);
                return num;
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

        public bool IsLoaded { get; private set; }

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
            Add(report, false);
        }

        internal void Add(ExecutionReport report, bool bool_0 = true)
        {
            throw new NotImplementedException();
            //            switch (report.ExecType)
            //            {
            //                case ExecType.ExecRejected:
            //                case ExecType.ExecCancelled:
            //                    Transaction transaction_0_1 = this.idArray_0[report.Order.Id];
            //                    if (transaction_0_1 == null)
            //                        break;
            //                    transaction_0_1.bool_0 = true;
            //                    this.method_4(transaction_0_1, true);
            //                    break;
            //                case ExecType.ExecTrade:
            //                    Transaction transaction_0_2 = this.idArray_0[report.Order.Id];
            //                    if (transaction_0_2 == null)
            //                    {
            //                        transaction_0_2 = new Transaction();
            //                        this.method_3(transaction_0_2, true);
            //                        this.idArray_0[report.Order.Id] = transaction_0_2;
            //                    }
            //                    Fill fill = new Fill(report);
            //                    transaction_0_2.Add(fill);
            //                    this.method_2(fill, bool_0);
            //                    if (report.OrdStatus != OrderStatus.Filled)
            //                        break;
            //                    transaction_0_2.bool_0 = true;
            //                    this.method_4(transaction_0_2, true);
            //                    break;
            //            }
        }

        public void Add(Fill fill)
        {
            throw new NotImplementedException();
        }

        internal Position Add(Instrument instrument)
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

        public bool HasPosition(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public bool HasPosition(Instrument instrument, PositionSide side, double qty)
        {
            throw new NotImplementedException();
        }

        public Position GetPosition(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public bool HasLongPosition(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public bool HasLongPosition(Instrument instrument, double qty)
        {
            throw new NotImplementedException();
        }

        public bool HasShortPosition(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public bool HasShortPosition(Instrument instrument, double qty)
        {
            throw new NotImplementedException();
        }

        internal void OnExecutionReport(ExecutionReport report)
        {
            OnExecutionReport(report, false);
        }

        internal void OnExecutionReport(ExecutionReport report, bool queued = true)
        {
            Transaction transaction;
            switch (report.ExecType)
            {
                case ExecType.ExecRejected:
                case ExecType.ExecCancelled:
                    transaction = TransactionsByOrderId[report.Order.Id];
                    if (transaction == null)
                        break;
                    transaction.IsDone = true;
                    OnTransaction(transaction, true);
                    break;
                case ExecType.ExecTrade:
                    transaction = TransactionsByOrderId[report.Order.Id];
                    if (transaction == null)
                    {
                        transaction = new Transaction();
                        method_4(transaction, true);
                        TransactionsByOrderId[report.Order.Id] = transaction;
                    }
                    Fill fill = new Fill(report);
                    transaction.Add(fill);
                    OnFill(fill, queued);
                    if (report.OrdStatus == OrderStatus.Filled)
                    {
                        transaction.IsDone = true;
                        OnTransaction(transaction, true);
                    }
                    break;
            }
        }

        internal void method_4(Transaction transaction, bool queued = true)
        {
            Transactions.Add(transaction);
            if (Parent != null)
                Parent.method_4(transaction, queued);
        }

        internal void OnTransaction(Transaction transaction, bool queued = true)
        {
            this.framework.EventServer.OnTransaction(this, transaction, queued);
            if (Parent != null)
                Parent.OnTransaction(transaction, queued);
            Statistics.OnTransaction(transaction);
        }

        internal void OnFill(Fill fill, bool queued = true)
        {
            Fills.Add(fill);
            this.framework.EventServer.OnFill(this, fill, queued);
            var instrument = fill.Instrument;
            bool flag = false;
            var position = FindPosition(instrument);
            if (position.Qty == 0)
                flag = true;
            position.Add(fill);
            //Account.Add(fill, false);
            if (flag)
            {
                Statistics.OnPositionChanged(position);
                this.framework.EventServer.OnPositionChanged(this, position, queued);
                Statistics.OnPositionOpened(position);
                this.framework.EventServer.OnPositionOpened(this, position, queued);
            }
            else
            {
                this.framework.EventServer.OnPositionChanged(this, position, queued);
                if (position.Qty == 0)
                {
                    Statistics.OnPositionClosed(position);
                    this.framework.EventServer.OnPositionClosed(this, position, queued);
                }
            }
            if (Parent != null)
                Parent.OnFill(fill, queued);
            Statistics.OnFill(fill);
        }

        internal Position FindPosition(Instrument instrument)
        {
            Position position = PositionsByInstrumentId[instrument.Id];
            if (position == null)
            {
                position = new Position(this, instrument);
                PositionsByInstrumentId[instrument.Id] = position;
                Positions.Add(position);
            }
            return position;
        }

        #region Extra Helper Methods

        internal string GetName()
        {
            return Name;
        }

        internal int GetId()
        {
            return Id;
        }

        #endregion
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

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public Portfolio this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }

        public PortfolioList()
        {
        }

        public void Add(Portfolio portfolio)
        {
            this.list.Add(portfolio);
        }

        public Portfolio GetByName(string name)
        {
            return this.list.GetByName(name);
        }

        public Portfolio GetByIndex(int index)
        {
            return this.list.GetByIndex(index);
        }

        public Portfolio GetById(int id)
        {
            return this.list.GetById(id);
        }

        internal void Remove(Portfolio portfolio)
        {
            this.list.Remove(portfolio);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public IEnumerator<Portfolio> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator<Portfolio> IEnumerable<Portfolio>.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }

}
