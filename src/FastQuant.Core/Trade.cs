// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class Trade : Tick
    {
        public override byte TypeId => DataObjectType.Trade;

        public Trade(DateTime dateTime, byte providerId, int instrumentId, double price, int size)
            : base(dateTime, providerId, instrumentId, price, size)
        {
        }

        public Trade(DateTime dateTime, DateTime exchangeDateTime, byte providerId, int instrumentId, double price, int size)
            : base(dateTime, providerId, instrumentId, price, size)
        {
        }

        public Trade()
        {
        }

        public Trade(Trade trade)
            : base(trade)
        {
        }

        public override string ToString()
        {
            return $"Trade {DateTime} {ProviderId} {InstrumentId} {Price} {Size}";
        }
    }
}
