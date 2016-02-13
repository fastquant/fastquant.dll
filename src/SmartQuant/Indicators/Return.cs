// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant.Indicators
{
    public class Return : Indicator
    {
        public Return(ISeries input) : base(input)
        {
        }

        public override void Calculate(int index)
        {
            Add(this.input.GetDateTime(index), index > 0 ? this.input[index, BarData.Close]/this.input[index - 1, BarData.Close] - 1 : 0);
        }
    }

    public class LogReturn : Indicator
    {
        public LogReturn(ISeries input) : base(input)
        {
        }

        public override void Calculate(int index)
        {
            Add(this.input.GetDateTime(index), index > 0 ? Math.Log(this.input[index, BarData.Close]/this.input[index - 1, BarData.Close]) : 0);
        }
    }
}
