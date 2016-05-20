// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastQuant
{
    public class Quote : DataObject
    {
        public override byte TypeId => DataObjectType.Quote;

        public Bid Bid { get; private set; }

        public Ask Ask { get; private set; }

        public Quote(Bid bid, Ask ask)
        {
            Bid = bid;
            Ask = ask;
            DateTime = bid.DateTime > ask.DateTime ? bid.DateTime : ask.DateTime;
        }

        public Quote(DateTime dateTime, byte providerId, int instrumentId, double bidPrice, int bidSize, double askPrice, int askSize)
            : this(new Bid(dateTime, providerId, instrumentId, bidPrice, bidSize), new Ask(dateTime, providerId, instrumentId, askPrice, askSize))
        {
        }

        #region Extra Helper Methods

        [NotOriginal]
        internal Quote()
        {
        }

        #endregion
    }

    public class Fundamental : DataObject
    {
        internal int ProviderId { get; }

        internal int InstrumentId { get; set; }

        internal ObjectTable Fields { get; set; } = new ObjectTable();

        public Fundamental(DateTime dateTime, int providerId, int instrumentId) : base(dateTime)
        {
            ProviderId = providerId;
            InstrumentId = instrumentId;
        }

        public override string ToString() => string.Join(";", Enumerable.Range(0, Fields.Size).Select(i => $"{i}={this[(byte)i]}"));

        public object this[byte index]
        {
            get
            {
                return Fields[index];
            }
            set
            {
                Fields[index] = value;
            }
        }

        public object this[string name]
        {
            get
            {
                return this[mapping[name]];
            }
            set
            {
                this[mapping[name]] = value;
            }
        }

        public override byte TypeId => DataObjectType.Fundamental;

        public static void AddField(string name, byte index)
        {
            if (mapping.ContainsKey(name))
            {
                Console.WriteLine($"{nameof(Fundamental)}::{nameof(AddField)} with Name = {name} is already exist");
                return;
            }
            if (mapping.ContainsValue(index))
            {
                Console.WriteLine($"{nameof(Fundamental)}::{nameof(AddField)} with Index = {index} is already exist");
                return;
            }
            mapping.Add(name, index);
        }

        public static bool Contains(string name) => mapping.ContainsKey(name);

        public static byte GetIndex(string name) => mapping[name];

        public static void Print() => Console.WriteLine(string.Join(Environment.NewLine, mapping.Keys.Select(k => $"{k}-{mapping[k]}")));

        [UglyNaming]
        public void Print_Temporary()
        {
            var sb = new StringBuilder();
            sb.Append($"Fundamental, Time: {DateTime}{Environment.NewLine}");
            sb.Append($"InstrumentId: {InstrumentId}{Environment.NewLine}");
            sb.Append($"ProviderId: {ProviderId}{Environment.NewLine}");

            foreach (var k in mapping.Keys)
            {
                if (this[k] != null)
                {
                    sb.Append($"{k}={this[k]}{Environment.NewLine}");
                }
            }
            Console.Write(sb.ToString());
        }

        public static void RemoveField(string name) => mapping.Remove(name);

        private static readonly Dictionary<string, byte> mapping = new Dictionary<string, byte>()
        {
            ["CashFlow"] = 1,
            ["PE"] = 2,
            ["Beta"] = 3,
            ["ProfitMargin"] = 4,
            ["ReturnOnEquity"] = 5,
            ["PriceBook"] = 6,
            ["DebtEquity"] = 7,
            ["InterestCoverage"] = 8,
            ["BookValue"] = 9,
            ["PriceSales"] = 10,
            ["DividendPayout"] = 11
        };
    }
}
