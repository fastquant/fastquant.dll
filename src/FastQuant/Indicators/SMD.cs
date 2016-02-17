// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class SMD : Indicator
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

        public SMD(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = $"SMD ({this.length}, {this.barData})";
            this.description = "Simple Moving Deviation";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var smd = Value(this.input, index, this.length, this.barData);
            if (!double.IsNaN(smd))
                Add(this.input.GetDateTime(index), smd);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            return index < length - 1 ? double.NaN : Math.Sqrt(SMV.Value(input, index, length, barData));
        }
    }
}