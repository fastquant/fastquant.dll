using SmartQuant;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace SmartQuant.FinChart
{
    public class SimpleBSView : SeriesView
    {
        private BarSeries series;

        [Category("Drawing Style")]
        public Color UpColor { get; set; }

        [Category("Drawing Style")]
        public Color DownColor { get; set; }

        public SimpleBSStyle Style { get; set; }

        public override ISeries MainSeries
        {
            get
            {
                return this.series;
            }
        }

        [Browsable(false)]
        public override Color Color
        {
            get
            {
                return DownColor;
            }
            set
            {
            }
        }

        public override double LastValue
        {
            get
            {
                return this.series[this.lastDate, IndexOption.Prev].Close;
            }
        }

        public SimpleBSView(Pad pad, BarSeries series)
            : base(pad)
        {
            this.series = series;
            UpColor = Color.Black;
            DownColor = Color.Lime;
            ToolTipFormat = "{0} {2}\n\nH : {3:F*}\nL : {4:F*}\nO : {5:F*}\nC : {6:F*}\nV : {7}".Replace("*", this.pad.Chart.LabelDigitsCount.ToString());
        }

        public override PadRange GetPadRangeY(Pad pad)
        {
            double min = this.series.LowestLow(this.firstDate, this.lastDate);
            double max = this.series.HighestHigh(this.firstDate, this.lastDate);
            if (min >= max)
            {
                double num = min / 10.0;
                min -= num;
                max += num;
            }
            return new PadRange(min, max);
        }

        public override void Paint()
        {
            Color color = DownColor;
            Pen pen1 = new Pen(color);
            Pen pen2 = new Pen(color);
            Pen pen3 = new Pen(color);
            Brush brush1 =  new SolidBrush(DownColor);
            Brush brush2 =  new SolidBrush(UpColor);
            long num1 = -1L;
            long num2 = -1L;
            int index1 = this.series.GetIndex(this.firstDate, IndexOption.Null);
            int index2 = this.series.GetIndex(this.lastDate, IndexOption.Null);
            if (index1 == -1 || index2 == -1)
                return;
            int width = (int)Math.Max(2.0, (double)(int)this.pad.IntervalWidth / 1.4);
            int num3 = 0;
            for (int i = index1; i <= index2; ++i)
            {
                int num4 = this.pad.ClientX(this.series[i].DateTime);
                Bar bar = this.series[i];
                double high = bar.High;
                double low = bar.Low;
                double open = bar.Open;
                double close = bar.Close;
                if (Style == SimpleBSStyle.Candle)
                {
                    this.pad.Graphics.DrawLine(pen1, num4, this.pad.ClientY(low), num4, this.pad.ClientY(high));
                    if (open != 0.0 && close != 0.0)
                    {
                        if (open > close)
                        {
                            int height = this.pad.ClientY(close) - this.pad.ClientY(open);
                            if (height == 0)
                                height = 1;
                            this.pad.Graphics.FillRectangle(brush1, num4 - width / 2, this.pad.ClientY(open), width, height);
                        }
                        else
                        {
                            int height = this.pad.ClientY(open) - this.pad.ClientY(close);
                            if (height == 0)
                                height = 1;
                            this.pad.Graphics.DrawRectangle(pen1, num4 - width / 2, this.pad.ClientY(close), width, height);
                            this.pad.Graphics.FillRectangle(brush2, num4 - width / 2 + 1, this.pad.ClientY(close) + 1, width - 1, height - 1);
                        }
                    }
                }
                if (Style == SimpleBSStyle.Bar)
                {
                    this.pad.Graphics.DrawLine(pen1, num4, this.pad.ClientY(low), num4, this.pad.ClientY(high));
                    this.pad.Graphics.DrawLine(pen1, num4 - width / 2, this.pad.ClientY(open), num4, this.pad.ClientY(open));
                    this.pad.Graphics.DrawLine(pen1, num4 + width / 2, this.pad.ClientY(close), num4, this.pad.ClientY(close));
                }
                if (Style == SimpleBSStyle.Line)
                {
                    long num5 = (long)num4;
                    int num6 = this.pad.ClientY(bar.Close);
                    if (num1 != -1 && num2 != -1)
                        this.pad.Graphics.DrawLine(pen1, num5, num6, num1, num2);
                    num1 = num5;
                    num2 = (long)num6;
                    ++num3;
                }
            }
        }

        public override Distance Distance(int x, double y)
        {
            var d = new Distance();
            var bar = this.series[this.pad.GetDateTime(x), IndexOption.Null];
            d.DX = 0;
            d.DY = bar.Low <= y && y <= bar.High ? 0 : d.DY;
            if (d.DX == double.MaxValue || d.DY == double.MaxValue)
                return null;
            d.ToolTipText = string.Format(ToolTipFormat, this.series.Name, this.series.Description, bar.DateTime, bar.High, bar.Low, bar.Open, bar.Close, bar.Volume);
            return d;
        }
    }
}
