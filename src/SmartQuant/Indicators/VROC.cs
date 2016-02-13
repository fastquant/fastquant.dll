using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class VROC : Indicator
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

        public VROC(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.length);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        protected override void Init()
        {
            this.name = $"VROC ({this.length})";
            this.description = "Volume Rate of Change";
            Clear();
            this.calculate = true;
        }

        public static double Value(ISeries input, int index, int length)
        {
            return index < length - 1
                ? double.NaN
                : (input[index, BarData.Volume] - input[index - length + 1, BarData.Volume])/
                  input[index - length + 1, BarData.Volume]*100;
        }
    }
}