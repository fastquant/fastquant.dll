// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

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

    public class Instrument
    {
        public int Factor { get; internal set; }

        public string Symbol { get; internal set; }
    }

    public class InstrumentList : IEnumerable<Instrument>
    {
        public Instrument this[string symbol] => Get(symbol);

        public Instrument Get(string symbol)
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