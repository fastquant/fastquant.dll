// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public enum CommissionType
    {
        PerShare,
        Percent,
        Absolute
    }

    public interface ICommissionProvider
    {
        CommissionType Type { get; set; }

        double Commission { get; set; }

        double MinCommission { get; set; }

        double GetCommission(ExecutionReport report);
    }

    public class CommissionProvider : ICommissionProvider
    {
        public double Commission { get; set; }

        public CommissionType Type { get; set; }

        public double MinCommission { get; set; }

        public virtual double GetCommission(ExecutionReport report)
        {
            double val;
            switch (Type)
            {
                case CommissionType.PerShare:
                    val = Commission*report.CumQty;
                    break;
                case CommissionType.Percent:
                    val = Commission*report.CumQty*report.AvgPx;
                    break;
                case CommissionType.Absolute:
                    val = Commission;
                    break;
                default:
                    throw new NotSupportedException($"Unknown commission type {Type}");
            }
            return Math.Max(val, MinCommission);
        }
    }
}
