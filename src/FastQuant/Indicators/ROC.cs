using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class ROC : Indicator
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

        public ROC(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"ROC ({this.length}, {this.barData}" : $"ROC ({this.length})";
            this.description = "Price Rate of Change";
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
            return index < length
                ? double.NaN
                : (input[index, barData] - input[index - length, barData])/input[index - length, barData]*100;
        }
    }
}