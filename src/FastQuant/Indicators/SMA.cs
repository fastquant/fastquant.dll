// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;

namespace FastQuant.Indicators
{
    [Serializable]
    public class SMA : Indicator
    {
        protected BarData barData;
        protected int length;

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

        public SMA(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        public override void Calculate(int index)
        {
            if (index >= this.length - 1)
            {
                double sma;
                if (index == this.length - 1)
                    sma = Value(this.input, index, this.length, this.barData);
                else
                    sma = base[this.input.GetDateTime(index - 1)] + (this.input[index, this.barData] - this.input[index - this.length, this.barData])/this.length;
                Add(this.input.GetDateTime(index), sma);
            }
        }

        protected override void Init()
        {
            this.name = $"SMA ({this.length})";
            this.description = "Simple Moving Average";
            Clear();
            this.calculate = true;
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            return index >= length - 1
                ? Enumerable.Range(index - length + 1, length).Sum(i => input[i, barData])/length
                : double.NaN;
        }
    }
}