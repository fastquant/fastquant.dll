using System;
using System.ComponentModel;
using System.Linq;

namespace FastQuant.Indicators
{
    [Serializable]
    public class ATR : Indicator
    {
        protected int length;
        protected IndicatorStyle style;

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
        public IndicatorStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                this.style = value;
                Init();
            }
        }

        public ATR(ISeries input, int length, IndicatorStyle style = IndicatorStyle.QuantStudio) : base(input)
        {
            this.length = length;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = $"ATR ({this.length})";
            this.description = "Average True Range";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            if (index >= this.length)
            {
                int num = -1 * this.length;
                double value;
                if (this.style == IndicatorStyle.QuantStudio)
                {
                    if (index == this.length)
                    {
                        var sum = 0d;
                        for (var i = index; i > index - this.length; i--)
                            sum += TR.Value(this.input, i);
                        value = sum/this.length;
                    }
                    else
                        value = (this[index - 1 + num]*this.length + TR.Value(this.input, index) - TR.Value(this.input, index - this.length))/this.length;
                }
                else if (index == this.length)
                {
                    var sum = 0d;
                    for (var j = index; j > index - this.length; j--)
                        sum += TR.Value(this.input, j);
                    value = sum/this.length;
                }
                else
                    value = (base[this.input.GetDateTime(index - 1)]*this.length + TR.Value(this.input, index) - TR.Value(this.input, index - this.length))/this.length;
                Add(this.input.GetDateTime(index), value);
            }
        }

        public static double Value(ISeries input, int index, int length, IndicatorStyle style = IndicatorStyle.QuantStudio)
        {
            if (index >= length)
            {
                double sum = 0;
                double result;
                if (style == IndicatorStyle.QuantStudio)
                {
                    for (var i = index; i > index - length; i--)
                        sum += TR.Value(input, i);
                    result = sum / length;
                }
                else
                {
                    for (int j = length; j > 0; j--)
                        sum += TR.Value(input, j)/length;
                    sum /= length;
                    for (var k = length + 1; k <= index; k++)
                        sum = (sum* (length - 1) + TR.Value(input, k))/length;
                    result = sum;
                }
                return result;
            }
            return double.NaN;
        }
    }
}