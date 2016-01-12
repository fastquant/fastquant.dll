// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class BBL : Indicator
    {
        protected int length;
        protected double k;
        protected BarData barData;

        [Category("Parameters"), Description("")]
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

        [Category("Parameters"), Description("")]
        public double K
        {
            get
            {
                return this.k;
            }
            set
            {
                this.k = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
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

        public BBL(ISeries input, int length, double k, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.k = k;
            this.barData = barData;
            Init();
        }

        public override void Calculate(int index)
        {
            double bbl = Value(this.input, index, this.length, this.k, this.barData);
            if (!double.IsNaN(bbl))
                Add(this.input.GetDateTime(index), bbl);
        }

        protected override void Init()
        {
            this.name = $"BBL ({this.length}, {this.k}, {this.barData})";
            this.description = "Bollinger Band Lower";
            Clear();
            this.calculate = true;
        }

        public static double Value(ISeries input, int index, int length, double k, BarData barData = BarData.Close)
        {
            return index >= length - 1
                ? SMA.Value(input, index, length, barData) - k*SMD.Value(input, index, length, barData)
                : double.NaN;
        }
    }
}