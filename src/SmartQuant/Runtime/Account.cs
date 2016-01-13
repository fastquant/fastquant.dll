// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class Account
    {
        private Framework framework;

        public Account(Framework framework)
        {
            this.framework = framework;
        }

        public bool UpdateParent { get; set; }

        public double Value { get; internal set; }

        public Account Parent { get; internal set; }

        public void Add(AccountReport report)
        {
            Add(new AccountTransaction(report), true);
        }

        public void Add(AccountTransaction transaction, bool updateParent = true)
        {
            throw new NotImplementedException();
        }

        public void Add(DateTime dateTime, double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            throw new NotImplementedException();
        }

        public void Add(double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            throw new NotImplementedException();
        }

        public void Deposit(DateTime dateTime, double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Add(dateTime, value, currencyId, text, updateParent);
        }

        public void Deposit(double value, byte currencyId = 148, string text = null, bool updateParent = true)
        {
            Add(value, currencyId, text, updateParent);
        }
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

    public class AccountTransaction
    {
        public DateTime DateTime { get; }

        public double Value { get; }

        public byte CurrencyId { get; }

        public string Text { get; }

        public AccountTransaction(DateTime dateTime, double value, byte currencyId, string text)
        {
            this.DateTime = dateTime;
            Value = value;
            CurrencyId = currencyId;
            Text = text;
        }

        public AccountTransaction(Fill fill)
            : this(fill.DateTime, fill.CashFlow, fill.CurrencyId, fill.Text)
        {
        }

        public AccountTransaction(AccountReport report)
        {
        }
    }
}