// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SmartQuant
{
    public class Pricer
    {
        private Framework framework;

        public Pricer(Framework framework)
        {
            this.framework = framework;
        }

        public virtual double GetPrice(Position position)
        {
            if (position.Side == PositionSide.Long)
            {
                var bid = position.Instrument.Bid;
                if (bid != null)
                    return bid.Price;
            }
            else
            {
                var ask = position.Instrument.Ask;
                if (ask != null)
                    return ask.Price;
            }
            var trade = position.Instrument.Trade;
            if (trade != null)
                return trade.Price;

            var bar = position.Instrument.Bar;
            if (bar != null)
                return bar.Close;

            return position.AvgPx;
        }

        public virtual double GetValue(Position position) => position.Instrument.Factor == 0 ? position.Price* position.Amount : position.Price* position.Amount* position.Instrument.Factor;
    }
}