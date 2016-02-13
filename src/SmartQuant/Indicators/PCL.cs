using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class PCL : Indicator
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

        public PCL(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"PCL ({this.length})";
            this.description = "Price Channel Lower";
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
            return index < length ? double.NaN : input.GetMin(index - length, index - 1, BarData.Low);
        }
    }
}