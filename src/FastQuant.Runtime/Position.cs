// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public enum PositionSide
    {
        Long,
        Short
    }

    public class PositionEventArgs : PortfolioEventArgs
    {
        public Position Position { get; private set; }

        public PositionEventArgs(Portfolio portfolio, Position position) : base(portfolio)
        {
            this.Position = position;
        }
    }

    public delegate void PositionEventHandler(object sender, PositionEventArgs args);

    public class Position
    {
        public double Amount { get; }
        public double EntryPrice { get; set; }
        public double Price => Portfolio.Pricer.GetPrice(this);

        public double Qty { get; set; }
        public double AvgPx { get; }

        public Portfolio Portfolio { get; }
        public int PortfolioId { get; }
        public Instrument Instrument { get; }
        public int InstrumentId { get; }

        public List<Fill> Fills { get; } = new List<Fill>();

        public double Value => Portfolio.Pricer.GetValue(this);

        public double OpenValue { get; }

        public PositionSide Side => Amount < 0 ? PositionSide.Short : PositionSide.Long;

        public double UPnL => Value - OpenValue;

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
            throw new NotImplementedException();
        }

        public string GetSideAsString() => Side == PositionSide.Long ? "Long" : Side == PositionSide.Short ? "Short" : "Undefined";
    }
}