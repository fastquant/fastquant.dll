using SmartQuant;
using SmartQuant.FinChart.Objects;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

#if GTK
using Compatibility.Gtk;

#else
using System.Windows.Forms;
#endif

namespace SmartQuant.FinChart
{
    public enum ChartUpdateStyle
    {
        WholeRange,
        Trailing,
        Fixed,
    }

    public enum EGridSize : long
    {
        sec1 = TimeSpan.TicksPerSecond,
        sec2 = 2 * sec1,
        sec5 = 5 * sec1,
        sec10 = 10 * sec1,
        sec20 = 20 * sec1,
        sec30 = 30 * sec1,
        min1 = TimeSpan.TicksPerMinute,
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
        day1 = TimeSpan.TicksPerDay,
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
        year20 = 20 * year1,
    }
    public enum ChartActionType
    {
        Cross,
        None
    }
    enum ChartCursorType
    {
        Default,
        VSplitter,
        Hand,
        Cross
    }
    public enum BSStyle
    {
        Candle,
        Bar,
        Line,
        Kagi,
        Ranko,
        LineBreak,
        PointAndFigure
    }

    public class Distance
    {
        public double DX { get; set; }

        public double DY { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public string ToolTipText { get; set; }

        public Distance()
        {
            DX = DY = double.MaxValue;
            ToolTipText = null;
        }
    }

    public partial class Chart
    {
        private int prevMouseX = -1;
        private int prevMouseY = -1;
        protected SmoothingMode TimeSeriesSmoothingMode = SmoothingMode.HighSpeed;
        protected PadList pads = new PadList();
        protected int minCountOfBars = 125;
        protected int canvasLeftOffset = 20;
        protected int canvasTopOffset = 20;
        protected int canvasRightOffset = 20;
        protected int canvasBottomOffset = 30;
        protected Color canvasColor = Color.MidnightBlue;
        protected ArrayList padsHeightArray = new ArrayList();
        protected int padSplitIndex = -1;
        protected bool contentUpdated = true;
        private ChartActionType actionType = ChartActionType.None;
        protected ChartUpdateStyle updateStyle = ChartUpdateStyle.Trailing;
        protected int minAxisGap = 50;
        private bool contextMenuEnabled = true;
        private int labelDigitsCount = 2;
        private int rightAxesFontSize = 7;
        private Color dateTipRectangleColor = Color.LightGray;
        private Color dateTipTextColor = Color.Black;
        private Color valTipRectangleColor = Color.LightGray;
        private Color valTipTextColor = Color.Black;
        private Color crossColor = Color.DarkGray;
        private Color borderColor = Color.Gray;
        private Color splitterColor = Color.LightGray;
        private Color candleUpColor = Color.Black;
        private Color candleDownColor = Color.Lime;
        private Color volumeColor = Color.SteelBlue;
        private Color rightAxisGridColor = Color.DimGray;
        private Color rightAxisTextColor = Color.LightGray;
        private Color rightAxisMinorTicksColor = Color.LightGray;
        private Color rightAxisMajorTicksColor = Color.LightGray;
        private Color itemTextColor = Color.LightGray;
        private Color selectedItemTextColor = Color.Yellow;
        private Color selectedFillHighlightColor = Color.LightBlue;
        private Color activeStopColor = Color.Yellow;
        private Color executedStopColor = Color.MediumSeaGreen;
        private Color canceledStopColor = Color.Gray;
        private object lockObject = new object();
        private DateTime lastDate = DateTime.MaxValue;
        private DateTime polosaDate = DateTime.MinValue;
        private IContainer components;
        private Image primitiveDeleteImage;
        private Image primitivePropertiesImage;
        protected Color sessionGridColor;
        protected TimeSpan sessionStart;
        protected TimeSpan sessionEnd;
        protected bool sessionGridEnabled;
        protected SmoothingMode smoothingMode;
        protected BSStyle barSeriesStyle;
        protected SeriesView mainSeriesView;
        protected ISeries mainSeries;
        protected ISeries series;
        protected int firstIndex;
        protected int lastIndex;
        protected Graphics graphics;
        protected double intervalWidth;
        protected AxisBottom axisBottom;
        protected int mouseX;
        protected int mouseY;
        protected bool padSplit;
        protected bool isMouseOverCanvas;
        protected Bitmap bitmap;
        protected DateTime leftDateTime;
        protected DateTime rightDateTime;
        protected bool volumePadShown;
        protected PadScaleStyle scaleStyle;

        private bool drawItems;
        internal Font RightAxesFont;
        private Color chartBackColor;

        public bool ContextMenuEnabled
        {
            get
            {
                return this.contextMenuEnabled;
            }
            set
            {
                this.contextMenuEnabled = value;
            }
        }

        public int RightAxesFontSize
        {
            get
            {
                return this.rightAxesFontSize;
            }
            set
            {
                this.rightAxesFontSize = value;
                this.RightAxesFont = new Font(Font.FontFamily, this.rightAxesFontSize);
            }
        }

        public int LabelDigitsCount
        {
            get
            {
                return this.labelDigitsCount;
            }
            set
            {
                this.labelDigitsCount = value;
            }
        }

        public Image PrimitiveDeleteImage
        {
            get
            {
                return this.primitiveDeleteImage;
            }
            set
            {
                this.primitiveDeleteImage = value;
            }
        }

        public Image PrimitivePropertiesImage
        {
            get
            {
                return this.primitivePropertiesImage;
            }
            set
            {
                this.primitivePropertiesImage = value;
            }
        }

        [Browsable(false)]
        public bool DrawItems
        {
            get
            {
                return this.drawItems;
            }
            set
            {
                this.drawItems = value;
            }
        }

