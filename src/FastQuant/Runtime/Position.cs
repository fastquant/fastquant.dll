// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace FastQuant
{
    public enum PositionSide
    {
        Long,
        Short
    }

    public class PositionEventArgs : PortfolioEventArgs
    {
        public Position Position { get; }

        public PositionEventArgs(Portfolio portfolio, Position position) : base(portfolio)
        {
            Position = position;
        }
    }

    public delegate void PositionEventHandler(object sender, PositionEventArgs args);

    public class Position
    {
        private Fill fill;
        private double double_3;

        public double Amount { get; internal set; }

        public DateTime EntryDate => this.fill.DateTime;

        public double EntryPrice => this.fill.Price;

        public double EntryQty => this.fill.Qty;

        public double Price => Portfolio.Pricer.GetPrice(this);

        public double AvgPx { get; private set; }

        public Portfolio Portfolio { get; }

        public int PortfolioId { get; internal set; } = -1;

        public Instrument Instrument { get; }

        public int InstrumentId { get; internal set; } = -1;

        public List<Fill> Fills { get; } = new List<Fill>();

        public double Value => Portfolio.Pricer.GetValue(this);

        public double UPnL => Value - OpenValue;

        public double OpenValue => Math.Sign(Amount) * this.double_3;

        public PositionSide Side => Amount < 0 ? PositionSide.Short : PositionSide.Long;

        public double PnL { get; set; }

        public double Qty => Math.Abs(Amount);

        public double QtyBought { get; internal set; }

        public double QtySold { get; internal set; }

        public Position()
        {
        }

        public Position(Portfolio portfolio, Instrument instrument)
        {
            Portfolio = portfolio;
            Instrument = instrument;
            PortfolioId = portfolio.Id;
            InstrumentId = instrument.Id;
        }

        public void Add(Fill fill)
        {
            Fills.Add(fill);
            if (Amount == 0)
                this.fill = fill;

            if (fill.Side == OrderSide.Buy)
               QtyBought += fill.Qty;
            else
                QtySold += fill.Qty;

            this.method_0(fill);
            Amount = QtyBought - QtySold;
        }

        public string GetSideAsString() => Side == PositionSide.Long ? "Long" : Side == PositionSide.Short ? "Short" : "Undefined";

        public override string ToString() => $"{Instrument} {Side} {Qty}";

        private void method_0(Fill fill_1)
        {
            if (Amount == 0)
            {
                this.double_3 = fill_1.Value;
                AvgPx = fill_1.Price;
                return;
            }
            if ((Side == PositionSide.Long && fill_1.Side == OrderSide.Buy) || (this.Side == PositionSide.Short && fill_1.Side == OrderSide.Sell))
            {
                this.double_3 += fill_1.Value;
                if (Instrument.Factor != 0)
                {
                    AvgPx = this.double_3 / (this.Qty + fill_1.Qty) / Instrument.Factor;
                    return;
                }
                AvgPx = this.double_3 / (this.Qty + fill_1.Qty);
                return;
            }
            else
            {
                if (this.Qty == fill_1.Qty)
                {
                    PnL += this.method_2(fill_1.Price, fill_1.Qty);
                    this.double_3 = 0.0;
                    this.AvgPx = 0.0;
                    return;
                }
                if (this.Qty > fill_1.Qty)
                {
                    PnL += this.method_2(fill_1.Price, fill_1.Qty);
                    this.double_3 -= this.method_1(fill_1.Qty * this.AvgPx);
                    return;
                }
                PnL += this.method_2(fill_1.Price, this.Qty);
                double num = fill_1.Qty - this.Qty;
                this.double_3 = this.method_1(num * fill_1.Price);
                if (Instrument.Factor != 0)
                {
                    this.AvgPx = this.double_3 / num / Instrument.Factor;
                    return;
                }
                this.AvgPx = this.double_3 / num;
                return;
            }
        }

        private double method_1(double double_5)
        {
            return double_5 = Instrument.Factor != 0 ? double_5 * Instrument.Factor : double_5;
        }

        private double method_2(double double_5, double double_6)
        {
            decimal value;
            if (Side == PositionSide.Long)
            {
                value = (decimal)double_5 - (decimal)this.AvgPx;
            }
            else
            {
                value = (decimal)this.AvgPx - (decimal)double_5;
            }
            return this.method_1(double_6 * (double)value);
        }

    }
}