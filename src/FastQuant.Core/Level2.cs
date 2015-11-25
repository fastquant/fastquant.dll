// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public enum Level2Side : byte
    {
        Bid,
        Ask
    }

    public enum Level2UpdateAction
    {
        New,
        Change,
        Delete,
        Reset
    }

    public class Level2 : Tick
    {
        public override byte TypeId
        {
            get
            {
                return DataObjectType.Level2;
            }
        }

        public Level2Side Side { get; set; }

        public Level2UpdateAction Action { get; set; }

        public int Position { get; set; }

        public Level2(DateTime dateTime, byte providerId, int instrumentId, double price, int size, Level2Side side, Level2UpdateAction action, int position) : base(dateTime, providerId, instrumentId, price, size)
        {
            Side = side;
            Action = action;
            Position = position;
        }

        public Level2()
        {
        }
    }

    public class Level2Update : DataObject
    {
        public byte ProviderId { get; set; }

        public int InstrumentId { get; set; }

        public Level2[] Entries { get; internal set; }

        public override byte TypeId => DataObjectType.Level2Update;

        public Level2Update(DateTime dateTime, byte providerId, int instrumentId, Level2[] entries)
            : base(dateTime)
        {
            ProviderId = providerId;
            InstrumentId = instrumentId;
            Entries = entries;
        }

        public Level2Update()
        {
        }
    }

    public class Level2Snapshot : DataObject
    {
        public override byte TypeId => DataObjectType.Level2Snapshot;

        public byte ProviderId { get; set; }

        public int InstrumentId { get; set; }

        public Bid[] Bids { get; internal set; }

        public Ask[] Asks { get; internal set; }

        public Level2Snapshot(DateTime dateTime, byte providerId, int instrumentId, Bid[] bids, Ask[] asks)
            : base(dateTime)
        {
            ProviderId = providerId;
            InstrumentId = instrumentId;
            Bids = bids;
            Asks = asks;
        }

        public Level2Snapshot()
        {
        }
    }
}