        [Browsable(false)]
        public bool VolumePadVisible
        {
            get
            {
                return this.volumePadShown;
            }
            set
            {
                if (value)
                    this.ShowVolumePad();
                else
                    this.HideVolumePad();
            }
        }

        public ChartUpdateStyle UpdateStyle
        {
            get
            {
                return this.updateStyle;
            }
            set
            {
                this.updateStyle = value;
                EmitUpdateStyleChanged();
            }
        }

        public BSStyle BarSeriesStyle
        {
            get
            {
                return this.barSeriesStyle;
            }
            set
            {
                this.drawItems = true;
                if (this.barSeriesStyle == value)
                    return;
                lock (this.lockObject)
                {
                    this.barSeriesStyle = value;
                    if (this.mainSeries != null)
                    {
                        bool local_0 = this.SetBarSeriesStyle(this.barSeriesStyle, false);
                        if (this.volumePadShown)
                        {
                            int temp_55 = local_0 ? 1 : 0;
                        }
                        if (local_0)
                        {
                            this.firstIndex = Math.Max(0, this.mainSeries.Count - this.minCountOfBars);
                            this.lastIndex = this.mainSeries.Count - 1;
                            if (this.mainSeries.Count == 0)
                                this.firstIndex = -1;
                            if (this.lastIndex >= 0)
                                SetIndexInterval(this.firstIndex, this.lastIndex);
                        }
                        this.contentUpdated = true;
                    }
                    EmitBarSeriesStyleChanged();
                    Invalidate();
                }
            }
        }

        [Category("Transformation")]
        [Description("")]
        public bool SessionGridEnabled
        {
            get
            {
                return this.sessionGridEnabled;
            }
            set
            {
                this.sessionGridEnabled = value;
            }
        }

        [Description("")]
        [Category("Transformation")]
        public Color SessionGridColor
        {
            get
            {
                return this.sessionGridColor;
            }
            set
            {
                this.sessionGridColor = value;
            }
        }

        [Description("")]
        [Category("Transformation")]
        public TimeSpan SessionStart
        {
            get
            {
                return this.sessionStart;
            }
            set
            {
                this.sessionStart = value;
            }
        }

        [Category("Transformation")]
        [Description("")]
        public TimeSpan SessionEnd
        {
            get
            {
                return this.sessionEnd;
            }
            set
            {
                this.sessionEnd = value;
            }
        }

        public double IntervalWidth
        {
            get
            {
                return this.intervalWidth;
            }
        }

        public Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public SmoothingMode SmoothingMode
        {
            get
            {
                return this.smoothingMode;
            }
            set
            {
                this.smoothingMode = value;
            }
        }

        internal ISeries Series
        {
            get
            {
                return this.series;
            }
        }

        public ISeries MainSeries
        {
            get
            {
                return this.mainSeries;
            }
        }

        public int FirstIndex
        {
            get
            {
                return this.firstIndex;
            }
        }

        public int LastIndex
        {
            get
            {
                return this.lastIndex;
            }
        }

        public int PadCount
        {
            get
            {
                return this.pads.Count;
            }
        }

        public Color CanvasColor
        {
            get
            {
                return this.canvasColor;
            }
            set
            {
                this.contentUpdated = true;
                this.canvasColor = value;
            }
        }

        public Color ChartBackColor
        {
            get
            {
                return this.chartBackColor;
            }
            set
            {
                this.contentUpdated = true;
                this.chartBackColor = value;
            }
        }

        public ChartActionType ActionType
        {
            get
            {
                return this.actionType;
            }
            set
            {
                if (this.actionType == value)
                    return;
                this.actionType = value;
                EmitActionTypeChanged();
                Invalidate();
            }
        }

        public int MinNumberOfBars
        {
            get
            {
                return this.minCountOfBars;
            }
            set
            {
                this.minCountOfBars = value;
            }
        }

        internal bool ContentUpdated
        {
            get
            {
                return this.contentUpdated;
            }
            set
            {
                this.contentUpdated = value;
            }
        }

        public PadList Pads
        {
            get
            {
                return this.pads;
            }
        }

        public Color SelectedFillHighlightColor
        {
            get
            {
                return this.selectedFillHighlightColor;
            }
            set
            {
                this.selectedFillHighlightColor = Color.FromArgb(100, value);
                this.contentUpdated = true;
            }
        }

        public Color ItemTextColor
        {
            get
            {
                return this.itemTextColor;
            }
            set
            {
                this.itemTextColor = value;
                this.contentUpdated = true;
            }
        }

        public Color SelectedItemTextColor
        {
            get
            {
                return this.selectedItemTextColor;
            }
            set
            {
                this.selectedItemTextColor = value;
                this.contentUpdated = true;
            }
        }

        public Color BottomAxisGridColor
        {
            get
            {
                return this.axisBottom.GridColor;
            }
            set
            {
                this.contentUpdated = true;
                this.axisBottom.GridColor = value;
            }
        }

        public Color BottomAxisLabelColor
        {
            get
            {
                return this.axisBottom.LabelColor;
            }
            set
            {
                this.contentUpdated = true;
                this.axisBottom.LabelColor = value;
            }
        }

        public Color RightAxisGridColor
        {
            get
            {
                return this.rightAxisGridColor;
            }
            set
            {
                this.contentUpdated = true;
                foreach (Pad pad in this.pads)
                    pad.Axis.GridColor = value;
                this.rightAxisGridColor = value;
            }
        }

