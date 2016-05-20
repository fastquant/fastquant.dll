// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FastQuant
{
    public class Direction
    {
        public const sbyte Undefined = -1;
        public const sbyte Plus = 0;
        public const sbyte ZeroPlus = 1;
        public const sbyte Minus = 2;
        public const sbyte ZeroMinus = 3;
    }

    public class Trade : Tick
    {
        public override byte TypeId => DataObjectType.Trade;

        public sbyte Direction { get; set; } = -1;

        public Trade(DateTime dateTime, byte providerId, int instrumentId, double price, int size)
            : base(dateTime, providerId, instrumentId, price, size)
        {
        }

        public Trade(DateTime dateTime, byte providerId, int instrumentId, double price, int size, sbyte direction)
            : this(dateTime, providerId, instrumentId, price, size)
        {
            Direction = direction;
        }

        public Trade(DateTime dateTime, DateTime exchangeDateTime, byte providerId, int instrumentId, double price, int size)
            : base(dateTime, providerId, instrumentId, price, size)
        {
        }

        public Trade(DateTime dateTime, DateTime exchangeDateTime, byte providerId, int instrumentId, double price, int size, sbyte direction)
            : this(dateTime, providerId, instrumentId, price, size)
        {
            Direction = direction;
        }

        public Trade()
        {
        }

        public Trade(Tick tick) : base(tick)
        {
        }

        public Trade(Trade trade) : base(trade)
        {
            Direction = trade.Direction;
        }

        public override string ToString()
        {
            return $"Trade {DateTime} {ProviderId} {InstrumentId} {Price} {Size}";
        }
    }

    public delegate void TradeEventHandler(object sender, Trade trade);
}
