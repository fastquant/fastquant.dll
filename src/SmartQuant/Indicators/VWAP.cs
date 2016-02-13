using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class VWAP : Indicator
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

        public VWAP(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"VWAP ({this.length}, {this.barData}" : $"VWAP ({this.length})";
            this.description = "Volume Weighted Average Price";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            if (index >= length - 1)
            {
                var sum = 0d;
                var size = 0d;
                for (var i = index; i >= index - length + 1; i--)
                {
                    sum += input[i, barData]*input[i, BarData.Volume];
                    size += input[i, BarData.Volume];
                }
                return sum/size;
            }
            return double.NaN;
        }
    }
}