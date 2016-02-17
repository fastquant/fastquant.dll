using System;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.FinChart.Objects
{
    public class LineView : IChartDrawable, IZoomable
    {
        private DrawingLine line;

        protected bool toolTipEnabled = true;
        protected string toolTipFormat = "";
        protected Pad pad;
        protected DateTime firstDate;
        protected DateTime lastDate;
        protected bool selected;
        protected DateTime chartFirstDate;
        protected DateTime chartLastDate;

        [Description("Enable or disable tooltip appearance for this marker.")]
        [Category("ToolTip")]
        public bool ToolTipEnabled
        {
            get
            {
                return this.toolTipEnabled;
            }
            set
            {
                this.toolTipEnabled = value;
            }
        }

        [Category("ToolTip")]
        [Description("Tooltip format string. {1} - X coordinate, {2} - Y coordinte.")]
        public string ToolTipFormat
        {
            get
            {
                return this.toolTipFormat;
            }
            set
            {
                this.toolTipFormat = value;
            }
        }

        public LineView(DrawingLine line, Pad pad)
        {
            this.line = line;
            this.pad = pad;
            this.toolTipEnabled = true;
            this.toolTipFormat = "{0} {1} {2} - {3:F6}";
            this.chartFirstDate = new DateTime(Math.Min(line.X1.Ticks, line.X2.Ticks));
            this.chartLastDate = new DateTime(Math.Max(line.X1.Ticks, line.X2.Ticks));
        }

        public void Paint()
        {
            DateTime dateTime1 = new DateTime(Math.Min(this.line.X1.Ticks, this.line.X2.Ticks));
            Math.Min(this.line.Y1, this.line.Y2);
            DateTime dateTime2 = new DateTime(Math.Max(this.line.X1.Ticks, this.line.X2.Ticks));
            Math.Max(this.line.Y1, this.line.Y2);
            int index1 = this.pad.MainSeries.GetIndex(this.line.X1, IndexOption.Null);
            int index2 = this.pad.MainSeries.GetIndex(this.line.X2, IndexOption.Null);
            int val1_1 = Math.Max(index1, this.pad.FirstIndex);
            int val1_2 = Math.Max(index2, this.pad.FirstIndex);
            int num1 = Math.Min(val1_1, this.pad.LastIndex);
            int num2 = Math.Min(val1_2, this.pad.LastIndex);
            double lineValueAt1 = this.GetLineValueAt(num1);
            double lineValueAt2 = this.GetLineValueAt(num2);
            if (Math.Max(lineValueAt1, lineValueAt2) <= this.pad.MinValue || Math.Min(lineValueAt1, lineValueAt2) >= this.pad.MaxValue)
                return;
            int y1 = this.pad.ClientY(lineValueAt1);
            int y2 = this.pad.ClientY(lineValueAt2);
            int x1 = this.pad.ClientX(this.pad.MainSeries.GetDateTime(num1));
            int x2 = this.pad.ClientX(this.pad.MainSeries.GetDateTime(num2));
            this.pad.Graphics.DrawLine(new Pen(this.line.Color, (float) this.line.Width), x1, y1, x2, y2);
        }

        public void SetInterval(DateTime minDate, DateTime maxDate)
        {
            this.firstDate = minDate;
            this.lastDate = maxDate;
        }

        public Distance Distance(int x, double y)
        {
            if (this.chartFirstDate > this.lastDate || this.chartLastDate < this.firstDate)
                return null;

            var d = new Distance();
            DateTime dateTime = this.pad.GetDateTime(x);
            double num;
            if (dateTime == this.chartFirstDate)
                num = this.line.Y1;
            else if (dateTime == this.chartLastDate)
            {
                num = this.line.Y2;
            }
            else
            {
                if (dateTime.Ticks > Math.Max(this.line.X1.Ticks, this.line.X2.Ticks) || dateTime.Ticks < Math.Min(this.line.X1.Ticks, this.line.X2.Ticks))
                    return (Distance) null;
                num = this.GetLineValueAt(this.pad.MainSeries.GetIndex(dateTime, IndexOption.Null));
            }
            d.X = x;
            d.Y = num;
            d.DX = 0.0;
            d.DY = Math.Abs(y - num);
            if (d.DX == double.MaxValue || d.DY == double.MaxValue)
                return  null;
            d.ToolTipText = string.Format(ToolTipFormat, "Line", this.line.Name, dateTime, num);
            return d;
        }

        public void Select()
        {
        }

        public void UnSelect()
        {
        }

        public PadRange GetPadRangeY(Pad pad) => new PadRange(0, 0);

        private double GetLineValueAt(int x)
        {
            double num1 = this.pad.MainSeries.GetIndex(this.line.X1);
            double num2 = this.pad.MainSeries.GetIndex(this.line.X2);
            return this.line.Y1 + (x - num1)/(num2 - num1)*(this.line.Y2 - this.line.Y1);
        }
    }
}
