using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class LRS : Indicator
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

        public LRS(ISeries input, int length, BarData barData = BarData.Close,
            RegressionDistanceMode distanceMode = RegressionDistanceMode.Time) : base(input)
        {
            this.length = length;
            this.barData = barData;
            DistanceMode = distanceMode;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"LRS ({this.length}, {this.barData})" : $"LRS ({this.length})";
            this.description = "Linear Regression Slope";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            double num = Value(this.input, index, this.length, this.barData, DistanceMode);
            if (!double.IsNaN(num))
                Add(this.input.GetDateTime(index), num);
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
                if (distanceMode == RegressionDistanceMode.Time)
                {
                    double num5 = input.GetDateTime(index).Subtract(input.GetDateTime(index - 1)).Ticks;
                    for (int i = index; i > index - length; i--)
                    {
                        num += input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5;
                        num2 += input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5 * input[i, barData];
                        num3 += input[i, barData];
                        num4 += input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5 * input.GetDateTime(i).Subtract(input.GetDateTime(index - length + 1)).Ticks / num5;
                    }
                }
                else
                {
                    for (int j = index; j > index - length; j--)
                    {
                        num += j - index + length - 1;
                        num2 += (j - index + length - 1) * input[j, barData];
                        num3 += input[j, barData];
                        num4 += ((j - index + length - 1) * (j - index + length - 1));
                    }
                }
                return (length * num2 - num * num3) / (length * num4 - Math.Pow(num, 2.0));
            }
            return double.NaN;
        }
    }
}