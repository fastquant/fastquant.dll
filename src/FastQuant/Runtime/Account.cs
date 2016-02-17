// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

using CId = FastQuant.CurrencyId;

namespace FastQuant
{
    public class Account
    {
        private Framework framework;

        public List<AccountPosition> Positions = new List<AccountPosition>();

        public List<AccountTransaction> Transactions = new List<AccountTransaction>();

        private IdArray<AccountPosition> positionsByCurrencyId = new IdArray<AccountPosition>();

        public byte CurrencyId { get; set; } = CId.USD;

        public bool UpdateParent { get; set; } = true;

        public double Value => Positions.Sum(p => this.framework.CurrencyConverter.Convert(p.Value, p.CurrencyId, CurrencyId));

        public Account Parent { get; internal set; }

        public Account(Framework framework)
        {
            this.framework = framework;
        }

        public void Add(AccountReport report)
        {
            Add(new AccountTransaction(report), true);
        }

        public void Add(Fill fill, bool updateParent = true)
        {
            Add(new AccountTransaction(fill), updateParent);
        }

        public void Add(AccountTransaction transaction, bool updateParent = true)
        {
            var position = this.positionsByCurrencyId[transaction.CurrencyId];
            if (position == null)
            {
                position = new AccountPosition(transaction);
                this.positionsByCurrencyId[position.CurrencyId] = position;
                Positions.Add(position);
            }
            else
            {
                position.Add(transaction);
            }

            Transactions.Add(transaction);
            if (updateParent && UpdateParent)
                Parent?.Add(transaction.DateTime, transaction.Value, transaction.CurrencyId, transaction.Text, updateParent);
        }

        public void Add(DateTime dateTime, double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Add(new AccountTransaction(dateTime, value, currencyId, text), updateParent);
        }

        public void Add(double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Add(new AccountTransaction(this.framework.Clock.DateTime, value, currencyId, text), updateParent);
        }

        public void Deposit(DateTime dateTime, double value, byte currencyId = CId.USD, string text = null,
            bool updateParent = true)
        {
            Add(dateTime, value, currencyId, text, updateParent);
        }

        public void Deposit(double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Add(value, currencyId, text, updateParent);
        }

        public void Withdraw(double value, byte currencyId = CId.USD, string text = null, bool updateParent = true)
        {
            Add(-value, currencyId, text, updateParent);
        }

        public void Withdraw(DateTime dateTime, double value, byte currencyId = CId.USD, string text = null,
            bool updateParent = true)
        {
            Add(dateTime, -value, currencyId, text, updateParent);
        }

        public AccountPosition GetByCurrencyId(byte currencyId) => this.positionsByCurrencyId[currencyId];

        public double GetValue(byte currencyId) => this.positionsByCurrencyId[currencyId]?.Value ?? 0;
    }

    public class AccountPosition
    {
        public byte CurrencyId { get; }

        public double Value { get; private set; }

        public AccountPosition(byte currencyId, double value)
        {
            CurrencyId = currencyId;
            Value = value;
        }

        public AccountPosition(AccountTransaction transaction)
            : this(transaction.CurrencyId, transaction.Value)
        {
        }

        public void Add(AccountTransaction transaction)
        {
            Value += transaction.Value;
        }
    }

    public class AccountTransaction : Event
    {
        public override byte TypeId => EventType.AccountTransaction;

        public double Value { get; }

        public byte CurrencyId { get; }

        public string Text { get; }

        public AccountTransaction(DateTime dateTime, double value, byte currencyId, string text)
        {
            DateTime = dateTime;
            Value = value;
            CurrencyId = currencyId;
            Text = text;
        }

        public AccountTransaction(Fill fill) : this(fill.DateTime, fill.CashFlow, fill.CurrencyId, fill.Text)
        {
        }

        public AccountTransaction(AccountReport report)
            : this(report.DateTime, report.Amount, report.CurrencyId, report.Text)
        {
        }
    }
}