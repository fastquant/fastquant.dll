// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public interface IIdNamedItem
    {
        int Id { get; }
        string Name { get; }
    }

    public class IdNamedItem
    {
        protected int id = -1;
        protected string name = string.Empty;

        public int Id => this.id;

        public string Name => this.name;
    }

    public class IdNamedList : IEnumerable<IIdNamedItem>
    {
        private readonly GetByList<IIdNamedItem> items;

        public int Count => this.items.Count;

        public IIdNamedItem this[string name] => Get(name);

        public IIdNamedItem this[int id] => GetById(id);

        public IdNamedList(int size = 10240)
        {
            items = new GetByList<IIdNamedItem>("Id", "Name", size);
        }

        public void Add(IIdNamedItem value)
        {
            if (!Contains(value.Id))
                this.items.Add(value);
            else
                Console.WriteLine($"IdNamedList::Add object {value.Name} with Id = {value.Id} is already in the list");
        }

        public void Clear() => this.items.Clear();

        public bool Contains(string name) => this.items.Contains(name);

        public bool Contains(int id) => this.items.Contains(id);

        public IIdNamedItem Get(string name) => this.items.GetByName(name);

        public IIdNamedItem GetById(int id) => this.items.GetById(id);

        public IIdNamedItem GetByIndex(int index) => this.items.GetByIndex(index);

        public override string ToString() => string.Join(Environment.NewLine, this.items.Select(i => i.Name));

        public void Remove(IIdNamedItem value) => this.items.Remove(value);

        public IEnumerator<IIdNamedItem> GetEnumerator() => this.items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}