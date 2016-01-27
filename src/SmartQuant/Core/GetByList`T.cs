// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace SmartQuant
{
    [NotOriginal]
    class GetByList<T> : IEnumerable<T>
    {
        private readonly Dictionary<string, T> dictionary;
        private readonly IdArray<T> array;
        private readonly List<T> list;

        public int Count => this.list.Count;

        private readonly MethodInfo nameMethod;
        private readonly MethodInfo idMethod;

        public T this[int index]
        {
            get
            {
                return this.list[index];
            }
            set
            {
                this.list[index] = value;
            }
        }

        public GetByList(string idPropName, string namePropName, int size = 1024)
        {
            var t = typeof(T);
            this.idMethod = t.GetProperty(idPropName).GetGetMethod();
            this.nameMethod = t.GetProperty(namePropName).GetGetMethod();
            this.dictionary = new Dictionary<string, T>();
            this.array = new IdArray<T>(size);
            this.list = new List<T>();
        }

        public bool Contains(T obj)
        {
            string name = (string)this.nameMethod.Invoke(obj, new object[0]);
            return Contains(name);
        }

        public bool Contains(string name) => this.dictionary.ContainsKey(name);

        public bool Contains(int id) => this.array[id] != null;

        public void Add(T obj)
        {
            int id = Convert.ToInt32(this.idMethod.Invoke(obj, new object[0]));
            if (this.array[id] == null)
            {
                this.list.Add(obj);
                string name = (string)this.nameMethod.Invoke(obj, new object[0]);
                if (name != null)
                    this.dictionary[name] = obj;
                this.array[id] = obj;
            }
            else
                Console.WriteLine($"GetByList::Add Object with id = {id} is already in the list");
        }

        public void Remove(int id)
        {
            throw new NotImplementedException("don't use it");
        }

        public void Remove(T obj)
        {
            string name = (string)this.nameMethod.Invoke(obj, new object[0]);
            int id = (int)this.idMethod.Invoke(obj, new object[0]);
            this.list.Remove(obj);
            if (name != null)
                this.dictionary.Remove(name);
            this.array.Remove(id);
        }

        public T GetByName(string name)
        {
            T obj;
            this.dictionary.TryGetValue(name, out obj);
            return obj;
        }

        public T GetByIndex(int index) => this.list[index];

        public T GetById(int id) => this.array[id];

        public void Clear()
        {
            this.dictionary.Clear();
            this.array.Clear();
            this.list.Clear();
        }

        public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
