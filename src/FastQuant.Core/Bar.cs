// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public enum BarData
    {
        Close,
        Open,
        High,
        Low,
        Median,
        Typical,
        Weighted,
        Average,
        Volume,
        OpenInt,
        Range,
        Mean,
        Variance,
        StdDev
    }

    public enum BarType : byte
    {
        Time = 1,
        Tick,
        Volume,
        Range,
        Session
    }

    public enum BarInput
    {
        Trade,
        Bid,
        Ask,
        Middle,
        Tick,
        BidAsk,
    }

    public enum BarStatus : byte
    {
        Incomplete,
        Complete,
        Open,
        High,
        Low,
        Close,
    }

    public class Bar : DataObject
    {
        private IdArray<double> fields;

        private static Dictionary<string, byte> mapping;

        public override byte TypeId => DataObjectType.Bar;

        public BarType Type { get; set; } = BarType.Time;

        public DateTime CloseDateTime => DateTime;

        public DateTime OpenDateTime { get; set; } = DateTime.MinValue;

        public TimeSpan Duration => CloseDateTime - OpenDateTime;

        public int InstrumentId { get; set; }

        public BarStatus Status { get; set; }

        public int ProviderId { get; set; }

        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; } = double.NaN;

        public long Volume { get; set; }

        public long OpenInt { get; set; }

        public long N { get; set; }

        public long Size { get; set; }

        public double Mean { get; set; }

        public double Variance { get; set; }

        public double StdDev => Math.Sqrt(Variance);

        public double Range => High - Low;

        public double Median => (High + Low) / 2;

        public double Typical => (High + Low + Close) / 3;

        public double Weighted => (High + Low + 2 * Close) / 4;

        public double Average => (Open + High + Low + Close) / 4;

        public double this[byte index]
        {
            get
            {
                return this.fields[index];
            }
            set
            {
                if (this.fields == null)
                    this.fields = new IdArray<double>(16);
                this.fields[index] = value;
            }
        }

        public double this[string name]
        {
            get
            {
                return this.fields[mapping[name]];
            }
            set
            {
                this[mapping[name]] = value;
            }
        }

        static Bar()
        {
            mapping = new Dictionary<string, byte>();
            foreach (var field in Enum.GetNames(typeof(BarData)))
                AddField(field, (byte)Enum.Parse(typeof(BarData), field));
        }

        public Bar(DateTime openDateTime, DateTime closeDateTime, int instrumentId, BarType type, long size, double open = 0.0, double high = 0.0, double low = 0.0, double close = 0.0, long volume = 0, long openInt = 0)
           : base(closeDateTime)
        {
            OpenDateTime = openDateTime;
            InstrumentId = instrumentId;
            Type = type;
            Size = size;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            OpenInt = openInt;
            Status = openDateTime == closeDateTime ? BarStatus.Open : BarStatus.Close;
        }

        public Bar()
        {
        }

        public Bar(Bar bar) : base(bar)
        {
            InstrumentId = bar.InstrumentId;
            Type = bar.Type;
            Size = bar.Size;
            OpenDateTime = bar.OpenDateTime;
            Open = bar.Open;
            High = bar.High;
            Low = bar.Low;
            Close = bar.Close;
            Volume = bar.Volume;
            OpenInt = bar.OpenInt;
        }

        public static void AddField(string name, byte index) => mapping.Add(name, index);

        public override string ToString()
        {
            return $"Bar [{OpenDateTime} - {DateTime}] Instrument={InstrumentId} Type={Type} Size={Size} Open={Open} High={High} Low={Low} Close={Close} Volume={Volume}";
        }
    }

}