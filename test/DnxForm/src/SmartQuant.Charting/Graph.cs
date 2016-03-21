using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace SmartQuant.Charting
{
    public delegate void ZoomEventHandler(object sender, ZoomEventArgs e);

    public class ZoomEventArgs : EventArgs
    {
        public double XMin { get; set; }

        public double XMax { get; set; }

        public double YMin { get; set; }

        public double YMax { get; set; }

        public bool ZoomUnzoom { get; set; }

        public ZoomEventArgs(double xMin, double xMax, double yMin, double yMax, bool zoomUnzoom)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
            ZoomUnzoom = zoomUnzoom;
        }
    }

    public delegate void NewTickEventHandler(object sender, NewTickEventArgs e);

    public class NewTickEventArgs : EventArgs
    {
        public DateTime DateTime { get; set; }

        public NewTickEventArgs(DateTime datetime)
        {
            DateTime = datetime;
        }
    }

    public enum EGraphMoveStyle
    {
        Graph,
        Point
    }

    public enum EGraphStyle
    {
        Scatter,
        Line,
        Bar
    }

    public enum EGridSize : long
    {
        min1 = 1 * TimeSpan.TicksPerMinute,
        min2 = 2 * min1,
        min5 = 5 * min1,
        min10 = 10 * min1,
        min15 = 15 * min1,
        min20 = 20 * min1,
        min30 = 30 * min1,
        hour1 = TimeSpan.TicksPerHour,
        hour2 = 2 * hour1,
        hour3 = 3 * hour1,
        hour4 = 4 * hour1,
        hour6 = 6 * hour1,
        hour12 = 12 * hour1,
        day1 = 1 * TimeSpan.TicksPerDay,
        day2 = 2 * day1,
        day3 = 3 * day1,
        day5 = 5 * day1,
        week1 = 7 * day1,
        week2 = 14 * day1,
        month1 = 30 * day1,
        month2 = 60 * day1,
        month3 = 90 * day1,
        month4 = 120 * day1,
        month6 = 180 * day1,
        year1 = 365 * 3 * day1,
        year2 = 2 * year1,
        year3 = 3 * year1,
        year4 = 4 * year1,
        year5 = 5 * year1,
        year10 = 10 * year1,
        year20 = 20 * year1
    }

    [Serializable]
    public enum ELegendPosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public enum EMarkerStyle
    {
        Point,
        Rectangle,
        Triangle,
        Circle,
        Bar,
        Candle,
        Buy,
        Sell,
        SellShort,
        BuyShort,
        Plus,
        Cross,
        None
    }

    public enum EMarkerTextPosition
    {
        Top,
        Bottom,
        Right,
        Left,
        Auto
    }

    public enum EMouseWheelMode
    {
        MoveX,
        MoveY,
        ZoomX,
        ZoomY,
        Zoom
    }

    public enum EPalette
    {
        Gray,
        Rainbow
    }

    public enum EPrintAlign
    {
        Veritcal,
        Horizontal,
        Center,
        None
    }

    public enum EPrintLayout
    {
        Portrait,
        Landscape
    }

    public enum ETextBoxPosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public enum ETextPosition
    {
        RightTop,
        LeftTop,
        CentreTop,
        RightBottom,
        LeftBottom,
        CentreBottom
    }

    public enum ETitlePosition
    {
        Left,
        Right,
        Centre,
        InsideLeft,
        InsideRight,
        InsideCentre
    }

    public enum ETransformationType
    {
        Empty,
        Intraday,
    }

    public enum EVerticalGridStyle
    {
        ByDateTime,
        NotByDateTime,
    }

    public interface IMovable
    {
        void Move(double x, double y, double dX, double dY);
    }

    public interface IZoomable
    {
        bool IsPadRangeX();

        bool IsPadRangeY();

        PadRange GetPadRangeX(Pad pad);

        PadRange GetPadRangeY(Pad pad);
    }

    public interface IDrawable
    {
        bool ToolTipEnabled { get; set; }

        string ToolTipFormat { get; set; }

        void Draw();

        void Paint(Pad pad, double minX, double maxX, double minY, double maxY);

        TDistance Distance(double x, double y);
    }

    [Serializable]
    public class Graph : IDrawable, IZoomable, IMovable
    {
        private EMarkerStyle markerStyle;
        private int markerSize;
        private Color markerColor;

        public string Name { get; set; }

        public string Title { get; set; }

        [Description("")]
        [Category("ToolTip")]
        public bool ToolTipEnabled { get; set; }

        [Description("")]
        [Category("ToolTip")]
        public string ToolTipFormat { get; set; }

        [Description("")]
        [Category("Style")]
        public EGraphStyle Style  { get; set; }

        [Category("Style")]
        [Description("")]
        public EGraphMoveStyle MoveStyle { get; set; }

        [Description("")]
        [Category("Marker")]
        public bool MarkerEnabled { get; set; }

        [Description("")]
        [Category("Marker")]
        public EMarkerStyle MarkerStyle
        {
            get
            {
                return this.markerStyle;
            }
            set
            {
                this.markerStyle = value;
                foreach (TMarker marker in Points)
                    marker.Style = this.markerStyle;
            }
        }

        [Category("Marker")]
        [Description("")]
        public int MarkerSize
        {
            get
            {
                return this.markerSize;
            }
            set
            {
                this.markerSize = value;
                foreach (TMarker marker in Points)
                    marker.Size = this.markerSize;
            }
        }

        [Description("")]
        [Category("Marker")]
        public Color MarkerColor
        {
            get
            {
                return this.markerColor;
            }
            set
            {
                this.markerColor = value;
                foreach (TMarker marker in Points)
                    marker.Color = this.markerColor;
            }
        }

        [Description("")]
        [Category("Bar")]
        public int BarWidth { get; set; }

        [Category("Line")]
        [Description("")]
        public bool LineEnabled { get; set; }

        [Category("Line")]
        [Description("")]
        public DashStyle LineDashStyle { get; set; }

        [Description("")]
        [Category("Line")]
        public Color LineColor { get; set; }

        [Browsable(false)]
        public ArrayList Points { get; }

        [Browsable(false)]
        public double MinX { get; private set; }

        [Browsable(false)]
        public double MinY { get; private set; }

        [Browsable(false)]
        public double MaxX { get; private set; }

        [Browsable(false)]
        public double MaxY { get; private set; }

        public Graph() : this(null, null)
        {
        }

        public Graph(string name) : this(name, null)
        {
        }

        public Graph(string name, string title)
        {
            Name = name;
            Title = title;
            Style = EGraphStyle.Line;
            MoveStyle = EGraphMoveStyle.Point;
            Points = new ArrayList();
            MinX = MinY = double.MaxValue;
            MaxX = MaxY = double.MinValue;
            MarkerEnabled = true;
            this.markerStyle = EMarkerStyle.Rectangle;
            this.markerSize = 5;
            this.markerColor = Color.Black;
            LineEnabled = true;
            LineDashStyle = DashStyle.Solid;
            LineColor = Color.Black;
            BarWidth = 20;
            ToolTipEnabled = true;
            ToolTipFormat = "{0}\nX = {2:F2} Y = {3:F2}";
        }

        private void MinMax(double x, double y)
        {
            MinX = Math.Min(MinX, x);
            MinY = Math.Min(MinY, y);
            MaxX = Math.Max(MaxX, x);
            MaxY = Math.Max(MaxY, y);
        }

        public void Add(double x, double y) => Add(x, y, MarkerColor);

        public void Add(double x, double y, Color color)
        {
            Points.Add(new TMarker(x, y) { Style = MarkerStyle, Size = MarkerSize, Color = color });
            MinMax(x, y);
        }

        public void Add(double x, double y, string text) => Add(x, y, text, MarkerColor);

        public void Add(double x, double y, string text, Color markerColor) => Add(x, y, text, markerColor, Color.Black);

        public void Add(double x, double y, string text, Color markerColor, Color textColor)
        {
            var label = new TLabel(text, x, y, markerColor, textColor)
            {
                Style = this.markerStyle,
                Size = this.markerSize
            };
            Points.Add(label);
            MinMax(x, y);
        }

        public void Add(TMarker marker)
        {
            Points.Add(marker);
            MinMax(marker.X, marker.Y);
        }

        public void Add(TLabel label)
        {
            Points.Add(label);
            MinMax(label.X, label.Y);
        }

        public virtual bool IsPadRangeX() => false;

        public virtual bool IsPadRangeY() => false;

        public virtual PadRange GetPadRangeX(Pad pad) => null;

        public virtual PadRange GetPadRangeY(Pad pad) => null;

        public virtual void Draw(string option)
        {
            if (Chart.Pad == null)
                new Canvas("Canvas", "Canvas");
            Chart.Pad.Add(this);
            Chart.Pad.Title.Add(Name, LineColor);
            Chart.Pad.Legend.Add(Name, LineColor);
            if (option.ToLower().IndexOf("s") >= 0)
                return;
            Chart.Pad.SetRange(MinX - (MaxX - MinX) / 10.0, MaxX + (MaxX - MinX) / 10.0, MinY - (MaxY - MinY) / 10.0, MaxY + (MaxY - MinY) / 10.0);
        }

        public virtual void Draw() => Draw("");

        // TODO: review
        public virtual void Paint(Pad pad, double xMin, double xMax, double yMin, double yMax)
        {
            if (Style == EGraphStyle.Line && LineEnabled)
            {
                var pen = new Pen(LineColor) {DashStyle = LineDashStyle};
                double X1 = 0.0;
                double Y1 = 0.0;
                bool flag = true;
                foreach (TMarker tmarker in Points)
                {
                    if (!flag)
                        pad.DrawLine(pen, X1, Y1, tmarker.X, tmarker.Y);
                    else
                        flag = false;
                    X1 = tmarker.X;
                    Y1 = tmarker.Y;
                }
            }
            if ((Style == EGraphStyle.Line || Style == EGraphStyle.Scatter) && MarkerEnabled)
            {
                foreach (TMarker tmarker in Points)
                    tmarker.Paint(pad, xMin, xMax, yMin, yMax);
            }
            if (Style != EGraphStyle.Bar)
                return;
            foreach (TMarker tmarker in Points)
            {
                if (tmarker.Y > 0.0)
                    pad.Graphics.FillRectangle(new SolidBrush(Color.Black), pad.ClientX(tmarker.X) - BarWidth / 2, pad.ClientY(tmarker.Y), BarWidth, pad.ClientY(0.0) - pad.ClientY(tmarker.Y));
                else
                    pad.Graphics.FillRectangle(new SolidBrush(Color.Black), pad.ClientX(tmarker.X) - BarWidth / 2, pad.ClientY(0.0), BarWidth, pad.ClientY(tmarker.Y) - pad.ClientY(0.0));
            }
        }

        public TDistance Distance(double x, double y)
        {
            var d = new TDistance();
            foreach (TMarker marker in Points)
            {
                var d2 = marker.Distance(x, y);
                if (d2.dX < d.dX && d2.dY < d.dY)
                    d = d2;
            }
            if (d != null)
                d.ToolTipText = string.Format(ToolTipFormat, Name, Title, d.X, d.Y);
            return d;
        }

        public void Move(double x, double y, double dX, double dY)
        {
            switch (MoveStyle)
            {
                case EGraphMoveStyle.Graph:
                    foreach (var tmarker in Points.Cast<TMarker>())
                    {
                        tmarker.X = dX;
                        tmarker.Y = dY;
                    }
                    break;
                    //IEnumerator enumerator1 = Points.GetEnumerator();
                    //try
                    //{
                    //    while (enumerator1.MoveNext())
                    //    {
                    //        TMarker tmarker = (TMarker)enumerator1.Current;
                    //        tmarker.X += dX;
                    //        tmarker.Y += dY;
                    //    }
                    //    break;
                    //}
                    //finally
                    //{
                    //    IDisposable disposable = enumerator1 as IDisposable;
                    //    if (disposable != null)
                    //        disposable.Dispose();
                    //}
                case EGraphMoveStyle.Point:
                    foreach (var tmarker in Points.Cast<TMarker>().Where(marker => marker.X == x && marker.Y == y))
                    {
                        tmarker.X = dX;
                        tmarker.Y = dY;
                        break;
                    }
                    break;

                    //IEnumerator enumerator2 = Points.GetEnumerator();
                    //try
                    //{
                    //    while (enumerator2.MoveNext())
                    //    {
                    //        TMarker tmarker = (TMarker)enumerator2.Current;
                    //        if (tmarker.X == x && tmarker.Y == y)
                    //        {
                    //            tmarker.X += dX;
                    //            tmarker.Y += dY;
                    //            break;
                    //        }
                    //    }
                    //    break;
                    //}
                    //finally
                    //{
                    //    IDisposable disposable = enumerator2 as IDisposable;
                    //    if (disposable != null)
                    //        disposable.Dispose();
                    //}
            }
        }
    }
}
