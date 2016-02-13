// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class WMA : Indicator
    {
        protected int length;

        protected BarData barData;

        [Category("Parameters"), Description("Length of Weighted Moving Average")]
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
                Init();
            }
        }

        [Category("Parameters"), Description("Which type of data to average")]
        public BarData BarData
        {
            get
            {
                return this.barData;
            }
            set
            {
                this.barData = value;
                Init();
            }
        }

        public WMA(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"WMA ({this.length}, {this.barData})" : $"WMA ({this.length})";
            this.description = "Weighted Moving Average";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var val = Value(this.input, index, this.length, this.barData);
            if (!double.IsNaN(val))
                Add(this.input.GetDateTime(index), val);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            return index < length - 1
                ? double.NaN
                : Enumerable.Range(1, length).Sum(i => input[index - length + i, barData]*i)/Enumerable.Range(1, length).Sum();
        }
    }
}