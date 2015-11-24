using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    public class SMA : Indicator
    {
        protected BarData barData;
        protected int length;

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

        public SMA(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
            this.length = length;
            this.barData = barData;
            Init();
        }

        public override void Calculate(int index)
        {
        }

        protected override void Init()
        {
            this.name = $"SMA ({ this.length })";
            this.description = "Simple Moving Average";
            Clear();
            this.calculate = true;
        }

        public static double Value(ISeries input, int index, int length, BarData barData = BarData.Close)
        {
            throw new NotImplementedException();
        }
    }
}