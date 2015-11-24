// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class BBU : Indicator
    {
        protected int length;
        protected double k;
        protected BarData barData;

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
                this.Init();
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
                this.Init();
            }
        }

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
                this.Init();
            }
        }

        public BBU(ISeries input, int length, double k, BarData barData = BarData.Close)
        {
            this.length = length;
            this.barData = barData;
            this.k = k;
            Init();
        }

        protected override void Init()
        {
            this.name = $"BBU ({this.length}, {this.k}, { this.barData})";
            this.description = "Bollinger Band Upper";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            double num = BBU.Value(this.input, index, this.length, this.k, this.barData);
            if (!double.IsNaN(num))
            {
                base.Add(this.input.GetDateTime(index), num);
            }
        }

        public static double Value(ISeries input, int index, int length, double k, BarData barData = BarData.Close)
        {
            if (index >= length - 1)
            {
                return SMA.Value(input, index, length, barData) + k * SMD.Value(input, index, length, barData);
            }
            return double.NaN;
        }
    }
}