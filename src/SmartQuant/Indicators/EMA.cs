using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class EMA : Indicator
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
        public EMA(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"EMA ({this.length}, {this.barData}" : $"EMA ({this.length})";
            this.description = "Exponential Moving Average";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            double value = double.NaN;
            if (index >= 1)
                value= ValueWithLastValue(input, index, length, Value(input, index - 1, length, barData), barData);
            if (index == 0)
                value= input[0, barData];

            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            if (index >= 1)
                return ValueWithLastValue(input, index, length, Value(input, index - 1, length, barData), barData);
            if (index == 0)
                return input[0, barData];
            return double.NaN;
        }

        public static double ValueWithLastValue(ISeries input, int index, int length, double last, BarData barData = BarData.Close)
        {
            return last + 2.0/(length + 1)*(input[index, barData] - last);
        }
    }
}