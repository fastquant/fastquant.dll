using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    public class HV : Indicator
    {
        protected int length;
        protected BarData barData;
        protected double span;

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
        public double Span
        {
            get
            {
                return this.span;
            }
            set
            {
                this.span = value;
                Init();
            }
        }


        public HV(ISeries input, int length, double span, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.span = span;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"HV ({this.length}, {this.span}, {this.barData})" : $"HV ({this.length}, {this.span})";
            this.description = "Historical Volatility";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.span, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, double span, BarData barData = BarData.Close)
        {
            if (index >= length)
            {
                var array = new double[length];
                var sum = 0.0;
                for (var i = index; i > index - length; i--)
                {
                    array[i - index + length - 1] = Math.Log(input[i, barData] / input[i - 1, barData]);
                    sum += array[i - index + length - 1];
                }
                return Math.Sqrt(array.Sum(t => Math.Pow(t - sum / length, 2.0)) / (length - 1)) * 100.0 * Math.Sqrt(span);
            }
            return double.NaN;
        }
    }
}