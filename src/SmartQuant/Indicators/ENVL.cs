using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class ENVL : Indicator
    {
        protected int length;
        protected double shift;
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
        public new double Shift
        {
            get
            {
                return this.shift;
            }
            set
            {
                this.shift = value;
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

        public ENVL(ISeries input, int length, double shift, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.shift = shift;
            this.barData = barData;
            Init();
        }

        protected override void Init()
        {
            this.name = this.input is BarSeries ? $"ENVL ({this.length}, {this.shift}, {this.barData}" : $"ENVL ({this.length}, {this.shift})";
            this.description = "Envelope Lower";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.shift, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, double shift, BarData barData = BarData.Close)
        {
            return index < length - 1 ? double.NaN : SMA.Value(input, index, length, barData)*(1 - shift/100.0);
        }
    }
}