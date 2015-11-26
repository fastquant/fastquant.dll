// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        public PositionSide Side => Amount < 0 ? PositionSide.Short : PositionSide.Long;
    }
}