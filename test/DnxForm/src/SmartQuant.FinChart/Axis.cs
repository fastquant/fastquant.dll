using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Text.RegularExpressions;

namespace SmartQuant.FinChart
{
    public enum EAxisLabelAlignment
    {
        Left,
        Right,
        Centre
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

    public interface IAxesMarked
    {
        Color Color { get; }
        double LastValue { get; }
        bool IsMarkEnable { get; }
        int LabelDigitsCount { get; }
    }

    public class Axis
    {
        protected Chart chart;
        protected EAxisTitlePosition titlePosition;
        protected bool enabled;
        protected bool zoomed;
        protected Color color;
        protected bool titleEnabled;
        protected string title;
        protected Font titleFont;
        protected Color titleColor;
        protected int titleOffset;
        protected bool labelEnabled;
        protected Font labelFont;
        protected Color labelColor;
        protected int labelOffset;
        protected string labelFormat;
        protected EAxisLabelAlignment labelAlignment;
        protected bool gridEnabled;
        protected Color gridColor;
        protected float gridWidth;
        protected DashStyle gridDashStyle;
        protected bool minorGridEnabled;
        protected Color minorGridColor;
        protected float minorGridWidth;
        protected DashStyle minorGridDashStyle;
        protected bool majorTicksEnabled;
        protected Color majorTicksColor;
        protected float majorTicksWidth;
        protected int majorTicksLength;
        protected bool minorTicksEnabled;
        protected Color minorTicksColor;
        protected float minorTicksWidth;
        protected int minorTicksLength;
        protected double min;
        protected double max;
        protected int width;
        protected int height;

        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
            }
        }

        public bool MajorTicksEnabled
        {
            get
            {
                return this.majorTicksEnabled;
            }
            set
            {
                this.majorTicksEnabled = value;
            }
        }

        public Color MajorTicksColor
        {
            get
            {
                return this.majorTicksColor;
            }
            set
            {
                this.majorTicksColor = value;
            }
        }

        public float MajorTicksWidth
        {
            get
            {
                return this.majorTicksWidth;
            }
            set
            {
                this.majorTicksWidth = value;
            }
        }

        public int MajorTicksLength
        {
            get
            {
                return this.majorTicksLength;
            }
            set
            {
                this.majorTicksLength = value;
            }
        }

        public bool MinorTicksEnabled
        {
            get
            {
                return this.minorTicksEnabled;
            }
            set
            {
                this.minorTicksEnabled = value;
            }
        }

        public Color MinorTicksColor
        {
            get
            {
                return this.minorTicksColor;
            }
            set
            {
                this.minorTicksColor = value;
            }
        }

        public float MinorTicksWidth
        {
            get
            {
                return this.minorTicksWidth;
            }
            set
            {
                this.minorTicksWidth = value;
            }
        }

        public int MinorTicksLength
        {
            get
            {
                return this.minorTicksLength;
            }
            set
            {
                this.minorTicksLength = value;
            }
        }

        public EAxisTitlePosition TitlePosition
        {
            get
            {
                return this.titlePosition;
            }
            set
            {
                this.titlePosition = value;
            }
        }

        public Font TitleFont
        {
            get
            {
                return this.titleFont;
            }
            set
            {
                this.titleFont = value;
            }
        }

        public Color TitleColor
        {
            get
            {
                return this.titleColor;
            }
            set
            {
                this.titleColor = value;
            }
        }

        public int TitleOffset
        {
            get
            {
                return this.titleOffset;
            }
            set
            {
                this.titleOffset = value;
            }
        }

        public double Min
        {
            get
            {
                return this.min;
            }
            set
            {
                this.min = value;
            }
        }

