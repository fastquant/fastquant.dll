// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class RSI : Indicator
    {
        protected IndicatorStyle style;
        protected int length;
        protected BarData barData;
        protected TimeSeries upTS;
        protected TimeSeries downTS;

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

        [Category("Parameters"), Description("")]
        public IndicatorStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                this.style = value;
                Init();
            }
        }

        public RSI(ISeries input, int length, BarData barData = BarData.Close, IndicatorStyle style = IndicatorStyle.QuantStudio):base(input)
        {
            this.length = length;
            this.barData = barData;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries
                ? $"RSI ({this.length}, {this.barData})"
                : $"RSI ({this.length})";
            this.description = "Relative Strength Index";
            Clear();
            this.calculate = true;
            this.upTS = new TimeSeries();
            this.downTS = new TimeSeries();
        }

        //TODO: rewrite it!
        public override void Calculate(int index)
        {
            double num = 0.0;
            double num2 = 0.0;
            if (index >= this.length)
            {
                if (this.style == IndicatorStyle.QuantStudio)
                {
                    if (index == this.length)
                    {
                        double num3 = this.input[index - this.length, this.barData];
                        for (int i = index - this.length + 1; i <= index; i++)
                        {
                            double num4 = this.input[i, this.barData];
                            if (num4 > num3)
                            {
                                num += num4 - num3;
                            }
                            else
                            {
                                num2 += num3 - num4;
                            }
                            num3 = num4;
                        }
                    }
                    else
                    {
                        num = this.upTS[index - 1] * this.length;
                        num2 = this.downTS[index - 1] * this.length;
                        double num4 = this.input[index, this.barData];
                        double num3 = this.input[index - 1, this.barData];
                        if (num4 > num3)
                        {
                            num += num4 - num3;
                        }
                        else
                        {
                            num2 += num3 - num4;
                        }
                        num4 = this.input[index - this.length, this.barData];
                        num3 = this.input[index - this.length - 1, this.barData];
                        if (num4 > num3)
                        {
                            num -= num4 - num3;
                        }
                        else
                        {
                            num2 -= num3 - num4;
                        }
                    }
                }
                else if (index == this.length)
                {
                    double num3 = this.input[index - this.length, this.barData];
                    for (int j = index - this.length + 1; j <= index; j++)
                    {
                        double num4 = this.input[j, this.barData];
                        if (num4 > num3)
                        {
                            num += num4 - num3;
                        }
                        else
                        {
                            num2 += num3 - num4;
                        }
                        num3 = num4;
                    }
                }
                else
                {
                    num = this.upTS[index - 1] * (this.length - 1);
                    num2 = this.downTS[index - 1] * (this.length - 1);
                    double num4 = this.input[index, this.barData];
                    double num3 = this.input[index - 1, this.barData];
                    if (num4 > num3)
                    {
                        num += num4 - num3;
                    }
                    else
                    {
                        num2 += num3 - num4;
                    }
                }
                double num5 = 100.0 - 100.0 / (1.0 + num / num2);
                num /= (double)this.length;
                num2 /= (double)this.length;
                if (!double.IsNaN(num5))
                {
                    base.Add(this.input.GetDateTime(index), num5);
                }
            }
            this.upTS.Add(this.input.GetDateTime(index), num);
            this.downTS.Add(this.input.GetDateTime(index), num2);
        }

        //TODO: rewrite it!
        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close, IndicatorStyle style = IndicatorStyle.QuantStudio)
        {
            double num = 0.0;
            double num2 = 0.0;
            if (index >= length)
            {
                if (style == IndicatorStyle.QuantStudio)
                {
                    double num3 = input[index - length, barData];
                    for (int i = index - length + 1; i <= index; i++)
                    {
                        double num4 = input[i, barData];
                        if (num4 > num3)
                        {
                            num += num4 - num3;
                        }
                        else
                        {
                            num2 += num3 - num4;
                        }
                        num3 = num4;
                    }
                }
                else
                {
                    double num3 = input[0, barData];
                    for (int j = 1; j <= length; j++)
                    {
                        double num4 = input[j, barData];
                        if (num4 > num3)
                        {
                            num += num4 - num3;
                        }
                        else
                        {
                            num2 += num3 - num4;
                        }
                        num3 = num4;
                    }
                    num /= (double)length;
                    num2 /= (double)length;
                    num3 = input[length, barData];
                    for (int k = length + 1; k <= index; k++)
                    {
                        num *= (double)(length - 1);
                        num2 *= (double)(length - 1);
                        double num4 = input[k, barData];
                        if (num4 > num3)
                        {
                            num += num4 - num3;
                        }
                        else
                        {
                            num2 += num3 - num4;
                        }
                        num3 = num4;
                        num /= (double)length;
                        num2 /= (double)length;
                    }
                }
                return 100.0 - 100.0 / (1.0 + num / num2);
            }
            return double.NaN;
        }
    }
}