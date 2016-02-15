using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Linq;

#if GTK
using Compatibility.Gtk;
using MouseButtons = System.Windows.Forms.MouseButtons;
#else
using System.Windows.Forms;
#endif

namespace SmartQuant.Charting
{
    public enum EAxisLabelAlignment
    {
        Left,
        Right,
        Centre,
    }

    public enum EAxisPosition
    {
        Left,
        Right,
        Top,
        Bottom,
        None
    }

    public enum EAxisTitlePosition
    {
        Left,
        Right,
        Top,
        Bottom,
        Centre,
        None
    }
    public enum EAxisType
    {
        Numeric,
        DateTime
    }

    [Serializable]
    public class Axis
    {
        private Pad pad;
        private bool fMouseDown;
        private int fMouseDownX;
        private int fMouseDownY;
        private bool fOutlineEnabled;
        private int fOutline1;
        private int fOutline2;
        private int fWidth;
        private int fHeight;

        public double X1 { get; set; }

        public double Y1 { get; set; }

        public double X2 { get; set; }

        public double Y2 { get; set; }

        public Color Color { get; set; }

        public EAxisType Type { get; set; }

        public EAxisPosition Position { get; set; }

        public EVerticalGridStyle VerticalGridStyle { get; set; }

        public bool MajorTicksEnabled { get; set; }

        public Color MajorTicksColor { get; set; }

        public float MajorTicksWidth { get; set; }

        public int MajorTicksLength { get; set; }

        public bool MinorTicksEnabled { get; set; }

        public Color MinorTicksColor { get; set; }

        public float MinorTicksWidth { get; set; }

        public int MinorTicksLength { get; set; }

        public EAxisTitlePosition TitlePosition { get; set; }

        public Font TitleFont { get; set; }

        public Color TitleColor { get; set; }

