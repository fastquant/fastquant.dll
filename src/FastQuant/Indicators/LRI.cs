using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class LRI : Indicator
    {
        protected int length;
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
        public RegressionDistanceMode DistanceMode { get; set; }

        public LRI(ISeries input, int length, BarData barData = BarData.Close,
            RegressionDistanceMode distanceMode = RegressionDistanceMode.Time) : base(input)
        {
            this.length = length;
            this.barData = barData;
            DistanceMode = distanceMode;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"LRI ({this.length}, {this.barData})" : $"LRI ({this.length})";
            this.description = "Linear Regression Indicator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.barData, DistanceMode);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        // TODO: rewrite
        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close, RegressionDistanceMode distanceMode = RegressionDistanceMode.Time)
        {
            if (index >= length - 1)
            {
                double num = 0.0;
                double num2 = 0.0;
                double num3 = 0.0;
                double num4 = 0.0;
                double num6;
                if (distanceMode == RegressionDistanceMode.Time)
                {
                    double num5 = (double)input.GetDateTime(index).Subtract(input.GetDateTime(index - 1)).Ticks;
                    for (int i = index; i > index - length; i--)
                    {
                        num += (double)input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5;
                        num2 += (double)input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5 * input[i, barData];
                        num3 += input[i, barData];
                        num4 += (double)input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5 * (double)input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5;
                    }
                    num6 = (double)input.GetDateTime(index).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5;
                }
                else
                {
                    for (int j = index; j > index - length; j--)
                    {
                        num += (double)(j - index + length - 1);
                        num2 += (double)(j - index + length - 1) * input[j, barData];
                        num3 += input[j, barData];
                        num4 += (double)((j - index + length - 1) * (j - index + length - 1));
                    }
                    num6 = (double)(length - 1);
                }
                double num7 = ((double)length * num2 - num * num3) / ((double)length * num4 - Math.Pow(num, 2.0));
                double num8 = (num3 - num7 * num) / (double)length;
                return num7 * num6 + num8;
            }
            return double.NaN;
        }
    }
}