using System;
using System.ComponentModel;
using System.Linq;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class DPO : Indicator
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

        public DPO(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"DPO ({this.length}, {this.barData})" : $"DPO ({this.length})";
            this.description = "Detrended Price Oscillator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, BarData barData)
        {
            return index < length/2 + length - 1
                ? double.NaN
                : input[index, barData] - Enumerable.Range(index - length - length/2 + 1, length).Reverse().Sum(i => input[i, barData])/length;
        }
    }
}