// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class Global : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public int Count => this.data.Count;

        public object this[string key]
        {
            get
            {
                return this.data[key];
            }
            set
            {
                this.data[key] = value;
            }
        }

        public bool ContainsKey(string key) => this.data.ContainsKey(key);

        public bool ContainsValue(object obj) => this.data.ContainsValue(obj);

        public void Add(string key, object obj) => this.data.Add(key, obj);

        public void Remove(string key) => this.data.Remove(key);

        public int GetInt(string key) => (int)this.data[key];

        public double GetDouble(string key) => (double)this.data[key];

        public string GetString(string key) => (string)this.data[key];

        public void Clear() => this.data.Clear();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => this.data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.data.Values.GetEnumerator();
    }
}
