using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class KRI : Indicator
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
                this.Init();
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
                this.Init();
            }
        }

        public KRI(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"KRI ({this.length}, {this.barData})" : $"KRI ({this.length})";
            this.description = "Kairi Indicator";
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
                var sma = SMA.Value(input, index, length, barData);
                return (input[index, barData] - sma) / sma * 100.0;
            }
            return double.NaN;
        }
    }
}