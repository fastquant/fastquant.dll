using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class OSC : Indicator
    {
        protected int length1;
        protected int length2;
        protected BarData barData;

        [Category("Parameters"), Description("")]
        public int Length1
        {
            get
            {
                return this.length1;
            }
            set
            {
                this.length1 = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int Length2
        {
            get
            {
                return this.length2;
            }
            set
            {
                this.length2 = value;
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

        public OSC(ISeries input, int length1, int length2, BarData barData = BarData.Close) : base(input)
        {
            this.length1 = length1;
            this.length2 = length2;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries
                ? $"OSC ({this.length1}, {this.length2}, {this.barData})"
                : $"OSC ({this.length1}, {this.length2}";
            this.description = "Oscillator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length1, this.length2, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length1, int length2, BarData barData = BarData.Close)
        {
            return index < length1 - 1 || index < length2 - 1
                ? double.NaN
                : SMA.Value(input, index, length1, barData) - SMA.Value(input, index, length2, barData);
        }
    }
}