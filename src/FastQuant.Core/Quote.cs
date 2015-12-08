// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
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

        internal Quote()
        {
        }

        #endregion
    }
}
