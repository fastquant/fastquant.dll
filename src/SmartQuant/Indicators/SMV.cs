// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class SMV : Indicator
    {
        protected int length;
        protected BarData barData;

        [Category("Parameters"), Description("")]
        public int Length
        {
            get { return this.length; }
            set
            {
                this.length = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public BarData BarData
        {
            get { return this.barData; }
            set
            {
                this.barData = value;
                Init();
            }
        }

        public SMV(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = $"SMV ({this.length}, {this.barData})";
            this.description = "Simple Moving Variance";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var smv = Value(this.input, index, this.length, this.barData);
            if (!double.IsNaN(smv))
                Add(this.input.GetDateTime(index), smv);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            if (index >= length - 1)
            {
                var sma = SMA.Value(input, index, length, barData);
                return Enumerable.Range(index - length + 1, length)
                    .Sum(i => (sma - input[i, barData])*(sma - input[i, barData]))/length;
            }
            else
                return double.NaN;
        }
    }
}