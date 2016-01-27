// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SmartQuant
{
    public interface ISlippageProvider
    {
        double Slippage { get; set; }

        double GetPrice(ExecutionReport report);
    }

    public class SlippageProvider : ISlippageProvider
    {
        public double Slippage { get; set; }

        public virtual double GetPrice(ExecutionReport report) => report.AvgPx*(1 + Slippage*(report.Side == OrderSide.Buy ? 1 : -1));
    }
}