        public Color RightAxisTextColor
        {
            get
            {
                return this.rightAxisTextColor;
            }
            set
            {
                this.contentUpdated = true;
                foreach (Pad pad in this.pads)
                    pad.Axis.LabelColor = value;
                this.rightAxisTextColor = value;
            }
        }

        public Color RightAxisMinorTicksColor
        {
            get
            {
                return this.rightAxisMinorTicksColor;
            }
            set
            {
                this.contentUpdated = true;
                foreach (Pad pad in this.pads)
                    pad.Axis.MinorTicksColor = value;
                this.rightAxisMinorTicksColor = value;
            }
        }

        public Color RightAxisMajorTicksColor
        {
            get
            {
                return this.rightAxisMajorTicksColor;
            }
            set
            {
                this.contentUpdated = true;
                foreach (Pad pad in this.pads)
                    pad.Axis.MajorTicksColor = value;
                this.rightAxisMajorTicksColor = value;
            }
        }

        public Color DateTipRectangleColor
        {
            get
            {
                return this.dateTipRectangleColor;
            }
            set
            {
                this.contentUpdated = true;
                this.dateTipRectangleColor = value;
            }
        }

        public Color DateTipTextColor
        {
            get
            {
                return this.dateTipTextColor;
            }
            set
            {
                this.contentUpdated = true;
                this.dateTipTextColor = value;
            }
        }

        public Color ValTipRectangleColor
        {
            get
            {
                return this.valTipRectangleColor;
            }
            set
            {
                this.contentUpdated = true;
                this.valTipRectangleColor = value;
            }
        }

        public Color ValTipTextColor
        {
            get
            {
                return this.valTipTextColor;
            }
            set
            {
                this.contentUpdated = true;
                this.valTipTextColor = value;
            }
        }

        public Color CrossColor
        {
            get
            {
                return this.crossColor;
            }
            set
            {
                this.contentUpdated = true;
                this.crossColor = value;
            }
        }

        public Color BorderColor
        {
            get
            {
                return this.borderColor;
            }
            set
            {
                this.contentUpdated = true;
                this.borderColor = value;
            }
        }

        public Color SplitterColor
        {
            get
            {
                return this.splitterColor;
            }
            set
            {
                this.contentUpdated = true;
                this.splitterColor = value;
            }
        }

        public PadScaleStyle ScaleStyle
        {
            get
            {
                return this.scaleStyle;
            }
            set
            {
                this.scaleStyle = value;
                this.pads[0].ScaleStyle = value;
                this.contentUpdated = true;
                this.Invalidate();
                this.EmitScaleStyleChanged();
            }
        }

        public event EventHandler UpdateStyleChanged;

        public event EventHandler VolumeVisibleChanged;

        public event EventHandler ActionTypeChanged;

        public event EventHandler BarSeriesStyleChanged;

        public event EventHandler ScaleStyleChanged;

        public Chart()
        {
            InitializeComponent();
            RightAxesFont = new Font(Font.FontFamily, this.rightAxesFontSize);
            this.canvasLeftOffset = 10;
            this.canvasTopOffset = 10;
            this.canvasRightOffset = 40;
            this.canvasBottomOffset = 40;
            AddPad();
            this.axisBottom = new AxisBottom(this, this.canvasLeftOffset, this.Width - this.canvasRightOffset, this.Height - this.canvasTopOffset);
            this.chartBackColor = Color.MidnightBlue;
            this.firstIndex = -1;
            this.lastIndex = -1;
        }

        public Chart(TimeSeries mainSeries)
            : this()
        {
            SetMainSeries(mainSeries);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        internal void DrawVerticalTick(Pen Pen, long x, int length)
        {
            this.graphics.DrawLine(Pen, this.ClientX(new DateTime(x)), this.canvasTopOffset + this.Height - (this.canvasBottomOffset + this.canvasTopOffset), this.ClientX(new DateTime(x)), this.canvasTopOffset + this.Height - (this.canvasBottomOffset + this.canvasTopOffset) + length);
        }

        internal void DrawVerticalGrid(Pen pen, long x)
        {
            int x1 = ClientX(new DateTime(x));
            this.graphics.DrawLine(pen, x1, this.canvasTopOffset, x1, this.canvasTopOffset + this.Height - (this.canvasBottomOffset + this.canvasTopOffset));
        }

        internal void DrawSessionGrid(Pen pen, long x)
        {
            this.graphics.DrawLine(pen, (int)((double)ClientX(new DateTime(x)) - this.intervalWidth / 2.0), this.canvasTopOffset, (int)((double)this.ClientX(new DateTime(x)) - this.intervalWidth / 2.0), this.canvasTopOffset + this.Height - (this.canvasBottomOffset + this.canvasTopOffset));
        }

        public void DrawSeries(TimeSeries series, int padNumber, Color color)
        {
            DrawSeries(series, padNumber, color, SearchOption.ExactFirst);
        }

        public void DrawSeries(TimeSeries series, int padNumber, Color color, SearchOption option)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                var view = new DSView(this.pads[padNumber], series, color, option, this.TimeSeriesSmoothingMode);
                this.pads[padNumber].AddPrimitive(view);
                view.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
            }
        }

        public void DrawSeries(TimeSeries series, int padNumber, Color color, SimpleDSStyle style)
        {
            DrawSeries(series, padNumber, color, style, SearchOption.ExactFirst, this.TimeSeriesSmoothingMode);
        }

        public void DrawSeries(TimeSeries series, int padNumber, Color color, SimpleDSStyle style, SmoothingMode smoothingMode)
        {
            DrawSeries(series, padNumber, color, style, SearchOption.ExactFirst, smoothingMode);
        }

