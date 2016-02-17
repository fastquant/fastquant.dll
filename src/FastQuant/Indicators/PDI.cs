using System;
using System.ComponentModel;

namespace FastQuant.Indicators
{
    [Serializable]
    public class PDI : Indicator
    {
        protected int length;
        protected IndicatorStyle style;
        protected TimeSeries pdmTS;
        protected TimeSeries trTS;

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

        public PDI(ISeries input, int length, IndicatorStyle style = IndicatorStyle.QuantStudio) : base(input)
        {
            this.length = length;
            this.style = style;
            Init();
        }

        protected override void Init()
        {
            this.name = $"PDI ({this.length})";
            this.description = "Plus Directional Indicator";
            Clear();
            this.calculate = true;
            this.pdmTS = new TimeSeries();
            this.trTS = new TimeSeries();
        }

        public override void Calculate(int index)
        {
            if (this.style == IndicatorStyle.QuantStudio)
            {
                var pdm = 0.0;
                var tr = 0.0;
                if (index >= this.length)
                {
                    if (index == this.length)
                    {
                        for (var i = index; i >= index - this.length + 1; i--)
                        {
                            tr += TR.Value(this.input, i);
                            pdm += PDM.Value(this.input, i);
                        }
                    }
                    else
                    {
                        pdm = this.pdmTS[index - 1] - PDM.Value(this.input, index - this.length) + PDM.Value(this.input, index);
                        tr = this.trTS[index - 1] - TR.Value(this.input, index - this.length) + TR.Value(this.input, index);
                    }
                    if (tr != 0.0)
                    {
                        var value = pdm/tr*100.0;
                        if (!double.IsNaN(value))
                            Add(this.input.GetDateTime(index), value);
                    }
                }
                this.pdmTS.Add(this.input.GetDateTime(index), pdm);
                this.trTS.Add(this.input.GetDateTime(index), tr);
            }
            else
            {
                var pdm = 0.0;
                var tr = 0.0;
                if (index >= this.length)
                {
                    if (index == this.length)
                    {
                        for (int j = index; j >= index - this.length + 1; j--)
                        {
                            tr += TR.Value(this.input, j);
                            pdm += PDM.Value(this.input, j);
                        }
                    }
                    else
                    {
                        pdm = this.pdmTS[index - 1] - this.pdmTS[index - 1]/this.length + PDM.Value(this.input, index);
                        tr = this.trTS[index - 1] - this.trTS[index - 1]/this.length + TR.Value(this.input, index);
                    }
                    if (tr != 0.0)
                    {
                        var value = pdm/tr*100.0;
                        if (!double.IsNaN(value))
                            Add(this.input.GetDateTime(index), value);
                    }
                }
                this.pdmTS.Add(this.input.GetDateTime(index), pdm);
                this.trTS.Add(this.input.GetDateTime(index), tr);
            }
        }

        public static double Value(ISeries input, int index, int length, IndicatorStyle style = IndicatorStyle.QuantStudio)
        {
            if (style == IndicatorStyle.QuantStudio)
            {
                var pdm = 0.0;
                var tr = 0.0;
                if (index >= length)
                {
                    for (int i = index; i > index - length; i--)
                    {
                        tr += TR.Value(input, i);
                        pdm += PDM.Value(input, i);
                    }
                    return pdm/tr*100.0;
                }
                return double.NaN;
            }
            else
            {
                var pdm = 0.0;
                var tr = 0.0;
                if (index >= length)
                {
                    for (var j = length; j >= 1; j--)
                    {
                        tr += TR.Value(input, j);
                        pdm += PDM.Value(input, j);
                    }
                    for (var k = length + 1; k <= index; k++)
                    {
                        pdm = pdm - pdm / length + PDM.Value(input, k);
                        tr = tr - tr / length + TR.Value(input, k);
                    }
                    return pdm/tr*100.0;
                }
                return double.NaN;
            }
        }
    }
}