using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class B : Indicator
    {
        protected int length;
        protected double k;
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
        public double K
        {
            get
            {
                return this.k;
            }
            set
            {
                this.k = value;
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

        public B(ISeries input, int length, double k, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            this.k = k;
            Init();
        }

        protected override void Init()
        {
            this.name = $"B ({this.length}, {this.k}, {this.barData})";
            this.description = "% Bollinger Bands";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length, this.k, this.barData);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length, double k, BarData barData = BarData.Close)
        {
            if (index >= length - 1)
            {
                var bbl = BBL.Value(input, index, length, k, barData);
                var bbu = BBU.Value(input, index, length, k, barData);
                return (input[index, barData] - bbl) / (bbu - bbl);
            }
            return double.NaN;
        }
    }
}