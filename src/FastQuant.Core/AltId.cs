// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class AltId
    {
        public byte ProviderId { get; set; }

        public string Symbol { get; set; }

        public string Exchange { get; set; }

        public AltId()
        {
        }

        public AltId(byte providerId, string symbol, string exchange)
        {
            ProviderId = providerId;
            Symbol = symbol;
            Exchange = exchange;
        }

        public override string ToString() => $"[{ProviderId}] {Symbol}@{Exchange}";
    }

    public class AltIdList : IEnumerable<AltId>
    {
        private GetByList<AltId> lists = new GetByList<AltId>();

        public int Count => this.lists.Count;

        public AltId this[int i] => this.lists.GetByIndex(i);

        public void Clear() => this.lists.Clear();

        public void Add(AltId id) => this.lists.Add(id);

        public void Remove(AltId id) => this.lists.Remove(id);

        public AltId Get(byte providerId) => this.lists.GetById(providerId);

        public IEnumerator<AltId> GetEnumerator() => this.lists.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.lists.GetEnumerator();
    }
}
