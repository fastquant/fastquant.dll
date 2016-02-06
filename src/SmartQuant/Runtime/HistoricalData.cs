// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class HistoricalData : Event
    {
        public override byte TypeId => EventType.HistoricalData;

        public string RequestId { get; set; }

        public int TotalNum { get; set; }

        public DataObject[] Objects { get; set; }
    }

    public class HistoricalDataEnd : Event
    {
        public override byte TypeId => EventType.HistoricalDataEnd;

        public string RequestId { get; set; }

        public RequestResult Result { get; set; }

        public string Text { get; set; }

        public HistoricalDataEnd()
        {
        }

        internal HistoricalDataEnd(string requestId, RequestResult result, string text)
        {
            RequestId = requestId;
            Result = result;
            Text = text;
        }
    }

    public class HistoricalDataEventArgs : EventArgs
    {
        public HistoricalData Data { get; private set; }

        public HistoricalDataEventArgs(HistoricalData data)
        {
            Data = data;
        }
    }

    public class HistoricalDataEndEventArgs : EventArgs
    {
        public HistoricalDataEnd End { get; private set; }

        public HistoricalDataEndEventArgs(HistoricalDataEnd end)
        {
            End = end;
        }
    }

    public class HistoricalDataRequest
    {
        public string RequestId { get; set; }

        public Instrument Instrument { get; set; }

        public DateTime DateTime1 { get; set; }

        public DateTime DateTime2 { get; set; }

        public byte DataType { get; set; }

        public BarType? BarType { get; set; }

        public long? BarSize { get; set; }

        public HistoricalDataRequest(Instrument instrument, DateTime dateTime1, DateTime dateTime2, byte dataType)
        {
            Instrument = instrument;
            DateTime1 = dateTime1;
            DateTime2 = dateTime2;
            DataType = dataType;
        }

        public HistoricalDataRequest()
        {
        }
    }

    public delegate void HistoricalDataEventHandler(object sender, HistoricalDataEventArgs args);

    public delegate void HistoricalDataEndEventHandler(object sender, HistoricalDataEndEventArgs args);
}