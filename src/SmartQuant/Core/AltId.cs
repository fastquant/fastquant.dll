// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class AltId
    {
        public byte ProviderId { get; set; }

        public string Symbol { get; set; }

        public string Exchange { get; set; }

        public byte CurrencyId { get; set; }

        public AltId()
        {
            throw new NotSupportedException("Don't use this!");
        }

        public AltId(byte providerId, string symbol, string exchange) : this(providerId, symbol, exchange, 0)
        {
        }

        public AltId(byte providerId, string symbol, string exchange, byte currencyId)
        {
            ProviderId = providerId;
            Symbol = symbol;
            Exchange = exchange;
            CurrencyId = currencyId;
        }

        public override string ToString() => $"[{ProviderId}] {Symbol}@{Exchange} {CurrencyId}";
    }

    public class AltIdList : IEnumerable<AltId>
    {
        private readonly IdArray<AltId> array = new IdArray<AltId>();

        private readonly List<AltId> list = new List<AltId>();

        public void Add(AltId id)
        {
            this.array[id.ProviderId] = id;
            this.list.Add(id);
        }

        public void Clear()
        {
            this.array.Clear();
            this.list.Clear();
        }

        public AltId Get(byte providerId)=> this.array[providerId];

        public IEnumerator<AltId> GetEnumerator() => this.list.GetEnumerator();

        public void Remove(AltId id)
        {
            this.array.Remove(id.ProviderId);
            this.list.Remove(id);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => this.list.Count;

        public AltId this[int i]=> this.list[i];
    }
}
