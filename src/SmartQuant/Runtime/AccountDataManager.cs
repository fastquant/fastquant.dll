using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    class Class3
    {
        public Class3(AccountData data, params string[] fieldNames)
        {
            this.text = string.Join("\u0001", fieldNames.Select(f => this.method_0(data, f)));
        }

        public override bool Equals(object obj) => obj is Class3 ? this.text.Equals(((Class3)obj).text) : base.Equals(obj);

        public override int GetHashCode() => this.text.GetHashCode();

        private string method_0(AccountData data, string name)
        {
            var obj = data.Fields[name];
            return obj != null ? obj.ToString() : string.Empty;
        }

        public override string ToString() => this.text;

        private string text;
    }

    class Class5
    {
        public AccountDataFieldList accountDataFieldList_0 = new AccountDataFieldList();

        public IDictionary<Class3, AccountDataFieldList> idictionary_0 = new Dictionary<Class3, AccountDataFieldList>();

        public IDictionary<Class3, AccountDataFieldList> idictionary_1 = new Dictionary<Class3, AccountDataFieldList>();
    }

    class Class4
    {
        public IDictionary<string, Class5> dict { get; set; } = new Dictionary<string, Class5>();
    }

    public class AccountDataManager
    {
        private Framework framework;
        private Dictionary<int, Class4> items = new Dictionary<int, Class4>();

        internal AccountDataManager(Framework framework)
        {
            this.framework = framework;
        }

        public AccountDataSnapshot GetSnapshot(byte providerId, byte route)
        {
            Class4 @class = Get(providerId, route, false);
            if (@class == null)
                return new AccountDataSnapshot(new AccountDataEntry[0]);

            AccountDataSnapshot result;
            lock (@class)
            {
                var list = new List<AccountDataEntry>();
                foreach (string current in @class.dict.Keys)
                {
                    var class2 = @class.dict[current];
                    var accountData = new AccountData(this.framework.Clock.DateTime, AccountDataType.AccountValue, current, providerId, route);
                    this.AddWith(class2.accountDataFieldList_0, accountData.Fields);
                    var list2 = new List<AccountData>();
                    foreach (var current2 in class2.idictionary_0.Values)
                    {
                        var accountData2 = new AccountData(this.framework.Clock.DateTime, AccountDataType.Position, current, providerId, route);
                        this.AddWith(current2, accountData2.Fields);
                        list2.Add(accountData2);
                    }
                    var list3 = new List<AccountData>();
                    foreach (var current3 in class2.idictionary_1.Values)
                    {
                        var accountData3 = new AccountData(this.framework.Clock.DateTime, AccountDataType.Order, current, providerId, route);
                        this.AddWith(current3, accountData3.Fields);
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
            var list = new List<AccountDataSnapshot>();
            lock (this.items)
            {
                foreach (int current in this.items.Keys)
                {
                    byte providerId = (byte)(current / 256);
                    byte route = (byte)(current % 256);
                    list.Add(GetSnapshot(providerId, route));
                }
            }
            return list.ToArray();
        }

        internal void Clear()
        {
            lock (this.items)
                this.items.Clear();
        }

        internal void OnAccountData(AccountData data)
        {
            Class4 @class = Get(data.ProviderId, data.Route, true);
            lock (@class)
            {
                Class5 class2;
                if (!@class.dict.TryGetValue(data.Account, out class2))
                {
                    class2 = new Class5();
                    @class.dict.Add(data.Account, class2);
                }
                switch (data.Type)
                {
                    case AccountDataType.AccountValue:
                        UpdateWith(data.Fields, class2.accountDataFieldList_0);
                        break;
                    case AccountDataType.Position:
                        {
                            Class3 key = new Class3(data, new[] { "Symbol", "Maturity", "PutOrCall", "Strike" });
                            AccountDataFieldList list;
                            if (!class2.idictionary_0.TryGetValue(key, out list))
                            {
                                list = new AccountDataFieldList();
                                class2.idictionary_0.Add(key, list);
                            }
                            list.Clear();
                            this.AddWith(data.Fields, list);
                            break;
                        }
                    case AccountDataType.Order:
                        {
                            Class3 key2 = new Class3(data, new[] { "OrderID" });
                            AccountDataFieldList list;
                            if (!class2.idictionary_1.TryGetValue(key2, out list))
                            {
                                list = new AccountDataFieldList();
                                class2.idictionary_1.Add(key2, list);
                            }
                            list.Clear();
                            this.AddWith(data.Fields, list);
                            break;
                        }
                }
            }
        }

        private Class4 Get(byte providerId, byte route, bool createNotExist)
        {
            int key = providerId * 256 + route;
            Class4 value;
            lock (this.items)
            {
                if (!this.items.TryGetValue(key, out value))
                {
                    if (createNotExist)
                    {
                        value = new Class4();
                        this.items.Add(key, value);
                    }
                    else
                    {
                        value = null;
                    }
                }
            }
            return value;
        }

        private void UpdateWith(AccountDataFieldList list1, AccountDataFieldList list2)
        {
            foreach (AccountDataField f in list1)
                list2[f.Name, f.Currency] = f.Value;
        }

        private void AddWith(AccountDataFieldList list1, AccountDataFieldList list2)
        {
            foreach (AccountDataField f in list1)
                list2.Add(f.Name, f.Currency, f.Value);
        }
    }
}