        public DSView DrawSeries(TimeSeries series, int padNumber, Color color, SimpleDSStyle style, SearchOption option, SmoothingMode smoothingMode)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                DSView local_0 = new DSView(this.pads[padNumber], series, color, option, smoothingMode);
                local_0.Style = style;
                this.pads[padNumber].AddPrimitive(local_0);
                local_0.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
                return local_0;
            }
        }

        private void OnPrimitiveUpdated(object sender, EventArgs e)
        {
            this.contentUpdated = true;
            Invalidate();
        }

        public void DrawFill(Fill fill, int padNumber)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                FillView view = new FillView(fill, this.pads[padNumber]);
                this.pads[padNumber].AddPrimitive(view);
                view.SetInterval(this.leftDateTime, this.rightDateTime);
            }
        }

        public void DrawLine(DrawingLine line, int padNumber)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                LineView local_0 = new LineView(line, this.pads[padNumber]);
                line.Updated += new EventHandler(this.OnPrimitiveUpdated);
                this.pads[padNumber].AddPrimitive((IChartDrawable)local_0);
                local_0.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
            }
        }

        public void DrawEllipse(DrawingEllipse circle, int padNumber)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                var view = new EllipseView(circle, this.pads[padNumber]);
                circle.Updated += new EventHandler(this.OnPrimitiveUpdated);
                this.pads[padNumber].AddPrimitive(view);
                view.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
            }
        }

        public void DrawRectangle(DrawingRectangle rect, int padNumber)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                var view = new RectangleView(rect, this.pads[padNumber]);
                rect.Updated += new EventHandler(this.OnPrimitiveUpdated);
                this.pads[padNumber].AddPrimitive(view);
                view.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
            }
        }

        public void DrawPath(DrawingPath path, int padNumber)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                PathView local_0 = new PathView(path, this.pads[padNumber]);
                path.Updated += new EventHandler(this.OnPrimitiveUpdated);
                this.pads[padNumber].AddPrimitive((IChartDrawable)local_0);
                local_0.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
            }
        }

        public void DrawImage(DrawingImage image, int padNumber)
        {
            lock (this.lockObject)
            {
                if (!this.volumePadShown && padNumber > 1)
                    --padNumber;
                ImageView local_0 = new ImageView(image, this.pads[padNumber]);
                image.Updated += new EventHandler(this.OnPrimitiveUpdated);
                this.pads[padNumber].AddPrimitive((IChartDrawable)local_0);
                local_0.SetInterval(this.leftDateTime, this.rightDateTime);
                this.contentUpdated = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            lock (this.lockObject)
            {
                try
                {
                    Update(e.Graphics);
                    if (this.firstIndex < 0 || this.lastIndex <= 0)
                        return;
                    #if GTK
                    if (this.scrollbar.Adjustment.Upper != MainSeries.Count-1)
                    this.scrollbar.Adjustment.Upper = MainSeries.Count-1;
//                    if (this.scrollbar.Adjustment.Upper != this.mainSeries.Count - (this.lastIndex - this.firstIndex + 1) + this.scrollbar.Adjustment.PageIncrement - 1)
//                        this.scrollbar.Adjustment.Upper = this.mainSeries.Count - (this.lastIndex - this.firstIndex + 1) + this.scrollbar.Adjustment.PageIncrement - 1;
//                    Console.WriteLine("scrollvalue:{0} min:{1} max:{2}", this.scrollbar.Value,this.scrollbar.Adjustment.Lower,this.scrollbar.Adjustment.Upper );

                     if (this.scrollbar.Value == this.firstIndex)
                        return;
                    this.scrollbar.Value = this.firstIndex;
                    Console.WriteLine("this.firstIndex:{0} this.lastIndex:{1}", this.firstIndex, this.lastIndex);

                    #else
                    if (this.scrollBar.Maximum != MainSeries.Count - (this.lastIndex - this.firstIndex + 1) + this.scrollBar.LargeChange - 1)
                        this.scrollBar.Maximum = MainSeries.Count - (this.lastIndex - this.firstIndex + 1) + this.scrollBar.LargeChange - 1;
                    Console.WriteLine("scrollvalue:{0} min:{1} max:{2}", this.scrollBar.Value, this.scrollBar.Minimum, this.scrollBar.Maximum);

                    Console.WriteLine("this.firstIndex:{0} this.lastIndex:{1}", this.firstIndex, this.lastIndex);

                    if (this.scrollBar.Value == this.firstIndex)
                        return;
                    this.scrollBar.Value = this.firstIndex;
                    #endif
                }
                catch (Exception)
                {
                }
            }
        }

        private void Update(Graphics graphics)
        {
            if (this.lastIndex - this.firstIndex + 1 == 0)
                return;
            int num1 = Width - this.canvasLeftOffset - this.canvasRightOffset;
            int height = Height;
            this.intervalWidth = (double)(num1 / (this.lastIndex - this.firstIndex + 1));

            if (this.contentUpdated)
            {
                if (this.bitmap != null)
                    this.bitmap.Dispose();
                this.bitmap = new Bitmap(Width, Height);
                using (var g = Graphics.FromImage(this.bitmap))
                {
                   
                    g.SmoothingMode = SmoothingMode;
                    g.Clear(ChartBackColor);

                    this.graphics = g;
                    int val1 = int.MinValue;
                    foreach (Pad pad in this.pads)
                    {
                        pad.PrepareForUpdate();
                        val1 = Math.Max(val1, pad.AxisGap + 2);
                    }
                    this.canvasRightOffset = Math.Max(val1, this.minAxisGap);
                    foreach (Pad pad in this.pads)
                    {
                        pad.DrawItems = DrawItems;
                        pad.Width = Width - this.canvasRightOffset - this.canvasLeftOffset;
                    }
                    g.FillRectangle(new SolidBrush(this.canvasColor), this.canvasLeftOffset, this.canvasTopOffset, Width - this.canvasRightOffset - this.canvasLeftOffset, this.Height - this.canvasBottomOffset - this.canvasLeftOffset);
                    if (this.polosaDate != DateTime.MinValue)
                    {
                        int num2 = this.ClientX(this.polosaDate);
                        if (num2 > this.canvasLeftOffset && num2 < this.Width - this.canvasRightOffset)
                            g.FillRectangle(new SolidBrush(this.selectedFillHighlightColor), (float)num2 - (float)this.intervalWidth / 2f, this.canvasTopOffset, (float)this.intervalWidth, Height - this.canvasBottomOffset - this.canvasLeftOffset);
                    }
                    g.DrawRectangle(new Pen(this.borderColor), this.canvasLeftOffset, this.canvasTopOffset, Width - this.canvasRightOffset - this.canvasLeftOffset, Height - this.canvasBottomOffset - this.canvasLeftOffset);
                    if (this.mainSeries != null && this.mainSeries.Count != 0)
                        this.axisBottom.PaintWithDates(this.mainSeries.GetDateTime(this.firstIndex), this.mainSeries.GetDateTime(this.lastIndex));
                    foreach (Pad pad in this.pads)
                        pad.Update(g);
                    for (int i = 1; i < this.pads.Count; ++i)
                        g.DrawLine(new Pen(this.splitterColor), this.pads[i].X1, this.pads[i].Y1, this.pads[i].X2, this.pads[i].Y1);
                }
                this.contentUpdated = false;
            }

            // Draw date and value tips
            if (MainSeries != null && MainSeries.Count != 0 && ActionType == ChartActionType.Cross && this.isMouseOverCanvas && this.bitmap != null)
            {
                graphics.DrawImage(this.bitmap, 0, 0);
                graphics.SmoothingMode = SmoothingMode;
                graphics.DrawLine(new Pen(CrossColor, 0.5f), this.canvasLeftOffset, this.mouseY, this.mouseX - 10, this.mouseY);
                graphics.DrawLine(new Pen(CrossColor, 0.5f), this.mouseX + 10, this.mouseY, Width - this.canvasRightOffset, this.mouseY);
                graphics.DrawLine(new Pen(CrossColor, 0.5f), this.mouseX, this.canvasTopOffset, this.mouseX, this.mouseY - 10);
                graphics.DrawLine(new Pen(CrossColor, 0.5f), this.mouseX, this.mouseY + 10, this.mouseX, Height - this.canvasBottomOffset);
                string dateTip = GetDateTime(this.mouseX).ToString();
                var dateTipSize = graphics.MeasureString(dateTip, this.Font);
                graphics.FillRectangle(new SolidBrush(DateTipRectangleColor), this.mouseX - dateTipSize.Width / 2 - 2f, Height - this.canvasBottomOffset, dateTipSize.Width, dateTipSize.Height + 2f);
                graphics.DrawString(dateTip, Font, new SolidBrush(DateTipTextColor), this.mouseX - dateTipSize.Width / 2 - 1f, Height - this.canvasBottomOffset + 2f);
                double num2 = 0.0;
                for (int i = 0; i < this.pads.Count; ++i)
                {
                    Pad pad = this.pads[i];
                    if (pad.Y1 < this.mouseY && this.mouseY < pad.Y2)
                    {
                        num2 = pad.WorldY(this.mouseY);
                        break;
                    }
                }
                string valTip = num2.ToString("F" + this.labelDigitsCount);
                var valTipSize = graphics.MeasureString(valTip, this.Font);
                graphics.FillRectangle(new SolidBrush(ValTipRectangleColor),  Width - this.canvasRightOffset, this.mouseY - valTipSize.Height / 2 - 2f, valTipSize.Width, valTipSize.Height + 2f);
                graphics.DrawString(valTip, Font, new SolidBrush(ValTipTextColor), Width - this.canvasRightOffset + 2f, this.mouseY - valTipSize.Height / 2 - 1f);
            }
            else
            {
                if (this.bitmap != null)
                    graphics.DrawImage(this.bitmap, 0, 0);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        private void OnChartMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (this.isMouseOverCanvas)
                {
                    foreach (Pad pad in this.pads)
                    {
                        if (pad.Y1 - 1 <= e.Y && e.Y <= pad.Y1 + 1)
                        {
                            this.padSplit = true;
                            this.padSplitIndex = this.pads.IndexOf(pad);
                            return;
                        }
                    }
                }
                foreach (Pad pad in this.pads)
                {
                    if (pad.X1 <= e.X && pad.X2 >= e.X && (pad.Y1 <= e.Y && pad.Y2 >= e.Y))
                        pad.MouseDown(e);
                }
            }
            catch
            {
            }
        }

        private void OnChartMouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (this.padSplit)
                    this.padSplit = false;
                foreach (Pad pad in this.pads)
                {
                    if (pad.X1 <= e.X && pad.X2 >= e.X && (pad.Y1 <= e.Y && pad.Y2 >= e.Y))
                        pad.MouseUp(e);
                }
                Invalidate();
            }
            catch
            {
            }
        }

        private void OnChartMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn(e.Delta / 20);
            else
                ZoomOut(-e.Delta / 20);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                this.mouseX = e.X;
                this.mouseY = e.Y;
                if (this.prevMouseX != this.mouseX || this.prevMouseY != this.mouseY)
                {
                    if (this.canvasLeftOffset < e.X && e.X < Width - this.canvasRightOffset && this.canvasTopOffset < e.Y && e.Y < Height - this.canvasBottomOffset)
                    {
                        this.isMouseOverCanvas = true;
                        if (this.actionType == ChartActionType.Cross)
                            SetCursor(ChartCursorType.Cross);
                    }
                    else
                    {
                        this.isMouseOverCanvas = false;
                        if (this.actionType == ChartActionType.Cross)
                            Invalidate();
                        SetCursor(ChartCursorType.Default);
                    }

                    if (this.padSplit && this.padSplitIndex != 0)
                    {
                        Pad pad1 = this.pads[this.padSplitIndex];
                        Pad pad2 = this.pads[this.padSplitIndex - 1];
                        int num1 = e.Y;
                        if (pad1.Y2 - e.Y < 20)
                            num1 = pad1.Y2 - 20;
                        if (e.Y - pad2.Y1 < 20)
                            num1 = pad2.Y1 + 20;
                        if (pad1.Y2 - num1 >= 20 && num1 - pad2.Y1 >= 20)
                        {
                            int num2 = pad1.Y2 - num1;
                            int num3 = num1 - pad2.Y1;
                            this.padsHeightArray[this.padSplitIndex] = (object)((double)num2 / (double)(this.Height - this.canvasTopOffset - this.canvasBottomOffset));
                            this.padsHeightArray[this.padSplitIndex - 1] = (object)((double)num3 / (double)(this.Height - this.canvasTopOffset - this.canvasBottomOffset));
                            pad1.SetCanvas(pad1.X1, pad1.X2, num1, pad1.Y2);
                            pad2.SetCanvas(pad2.X1, pad2.X2, pad2.Y1, num1);
                        }
                        this.contentUpdated = true;
                        Invalidate();
                    }

                    foreach (Pad pad in this.pads)
                        if (pad.Y1 - 1 <= e.Y && e.Y <= pad.Y1 + 1 && this.pads.IndexOf(pad) != 0)
                            SetCursor(ChartCursorType.VSplitter);

                    foreach (Pad pad in this.pads)
                        if (pad.X1 <= e.X && e.X <= pad.X2 && pad.Y1 <= e.Y && e.Y <= pad.Y2)
                            pad.MouseMove(e);

                    if (this.isMouseOverCanvas && this.actionType == ChartActionType.Cross)
                        Invalidate();
                }
                this.prevMouseX = this.mouseX;
                this.prevMouseY = this.mouseY;
            }
            catch
            {
            }
        }

        private void OnChartMouseLeave(object sender, EventArgs e)
        {
            this.isMouseOverCanvas = false;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetPadSizes();
            this.contentUpdated = true;
            if (this.axisBottom != null)
                this.axisBottom.SetBounds(this.canvasLeftOffset, Width - this.canvasRightOffset, Height - this.canvasBottomOffset);
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // no-op
        }

        private void ZoomIn(int delta)
        {
            SetIndexInterval(Math.Min(this.firstIndex + delta, this.lastIndex - 1 + 1), this.lastIndex);
            Invalidate();
        }

        private void ZoomOut(int delta)
        {
            if (MainSeries == null || MainSeries.Count == 0)
                return;
            SetIndexInterval(Math.Max(0, this.firstIndex - delta), this.lastIndex);
            Invalidate();
        }

        public void ZoomIn()
        {
            ZoomIn((this.lastIndex - this.firstIndex) / 5);
        }

        public void ZoomOut()
        {
            ZoomOut((this.lastIndex - this.firstIndex) / 10 + 1);
        }

        public void UnSelectAll()
        {
            foreach (Pad pad in this.pads)
            {
                if (pad.SelectedPrimitive != null)
                {
                    pad.SelectedPrimitive.UnSelect();
                    pad.SelectedPrimitive = null;
                }
            }
        }

        public virtual void ShowProperties(DSView view, Pad pad, bool forceShowProperties)
        {
        }

        public void AddPad()
        {
            lock (this.lockObject)
            {
                FillPadsHeightArray();
                this.pads.Add(new Pad(this, this.canvasLeftOffset, this.Width - this.canvasRightOffset, this.canvasTopOffset, this.Height - this.canvasBottomOffset));
                SetPadSizes();
                this.contentUpdated = true;
            }
        }

        private void SetPadSizes()
        {
            int y1 = this.canvasTopOffset;
            int num1 = Height - this.canvasBottomOffset - this.canvasTopOffset;
            int index = 0;
            double num2 = 0.0;
            foreach (Pad pad in this.pads)
            {
                num2 += (double)this.padsHeightArray[index];
                int y2 = (int)((double)this.canvasTopOffset + (double)num1 * num2);
                pad.SetCanvas(this.canvasLeftOffset, this.Width - this.canvasRightOffset, y1, y2);
                ++index;
                y1 = y2;
            }
        }

        private void FillPadsHeightArray()
        {
            if (this.padsHeightArray.Count == 0)
            {
                this.padsHeightArray.Add((object)1.0);
            }
            else
            {
                this.padsHeightArray.Add((object)0.0);
                int count = this.padsHeightArray.Count;
                if (this.volumePadShown)
                    --count;
                this.padsHeightArray[0] = (object)(3.0 / (double)(count + 2));
                for (int i = 1; i < this.padsHeightArray.Count; ++i)
                {
                    if (this.volumePadShown && i == 1)
                    {
                        this.padsHeightArray[1] = (object)((double)this.padsHeightArray[0] / 6.0);
                        this.padsHeightArray[0] = (object)((double)this.padsHeightArray[1] * 5.0);
                    }
                    else
                        this.padsHeightArray[i] = (object)(1.0 / (double)(count + 2));
                }
            }
        }

        public void ShowVolumePad()
        {
        }

        public void HideVolumePad()
        {
        }

        public int ClientX(DateTime dateTime)
        {
            double num = (double)(this.Width - this.canvasLeftOffset - this.canvasRightOffset) / (double)(this.lastIndex - this.firstIndex + 1);
            return this.canvasLeftOffset + (int)((double)(this.mainSeries.GetIndex(dateTime, IndexOption.Null) - this.firstIndex) * num + num / 2.0);
        }

        public DateTime GetDateTime(int x)
        {
            double num = (double)(Width - this.canvasLeftOffset - this.canvasRightOffset) / (double)(this.lastIndex - this.firstIndex + 1);
            return this.mainSeries.GetDateTime((int)Math.Floor((double)(x - this.canvasLeftOffset) / num) + this.firstIndex);
        }

        public void Reset()
        {
            lock (this.lockObject)
            {
                foreach (Pad pad in this.pads)
                {
                    pad.Reset();
                    foreach (object primitive in pad.Primitives)
                        if (primitive is IUpdatable)
                            (primitive as IUpdatable).Updated -= OnPrimitiveUpdated;
                }
                this.pads.Clear();
                this.padsHeightArray.Clear();
                this.volumePadShown = false;
                AddPad();
                this.firstIndex = -1;
                this.lastIndex = -1;
                this.mainSeries = null;
                this.polosaDate = DateTime.MinValue;
                this.contentUpdated = true;
                if (this.updateStyle == ChartUpdateStyle.Fixed)
                    this.UpdateStyle = ChartUpdateStyle.Trailing;
                BarSeriesStyle = BSStyle.Candle;
            }
        }

        public void SetMainSeries(ISeries mainSeries)
        {
            SetMainSeries(mainSeries, false, Color.Black);
        }

        public void SetMainSeries(ISeries mainSeries, bool showVolumePad, Color color)
        {
            lock (this.lockObject)
            {
                ISeries temp_5 = this.mainSeries;
                this.series = mainSeries;
                if (mainSeries is BarSeries)
                    SetBarSeriesStyle(BarSeriesStyle, true);
                else
                {
                    this.mainSeries = this.series;
                    this.mainSeriesView = new DSView(this.pads[0], mainSeries as TimeSeries, color, SearchOption.ExactFirst, SmoothingMode.HighSpeed);
                    this.pads[0].AddPrimitive(this.mainSeriesView);
                }
                this.pads[0].ScaleStyle = this.scaleStyle;
                if (showVolumePad)
                    this.ShowVolumePad();
                this.firstIndex = this.updateStyle != ChartUpdateStyle.WholeRange ? Math.Max(0, mainSeries.Count - this.minCountOfBars) : 0;
                this.lastIndex = mainSeries.Count - 1;
                if (mainSeries.Count == 0)
                    this.firstIndex = -1;
                if (this.lastIndex >= 0)
                    SetIndexInterval(this.firstIndex, this.lastIndex);
                this.contentUpdated = true;
                Invalidate();
            }
        }

        private void SetIndexInterval(int firstIndex, int lastIndex)
        {
            if (MainSeries == null || firstIndex < 0 || lastIndex > MainSeries.Count - 1)
                return;
            this.firstIndex = firstIndex;
            this.lastIndex = lastIndex;
            this.leftDateTime = firstIndex >= 0 ? MainSeries.GetDateTime(this.firstIndex) : DateTime.MaxValue;
            this.rightDateTime = lastIndex < 0 || lastIndex > MainSeries.Count - 1 ? DateTime.MinValue : MainSeries.GetDateTime(this.lastIndex);
            foreach (Pad pad in this.pads)
                pad.SetInterval(this.leftDateTime, this.rightDateTime);
            this.contentUpdated = true;
        }

        private void SetDateInterval(DateTime firstDateTime, DateTime lastDateTime)
        {
            SetIndexInterval(MainSeries.GetIndex(firstDateTime, IndexOption.Next), MainSeries.GetIndex(lastDateTime, IndexOption.Prev));
        }

        #if GTK
        #else
        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            Console.WriteLine("scrollvalue:{0} min:{1} max:{2}", e.NewValue, this.scrollBar.Minimum, this.scrollBar.Maximum);
            if (this.scrollBar.Value == e.NewValue)
                return;
            int delta = e.NewValue - this.scrollBar.Value;
            SetIndexInterval(this.firstIndex + delta, this.lastIndex + delta);
            Invalidate();
        }
        #endif

        public void OnItemAdedd(DateTime dateTime)
        {
            bool flag = false;
            lock (this.lockObject)
            {
                this.contentUpdated = true;
                if (this.firstIndex == -1)
                    this.firstIndex = 0;
                switch (this.updateStyle)
                {
                    case ChartUpdateStyle.WholeRange:
                            SetIndexInterval(0, MainSeries.Count - 1);
                            flag = true;
                            break;
                    case ChartUpdateStyle.Trailing:
                        if (this.lastIndex - this.firstIndex + 1 < this.minCountOfBars)
                            SetIndexInterval(this.firstIndex, this.lastIndex + 1);
                        else
                            SetIndexInterval(this.firstIndex + 1, this.lastIndex + 1);
                        flag = true;
                        break;
                }
            }
            if (flag)
                Invalidate();
            #if !GTK
            Application.DoEvents();
            #endif
        }

        private bool SetBarSeriesStyle(BSStyle barSeriesStyle, bool force)
        {
            bool flag = true;
            if (barSeriesStyle == BSStyle.Candle || barSeriesStyle == BSStyle.Bar || barSeriesStyle == BSStyle.Line)
            {
                if (!(this.mainSeriesView is SimpleBSView) || force)
                {
                    this.pads[0].RemovePrimitive(this.mainSeriesView);
                    this.mainSeriesView = new SimpleBSView(this.pads[0], this.series as BarSeries);
                    (this.mainSeriesView as SimpleBSView).UpColor = this.candleUpColor;
                    (this.mainSeriesView as SimpleBSView).DownColor = this.candleDownColor;
                    this.mainSeries = this.mainSeriesView.MainSeries;
                    this.pads[0].AddPrimitive(this.mainSeriesView);
                }
                else
                    flag = false;
                if (barSeriesStyle == BSStyle.Candle)
                    (this.mainSeriesView as SimpleBSView).Style = SimpleBSStyle.Candle;
                if (barSeriesStyle == BSStyle.Bar)
                    (this.mainSeriesView as SimpleBSView).Style = SimpleBSStyle.Bar;
                if (barSeriesStyle == BSStyle.Line)
                    (this.mainSeriesView as SimpleBSView).Style = SimpleBSStyle.Line;
            }
            return flag;
        }

        private void EmitUpdateStyleChanged()
        {
            if (UpdateStyleChanged != null)
                UpdateStyleChanged(this, EventArgs.Empty);
        }

        private void EmitVolumeVisibleChanged()
        {
            if (VolumeVisibleChanged != null)
                VolumeVisibleChanged(this, EventArgs.Empty);
        }

        private void EmitBarSeriesStyleChanged()
        {
            if (BarSeriesStyleChanged != null)
                BarSeriesStyleChanged(this, EventArgs.Empty);
        }

        private void EmitActionTypeChanged()
        {
            if (ActionTypeChanged != null)
                ActionTypeChanged(this, EventArgs.Empty);
        }

        private void EmitScaleStyleChanged()
        {
            if (ScaleStyleChanged != null)
                ScaleStyleChanged(this, EventArgs.Empty);
        }

        public void EnsureVisible(Fill fill)
        {
            if (fill.DateTime < MainSeries.FirstDateTime)
                return;
            int num = Math.Max(MainSeries.GetIndex(fill.DateTime, IndexOption.Prev), 0);
            int val2 = this.lastIndex - this.firstIndex + 1;
            int lastIndex = Math.Max(Math.Min(MainSeries.Count - 1, num + val2 / 5), val2);
            SetIndexInterval(lastIndex - val2 + 1, lastIndex);
            this.pads[0].SetSelectedObject(fill);
            this.polosaDate = MainSeries.GetDateTime(MainSeries.GetIndex(fill.DateTime, IndexOption.Prev));
            this.contentUpdated = true;
            Invalidate();
        }

        public int GetPadNumber(Point point)
        {
            for (int i = 0; i < this.pads.Count; ++i)
                if (this.pads[i].Y1 <= point.Y && point.Y <= this.pads[i].Y2)
                    return i;
            return -1;
        }

        private delegate void SetIndexIntervalHandler(int firstIndex,int lastIndex);

        #region Helper Methods

        internal void SetCursor(ChartCursorType type)
        {
            #if GTK
            switch(type)
            {
                case ChartCursorType.VSplitter:
                    GdkWindow.Cursor = UserControl.VSplitterCursor;
                    break;
                case ChartCursorType.Cross:
                    GdkWindow.Cursor = UserControl.CrossCursor;
                    break;
                case ChartCursorType.Hand:
                    GdkWindow.Cursor = UserControl.HandCursor;
                    break;
                case ChartCursorType.Default:
                    GdkWindow.Cursor = null;
                    break;
            }
            #else
            switch (type)
            {
                case ChartCursorType.VSplitter:
                    Cursor.Current = Cursors.HSplit;
                    break;
                case ChartCursorType.Cross:
                    Cursor.Current = Cursors.Cross;
                    break;
                case ChartCursorType.Hand:
                    Cursor.Current = Cursors.Hand;
                    break;
                case ChartCursorType.Default:
                default:
                    Cursor.Current = Cursors.Default;
                    break;
            }
            #endif
        }
        //        private void DrawUpdatableObject(IUpdatable obj, int padNumber)
        //        {
        //            lock (this.lockObject)
        //            {
        //                if (!this.volumePadShown && padNumber > 1)
        //                    --padNumber;
        //                IChartDrawable drawable;
        //                if (obj is DrawingImage)
        //                    drawable = new ImageView(obj, this.pads[padNumber]);
        //                else if(obj is DrawingLine)
        //                    drawable = new LineView(obj, this.pads[padNumber]);
        //                else if(obj is DrawingEllipse)
        //                    drawable = new EllipseView(obj, this.pads[padNumber]);
        //                else if(obj is DrawingLine)
        //                    drawable = new LineView(obj, this.pads[padNumber]);
        //                else if(obj is DrawingLine)
        //                    drawable = new LineView(obj, this.pads[padNumber]);
        //                else if(obj is DrawingLine)
        //                    drawable = new LineView(obj, this.pads[padNumber]);
        //                ImageView local_0 = new ImageView(image, this.pads[padNumber]);
        //                image.Updated += new EventHandler(this.primitive_Updated);
        //                this.pads[padNumber].AddPrimitive((IChartDrawable)local_0);
        //                local_0.SetInterval(this.leftDateTime, this.rightDateTime);
        //                this.contentUpdated = true;
        //            }
        //        }

        #endregion
    }
}
