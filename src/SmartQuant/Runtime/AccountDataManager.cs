// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    internal class AccountDataKey
    {
        public AccountDataKey(AccountData data, params string[] fieldNames)
        {
            _key = string.Join("\u0001", fieldNames.Select(f => GetValue(data, f)));
        }

        public override bool Equals(object obj) => obj is AccountDataKey ? _key.Equals(((AccountDataKey) obj)._key) : base.Equals(obj);

        public override int GetHashCode() => _key.GetHashCode();

        private static string GetValue(AccountData data, string fieldName) => data.Fields[fieldName]?.ToString() ?? string.Empty;

        public override string ToString() => _key;

        private readonly string _key;
    }

    internal class AccountDataTableItem
    {
        public AccountDataFieldList Values { get; }= new AccountDataFieldList();

        public IDictionary<AccountDataKey, AccountDataFieldList> Positions { get; } = new Dictionary<AccountDataKey, AccountDataFieldList>();

        public IDictionary<AccountDataKey, AccountDataFieldList> Orders { get; }= new Dictionary<AccountDataKey, AccountDataFieldList>();
    }

    internal class AccountDataTable
    {
        public IDictionary<string, AccountDataTableItem> Items { get; } = new Dictionary<string, AccountDataTableItem>();
    }

    public class AccountDataManager
    {
        private readonly Framework framework;
        private readonly Dictionary<int, AccountDataTable> tables = new Dictionary<int, AccountDataTable>();

        internal AccountDataManager(Framework framework)
        {
            this.framework = framework;
        }

        public AccountDataSnapshot GetSnapshot(byte providerId, byte route)
        {
            var table = GetTable(providerId, route, false);
            if (table == null)
                return new AccountDataSnapshot(new AccountDataEntry[0]);

            AccountDataSnapshot result;
            lock (table)
            {
                var list = new List<AccountDataEntry>();
                foreach (string current in table.Items.Keys)
                {
                    var class2 = table.Items[current];
                    var accountData = new AccountData(this.framework.Clock.DateTime, AccountDataType.AccountValue, current, providerId, route);
                    CopyFields(class2.Values, accountData.Fields);
                    var list2 = new List<AccountData>();
                    foreach (var current2 in class2.Positions.Values)
                    {
                        var accountData2 = new AccountData(this.framework.Clock.DateTime, AccountDataType.Position, current, providerId, route);
                        CopyFields(current2, accountData2.Fields);
                        list2.Add(accountData2);
                    }
                    var list3 = new List<AccountData>();
                    foreach (var current3 in class2.Orders.Values)
                    {
                        var accountData3 = new AccountData(this.framework.Clock.DateTime, AccountDataType.Order, current, providerId, route);
                        CopyFields(current3, accountData3.Fields);
                        list3.Add(accountData3);
                    }
                    list.Add(new AccountDataEntry(current, accountData, list2.ToArray(), list3.ToArray()));
                }
                result = new AccountDataSnapshot(list.ToArray());
            }
            return result;
        }

        public AccountDataSnapshot[] GetSnapshots()
        {
            lock (this.tables)
                return this.tables.Keys.Select(k => GetSnapshot((byte) (k/256), (byte) (k%256))).ToArray();
        }

        internal void Clear()
        {
            lock (this.tables)
                this.tables.Clear();
        }

        internal void OnAccountData(AccountData data)
        {
            AccountDataTable @class = GetTable(data.ProviderId, data.Route, true);
            lock (@class)
            {
                AccountDataTableItem class2;
                if (!@class.Items.TryGetValue(data.Account, out class2))
                {
                    class2 = new AccountDataTableItem();
                    @class.Items.Add(data.Account, class2);
                }
                switch (data.Type)
                {
                    case AccountDataType.AccountValue:
                        MergeFields(data.Fields, class2.Values);
                        break;
                    case AccountDataType.Position:
                        {
                            AccountDataKey key = new AccountDataKey(data, new[] { "Symbol", "Maturity", "PutOrCall", "Strike" });
                            AccountDataFieldList list;
                            if (!class2.Positions.TryGetValue(key, out list))
                            {
                                list = new AccountDataFieldList();
                                class2.Positions.Add(key, list);
                            }
                            list.Clear();
                            CopyFields(data.Fields, list);
                            break;
                        }
                    case AccountDataType.Order:
                        {
                            AccountDataKey key2 = new AccountDataKey(data, new[] { "OrderID" });
                            AccountDataFieldList list;
                            if (!class2.Orders.TryGetValue(key2, out list))
                            {
                                list = new AccountDataFieldList();
                                class2.Orders.Add(key2, list);
                            }
                            list.Clear();
                            CopyFields(data.Fields, list);
                            break;
                        }
                }
            }
        }

        private AccountDataTable GetTable(byte providerId, byte route, bool addNew)
        {
            var key = providerId * 256 + route;
            AccountDataTable value;
            lock (this.tables)
            {
                if (!this.tables.TryGetValue(key, out value))
                {
                    if (addNew)
                    {
                        value = new AccountDataTable();
                        this.tables.Add(key, value);
                    }
                    else
                    {
                        value = null;
                    }
                }
            }
            return value;
        }

        private static void MergeFields(AccountDataFieldList srcList, AccountDataFieldList dstList)
        {
            foreach (AccountDataField f in srcList)
                dstList[f.Name, f.Currency] = f.Value;
        }

        private static void CopyFields(AccountDataFieldList srcList, AccountDataFieldList dstList)
        {
            foreach (AccountDataField f in srcList)
                dstList.Add(f.Name, f.Currency, f.Value);
        }
    }
}