        public int TitleOffset { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public bool Enabled { get; set; }

        public bool Zoomed { get; set; }

        public bool GridEnabled { get; set; }

        public Color GridColor { get; set; }

        public float GridWidth { get; set; }

        public DashStyle GridDashStyle { get; set; }

        public bool MinorGridEnabled { get; set; }

        public Color MinorGridColor { get; set; }

        public float MinorGridWidth { get; set; }

        public DashStyle MinorGridDashStyle { get; set; }

        public bool TitleEnabled { get; set; }

        public bool LabelEnabled { get; set; }

        public Font LabelFont { get; set; }

        public Color LabelColor { get; set; }

        public int LabelOffset { get; set; }

        public string LabelFormat { get; set; }

        public EAxisLabelAlignment LabelAlignment { get; set; }

        public int Width
        {
            get
            {
                if (!Enabled)
                    return 0;
                if (this.fWidth != -1)
                    return this.fWidth;
                int num1 = 0;
                int num2 = 0;
                LastValidAxisWidth = 0;
                if (TitleEnabled)
                    num1 = (int)((double)TitleOffset + (double)this.pad.Graphics.MeasureString(Max.ToString("F1"), TitleFont).Height);
                if (LabelEnabled)
                    num2 = LabelFormat != null ? (int)((double)LabelOffset + (double)this.pad.Graphics.MeasureString(Max.ToString(LabelFormat), LabelFont).Width) : (int)((double)LabelOffset + (double)this.pad.Graphics.MeasureString(Max.ToString("F1"), LabelFont).Width);
                LastValidAxisWidth = num2 + num1 + 2;
                return num2 + num1 + 2;
            }
            set
            {
                this.fWidth = value;
            }
        }

        public int Height
        {
            get
            {
                if (!Enabled)
                    return 0;
                if (this.fHeight != -1)
                    return this.fHeight;
                int num1 = 0;
                int num2 = 0;
                if (TitleEnabled)
                    num1 = (int)((double)TitleOffset + (double)this.pad.Graphics.MeasureString("Example", TitleFont).Height);
                if (LabelEnabled)
                    num2 = (int)((double)LabelOffset + (double)this.pad.Graphics.MeasureString("Example", LabelFont).Height);
                return num2 + num1;
            }
            set
            {
                this.fHeight = value;
            }
        }

        public int LastValidAxisWidth { get; set; }

        public string Title { get; set; }

        public Axis(Pad pad)
            : this(pad, EAxisPosition.None)
        {
        }

        public Axis(Pad pad, EAxisPosition position)
            : this(pad, 0, 0, 0, 0, position)
        {
        }

        public Axis(Pad pad, double x1, double y1, double x2, double y2)
            : this(pad, x1, y1, x2, y2, EAxisPosition.None)
        {
        }

        private Axis(Pad pad, double x1, double y1, double x2, double y2, EAxisPosition position)
        {
            this.pad = pad;
            Position = position;
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
            Enabled = true;
            Zoomed = false;
            Color = Color.Black;
            Title = "";
            TitleEnabled = true;
            TitlePosition = EAxisTitlePosition.Centre;
            TitleFont = new Font("Arial", 8f);
            TitleColor = Color.Black;
            TitleOffset = 2;
            LabelEnabled = true;
            LabelFont = new Font("Arial", 8f);
            LabelColor = Color.Black;
            LabelFormat = null;
            LabelOffset = 2;
            LabelAlignment = EAxisLabelAlignment.Centre;
            GridEnabled = true;
            GridColor = Color.Gray;
            GridDashStyle = DashStyle.Solid;
            GridWidth = 0.5f;
            MinorGridEnabled = false;
            MinorGridColor = Color.Gray;
            MinorGridDashStyle = DashStyle.Solid;
            MinorGridWidth = 0.5f;
            MajorTicksEnabled = true;
            MajorTicksColor = Color.Black;
            MajorTicksWidth = 0.5f;
            MajorTicksLength = 4;
            MinorTicksEnabled = true;
            MinorTicksColor = Color.Black;
            MinorTicksWidth = 0.5f;
            MinorTicksLength = 1;
            Type = EAxisType.Numeric;
            VerticalGridStyle = EVerticalGridStyle.ByDateTime;
            this.fMouseDown = false;
            this.fMouseDownX = 0;
            this.fMouseDownY = 0;
            this.fOutlineEnabled = false;
            this.fOutline1 = 0;
            this.fOutline2 = 0;
            this.fWidth = -1;
            this.fHeight = -1;
        }
            
        public void SetLocation(double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
        }

        public void SetRange(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public void Zoom(double min, double max)
        {
            SetRange(min, max);
            Zoomed = true;
            this.pad.EmitZoom(true);
            if (this.pad.Chart.GroupZoomEnabled)
                return;
            this.pad.Update();
        }

        public void Zoom(DateTime min, DateTime max)
        {
            Zoom(min.Ticks, max.Ticks);
        }

        public void Zoom(string min, string max)
        {
            Zoom(DateTime.Parse(min), DateTime.Parse(max));
        }

        public void UnZoom()
        {
            switch (Position)
            {
                case EAxisPosition.Left:
                    SetRange(this.pad.YMin, this.pad.YMax);
                    break;
                case EAxisPosition.Bottom:
                    SetRange(this.pad.XMin, this.pad.XMax);
                    break;
            }
            Zoomed = false;
            this.pad.EmitZoom(false);
            if (this.pad.Chart.GroupZoomEnabled)
                return;
            this.pad.Update();
        }

        public static long GetNextMajor(long prevMajor, EGridSize gridSize)
        {
            var name = typeof(EGridSize).GetEnumName(gridSize);
            var m = new Regex(@"^([a-z]+)(\d*)$").Match(name);
            var unit = m.Groups[1].ToString();
            var num = int.Parse(m.Groups[2].ToString());
            switch (unit)
            {
                case "year":
                    return new DateTime(prevMajor).AddYears(num).Ticks;
                case "month":
                    return new DateTime(prevMajor).AddMonths(num).Ticks;
                default:
                    return prevMajor + (long)gridSize;
            }
        }

        public static EGridSize CalculateSize(double ticks)
        {
            foreach (long size in typeof(EGridSize).GetEnumValues())
            {
                var n = Math.Floor(ticks / size);
                if (3 <= n && n <= 10)
                    return (EGridSize)size;
            }
            return EGridSize.year20;
        }

        public void PaintWithDates()
        {
            if (!Enabled)
                return;
            this.pad.DrawLine(new Pen(Color), X1, Y1, X2, Y2, false);
            SolidBrush solidBrush1 = new SolidBrush(TitleColor);
            SolidBrush solidBrush2 = new SolidBrush(LabelColor);
            Pen Pen1 = new Pen(TitleColor);
            Pen Pen2 = new Pen(GridColor);
            Pen Pen3 = new Pen(MinorGridColor);
            Pen Pen4 = new Pen(MinorTicksColor);
            Pen Pen5 = new Pen(MajorTicksColor);
            Pen2.Width = GridWidth;
            Pen2.DashStyle = GridDashStyle;
            Pen3.Width = MinorGridWidth;
            Pen3.DashStyle = MinorGridDashStyle;
            var firstDateTime = new DateTime();
            double min = Min;
            double max = Max;
            firstDateTime = new DateTime(Math.Max(0, (long)min));
            DateTime dateTime1 = new DateTime((long)max);
            EGridSize gridSize = EGridSize.min1;
            this.pad.GetFirstGridDivision(ref gridSize, ref min, ref max, ref firstDateTime);
            double num1 = 5.0;
            double num2;
            DateTime dateTime2;
            switch (gridSize)
            {
                case EGridSize.year10:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1 + (9 - firstDateTime.Year % 10));
                    num2 = dateTime2.Ticks;
                    break;
                case EGridSize.year20:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1 + (19 - firstDateTime.Year % 20));
                    num2 = dateTime2.Ticks;
                    break;
                case EGridSize.year4:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1 + (3 - firstDateTime.Year % 4));
                    num2 = dateTime2.Ticks;
                    break;
                case EGridSize.year5:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1 + (4 - firstDateTime.Year % 5));
                    num2 = dateTime2.Ticks;
                    break;
                case EGridSize.year2:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1 + (1 - firstDateTime.Year % 2));
                    num2 = dateTime2.Ticks;
                    break;
                case EGridSize.year3:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1 + (2 - firstDateTime.Year % 3));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.month6:
                    DateTime dateTime3 = new DateTime(firstDateTime.Year, firstDateTime.Month, 1);
                    dateTime3 = dateTime3.AddMonths(1 + (12 - firstDateTime.Month) % 6);
                    num2 = (double)dateTime3.Ticks;
                    break;
                case EGridSize.year1:
                    dateTime2 = new DateTime(firstDateTime.Year, 1, 1);
                    dateTime2 = dateTime2.AddYears(1);
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.month3:
                    DateTime dateTime4 = new DateTime(firstDateTime.Year, firstDateTime.Month, 1);
                    dateTime4 = dateTime4.AddMonths(1 + (12 - firstDateTime.Month) % 3);
                    num2 = (double)dateTime4.Ticks;
                    break;
                case EGridSize.month4:
                    DateTime dateTime5 = new DateTime(firstDateTime.Year, firstDateTime.Month, 1);
                    dateTime5 = dateTime5.AddMonths(1 + (12 - firstDateTime.Month) % 4);
                    num2 = (double)dateTime5.Ticks;
                    break;
                case EGridSize.month1:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, 1);
                    dateTime2 = dateTime2.AddMonths(1);
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.month2:
                    DateTime dateTime6 = new DateTime(firstDateTime.Year, firstDateTime.Month, 1);
                    dateTime6 = dateTime6.AddMonths(1 + firstDateTime.Month % 2);
                    num2 = (double)dateTime6.Ticks;
                    break;
                case EGridSize.week1:
                    DateTime dateTime7 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day);
                    dateTime7 = dateTime7.AddDays(8.0 - (double)firstDateTime.DayOfWeek);
                    num2 = (double)dateTime7.Ticks;
                    break;
                case EGridSize.week2:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day).AddDays(8.0 - (double)firstDateTime.DayOfWeek + (double)(7 * (1 - (int)Math.Floor(new TimeSpan(firstDateTime.AddDays(8.0 - (double)firstDateTime.DayOfWeek).Ticks).TotalDays) / 7 % 2))).Ticks;
                    break;
                case EGridSize.day3:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day).AddDays((double)(1 + (2 - (int)new TimeSpan(firstDateTime.Ticks).TotalDays % 3)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.day5:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day).AddDays((double)(1 + (4 - (int)new TimeSpan(firstDateTime.Ticks).TotalDays % 5)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.day1:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day);
                    dateTime2 = dateTime2.AddDays(1.0);
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.day2:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day).AddDays((double)(1 + (int)new TimeSpan(firstDateTime.Ticks).TotalDays % 2)).Ticks;
                    break;
                case EGridSize.hour6:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, 0, 0).AddHours((double)(1 + (5 - (int)new TimeSpan(firstDateTime.Ticks).TotalHours % 6)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.hour12:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, 0, 0).AddHours((double)(1 + (11 - (int)new TimeSpan(firstDateTime.Ticks).TotalHours % 12)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.hour3:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, 0, 0).AddHours((double)(1 + (2 - (int)new TimeSpan(firstDateTime.Ticks).TotalHours % 3)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.hour4:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, 0, 0).AddHours((double)(1 + (3 - (int)new TimeSpan(firstDateTime.Ticks).TotalHours % 4)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.hour1:
                    DateTime dateTime8 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, 0, 0);
                    dateTime8 = dateTime8.AddHours(1.0);
                    num2 = (double)dateTime8.Ticks;
                    break;
                case EGridSize.hour2:
                    dateTime2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, 0, 0).AddHours((double)(1 + (1 - (int)new TimeSpan(firstDateTime.Ticks).TotalHours % 2)));
                    num2 = (double)dateTime2.Ticks;
                    break;
                case EGridSize.min20:
                    num2 = new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes((double)(1 + (19 - (int)new TimeSpan(firstDateTime.Ticks).TotalMinutes % 20))).Ticks;
                    break;
                case EGridSize.min30:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes((double)(1 + (29 - (int)new TimeSpan(firstDateTime.Ticks).TotalMinutes % 30))).Ticks;
                    break;
                case EGridSize.min10:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes((double)(1 + (9 - (int)new TimeSpan(firstDateTime.Ticks).TotalMinutes % 10))).Ticks;
                    break;
                case EGridSize.min15:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes((double)(1 + (14 - (int)new TimeSpan(firstDateTime.Ticks).TotalMinutes % 15))).Ticks;
                    break;
                case EGridSize.min1:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes(1.0).Ticks;
                    break;
                case EGridSize.min2:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes((double)(1 + (1 - (int)new TimeSpan(firstDateTime.Ticks).TotalMinutes % 2))).Ticks;
                    break;
                case EGridSize.min5:
                    num2 = (double)new DateTime(firstDateTime.Year, firstDateTime.Month, firstDateTime.Day, firstDateTime.Hour, firstDateTime.Minute, 0).AddMinutes((double)(1 + (4 - (int)new TimeSpan(firstDateTime.Ticks).TotalMinutes % 5))).Ticks;
                    break;
                default:
                    num2 = (double)(firstDateTime.Ticks + gridSize);
                    break;
            }
            int num3 = 0;
            int num4 = 0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num7 = 0.0;
            string str = "";
            int majorCount = 0;
            double num8 = max;
            firstDateTime = new DateTime((long)num2);
            DateTime dateTime9 = new DateTime((long)num8);
            while (num5 < num8)
            {
                num5 = this.pad.GetNextGridDivision(num2, num6, majorCount, gridSize);
                if (num5 < num8)
                {
                    if (Type == EAxisType.DateTime)
                    {
                        if (LabelFormat == null)
                        {
                            dateTime2 = new DateTime((long)num5);
                            str = dateTime2.ToString("MMM yyyy");
                        }
                        else
                        {
                            long num9 = (long)num5 % 864000000000L;
                            TimeSpan timeSpan = this.pad.SessionStart;
                            long ticks1 = timeSpan.Ticks;
                            string format;
                            if (num9 != ticks1)
                            {
                                long num10 = (long)num5 % 864000000000L;
                                timeSpan = this.pad.SessionEnd;
                                long ticks2 = timeSpan.Ticks;
                                if (num10 != ticks2)
                                {
                                    if (num6 == 0.0)
                                    {
                                        format = "yyy MMM dd HH:mm";
                                        goto label_47;
                                    }
                                    else
                                    {
                                        DateTime dateTime10 = new DateTime((long)num6);
                                        DateTime dateTime11 = new DateTime((long)num5);
                                        format = dateTime10.Year == dateTime11.Year ? (dateTime10.Month == dateTime11.Month ? (dateTime10.Day == dateTime11.Day ? (dateTime10.Minute != dateTime11.Minute || dateTime10.Hour != dateTime11.Hour ? "HH:mm" : "HH:mm:ss") : "MMM dd HH:mm") : "MMM dd HH:mm") : "yyy MMM dd HH:mm";
                                        goto label_47;
                                    }
                                }
                            }
                            format = num6 != 0.0 ? (new DateTime((long)num6).Year == new DateTime((long)num5).Year ? "MMM dd" : "yyyy MMM dd") : "yyy MMM dd";
                            label_47:
                            dateTime2 = new DateTime((long)num5);
                            str = dateTime2.ToString(format);
                            solidBrush2 = new SolidBrush(Color.Black);
                        }
                    }
                    if (Position == EAxisPosition.Bottom)
                    {
                        if (GridEnabled)
                            this.pad.DrawVerticalGrid(Pen2, num5);
                        if (MajorTicksEnabled)
                            this.pad.DrawVerticalTick(Pen1, num5, Y2, -5);
                        if (LabelEnabled)
                        {
                            SizeF sizeF = this.pad.Graphics.MeasureString(str, LabelFont);
                            int num9 = (int)sizeF.Width;
                            int num10 = (int)sizeF.Height;
                            if (LabelAlignment == EAxisLabelAlignment.Right)
                                this.pad.Graphics.DrawString(str, LabelFont, (Brush)solidBrush2, (float)this.pad.ClientX(num5), (float)(int)(Y2 + (double)LabelOffset));
                            if (LabelAlignment == EAxisLabelAlignment.Left)
                                this.pad.Graphics.DrawString(str, LabelFont, (Brush)solidBrush2, (float)(this.pad.ClientX(num5) - num9), (float)(int)(Y2 + (double)LabelOffset));
                            if (LabelAlignment == EAxisLabelAlignment.Centre)
                            {
                                int num11 = this.pad.ClientX(num5) - num9 / 2;
                                int num12 = (int)(Y2 + (double)LabelOffset);
                                if (majorCount == 0 || num11 - num3 >= 1)
                                {
                                    this.pad.Graphics.DrawString(str, LabelFont, (Brush)solidBrush2, (float)num11, (float)num12);
                                    num3 = num11 + num9;
                                }
                            }
                        }
                    }
                    if (Position == EAxisPosition.Left || Position == EAxisPosition.Right)
                    {
                        if (GridEnabled)
                            this.pad.DrawHorizontalGrid(Pen2, num5);
                        if (MajorTicksEnabled)
                            this.pad.DrawHorizontalTick(Pen5, X1, num5, 5);
                        if (LabelEnabled)
                        {
                            SizeF sizeF = this.pad.Graphics.MeasureString(str, LabelFont);
                            int num9 = (int)((double)sizeF.Width + (double)LabelOffset);
                            int num10 = (int)sizeF.Height;
                            if (LabelAlignment == EAxisLabelAlignment.Centre)
                            {
                                int num11 = (int)(X1 - (double)num9);
                                int num12 = this.pad.ClientY(num5) - num10 / 2;
                                if (majorCount == 0 || num4 - (num12 + num10) >= 1)
                                {
                                    this.pad.Graphics.DrawString(str, LabelFont, solidBrush2, (float)num11, (float)num12);
                                    num4 = num12;
                                }
                            }
                        }
                    }
                }
                if (majorCount != 0)
                {
                    if (majorCount == 1)
                        num7 = (num5 - num6 - this.pad.Transformation.CalculateNotInSessionTicks(num6, num5)) / num1;
                    for (int index = 1; (double)index <= num1; ++index)
                    {
                        double num9 = num6 + this.pad.Transformation.CalculateRealQuantityOfTicks_Right(num6, num6 + (double)index * num7);
                        if (num9 < max)
                        {
                            if (Position == EAxisPosition.Bottom)
                            {
                                if (MinorGridEnabled)
                                    this.pad.DrawVerticalGrid(Pen3, num9);
                                if (MinorTicksEnabled)
                                    this.pad.DrawVerticalTick(Pen4, num9, Y2, -2);
                            }
                            if (Position == EAxisPosition.Left)
                            {
                                if (MinorGridEnabled)
                                    this.pad.DrawHorizontalGrid(Pen3, num9);
                                if (MinorTicksEnabled)
                                    this.pad.DrawHorizontalTick(Pen5, X1, num9, 2);
                                if (Position == EAxisPosition.Right)
                                {
                                    if (MinorGridEnabled)
                                        this.pad.DrawHorizontalGrid(Pen3, num9);
                                    if (MinorTicksEnabled)
                                        this.pad.DrawHorizontalTick(Pen4, X2 - 2.0, num9, 2);
                                }
                            }
                        }
                    }
                }
                else
                    num2 = num5;
                num6 = num5;
                ++majorCount;
            }
            for (int index = 0; (double)index <= num1; ++index)
            {
                double num9 = num2 + this.pad.Transformation.CalculateRealQuantityOfTicks_Left(num2, num2 - (double)index * num7);
                if (num9 > Min)
                {
                    if (Position == EAxisPosition.Bottom)
                    {
                        if (MinorGridEnabled)
                            this.pad.DrawVerticalGrid(Pen3, num9);
                        if (MinorTicksEnabled)
                            this.pad.DrawVerticalTick(Pen4, num9, Y2, -2);
                    }
                    if (Position == EAxisPosition.Left)
                    {
                        if (MinorGridEnabled)
                            this.pad.DrawHorizontalGrid(Pen3, num9);
                        if (MinorTicksEnabled)
                            this.pad.DrawHorizontalTick(Pen4, X1, num9, 2);
                    }
                }
            }
            if (this.pad.SessionGridEnabled && ((TIntradayTransformation)this.pad.Transformation).Session >= 2L * (long)gridSize)
            {
                int num9 = 0;
                double x1;
                for (double x2 = (double)(((long)Min / 864000000000L + 1L) * 864000000000L); (x1 = x2 + this.pad.Transformation.CalculateRealQuantityOfTicks_Right(X2, X2 + (double)((long)num9 * ((TIntradayTransformation)this.pad.Transformation).Session))) < max; ++num9)
                    this.pad.DrawVerticalGrid(new Pen(this.pad.SessionGridColor), X1);
            }
            if (this.fOutlineEnabled)
            {
                if (Position == EAxisPosition.Bottom)
                {
                    this.pad.DrawVerticalGrid(new Pen(Color.Green), this.pad.WorldX(this.fOutline1));
                    this.pad.DrawVerticalGrid(new Pen(Color.Green), this.pad.WorldX(this.fOutline2));
                }
                if (Position == EAxisPosition.Left)
                {
                    this.pad.DrawHorizontalGrid(new Pen(Color.Green), this.pad.WorldY(this.fOutline1));
                    this.pad.DrawHorizontalGrid(new Pen(Color.Green), this.pad.WorldY(this.fOutline2));
                }
            }
            if (!TitleEnabled)
                return;
            int num13 = (int)this.pad.Graphics.MeasureString("Example", LabelFont).Height;
            int num14 = (int)this.pad.Graphics.MeasureString(Max.ToString("F1"), LabelFont).Width;
            int num15 = (int)this.pad.Graphics.MeasureString(Title, TitleFont).Height;
            int num16 = (int)this.pad.Graphics.MeasureString(Title, TitleFont).Width;
            if (Position == EAxisPosition.Bottom)
            {
                if (TitlePosition == EAxisTitlePosition.Left)
                    this.pad.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)(int)X1, (float)(int)(Y2 + (double)LabelOffset + (double)num13 + (double)TitleOffset));
                if (TitlePosition == EAxisTitlePosition.Right)
                    this.pad.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)((int)X2 - num16), (float)(int)(Y2 + (double)LabelOffset + (double)num13 + (double)TitleOffset));
                if (TitlePosition == EAxisTitlePosition.Centre)
                    this.pad.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)(int)(X1 + (X2 - X1 - (double)num16) / 2.0), (float)(int)(Y2 + (double)LabelOffset + (double)num13 + (double)TitleOffset));
            }
            if (Position != EAxisPosition.Left || TitlePosition != EAxisTitlePosition.Centre)
                return;
            this.pad.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)(int)(X1 - (double)LabelOffset - (double)num14 - (double)TitleOffset - (double)num15), (float)(int)(Y1 + (Y2 - Y1 - (double)num16) / 2.0), new StringFormat()
            {
                FormatFlags = StringFormatFlags.DirectionRightToLeft | StringFormatFlags.DirectionVertical
            });
            this.pad.Graphics.ResetTransform();
        }

        public virtual void Paint()
        {
            try
            {
                if (!Enabled)
                    return;
                if (VerticalGridStyle == EVerticalGridStyle.ByDateTime && Type == EAxisType.DateTime && Max > 100000.0)
                {
                    this.PaintWithDates();
                }
                else
                {
                    bool flag = false;
                    string str1 = "";
                    if (Max <= 1000000.0 && Type == EAxisType.DateTime)
                    {
                        Type = EAxisType.Numeric;
                        flag = true;
                        str1 = LabelFormat;
                        LabelFormat = "F1";
                    }
                    SolidBrush solidBrush1 = new SolidBrush(TitleColor);
                    SolidBrush solidBrush2 = new SolidBrush(LabelColor);
                    Pen pen = new Pen(TitleColor);
                    Pen Pen1 = new Pen(GridColor);
                    Pen Pen2 = new Pen(MinorGridColor);
                    Pen Pen3 = new Pen(MinorTicksColor);
                    Pen Pen4 = new Pen(MajorTicksColor);
                    Pen1.DashStyle = GridDashStyle;
                    Pen2.DashStyle = MinorGridDashStyle;
                    this.pad.DrawLine(new Pen(Color), X1, Y1, X2, Y2, false);
                    int num1 = 10;
                    int num2 = 5;
                    double num3 = Axis.Ceiling125(Math.Abs(Max - Min) * 0.999999 / (double)num1);
                    double num4 = Axis.Ceiling125(num3 / (double)num2);
                    double num5 = Math.Ceiling((Min - 0.001 * num3) / num3) * num3;
                    double num6 = Math.Floor((Max + 0.001 * num3) / num3) * num3;
                    int num7 = 0;
                    int num8 = 0;
                    if (num3 != 0.0)
                        num7 = Math.Min(10000, (int)Math.Floor((num6 - num5) / num3 + 0.5) + 1);
                    if (num3 != 0.0)
                        num8 = Math.Abs((int)Math.Floor(num3 / num4 + 0.5)) - 1;
                    int num9 = 0;
                    int num10 = 0;
                    int num11 = 0;
                    string str2 = "";
                    int num12 = 0;
                    for (int index1 = 0; index1 < num7; ++index1)
                    {
                        double num13 = num5 + (double)index1 * num3;
                        switch (Type)
                        {
                            case EAxisType.Numeric:
                                str2 = LabelFormat != null ? num13.ToString(LabelFormat) : num13.ToString("F1");
                                break;
                            case EAxisType.DateTime:
                                str2 = LabelFormat != null ? new DateTime((long)num13).ToString(LabelFormat) : new DateTime((long)num13).ToString("MMM yyyy");
                                break;
                        }
                        if (Position == EAxisPosition.Bottom)
                        {
                            if (GridEnabled)
                                this.pad.DrawVerticalGrid(Pen1, num13);
                            if (MajorTicksEnabled)
                                this.pad.DrawVerticalTick(Pen4, num13, Y2 - 1.0, -MajorTicksLength);
                            if (LabelEnabled)
                            {
                                SizeF sizeF = this.pad.Graphics.MeasureString(str2, LabelFont);
                                int num14 = (int)sizeF.Width;
                                num12 = (int)sizeF.Height;
                                if (LabelAlignment == EAxisLabelAlignment.Right)
                                    this.pad.Graphics.DrawString(str2, LabelFont, (Brush)solidBrush2, (float)this.pad.ClientX(num13), (float)(int)(Y2 + (double)LabelOffset));
                                if (LabelAlignment == EAxisLabelAlignment.Left)
                                    this.pad.Graphics.DrawString(str2, LabelFont, (Brush)solidBrush2, (float)(this.pad.ClientX(num13) - num14), (float)(int)(Y2 + (double)LabelOffset));
                                if (LabelAlignment == EAxisLabelAlignment.Centre)
                                {
                                    num9 = this.pad.ClientX(num13) - num14 / 2;
                                    int num15 = (int)(Y2 + (double)LabelOffset);
                                    if (index1 == 0 || num9 - num10 >= 1)
                                    {
                                        this.pad.Graphics.DrawString(str2, LabelFont, (Brush)solidBrush2, (float)num9, (float)num15);
                                        num10 = num9 + num14;
                                    }
                                }
                            }
                        }
                        if (Position == EAxisPosition.Left || Position == EAxisPosition.Right)
                        {
                            if (Position == EAxisPosition.Left && GridEnabled)
                                this.pad.DrawHorizontalGrid(Pen1, num13);
                            if (Position == EAxisPosition.Right && (!this.pad.AxisLeft.Enabled || !this.pad.AxisLeft.GridEnabled) && GridEnabled)
                                this.pad.DrawHorizontalGrid(Pen1, num13);
                            if (MajorTicksEnabled)
                            {
                                switch (Position)
                                {
                                    case EAxisPosition.Left:
                                        this.pad.DrawHorizontalTick(Pen4, X1 + 1.0, num13, MajorTicksLength);
                                        break;
                                    case EAxisPosition.Right:
                                        this.pad.DrawHorizontalTick(Pen4, X1 - (double)MajorTicksLength - 1.0, num13, MajorTicksLength);
                                        break;
                                }
                            }
                            if (LabelEnabled)
                            {
                                SizeF sizeF = this.pad.Graphics.MeasureString(str2, LabelFont);
                                int num14 = (int)((double)sizeF.Width + (double)LabelOffset);
                                int num15 = (int)sizeF.Height;
                                if (LabelAlignment == EAxisLabelAlignment.Centre)
                                {
                                    switch (Position)
                                    {
                                        case EAxisPosition.Left:
                                            num9 = (int)(X1 - (double)num14);
                                            break;
                                        case EAxisPosition.Right:
                                            num9 = (int)(X1 + 2.0);
                                            break;
                                    }
                                    int num16 = this.pad.ClientY(num13) - num15 / 2;
                                    if (index1 == 0 || num11 - (num16 + num15) >= 1)
                                    {
                                        if ((double)num16 > Y1 && (double)(num16 + num15) < Y2)
                                            this.pad.Graphics.DrawString(str2, LabelFont, solidBrush2, (float)num9, (float)num16);
                                        num11 = num16;
                                    }
                                }
                            }
                        }
                        for (int index2 = 1; index2 <= num8; ++index2)
                        {
                            double num14 = num5 + (double)index1 * num3 + (double)index2 * num4;
                            if (num14 < Max)
                            {
                                if (Position == EAxisPosition.Bottom)
                                {
                                    if (MinorGridEnabled)
                                        this.pad.DrawVerticalGrid(Pen2, num14);
                                    if (MinorTicksEnabled)
                                        this.pad.DrawVerticalTick(Pen3, num14, Y2 - 1.0, -MinorTicksLength);
                                }
                                if (Position == EAxisPosition.Left || Position == EAxisPosition.Right)
                                {
                                    if (Position == EAxisPosition.Left && MinorGridEnabled)
                                        this.pad.DrawHorizontalGrid(Pen1, num14);
                                    if (Position == EAxisPosition.Right && (!this.pad.AxisLeft.Enabled || !this.pad.AxisLeft.MinorGridEnabled) && MinorGridEnabled)
                                        this.pad.DrawHorizontalGrid(Pen1, num14);
                                    if (MinorTicksEnabled)
                                    {
                                        switch (Position)
                                        {
                                            case EAxisPosition.Left:
                                                this.pad.DrawHorizontalTick(Pen3, X1 + 1.0, num14, MinorTicksLength);
                                                continue;
                                            case EAxisPosition.Right:
                                                this.pad.DrawHorizontalTick(Pen3, X1 - (double)MinorTicksLength - 1.0, num14, MinorTicksLength);
                                                continue;
                                            default:
                                                continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (int index = 1; index <= num8; ++index)
                    {
                        double num13 = num5 - (double)index * num4;
                        if (num13 > Min)
                        {
                            if (Position == EAxisPosition.Bottom)
                            {
                                if (MinorGridEnabled)
                                    this.pad.DrawVerticalGrid(Pen2, num13);
                                if (MinorTicksEnabled)
                                    this.pad.DrawVerticalTick(Pen3, num13, Y2 - 1.0, -MinorTicksLength);
                            }
                            if (Position == EAxisPosition.Left || Position == EAxisPosition.Right)
                            {
                                if (Position == EAxisPosition.Left && MinorGridEnabled)
                                    this.pad.DrawHorizontalGrid(Pen1, num13);
                                if (Position == EAxisPosition.Right && (!this.pad.AxisLeft.Enabled || !this.pad.AxisLeft.MinorGridEnabled) && MinorGridEnabled)
                                    this.pad.DrawHorizontalGrid(Pen1, num13);
                                if (MinorTicksEnabled)
                                {
                                    switch (Position)
                                    {
                                        case EAxisPosition.Left:
                                            this.pad.DrawHorizontalTick(Pen3, X1 + 1.0, num13, MinorTicksLength);
                                            continue;
                                        case EAxisPosition.Right:
                                            this.pad.DrawHorizontalTick(Pen3, X1 - (double)MinorTicksLength - 1.0, num13, MinorTicksLength);
                                            continue;
                                        default:
                                            continue;
                                    }
                                }
                            }
                        }
                    }
                    if (this.fOutlineEnabled)
                    {
                        if (Position == EAxisPosition.Bottom)
                        {
                            this.pad.DrawVerticalGrid(new Pen(Color.Green), this.pad.WorldX(this.fOutline1));
                            this.pad.DrawVerticalGrid(new Pen(Color.Green), this.pad.WorldX(this.fOutline2));
                        }
                        if (Position == EAxisPosition.Left)
                        {
                            this.pad.DrawHorizontalGrid(new Pen(Color.Green), this.pad.WorldY(this.fOutline1));
                            this.pad.DrawHorizontalGrid(new Pen(Color.Green), this.pad.WorldY(this.fOutline2));
                        }
                    }
                    if (TitleEnabled)
                    {
                        int num13 = (int)this.pad.Graphics.MeasureString("Example", LabelFont).Height;
                        int num14 = (int)this.pad.Graphics.MeasureString(Max.ToString("F1"), LabelFont).Width;
                        int num15 = (int)this.pad.Graphics.MeasureString(Title, TitleFont).Height;
                        int num16 = (int)this.pad.Graphics.MeasureString(Title, TitleFont).Width;
                        if (Position == EAxisPosition.Bottom)
                        {
                            if (TitlePosition == EAxisTitlePosition.Left)
                                this.pad.Graphics.DrawString(Title, TitleFont, (Brush)solidBrush1, (float)(int)X1, (float)(int)(Y2 + (double)LabelOffset + (double)num13 + (double)TitleOffset));
                            if (TitlePosition == EAxisTitlePosition.Right)
                                this.pad.Graphics.DrawString(Title, TitleFont, (Brush)solidBrush1, (float)((int)X2 - num16), (float)(int)(Y2 + (double)LabelOffset + (double)num13 + (double)TitleOffset));
                            if (TitlePosition == EAxisTitlePosition.Centre)
                                this.pad.Graphics.DrawString(Title, TitleFont, (Brush)solidBrush1, (float)(int)(X1 + (X2 - X1 - (double)num16) / 2.0), (float)(int)(Y2 + (double)LabelOffset + (double)num13 + (double)TitleOffset));
                        }
                        if (Position == EAxisPosition.Left && TitlePosition == EAxisTitlePosition.Centre)
                        {
                            this.pad.Graphics.DrawString(Title, TitleFont, (Brush)solidBrush1, (float)(int)(X1 - (double)LabelOffset - (double)num14 - (double)TitleOffset - (double)num15), (float)(int)(Y1 + (Y2 - Y1 - (double)num16) / 2.0), new StringFormat()
                            {
                                FormatFlags = StringFormatFlags.DirectionRightToLeft | StringFormatFlags.DirectionVertical
                            });
                            this.pad.Graphics.ResetTransform();
                        }
                    }
                    if (!flag)
                        return;
                    Type = EAxisType.DateTime;
                    LabelFormat = str1;
                }
            }
            catch
            {
            }
        }

        public virtual void MouseMove(MouseEventArgs Event)
        {
            if (!this.fMouseDown)
                return;
            switch (Position)
            {
                case EAxisPosition.Left:
                    if (!this.pad.MouseZoomYAxisEnabled)
                        break;
                    this.fOutline2 = Event.Y;
                    this.pad.Update();
                    break;
                case EAxisPosition.Bottom:
                    if (!this.pad.MouseZoomXAxisEnabled)
                        break;
                    this.fOutline2 = Event.X;
                    this.pad.Update();
                    break;
            }
        }

        public virtual void MouseDown(MouseEventArgs Event)
        {
            if (Event.Button != MouseButtons.Left)
                return;
            switch (Position)
            {
                case EAxisPosition.Left:
                    if (!this.pad.MouseZoomYAxisEnabled || X1 - 10.0 > (double)Event.X || (X1 < (double)Event.X || Y1 > (double)Event.Y) || Y2 < (double)Event.Y)
                        break;
                    this.fMouseDown = true;
                    this.fMouseDownX = Event.X;
                    this.fMouseDownY = Event.Y;
                    this.fOutline1 = Event.Y;
                    this.fOutlineEnabled = true;
                    break;
                case EAxisPosition.Bottom:
                    if (!this.pad.MouseZoomXAxisEnabled || X1 > (double)Event.X || (X2 < (double)Event.X || Y1 > (double)Event.Y) || Y1 + 10.0 < (double)Event.Y)
                        break;
                    this.fMouseDown = true;
                    this.fMouseDownX = Event.X;
                    this.fMouseDownY = Event.Y;
                    this.fOutline1 = Event.X;
                    this.fOutlineEnabled = true;
                    break;
            }
        }

        public virtual void MouseUp(MouseEventArgs Event)
        {
            this.fOutlineEnabled = false;
            if (Event.Button == MouseButtons.Right)
            {
                switch (Position)
                {
                    case EAxisPosition.Left:
                        if (X1 - 10 <= Event.X && X1 >= Event.X && (Y1 <= Event.Y && Y2 >= Event.Y))
                        {
                            this.UnZoom();
                            break;
                        }
                        else
                            break;
                    case EAxisPosition.Bottom:
                        if (X1 <= Event.X && X2 >= Event.X && (Y1 <= Event.Y && Y1 + 10.0 >= Event.Y))
                        {
                            this.UnZoom();
                            break;
                        }
                        else
                            break;
                }
            }
            if (this.fMouseDown && Event.Button == MouseButtons.Left)
            {
                switch (Position)
                {
                    case EAxisPosition.Left:
                        if (this.pad.MouseZoomYAxisEnabled)
                        {
                            double num1 = this.pad.WorldY(this.fMouseDownY);
                            double num2 = this.pad.WorldY(Event.Y);
                            if (num1 < num2)
                            {
                                this.Zoom(num1, num2);
                                break;
                            }
                            else
                            {
                                this.Zoom(num2, num1);
                                break;
                            }
                        }
                        else
                            break;
                    case EAxisPosition.Bottom:
                        if (this.pad.MouseZoomXAxisEnabled)
                        {
                            double num1 = this.pad.WorldX(this.fMouseDownX);
                            double num2 = this.pad.WorldX(Event.X);
                            if (num1 < num2)
                            {
                                this.Zoom(num1, num2);
                                break;
                            }
                            else
                            {
                                this.Zoom(num2, num1);
                                break;
                            }
                        }
                        else
                            break;
                }
            }
            this.fMouseDown = false;
        }

        private static double Ceiling125(double x)
        {
            double num1 = x > 0.0 ? 1.0 : -1.0;
            if (x == 0.0)
                return 0.0;
            double d = Math.Log10(Math.Abs(x));
            double y = Math.Floor(d);
            double num2 = Math.Pow(10.0, d - y);
            double num3 = (num2 > 1.0 ? (num2 > 2.0 ? (num2 > 5.0 ? 10.0 : 5.0) : 2.0) : 1.0) * Math.Pow(10.0, y);
            return num1 * num3;
        }

        private static double Floor125(double x)
        {
            double num1 = x > 0.0 ? 1.0 : -1.0;
            if (x == 0.0)
                return 0.0;
            double d = Math.Log10(Math.Abs(x));
            double y = Math.Floor(d);
            double num2 = Math.Pow(10.0, d - y);
            double num3 = (num2 < 10.0 ? (num2 < 5.0 ? (num2 < 2.0 ? 1.0 : 2.0) : 5.0) : 10.0) * Math.Pow(10.0, y);
            return num1 * num3;
        }
    }
}