        public double Max
        {
            get
            {
                return this.max;
            }
            set
            {
                this.max = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        public bool Zoomed
        {
            get
            {
                return this.zoomed;
            }
            set
            {
                this.zoomed = value;
            }
        }

        public bool GridEnabled
        {
            get
            {
                return this.gridEnabled;
            }
            set
            {
                this.gridEnabled = value;
            }
        }

        public Color GridColor
        {
            get
            {
                return this.gridColor;
            }
            set
            {
                this.gridColor = value;
            }
        }

        public float GridWidth
        {
            get
            {
                return this.gridWidth;
            }
            set
            {
                this.gridWidth = value;
            }
        }

        public DashStyle GridDashStyle
        {
            get
            {
                return this.gridDashStyle;
            }
            set
            {
                this.gridDashStyle = value;
            }
        }

        public bool MinorGridEnabled
        {
            get
            {
                return this.minorGridEnabled;
            }
            set
            {
                this.minorGridEnabled = value;
            }
        }

        public Color MinorGridColor
        {
            get
            {
                return this.minorGridColor;
            }
            set
            {
                this.minorGridColor = value;
            }
        }

        public float MinorGridWidth
        {
            get
            {
                return this.minorGridWidth;
            }
            set
            {
                this.minorGridWidth = value;
            }
        }

        public DashStyle MinorGridDashStyle
        {
            get
            {
                return this.minorGridDashStyle;
            }
            set
            {
                this.minorGridDashStyle = value;
            }
        }

        public bool TitleEnabled
        {
            get
            {
                return this.titleEnabled;
            }
            set
            {
                this.titleEnabled = value;
            }
        }

        public bool LabelEnabled
        {
            get
            {
                return this.labelEnabled;
            }
            set
            {
                this.labelEnabled = value;
            }
        }

        public Font LabelFont
        {
            get
            {
                return this.labelFont;
            }
            set
            {
                this.labelFont = value;
            }
        }

        public Color LabelColor
        {
            get
            {
                return this.labelColor;
            }
            set
            {
                this.labelColor = value;
            }
        }

        public int LabelOffset
        {
            get
            {
                return this.labelOffset;
            }
            set
            {
                this.labelOffset = value;
            }
        }

        public string LabelFormat
        {
            get
            {
                return this.labelFormat;
            }
            set
            {
                this.labelFormat = value;
            }
        }

        public EAxisLabelAlignment LabelAlignment
        {
            get
            {
                return this.labelAlignment;
            }
            set
            {
                this.labelAlignment = value;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }
    }

    public class AxisBottom : Axis
    {
        protected double x1;
        protected double x2;
        protected double y;

        public double X1
        {
            get
            {
                return this.x1;
            }
            set
            {
                this.x1 = value;
            }
        }

        public double X2
        {
            get
            {
                return this.x2;
            }
            set
            {
                this.x2 = value;
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }

        public AxisBottom(Chart chart, int x1, int x2, int y)
        {
            this.chart = chart;
            SetBounds(x1, x2, y);
            Enabled = true;
            Zoomed = false;
            Color = Color.LightGray;
            Title = "";
            TitleEnabled = true;
            TitlePosition = EAxisTitlePosition.Centre;
            TitleFont = this.chart.Font;
            TitleColor = Color.Black;
            TitleOffset = 2;
            LabelEnabled = true;
            LabelFont = this.chart.Font;
            LabelColor = Color.LightGray;
            LabelFormat = null;
            LabelOffset = 2;
            LabelAlignment = EAxisLabelAlignment.Centre;
            GridEnabled = true;
            GridColor = Color.LightGray;
            GridDashStyle = DashStyle.DashDotDot;
            GridWidth = 0.5f;
            MinorGridEnabled = false;
            MinorGridColor = Color.LightGray;
            MinorGridDashStyle = DashStyle.Solid;
            MinorGridWidth = 0.5f;
            MajorTicksEnabled = true;
            MajorTicksColor = Color.LightGray;
            MajorTicksWidth = 0.5f;
            MajorTicksLength = 4;
            MinorTicksEnabled = true;
            MinorTicksColor = Color.LightGray;
            MinorTicksWidth = 0.5f;
            MinorTicksLength = 1;
            Width = -1;
            Height = -1;
        }

        public void SetBounds(int x1, int x2, int y)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.y = y;
        }

        //TODO: rewrite it!
        private long GetGridDivision(DateTime dateTime, EGridSize gridSize)
        {
            long num;
            switch (gridSize)
            {
                case EGridSize.year5:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1 + 4 - dateTime.Year % 5).Ticks;
                    break;
                case EGridSize.year10:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1 + (9 - dateTime.Year % 10)).Ticks;
                    break;
                case EGridSize.year20:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1 + (19 - dateTime.Year % 20)).Ticks;
                    break;
                case EGridSize.year3:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1 + (2 - dateTime.Year % 3)).Ticks;
                    break;
                case EGridSize.year4:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1 + (3 - dateTime.Year % 4)).Ticks;
                    break;
                case EGridSize.month6:
                    DateTime dateTime1 = new DateTime(dateTime.Year, dateTime.Month, 1);
                    dateTime1 = dateTime1.AddMonths(1 + (12 - dateTime.Month) % 6);
                    num = dateTime1.Ticks;
                    break;
                case EGridSize.year1:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1).Ticks;
                    break;
                case EGridSize.year2:
                    num = new DateTime(dateTime.Year, 1, 1).AddYears(1 + (1 - dateTime.Year % 2)).Ticks;
                    break;
                case EGridSize.month3:
                    DateTime dateTime2 = new DateTime(dateTime.Year, dateTime.Month, 1);
                    dateTime2 = dateTime2.AddMonths(1 + (12 - dateTime.Month) % 3);
                    num = dateTime2.Ticks;
                    break;
                case EGridSize.month4:
                    DateTime dateTime3 = new DateTime(dateTime.Year, dateTime.Month, 1);
                    dateTime3 = dateTime3.AddMonths(1 + (12 - dateTime.Month) % 4);
                    num = dateTime3.Ticks;
                    break;
                case EGridSize.week2:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays(8.0 - (double)dateTime.DayOfWeek + (double)(7 * (1 - (int)Math.Floor(new TimeSpan(dateTime.AddDays(8.0 - (double)dateTime.DayOfWeek).Ticks).TotalDays) / 7 % 2))).Ticks;
                    break;
                case EGridSize.month1:
                    DateTime dateTime4 = new DateTime(dateTime.Year, dateTime.Month, 1);
                    dateTime4 = dateTime4.AddMonths(1);
                    num = dateTime4.Ticks;
                    break;
                case EGridSize.month2:
                    DateTime dateTime5 = new DateTime(dateTime.Year, dateTime.Month, 1);
                    dateTime5 = dateTime5.AddMonths(1 + dateTime.Month % 2);
                    num = dateTime5.Ticks;
                    break;
                case EGridSize.day5:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays((double)(1 + (4 - (int)new TimeSpan(dateTime.Ticks).TotalDays % 5))).Ticks;
                    break;
                case EGridSize.week1:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays(8.0 - (double)dateTime.DayOfWeek).Ticks;
                    break;
                case EGridSize.day2:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays((double)(1 + (int)new TimeSpan(dateTime.Ticks).TotalDays % 2)).Ticks;
                    break;
                case EGridSize.day3:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays((double)(1 + (2 - (int)new TimeSpan(dateTime.Ticks).TotalDays % 3))).Ticks;
                    break;
                case EGridSize.hour12:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours((double)(1 + (11 - (int)new TimeSpan(dateTime.Ticks).TotalHours % 12))).Ticks;
                    break;
                case EGridSize.day1:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddDays(1).Ticks;
                    break;
                case EGridSize.hour3:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours((double)(1 + (2 - (int)new TimeSpan(dateTime.Ticks).TotalHours % 3))).Ticks;
                    break;
                case EGridSize.hour4:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours((double)(1 + (3 - (int)new TimeSpan(dateTime.Ticks).TotalHours % 4))).Ticks;
                    break;
                case EGridSize.hour6:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours((double)(1 + (5 - (int)new TimeSpan(dateTime.Ticks).TotalHours % 6))).Ticks;
                    break;
                case EGridSize.hour1:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours(1.0).Ticks;
                    break;
                case EGridSize.hour2:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours((double)(1 + (1 - (int)new TimeSpan(dateTime.Ticks).TotalHours % 2))).Ticks;
                    break;
                case EGridSize.min20:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes((double)(1 + (19 - (int)new TimeSpan(dateTime.Ticks).TotalMinutes % 20))).Ticks;
                    break;
                case EGridSize.min30:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes((double)(1 + (29 - (int)new TimeSpan(dateTime.Ticks).TotalMinutes % 30))).Ticks;
                    break;
                case EGridSize.min10:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes((double)(1 + (9 - (int)new TimeSpan(dateTime.Ticks).TotalMinutes % 10))).Ticks;
                    break;
                case EGridSize.min15:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes((double)(1 + (14 - (int)new TimeSpan(dateTime.Ticks).TotalMinutes % 15))).Ticks;
                    break;
                case EGridSize.min1:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes(1.0).Ticks;
                    break;
                case EGridSize.min2:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes((double)(1 + (1 - (int)new TimeSpan(dateTime.Ticks).TotalMinutes % 2))).Ticks;
                    break;
                case EGridSize.min5:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes((double)(1 + (4 - (int)new TimeSpan(dateTime.Ticks).TotalMinutes % 5))).Ticks;
                    break;
                case EGridSize.sec20:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second).AddSeconds((double)(1 + (19 - (int)new TimeSpan(dateTime.Ticks).TotalSeconds % 20))).Ticks;
                    break;
                case EGridSize.sec30:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second).AddSeconds((double)(1 + (29 - (int)new TimeSpan(dateTime.Ticks).TotalSeconds % 30))).Ticks;
                    break;
                case EGridSize.sec5:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second).AddSeconds((double)(1 + (4 - (int)new TimeSpan(dateTime.Ticks).TotalSeconds % 5))).Ticks;
                    break;
                case EGridSize.sec10:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second).AddSeconds((double)(1 + (9 - (int)new TimeSpan(dateTime.Ticks).TotalSeconds % 10))).Ticks;
                    break;
                case EGridSize.sec1:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second).AddSeconds(1).Ticks;
                    break;
                case EGridSize.sec2:
                    num = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second).AddSeconds((double)(1 + (1 - (int)new TimeSpan(dateTime.Ticks).TotalSeconds % 2))).Ticks;
                    break;
                default:
                    num = (long)(dateTime.Ticks + gridSize);
                    break;
            }
            return num;
        }

        //TODO: rewrite it!
        public void PaintWithDates(DateTime minDate, DateTime maxDate)
        {
            var solidBrush1 = new SolidBrush(TitleColor);
            var solidBrush2 = new SolidBrush(LabelColor);
            var Pen1 = new Pen(TitleColor);
            var Pen2 = new Pen(GridColor);
            var pen1 = new Pen(MinorGridColor);
            var pen2 = new Pen(MinorTicksColor);
            var pen3 = new Pen(MajorTicksColor);
            Pen2.Width = GridWidth;
            Pen2.DashStyle = GridDashStyle;
            pen1.Width = MinorGridWidth;
            pen1.DashStyle = MinorGridDashStyle;
            long ticks1 = minDate.Ticks;
            long ticks2 = maxDate.Ticks;
            var dateTime1 = new DateTime(Math.Max(0, ticks1));
            var egridSize = CalculateSize(ticks2 - ticks1);
            long num1 = 0;
            long gridDivision = GetGridDivision(dateTime1, egridSize);
            int num2 = 0;
            long num3 = gridDivision;
            long num4 = 0;
            int num5 = 0;
            long num6 = ticks2;
            int num7 = -1;
            while (num3 < num6)
            {
                if (num5 != 0)
                    num3 = GetNextMajor(num4, egridSize);
                long num8 = num3;
                int index = this.chart.MainSeries.GetIndex(new DateTime(num3 - 1), IndexOption.Next);
                if (num7 == index)
                {
                    num4 = num3;
                }
                else
                {
                    num7 = index;
                    if (index != -1)
                    {
                        DateTime dateTime2 = this.chart.MainSeries.GetDateTime(index);
                        TimeSpan timeOfDay = dateTime2.TimeOfDay;
                        long ticks3 = dateTime2.Ticks;
                        if (ticks3 < num6)
                        {
                            if (GridEnabled)
                                this.chart.DrawVerticalGrid(Pen2, ticks3);
                            if (MajorTicksEnabled)
                                this.chart.DrawVerticalTick(Pen1, ticks3, -5);
                            if (LabelEnabled)
                            {
                                string format;
                                if (ticks3 % 864000000000L == this.chart.SessionStart.Ticks || ticks3 % 864000000000L == this.chart.SessionEnd.Ticks)
                                    format = num4 != 0 ? (new DateTime(num4).Year == new DateTime(ticks3).Year ? "MMM dd" : "yyyy MMM dd") : "yyy MMM dd";
                                else if (num4 == 0)
                                {
                                    format = "yyy MMM dd HH:mm";
                                }
                                else
                                {
                                    DateTime dateTime3 = new DateTime(num4);
                                    DateTime dateTime4 = new DateTime(ticks3);
                                    format = dateTime3.Year == dateTime4.Year ? (dateTime3.Month == dateTime4.Month ? (dateTime3.Day == dateTime4.Day ? (dateTime3.Minute != dateTime4.Minute || dateTime3.Hour != dateTime4.Hour ? "HH:mm" : "HH:mm:ss") : "MMM dd HH:mm") : "MMM dd HH:mm") : "yyy MMM dd HH:mm";
                                }
                                string str = new DateTime(ticks3).ToString(format);
                                SizeF sizeF = this.chart.Graphics.MeasureString(str, LabelFont);
                                int num9 = (int)sizeF.Width;
                                int num10 = (int)sizeF.Height;
                                if (LabelAlignment == EAxisLabelAlignment.Right)
                                    this.chart.Graphics.DrawString(str, this.labelFont, solidBrush2, (float)this.chart.ClientX(new DateTime(ticks3)), (float)(int)(Y + LabelOffset));
                                if (LabelAlignment == EAxisLabelAlignment.Left)
                                    this.chart.Graphics.DrawString(str, this.labelFont, solidBrush2, (float)(this.chart.ClientX(new DateTime(ticks3)) - num9), (float)(int)(Y + LabelOffset));
                                if (LabelAlignment == EAxisLabelAlignment.Centre)
                                {
                                    int num11 = this.chart.ClientX(new DateTime(ticks3)) - num9 / 2;
                                    int num12 = (int)(Y + LabelOffset);
                                    if (num5 == 0 || num11 - num2 >= 1)
                                    {
                                        this.chart.Graphics.DrawString(str, LabelFont, solidBrush2, num11, num12);
                                        num2 = num11 + num9;
                                    }
                                }
                            }
                        }
                        num1 = ticks3;
                        num3 = num8;
                        num4 = num3;
                        ++num5;
                    }
                }
            }
            if (this.chart.SessionGridEnabled && (EGridSize)(this.chart.SessionEnd - this.chart.SessionStart).Ticks >= egridSize)
            {
                int num8 = 0;
                long X;
                for (long index = ticks1 / 864000000000L * 864000000000L + this.chart.SessionStart.Ticks; (X = index + (long)num8 * 864000000000L) < ticks2; ++num8)
                    this.chart.DrawSessionGrid(new Pen(this.chart.SessionGridColor), X);
            }
            if (!this.titleEnabled)
                return;
            int num13 = (int)this.chart.Graphics.MeasureString("Example", LabelFont).Height;
            int num14 = (int)this.chart.Graphics.MeasureString(ticks2.ToString("F1"), LabelFont).Width;
            double num15 = (double)this.chart.Graphics.MeasureString(Title, TitleFont).Height;
            int num16 = (int)this.chart.Graphics.MeasureString(Title, TitleFont).Width;
            if (this.titlePosition == EAxisTitlePosition.Left)
                this.chart.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)(int)this.x1, (float)(int)(this.y + (double)this.labelOffset + (double)num13 + (double)this.titleOffset));
            if (this.titlePosition == EAxisTitlePosition.Right)
                this.chart.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)((int)this.x2 - num16), (float)(int)(this.y + (double)this.labelOffset + (double)num13 + (double)this.titleOffset));
            if (this.titlePosition != EAxisTitlePosition.Centre)
                return;
            this.chart.Graphics.DrawString(Title, TitleFont, solidBrush1, (float)(int)(this.x1 + (this.x2 - this.x1 - (double)num16) / 2.0), (float)(int)(this.y + (double)this.labelOffset + (double)num13 + (double)this.titleOffset));
        }

        public static EGridSize CalculateSize(double ticks)
        {
            foreach (object value in typeof(EGridSize).GetEnumValues())
            {
                long size = (long)value;
                double n = Math.Floor(ticks / size);
                if (3 <= n && n <= 10)
                    return (EGridSize)size;
            }
            return EGridSize.year20;
        }

        public static long GetNextMajor(long prevMajor, EGridSize gridSize)
        {
            string name = typeof(EGridSize).GetEnumName(gridSize);
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
    }

    public class AxisRight : Axis
    {
        protected double x;
        protected double y1;
        protected double y2;
        private Pad pad;

        public double X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public double Y1
        {
            get
            {
                return this.y1;
            }
            set
            {
                this.y1 = value;
            }
        }

        public double Y2
        {
            get
            {
                return this.y2;
            }
            set
            {
                this.y2 = value;
            }
        }

        public AxisRight(Chart chart, Pad pad, int x, int y1, int y2)
        {
            this.chart = chart;
            this.pad = pad;
            SetBounds(x, y1, y2);
            Enabled = true;
            Zoomed = false;
            Color = Color.LightGray;
            Title = "";
            TitleEnabled = true;
            TitlePosition = EAxisTitlePosition.Centre;
            TitleFont = this.chart.Font;
            TitleColor = Color.Black;
            TitleOffset = 2;
            LabelEnabled = true;
            LabelFont = this.chart.Font;
            LabelColor = this.chart.RightAxisTextColor;
            LabelFormat = null;
            LabelOffset = 2;
            LabelAlignment = EAxisLabelAlignment.Centre;
            GridEnabled = true;
            GridColor = this.chart.RightAxisGridColor;
            GridDashStyle = DashStyle.Dash;
            GridWidth = 0.5f;
            MinorGridEnabled = false;
            MinorGridColor = this.chart.RightAxisMinorTicksColor;
            MinorGridDashStyle = DashStyle.Solid;
            MinorGridWidth = 0.5f;
            MajorTicksEnabled = true;
            MajorTicksColor = this.chart.RightAxisMajorTicksColor;
            MajorTicksWidth = 0.5f;
            MajorTicksLength = 4;
            MinorTicksEnabled = true;
            MinorTicksColor = Color.LightGray;
            MinorTicksWidth = 0.5f;
            MinorTicksLength = 1;
            Width = -1;
            Height = -1;
        }

        public void SetBounds(int x, int y1, int y2)
        {
            this.x = x;
            this.y1 = y1;
            this.y2 = y2;
        }

        public int GetAxisGap()
        {
            return (int)this.chart.Graphics.MeasureString(this.pad.MaxValue.ToString(this.pad.AxisLabelFormat), this.chart.Font).Width;
        }

        public void Paint()
        {
            var solidBrush1 = new SolidBrush(TitleColor);
            var solidBrush2 = new SolidBrush(LabelColor);
            Pen pen1 = new Pen(TitleColor);
            Pen pen2 = new Pen(GridColor);
            pen2.DashStyle = GridDashStyle;
            Pen pen3 = new Pen(MinorGridColor);
            pen3.DashStyle = MinorGridDashStyle;
            Pen pen4 = new Pen(MinorTicksColor);
            Pen pen5 = new Pen(MajorTicksColor);
            this.min = this.pad.MinValue;
            this.max = this.pad.MaxValue;
            int num1 = 10;
            int num2 = 5;
            double num3 = AxisRight.Ceiling125(Math.Abs(this.max - this.min) * 0.999999 / (double)num1);
            double num4 = AxisRight.Ceiling125(num3 / (double)num2);
            double num5 = Math.Ceiling((this.min - 0.001 * num3) / num3) * num3;
            double num6 = Math.Floor((this.max + 0.001 * num3) / num3) * num3;
            int num7 = 0;
            int num8 = 0;
            if (num3 != 0.0)
                num7 = Math.Min(10000, (int)Math.Floor((num6 - num5) / num3 + 0.5) + 1);
            if (num3 != 0.0)
                num8 = Math.Abs((int)Math.Floor(num3 / num4 + 0.5)) - 1;
            int num9 = 0;
            for (int index1 = 0; index1 < num7; ++index1)
            {
                double num10 = num5 + (double)index1 * num3;
                string str = num10.ToString(this.pad.AxisLabelFormat);
                this.pad.DrawHorizontalGrid(pen2, num10);
                this.pad.DrawHorizontalTick(pen5, this.x - (double)this.majorTicksLength - 1.0, num10, this.majorTicksLength);
                if (this.labelEnabled)
                {
                    var sizeF = this.pad.Graphics.MeasureString(str, this.labelFont);
                    double num11 = (double)sizeF.Width;
                    int num12 = this.labelOffset;
                    int num13 = (int)sizeF.Height;
                    if (this.labelAlignment == EAxisLabelAlignment.Centre)
                    {
                        int num14 = (int)(this.x + 2.0);
                        int num15 = this.pad.ClientY(num10) - num13 / 2;
                        if (index1 == 0 || num9 - (num15 + num13) >= 1)
                        {
                            if ((double)num15 > this.y1 && (double)(num15 + num13) < this.y2)
                                this.pad.Graphics.DrawString(str, LabelFont, solidBrush2, num14, num15);
                            num9 = num15;
                        }
                    }
                }
                for (int index2 = 1; index2 <= num8; ++index2)
                {
                    double y = num5 + (double)index1 * num3 + (double)index2 * num4;
                    if (y < this.max)
                        this.pad.DrawHorizontalTick(pen4, this.x - (double)this.minorTicksLength - 1.0, y, this.minorTicksLength);
                }
            }
            for (int index = 1; index <= num8; ++index)
            {
                double y = num5 - (double)index * num4;
                if (y > this.min && this.minorTicksEnabled)
                    this.pad.DrawHorizontalTick(pen4, this.x - (double)this.minorTicksLength - 1.0, y, this.minorTicksLength);
            }
            foreach (IChartDrawable chartDrawable in this.pad.Primitives)
            {
                if (chartDrawable is IAxesMarked)
                {
                    var axesMarked = chartDrawable as IAxesMarked;
                    if (axesMarked.IsMarkEnable)
                    {
                        double lastValue = axesMarked.LastValue;
                        if (!double.IsNaN(lastValue))
                        {
                            string str = lastValue.ToString("F" + axesMarked.LabelDigitsCount.ToString());
                            SizeF sizeF = this.chart.Graphics.MeasureString(str, this.chart.Font);
                            Color color = Color.FromArgb((int)axesMarked.Color.R ^ 128, (int)axesMarked.Color.G ^ 128, (int)axesMarked.Color.B ^ 128);
                            if (Color.Equals(axesMarked.Color, Color.Black))
                        //    if (this.CompareColors(axesMarked.Color, Color.Black))
                                color = Color.White;
                            if (Color.Equals(axesMarked.Color, Color.White))
                           // if (this.CompareColors(axesMarked.Color, Color.White))
                                color = Color.Black;
                            this.pad.Graphics.FillRectangle((Brush)new SolidBrush(axesMarked.Color), (float)this.X, (float)((double)this.pad.ClientY(axesMarked.LastValue) - (double)sizeF.Height / 2.0 - 2.0), sizeF.Width, sizeF.Height + 2f);
                            this.pad.Graphics.DrawString(str, this.chart.RightAxesFont, (Brush)new SolidBrush(color), (float)this.X + 2f, (float)((double)this.pad.ClientY(axesMarked.LastValue) - (double)sizeF.Height / 2.0 - 1.0));
                        }
                    }
                }
            }
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
