using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    [Serializable]
    public class DX : Indicator
    {
        protected int length;
        protected IndicatorStyle style;
        protected TimeSeries pdmTS;
        protected TimeSeries mdmTS;


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

        public DX(ISeries input, int length, IndicatorStyle style = IndicatorStyle.QuantStudio) : base(input)
        {
            this.length = length;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = $"DX ({this.length})";
            this.description = "Directional Movement Index";
            Clear();
            this.calculate = true;
            this.pdmTS = new TimeSeries();
            this.mdmTS = new TimeSeries();
        }

        // TODO: rewrite
        public override void Calculate(int index)
        {
            if (this.style == IndicatorStyle.QuantStudio)
            {
                double num = 0.0;
                double num2 = 0.0;
                double value = 0.0;
                if (index >= this.length)
                {
                    if (index == this.length)
                    {
                        for (int i = index; i >= index - this.length + 1; i--)
                        {
                            num += PDM.Value(this.input, i);
                            num2 += MDM.Value(this.input, i);
                        }
                    }
                    else
                    {
                        num2 = this.mdmTS[index - 1] - MDM.Value(this.input, index - this.length) +
                               MDM.Value(this.input, index);
                        num = this.pdmTS[index - 1] - PDM.Value(this.input, index - this.length) +
                              PDM.Value(this.input, index);
                    }
                    if (num + num2 != 0.0)
                    {
                        value = Math.Abs(num - num2)/(num + num2)*100.0;
                    }
                    Add(this.input.GetDateTime(index), value);
                }
                this.pdmTS.Add(this.input.GetDateTime(index), num);
                this.mdmTS.Add(this.input.GetDateTime(index), num2);
            }
            else
            {
                double num3 = 0.0;
                double num4 = 0.0;
                double value2 = 0.0;
                if (index >= this.length)
                {
                    if (index == this.length)
                    {
                        for (int j = index; j >= index - this.length + 1; j--)
                        {
                            num3 += PDM.Value(this.input, j);
                            num4 += MDM.Value(this.input, j);
                        }
                    }
                    else
                    {
                        num3 = this.pdmTS[index - 1] - this.pdmTS[index - 1]/(double) this.length +
                               PDM.Value(this.input, index);
                        num4 = this.mdmTS[index - 1] - this.mdmTS[index - 1]/(double) this.length +
                               MDM.Value(this.input, index);
                    }
                    if (num3 + num4 != 0.0)
                    {
                        value2 = Math.Abs(num3 - num4)/(num3 + num4)*100.0;
                    }
                    base.Add(this.input.GetDateTime(index), value2);
                }
                this.pdmTS.Add(this.input.GetDateTime(index), num3);
                this.mdmTS.Add(this.input.GetDateTime(index), num4);

            }
        }


        public static double Value(ISeries input, int index, int length, IndicatorStyle style = IndicatorStyle.QuantStudio)
        {
            if (index >= length)
            {
                var p = PDI.Value(input, index, length, style);
                var n = MDI.Value(input, index, length, style);
                return p + n != 0.0 ? 100.0*Math.Abs(p - n)/(p + n) : 0;
            }
            return double.NaN;
        }

    }
}