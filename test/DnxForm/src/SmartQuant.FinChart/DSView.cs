using SmartQuant;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace SmartQuant.FinChart
{
    public class DSView : SeriesView
    {
        private TimeSeries series;

        public override ISeries MainSeries
        {
            get
            {
                return this.series;
            }
        }

        public SearchOption Option { get; private set; }

        internal SmoothingMode SmoothingMode { get; set; }

        public SimpleDSStyle Style { get; set; }

        public override Color Color { get; set; }

        public int DrawWidth { get; set; }

        public override double LastValue
        {
            get
            {
                if (this.series.Count == 0 || this.lastDate < this.series.FirstDateTime)
                    return double.NaN;
                if (Option == SearchOption.ExactFirst)
                    return this.series[this.lastDate, SearchOption.Prev];
                if (Option == SearchOption.Next)
                    return this.series[this.lastDate.AddTicks(1), SearchOption.Next];
                else
                    return -1;
            }
        }

        public DSView(Pad pad, TimeSeries series)
            : this(pad, series, SearchOption.ExactFirst)
        {
        }

        public DSView(Pad pad, TimeSeries series, Color color)
            : this(pad, series, color, SearchOption.ExactFirst, SmoothingMode.AntiAlias)
        {
        }

        public DSView(Pad pad, TimeSeries series, SearchOption option)
            : this(pad, series, Color.White, option, SmoothingMode.AntiAlias)
        {
        }

        public DSView(Pad pad, TimeSeries series, Color color, SearchOption option, SmoothingMode smoothing)
            : base(pad)
        {
            this.series = series;
            Option = option;
            Color = color;
            SmoothingMode = smoothing;
            ToolTipFormat = "{0}\n{2} - {3:F*}".Replace("*", pad.Chart.LabelDigitsCount.ToString());
        }

        public override PadRange GetPadRangeY(Pad Pad)
        {
            DateTime datetime1;
            DateTime datetime2;
            if (Option == SearchOption.ExactFirst)
            {
                datetime1 = this.firstDate;
                datetime2 = this.lastDate;
            }
            else
            {
                int index1 = this.series.GetIndex(this.firstDate.AddTicks(1L), IndexOption.Null);
                int index2 = this.series.GetIndex(this.lastDate.AddTicks(1L), IndexOption.Next);
                if (index1 == -1 || index2 == -1)
                    return new PadRange(0, 0);
                datetime1 = this.series.GetDateTime(index1);
                datetime2 = this.series.GetDateTime(index2);
            }
            if (this.series.Count == 0 || !(this.series.LastDateTime >= datetime1) || !(this.series.FirstDateTime <= datetime2))
                return new PadRange(0, 0);
            int index3 = this.series.GetIndex(datetime1, IndexOption.Next);
            int index4 = this.series.GetIndex(datetime2, IndexOption.Prev);
            double min = this.series.GetMin(Math.Min(index3, index4), Math.Max(index3, index4));
            double max = this.series.GetMax(Math.Min(index3, index4), Math.Max(index3, index4));
            if (min >= max)
            {
                double num = Math.Abs(min) / 1000.0;
                min -= num;
                max += num;
            }
            return new PadRange(min, max);
        }

        public override void Paint()
        {
            var pen = new Pen(Color, DrawWidth);
            int num1 = 0;
            GraphicsPath path = new GraphicsPath();
            List<Point> list = new List<Point>();
            double worldY1 = 0.0;
            long ticks1 = 0L;
            double worldY2 = 0.0;
            int x1 = 0;
            int x2 = 0;
            int num2 = 0;
            int y2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int index1 = this.pad.MainSeries.GetIndex(this.firstDate, IndexOption.Null);
            int index2 = this.pad.MainSeries.GetIndex(this.lastDate, IndexOption.Null);
            ArrayList arrayList = (ArrayList)null;
            int val2_1 = this.pad.ClientY(0.0);
            int val2_2 = this.pad.ClientY(this.pad.MaxValue);
            int val2_3 = this.pad.ClientY(this.pad.MinValue);
            if (this.selected)
                arrayList = new ArrayList();
            int num7 = (int)Math.Max(2.0, (double)(int)this.pad.IntervalWidth / 1.2);
            for (int index3 = index1; index3 <= index2; ++index3)
            {
                DateTime dateTime = this.pad.MainSeries.GetDateTime(index3);
                if (this.selected)
                    arrayList.Add((object)dateTime);
                long ticks2 = dateTime.Ticks;
                if (Option == SearchOption.ExactFirst)
                {
                    if (this.series.Contains(dateTime))
                        worldY1 = this.series[dateTime, SearchOption.Next];
                    else
                        continue;
                }
                if (Option == SearchOption.Next)
                {
                    if (this.series.Contains(dateTime.AddTicks(1L)))
                        worldY1 = this.series[dateTime.AddTicks(1L), 0, SearchOption.ExactFirst];
                    else if (dateTime.AddTicks(1L) >= this.series.FirstDateTime)
                        worldY1 = this.series[dateTime.AddTicks(1L), SearchOption.Next];
                    else
                        continue;
                }
                if (Style == SimpleDSStyle.Line)
                {
                    if (num1 != 0)
                    {
                        x1 = this.pad.ClientX(new DateTime(ticks1));
                        num2 = this.pad.ClientY(worldY2);
                        x2 = this.pad.ClientX(new DateTime(ticks2));
                        y2 = this.pad.ClientY(worldY1);
                        if (x1 != num3 || x2 != num4 || (num2 != num5 || y2 != num6))
                            path.AddLine(x1, num2, x2, y2);
                    }
                    num3 = x1;
                    num5 = num2;
                    num4 = x2;
                    num6 = y2;
                    ticks1 = ticks2;
                    worldY2 = worldY1;
                    list.Add(new Point(this.pad.ClientX(new DateTime(ticks1)), this.pad.ClientY(worldY2)));
                }
                if (Style == SimpleDSStyle.Bar)
                {
                    x1 = this.pad.ClientX(new DateTime(ticks2));
                    num2 = this.pad.ClientY(worldY1);
                    float y = (float)Math.Max(Math.Min(num2, val2_1), val2_2);
                    float num8 = (float)Math.Min(Math.Max(num2, val2_1), val2_3);
                    this.pad.Graphics.FillRectangle((Brush)new SolidBrush(this.Color), (float)(x1 - num7 / 2), y, (float)num7, Math.Abs(y - num8));
                }
                if (Style == SimpleDSStyle.Circle)
                {
                    x1 = this.pad.ClientX(new DateTime(ticks2));
                    num2 = this.pad.ClientY(worldY1);
                    Math.Max(Math.Min(num2, val2_1), val2_2);
                    Math.Min(Math.Max(num2, val2_1), val2_3);
                    this.pad.Graphics.FillEllipse((Brush)new SolidBrush(this.Color), x1 - this.DrawWidth, num2 - this.DrawWidth, this.DrawWidth * 2, this.DrawWidth * 2);
                }
                ++num1;
            }
            if (this.selected)
            {
                int num8 = Math.Max(1, (int)Math.Round((double)arrayList.Count / 8.0));
                int index3 = 1;
                while (index3 < arrayList.Count)
                {
                    int num9 = this.pad.ClientX(new DateTime(((DateTime)arrayList[index3]).Ticks));
                    if (this.series.Contains((DateTime)arrayList[index3]))
                    {
                        int num10 = this.pad.ClientY(this.series[(DateTime)arrayList[index3], 0, SearchOption.ExactFirst]);
                        Color midnightBlue = Color.MidnightBlue;
                        this.pad.Graphics.FillRectangle((Brush)new SolidBrush(Color.FromArgb((int)midnightBlue.R ^ (int)byte.MaxValue, (int)midnightBlue.G ^ (int)byte.MaxValue, (int)midnightBlue.B ^ (int)byte.MaxValue)), num9 - 2, num10 - 2, 4, 4);
                    }
                    index3 += num8;
                }
            }
            if (Style != SimpleDSStyle.Line)
                return;
            SmoothingMode smoothingMode = this.pad.Graphics.SmoothingMode;
            this.pad.Graphics.SmoothingMode = SmoothingMode;
            this.pad.Graphics.DrawPath(pen, path);
            this.pad.Graphics.SmoothingMode = smoothingMode;
        }

        public override Distance Distance(int x, double y)
        {
            var d = new Distance();
            DateTime dateTime = this.pad.GetDateTime(x);
            double num = 0.0;
            if (Option == SearchOption.ExactFirst)
            {
                if (!this.series.Contains(dateTime))
                    return null;
                num = this.series[dateTime, SearchOption.ExactFirst];
            }
            if (Option == SearchOption.Next)
            {
                if (this.series.LastDateTime < dateTime.AddTicks(1))
                    return null;
                num = this.series[dateTime.AddTicks(1), SearchOption.Next];
            }
            d.X = x;
            d.Y = num;
            d.DX = 0;
            d.DY = Math.Abs(y - num);

            if (d.DX == double.MaxValue || d.DY == double.MaxValue)
                return null;
            d.ToolTipText = string.Format(ToolTipFormat, this.series.Name, this.series.Description, dateTime, d.Y);
            return d;
        }
    }
}
