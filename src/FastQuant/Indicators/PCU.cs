using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class PCU : Indicator
    {
        protected int length;

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

        public PCU(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"PCU ({this.length})";
            this.description = "Price Channel Upper";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int length)
        {
            return index < length ? double.NaN : input.GetMax(index - length, index - 1, BarData.High);
        }
    }
}