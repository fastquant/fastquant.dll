using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SmartQuant.FinChart
{
    public enum SimpleDSStyle
    {
        Line,
        Bar,
        Circle,
    }

    public enum SimpleBSStyle
    {
        Candle,
        Bar,
        Line,
    }

    public abstract class SeriesView : IChartDrawable, IAxesMarked, IZoomable
    {
        protected bool isMarkEnable = true;
        protected bool toolTipEnabled = true;
        protected string toolTipFormat = "";
        protected bool displayNameEnabled = true;
        protected Pad pad;
        protected DateTime firstDate;
        protected DateTime lastDate;
        protected bool selected;

        public virtual string DisplayName => MainSeries.Name;

        public virtual bool DisplayNameEnabled
        {
            get
            {
                return this.displayNameEnabled;
            }
            set
            {
                this.displayNameEnabled = value;
            }
        }

        public bool IsMarkEnable
        {
            get
            {
                return this.isMarkEnable;
            }
            set
            {
                this.isMarkEnable = value;
            }
        }

        public virtual int LabelDigitsCount => this.pad.Chart.LabelDigitsCount;

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

        public abstract Color Color { get; set; }

        [Browsable(false)]
        public abstract double LastValue { get; }

        [Browsable(false)]
        public abstract ISeries MainSeries { get; }

        public SeriesView(Pad pad)
        {
            this.pad = pad;
        }

        public void SetInterval(DateTime minDate, DateTime maxDate)
        {
            this.firstDate = minDate;
            this.lastDate = maxDate;
        }

        public abstract PadRange GetPadRangeY(Pad pad);

        public abstract void Paint();

        public abstract Distance Distance(int x, double y);

        public void Select() => this.selected = true;

        public void UnSelect() => this.selected = false;
    }

    public class SimpleBSView : SeriesView
    {
        private BarSeries series;

        [Category("Drawing Style")]
        public Color UpColor { get; set; }

        [Category("Drawing Style")]
        public Color DownColor { get; set; }

        public SimpleBSStyle Style { get; set; }

        public override ISeries MainSeries => this.series;

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

        public override double LastValue => this.series[this.lastDate, IndexOption.Prev].Close;

        public SimpleBSView(Pad pad, BarSeries series) : base(pad)
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
            Brush brush1 = new SolidBrush(DownColor);
            Brush brush2 = new SolidBrush(UpColor);
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

    public abstract class BSView : SeriesView
    {
        public string ToolTipDateTimeFormat { get; set; }

        public BSView(Pad pad) : base(pad)
        {
        }

        public override PadRange GetPadRangeY(Pad pad)
        {
            double max = MainSeries.GetMax(this.firstDate, this.lastDate);
            double min = MainSeries.GetMin(this.firstDate, this.lastDate);
            if (max >= min)
            {
                double num = max / 10.0;
                max -= num;
                min += num;
            }
            return new PadRange(max, min);
        }

        public override Distance Distance(int x, double y)
        {
            var d = new Distance();
            var bar = (MainSeries as BarSeries)[this.pad.GetDateTime(x), IndexOption.Null];
            d.DX = 0;
            d.DY = bar.Low <= y && y <= bar.High ? 0 : d.DY;
            if (d.DX == double.MaxValue || d.DY == double.MaxValue)
                return null;
            d.ToolTipText = string.Format(ToolTipFormat, MainSeries.Name, MainSeries.Description, ToolTipDateTimeFormat, bar.High, bar.Low, bar.Open, bar.Close, bar.Volume);
            return d;
        }
    }

    public class DSView : SeriesView
    {
        private TimeSeries series;

        public override ISeries MainSeries => this.series;

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
            var path = new GraphicsPath();
            var list = new List<Point>();
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
            ArrayList arrayList = null;
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
