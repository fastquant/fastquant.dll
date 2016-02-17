// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FastQuant
{
    public enum AccountDataType
    {
        AccountValue,
        Position,
        Order
    }

    public class AccountData : DataObject
    {
        public override byte TypeId => DataObjectType.AccountData;

        public AccountDataType Type { get; }

        public string Account { get; }

        public byte ProviderId { get; }

        public byte Route { get; }

        public AccountDataFieldList Fields { get; } = new AccountDataFieldList();

        public AccountData(DateTime datetime, AccountDataType type, string account, byte providerId, byte route) : base(datetime)
        {
            Type = type;
            Account = account;
            ProviderId = providerId;
            Route = route;
        }
    }

    public class AccountDataSnapshot
    {
        public AccountDataEntry[] Entries { get; }

        internal AccountDataSnapshot(AccountDataEntry[] entries)
        {
            Entries = entries;
        }
    }

    public class AccountDataEntry
    {
        public string Account { get; }

        public AccountData Values { get; }

        public AccountData[] Positions { get; }

        public AccountData[] Orders { get; }

        internal AccountDataEntry(string account, AccountData values, AccountData[] positions, AccountData[] orders)
        {
            Account = account;
            Values = values;
            Positions = positions;
            Orders = orders;
        }
    }

    public class AccountDataField
    {
        public const string SYMBOL = "Symbol";
        public const string EXCHANGE = "Exchange";
        public const string SECURITY_TYPE = "SecurityType";
        public const string CURRENCY = "Currency";
        public const string MATURITY = "Maturity";
        public const string PUT_OR_CALL = "PutOrCall";
        public const string STRIKE = "Strike";
        public const string QTY = "Qty";
        public const string LONG_QTY = "LongQty";
        public const string SHORT_QTY = "ShortQty";
        public const string ORDER_ID = "OrderID";
        public const string ORDER_TYPE = "OrderType";
        public const string ORDER_SIDE = "OrderSide";
        public const string ORDER_STATUS = "OrderStatus";
        public const string PRICE = "Price";
        public const string STOP_PX = "StopPx";

        public string Name { get; }

        public string Currency { get; }

        public object Value { get; }

        public AccountDataField(string name, string currency, object value)
        {
            Name = name;
            Currency = currency;
            Value = value;
        }

        public AccountDataField(string name, object value) : this(name, string.Empty, value)
        {
        }
    }

    public class AccountDataFieldList : ICollection
    {
        private readonly Dictionary<string, Dictionary<string, object>> _fields = new Dictionary<string, Dictionary<string, object>>();

        public int Count => _fields.Values.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public object this[string name, string currency]
        {
            get
            {
                Dictionary<string, object> logger;
                if (!_fields.TryGetValue(name, out logger))
                    return null;
                object value;
                logger.TryGetValue(currency, out value);
                return value;
            }
            internal set
            {
                Add(name, currency, value);
            }
        }

        public object this[string name] => this[name, string.Empty];

        internal AccountDataFieldList()
        {
        }

        /// <summary>
        /// Update if the key is already there.
        /// </summary>
        public void Add(string name, string currency, object value)
        {
            Dictionary<string, object> logger;
            if (!_fields.TryGetValue(name, out logger))
            {
                logger = new Dictionary<string, object>();
                _fields.Add(name, logger);
            }
            logger.Add(currency, value);
        }

        public void Add(string name, object value) => Add(name, string.Empty, value);

        public void Clear() => _fields.Clear();

        public AccountDataField[] ToArray() => _fields.SelectMany(name => name.Value, (logger, field) => new AccountDataField(logger.Key, field.Key, field.Value)).ToArray();

        public void CopyTo(Array array, int index) => ToArray().CopyTo(array, index);

        public IEnumerator GetEnumerator() => ToArray().GetEnumerator();
    }

    public class AccountDataEventArgs : EventArgs
    {
        public AccountData Data { get; }

        public AccountDataEventArgs(AccountData data)
        {
            Data = data;
        }
    }

    public delegate void AccountDataEventHandler(object sender, AccountDataEventArgs args);
}