// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace FastQuant
{
    public class UserItem : INotifyPropertyChanged
    {
        public UserItem()
        {
        }

        public UserItem(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public UserItem(string name, bool value)
        {
            this.name = name;
            this.value = value.ToString();
        }

        public UserItem(string name, int value)
        {
            this.name = name;
            this.value = value.ToString();
        }

        public UserItem(string name, double value)
        {
            this.name = name;
            this.value = value.ToString();
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => $"{this.name} - {this.value}";

        [XmlElement("Name")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                OnPropertyChanged("Name");
            }
        }

        [XmlElement("Value")]
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string name;

        private string value;
    }


    public class User : DataObject, INotifyPropertyChanged
    {
        [Browsable(false)]
        public override byte TypeId => DataObjectType.User;

        [ReadOnly(true), XmlElement("Id")]
        public int Id { get; set; }

        [ReadOnly(true), XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }

        [XmlArray("Items"), XmlArrayItem("Item")]
        public BindingList<UserItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                if (this.items != null)
                    this.items.ListChanged -= this.method_0;
                if (value != null)
                    value.ListChanged += this.method_0;
                this.items = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Add(string name, string value)
        {
            this.items.Add(new UserItem(name, value));
        }

        public void AddDefaultItems()
        {
            Add("Risk Management Enabled", "true");
            Add("Maximum Order Quantity", "100");
            Add("Maximum Order Value", "100");
            Add("Allowed to Trade", "IBM");
            Add("Allowed to Trade", "MSFT");
            Add("Allowed to Trade", "CSCO");
            Add("Restricted to Trade", "GOOG");
            Add("Restricted to Trade", "NVDA");
            Add("Allow Market Order", "true");
            Add("Enable Portfolio Limits", "true");
            Add("Open Order Limit", "100");
            Add("Stop Loss Limit", "100");
            Add("Commited Capital Limit", "100");
            Add("Total Capital Limit", "100");
            Add("Capital Exposure Limit", "100");
            Add("Allowed AlgoId", "203");
            Add("Allowed AlgoId", "204");
            Add("Base Currency", "USD");
            Add("Default AlgoId", "203");
            Add("Allow HistoricalData", "true");
        }

        public bool Contains(string name) => this.items.Any(current => current.Name == name);

        public bool GetBoolValue(string name)
        {
            return this.items.Where(current => current.Name == name).Select(current => bool.Parse(current.Value)).FirstOrDefault();
        }

        public double GetDoubleValue(string name)
        {
            return this.items.Where(current => current.Name == name).Select(current => double.Parse(current.Value)).FirstOrDefault();
        }

        public int GetIntValue(string name)
        {
            return this.items.Where(current => current.Name == name).Select(current => int.Parse(current.Value)).FirstOrDefault();
        }

        public List<UserItem> GetItems(string name) => this.items.Where(current => current.Name == name).ToList();

        public string GetStringValue(string name)
        {
            return this.items.Where(current => current.Name == name).Select(current => current.Value).FirstOrDefault();
        }


        public AccountData ToAccountData(DateTime time, byte providerId, byte route)
        {
            var accountData = new AccountData(time, AccountDataType.AccountValue, Name, providerId, route);
            accountData.Fields.Add("UserName", this.Name);
            foreach (var current in this.items)
            {
                if (!string.IsNullOrWhiteSpace(current.Value))
                {
                    var userItems = GetItems(current.Value);
                    if (userItems.Count == 1)
                    {
                        accountData.Fields.Add(current.Name, current.Value);
                    }
                    else if (accountData.Fields[current.Name] == null)
                    {
                        var array = new object[userItems.Count];
                        for (var i = 0; i < userItems.Count; i++)
                            array[i] = userItems[i].Value;
                        accountData.Fields.Add(current.Name, array);
                    }
                }
            }
            return accountData;
        }


        private BindingList<UserItem> items;

        private void method_0(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.ItemChanged)
                return;
            OnPropertyChanged();
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserList : IEnumerable<User>
    {
        public IEnumerator<User> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class UserManager
    {
        private Framework framework;
        private UserServer server;

        public UserManager(Framework framework, UserServer server)
        {
            this.framework = framework;
            this.server = server;
        }

        public void Load()
        {
            this.server.Load();
        }
    }

    public class UserServer
    {
        private Framework framework;

        public UserServer(Framework framework)
        {
            this.framework = framework;
        }

        public virtual UserList Load()
        {
            return null;
        }

        public virtual void Save(UserList clients)
        {
        }
    }
}