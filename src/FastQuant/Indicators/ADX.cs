using System.ComponentModel;

namespace FastQuant.Indicators
{
    public class ADX : Indicator
    {
        protected int length;
        protected IndicatorStyle style;
        protected DX dx;

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

        public ADX(ISeries input, int length, IndicatorStyle style = IndicatorStyle.QuantStudio) : base(input)
        {
            this.length = length;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = $"ADX ({this.length})";
            this.description = "Average Directional Index";
            Clear();
            this.calculate = true;
            Detach();
            this.dx = new DX(this.input, this.length, this.style);
            Attach();
        }

        //TODO: rewrite
        public override void Calculate(int index)
        {
            if (index >= 2 * this.length)
            {
                double num = 0.0;
                int num2 = -2 * this.length;
                int num3 = -1 * this.length;
                if (index == 2 * this.length)
                {
                    for (int i = index; i > index - this.length; i--)
                    {
                        num += this.dx[i + num3];
                    }
                }
                else if (this.style == IndicatorStyle.QuantStudio)
                {
                    num = this[index - 1 + num2] * (double)this.length - this.dx[index - this.length + num3] + this.dx[index + num3];
                }
                else
                {
                    num = this[index - 1 + num2] * (double)(this.length - 1) + this.dx[index + num3];
                }
                double value = num / (double)this.length;
                base.Add(this.input.GetDateTime(index), value);
            }
        }

        //TODO: rewrite
        public static double Value(ISeries input, int index, int length, IndicatorStyle style = IndicatorStyle.QuantStudio)
        {
            if (index >= 2 * length)
            {
                double num = 0.0;
                double result;
                if (style == IndicatorStyle.QuantStudio)
                {
                    for (int i = index; i > index - length; i--)
                    {
                        num += DX.Value(input, i, length, IndicatorStyle.QuantStudio);
                    }
                    result = num / (double)length;
                }
                else
                {
                    for (int j = 2 * length; j > length; j--)
                    {
                        num += DX.Value(input, j, length, style);
                    }
                    num /= (double)length;
                    for (int k = 2 * length + 1; k <= index; k++)
                    {
                        num = (num * (double)(length - 1) + DX.Value(input, k, length, style)) / (double)length;
                    }
                    result = num;
                }
                return result;
            }
            return double.NaN;
        }


    }
}