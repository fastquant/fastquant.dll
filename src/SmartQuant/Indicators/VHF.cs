using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class VHF : Indicator
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

        public VHF(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        { 
            this.name = this.input is BarSeries ? $"VHF ({this.length}, {this.barData}" : $"VHF ({this.length})";
            this.description = "Vertical Horizontal Filter";
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
            if (index >= length)
            {
                var hcp = input.GetMax(index - length + 1, index, barData);
                var lcp = input.GetMin(index - length + 1, index, barData);
                double sum = 0;
                for (var i = index; i > index - length; i--)
                    sum += Math.Abs(input[i, barData] - input[i - 1, barData]);
                return Math.Abs(hcp - lcp) / sum;
            }
            return double.NaN;
        }
    }
}