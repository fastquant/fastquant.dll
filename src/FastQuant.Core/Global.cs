// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class Global : IEnumerable, IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }

        public object this [string key]
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

        public bool ContainsKey(string key)
        {
            return this.data.ContainsKey(key);
        }

        public bool ContainsValue(object obj)
        {
            return this.data.ContainsValue(obj);
        }

        public void Add(string key, object obj)
        {
            this.data.Add(key, obj);
        }

        public void Remove(string key)
        {
            this.data.Remove(key);
        }

        public int GetInt(string key)
        {
            return (int)this.data[key];
        }

        public double GetDouble(string key)
        {
            return (double)this.data[key];
        }

        public string GetString(string key)
        {
            return (string)this.data[key];
        }

        public void Clear()
        {
            this.data.Clear();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.Values.GetEnumerator();
        }
    }
}
