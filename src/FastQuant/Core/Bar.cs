// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace FastQuant
{
    public class BarSize
    {
        public const long Second = 1;
        public const long Minute = 60;
        public const long Hour = Minute * 60;
        public const long Day = Hour * 24;
        public const long Week = Day * 7;
        public const long Month = Day * 30;
        public const long Year = Day * 365;
    }

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
        private ObjectTable fields;

        private static readonly Dictionary<string, byte> mapping = new Dictionary<string, byte>()
        {
            ["Close"] = 0,
            ["Open"] = 1,
            ["High"] = 2,
            ["Low"] = 3,
            ["Median"] = 4,
            ["Typical"] = 5,
            ["Weighted"] = 6,
            ["Volume"] = 8,
            ["OpenInt"] = 9,
            ["Range"] = 10,
            ["Mean"] = 11,
            ["Variance"] = 12,
            ["StdDev"] = 13
        };

        public override byte TypeId => DataObjectType.Bar;

        public BarType Type { get; set; } = BarType.Time;

        public DateTime CloseDateTime
        {
            get
            {
                return DateTime;
            }
            internal set
            {
                DateTime = value;
            }
        }

        public DateTime OpenDateTime { get; set; } = DateTime.MinValue;

        public TimeSpan Duration => CloseDateTime - OpenDateTime;

        public int InstrumentId { get; set; }

        public BarStatus Status { get; set; }

        public int ProviderId { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Open { get; set; }

        public double Close { get; set; } = double.NaN;

        public long Volume { get; set; }

        public long OpenInt { get; set; }

        public long Size { get; set; }

        public double Range => High - Low;

        public double Median => (High + Low) / 2;

        public double Typical => (High + Low + Close) / 3;

        public double Weighted => (High + Low + 2 * Close) / 4;

        public double Average => (Open + High + Low + Close) / 4;

        public ObjectTable Fields => this.fields ?? (this.fields = new ObjectTable());

        internal long N { get; set; }

        public object this[int index]
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

        public Bar(DateTime openDateTime, DateTime closeDateTime, int instrumentId, BarType type, long size, double open = 0, double high = 0, double low = 0, double close = 0.0, long volume = 0, long openInt = 0)
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
            return $"{nameof(Bar)} [{OpenDateTime} - {CloseDateTime}] Instrument={InstrumentId} Type={Type} Size={Size} Open={Open} High={High} Low={Low} Close={Close} Volume={Volume}";
        }
    }
}