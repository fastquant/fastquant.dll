// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SmartQuant
{
    public enum InstrumentType : byte
    {
        Stock,
        Future,
        Option,
        FutureOption,
        Bond,
        FX,
        Index,
        ETF,
        MultiLeg,
        Synthetic
    }

    public enum PutCall : byte
    {
        Put,
        Call
    }

    public enum RequestResult
    {
        Completed,
        Cancelled,
        Error,
    }

    public class Instrument
    {
        [Category("Appearance"), Description("Unique instrument id in the framework")]
        public int Id { get; set; }

        [Browsable(false)]
        public Bid Bid { get; internal set; }

        [Browsable(false)]
        public Ask Ask { get; internal set; }

        [Browsable(false)]
        public Bar Bar { get; }

        [Browsable(false)]
        public Trade Trade { get; }

        [Browsable(false)]
        public ObjectTable Fields { get; set; } = new ObjectTable();

        public List<Leg> Legs { get; } = new List<Leg>();

        public AltIdList AltId { get; } = new AltIdList();

        [Category("Derivative"), DefaultValue(0.0), Description("Contract Value Factor by which price must be adjusted to determine the true nominal value of one futures/options contract. (Qty * Price) * Factor = Nominal Value")]
        public double Factor { get; set; }

        [Category("Appearance"), Description("Instrument symbol")]
        public string Symbol { get; }

        [Category("Appearance"), Description("Instrument description")]
        public string Description { get; set; }

        [Category("Appearance"), Description("Instrument exchange")]
        public string Exchange { get; set; }

        [Category("FX"), Description("Base currency code")]
        public byte CCY1 { get; set; }

        [Category("FX"), Description("Counter currency code")]
        public byte CCY2 { get; set; }

        [Category("Margin"), DefaultValue(0.0), Description("Initial margin (used in simulations)")]
        public double Margin { get; set; }

        [Category("Derivative"), Description("Instrument maturity")]
        public DateTime Maturity { get; set; }

        [Browsable(false)]
        public Instrument Parent { get; set; }

        [Category("Derivative"), Description("Option type : put or call")]
        public PutCall PutCall { get; set; }

        [Category("Derivative"), DefaultValue(0.0), Description("Instrument strike price")]
        public double Strike { get; set; }

        [Category("TickSize"), DefaultValue(0.0), Description("Instrument tick size")]
        public double TickSize { get; set; }

        [Category("Appearance"), Description("Instrument Type (Stock, Futures, Option, Bond, ETF, Index, etc.)")]
        public InstrumentType Type { get; }

        internal Framework Framework { get; set; }

        public byte CurrencyId { get; set; }

        public string Formula { get; set; } = "";

        [Category("Display"), DefaultValue("F2"), Description("C# price format string (example: F4 - show four decimal numbers for Forex contracts)")]
        public string PriceFormat { get; set; } = "F2";

        public Instrument()
        {
        }

        public Instrument(InstrumentType type, string symbol, string description = "", byte currencyId = 148 /* USD */)
        {
            Type = type;
            Symbol = symbol;
            Description = description;
            CurrencyId = currencyId;
        }

        public byte GetCurrencyId(byte providerId)
        {
            var altId = AltId.Get(providerId);
            if (altId != null && altId.CurrencyId != 0)
                return altId.CurrencyId;
            else
                return CurrencyId;
        }

        public override string ToString() => string.IsNullOrEmpty(Description) ? Symbol : $"{Symbol} ({Description})";

        #region Extra

        public bool Saved { get; set; }

        public Instrument(int id, InstrumentType type, string symbol, string description, byte currencyId, string exchange)
        : this(type, symbol, description, currencyId)
        {
            Id = id;
            Exchange = exchange;
        }

        #endregion
    }

    public class InstrumentList : IEnumerable<Instrument>
    {
        public Instrument this[string symbol] => Get(symbol);

        public bool Contains(string symbol)
        {
            throw new NotImplementedException();
        }

        public Instrument Get(string symbol)
        {
            throw new NotImplementedException();
        }

        public Instrument GetByIndex(int index)
        {
            throw new NotImplementedException();
        }


        public IEnumerator<Instrument> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }

    public class InstrumentEventArgs : EventArgs
    {
        public Instrument Instrument { get; private set; }

        public InstrumentEventArgs(Instrument instrument)
        {
            Instrument = instrument;
        }
    }

    public class InstrumentDefinitionEndEventArgs : EventArgs
    {
        public InstrumentDefinitionEnd End { get; private set; }

        public InstrumentDefinitionEndEventArgs(InstrumentDefinitionEnd end)
        {
            End = end;
        }
    }

    public delegate void InstrumentDefinitionEndEventHandler(object sender, InstrumentDefinitionEndEventArgs args);

    public delegate void InstrumentEventHandler(object sender, InstrumentEventArgs args);

    public class InstrumentDefinition
    {
        public string RequestId { get; set; }

        public byte ProviderId { get; set; }

        public int TotalNum { get; set; }

        public Instrument[] Instruments { get; set; }
    }

    public class InstrumentDefinitionRequest
    {
        public string Id { get; set; }

        public InstrumentType? FilterType { get; set; }

        public string FilterSymbol { get; set; }

        public string FilterExchange { get; set; }
    }

    public class InstrumentDefinitionEnd
    {
        public string RequestId { get; set; }

        public RequestResult Result { get; set; }

        public string Text { get; set; }

        public InstrumentDefinitionEnd()
        {
        }

        internal InstrumentDefinitionEnd(string requestId, RequestResult result, string text)
        {
            RequestId = requestId;
            Result = result;
            Text = text;
        }
    }
}