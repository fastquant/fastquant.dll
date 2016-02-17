using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class MFI : Indicator
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

        public MFI(ISeries input, int length) : base(input)
        {
            this.length = length;
            Init();
        }

        protected override void Init()
        {
            this.name = $"MFI ({this.length})";
            this.description = "Money Flow Index";
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
            if (index >= length)
            {
                var positive_money_flow = 0.0;
                var negative_money_flow = 0.0;
                for (var i = index; i > index - length; i--)
                {
                    var current_typical = input[i, BarData.Typical];
                    var previous_typical = input[i - 1, BarData.Typical];
                    var vol = input[i, BarData.Volume];
                    if (current_typical > previous_typical)
                        positive_money_flow += current_typical*vol;
                    else
                        negative_money_flow += current_typical*vol;
                }
                var money_flow_ratio = positive_money_flow / negative_money_flow;
                return 100.0 - 100.0 / (1.0 + money_flow_ratio);
            }
            return double.NaN;
        }
    }
}