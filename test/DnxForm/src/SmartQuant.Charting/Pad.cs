using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Resources;

#if GTK
using Gtk;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Compatibility.Gtk;

#else
using System.Windows.Forms;
#endif

namespace SmartQuant.Charting
{
    [Serializable]
    public class Pad
    {
        private Dictionary<Type, Viewer> viewers = new Dictionary<Type, Viewer>();
        private List<ObjectViewer> objectViewers = new List<ObjectViewer>();
        private TFeatures3D features3D;
        [Browsable(false)]
        public bool Grid3D;
        protected int fX1;
        protected int fX2;
        protected int fY1;
        protected int fY2;
        protected double fCanvasX1;
        protected double fCanvasX2;
        protected double fCanvasY1;
        protected double fCanvasY2;
        protected int fClientX;
        protected int fClientY;
        protected int fClientWidth;
        protected int fClientHeight;
        protected double fXMin;
        protected double fXMax;
        protected double fYMin;
        protected double fYMax;
        protected int fMarginLeft;
        protected int fMarginRight;
        protected int fMarginTop;
        protected int fMarginBottom;
        protected int fWidth;
        protected int fHeight;
        [NonSerialized]
        protected Chart fChart;
        [NonSerialized]
        protected Graphics fGraphics;
        protected ArrayList fPrimitives;
        protected Color fBackColor;
        protected Color fForeColor;
        protected string fName;
        protected TTitle fTitle;
        protected bool fTitleEnabled;
        protected int fTitleOffsetX;
        protected int fTitleOffsetY;
        protected Axis fAxisLeft;
        protected Axis fAxisRight;
        protected Axis fAxisTop;
        protected Axis fAxisBottom;
        protected TLegend fLegend;
        protected bool fLegendEnabled;
        protected ELegendPosition fLegendPosition;
        protected int fLegendOffsetX;
        protected int fLegendOffsetY;
        protected bool fBorderEnabled;
        protected Color fBorderColor;
        protected int fBorderWidth;
        protected IDrawable fSelectedPrimitive;
        protected TDistance fSelectedPrimitiveDistance;
        protected bool fOnAxis;
        protected bool fOnPrimitive;
        protected bool fMouseDown;
        protected int fMouseDownX;
        protected int fMouseDownY;
        [NonSerialized]
        protected MouseButtons fMouseDownButton;
        protected bool fOutlineEnabled;
        protected Rectangle fOutlineRectangle;
        protected bool fMouseZoomEnabled;
        protected bool fMouseZoomXAxisEnabled;
        protected bool fMouseZoomYAxisEnabled;
        protected bool fMouseUnzoomEnabled;
        protected bool fMouseUnzoomXAxisEnabled;
        protected bool fMouseUnzoomYAxisEnabled;
        protected bool fMouseMoveContentEnabled;
        protected bool fMouseMovePrimitiveEnabled;
        protected bool fMouseDeletePrimitiveEnabled;
        protected bool fMousePadPropertiesEnabled;
        protected bool fMousePrimitivePropertiesEnabled;
        protected bool fMouseContextMenuEnabled;
        protected bool fMouseWheelEnabled;
        protected double fMouseWheelSensitivity;
        protected EMouseWheelMode fMouseWheelMode;
        protected int fWindowSize;
        protected bool fMonitored;
        protected bool fUpdating;
        protected int fLastTickTime;
        protected int fUpdateInterval;
        protected DateTime fLastUpdateDateTime;
        protected ETransformationType fTransformationType;
        protected IChartTransformation fTransformation;
        protected Color fSessionGridColor;
        #if GTK
        private Menu primitiveContextMenu;
        private MenuItem deleteMenuItem;
        private MenuItem propertiesMenuItem;
        #else
        [NonSerialized]
        private ContextMenu primitiveContextMenu;
        [NonSerialized]
        private MenuItem deleteMenuItem;
        [NonSerialized]
        private MenuItem propertiesMenuItem;
        #endif
        private int titleHeight;
        private int axisBottomHeight;
        private int axisTopHeight;
        private int axisRightWidth;
        private int axisLeftWidth;

        [Browsable(false)]
        public bool For3D
        {
            get
            {
                return this.features3D.Active;
            }
            set
            {
                this.features3D.Active = value;
            }
        }

        [Browsable(false)]
        public object View3D
        {
            get
            {
                return this.features3D.View;
            }
            set
            {
                this.features3D.View = value;
            }
        }

        [Browsable(false)]
        public Axis[] Axes3D => this.features3D.Axes;

        [Browsable(false)]
        public Axis AxisX3D => this.features3D.Axes[0];

        [Browsable(false)]
        public Axis AxisY3D => this.features3D.Axes[1];

        [Browsable(false)]
        public Axis AxisZ3D => this.features3D.Axes[2];

        [Browsable(false)]
        public Chart Chart
        {
            get
            {
                return this.fChart;
            }
            set
            {
                this.fChart = value;
            }
        }

        [Description("Enable or disable double buffering")]
        [Category("Appearance")]
        public bool DoubleBufferingEnabled
        {
            get
            {
                return this.fChart.DoubleBufferingEnabled;
            }
            set
            {
                this.fChart.DoubleBufferingEnabled = value;
            }
        }

        [Description("Enable or disable smoothing")]
        [Category("Appearance")]
        public bool SmoothingEnabled
        {
            get
            {
                return this.fChart.SmoothingEnabled;
            }
            set
            {
                this.fChart.SmoothingEnabled = value;
            }
        }

        [Description("Enable or disable antialiasing")]
        [Category("Appearance")]
        public bool AntiAliasingEnabled
        {
            get
            {
                return this.fChart.AntiAliasingEnabled;
            }
            set
            {
                this.fChart.AntiAliasingEnabled = value;
            }
        }

        [Category("Position")]
        [Description("")]
        public double CanvasX1
        {
            get
            {
                return this.fCanvasX1;
            }
            set
            {
                this.fCanvasX1 = value;
            }
        }

        [Category("Position")]
        [Description("")]
        public double CanvasX2
        {
            get
            {
                return this.fCanvasX2;
            }
            set
            {
                this.fCanvasX2 = value;
            }
        }

        [Description("")]
        [Category("Position")]
        public double CanvasY1
        {
            get
            {
                return this.fCanvasY1;
            }
            set
            {
                this.fCanvasY1 = value;
            }
        }

        [Category("Position")]
        [Description("")]
        public double CanvasY2
        {
            get
            {
                return this.fCanvasY2;
            }
            set
            {
                this.fCanvasY2 = value;
            }
        }

        [Browsable(false)]
        public double CanvasWidth
        {
            get
            {
                return Math.Abs(this.fCanvasX2 - this.fCanvasX1);
            }
        }

        [Browsable(false)]
        public double CanvasHeight => Math.Abs(this.fCanvasY2 - this.fCanvasY1);

        [Browsable(false)]
        public virtual int X1
        {
            get
            {
                return this.fX1;
            }
            set
            {
                this.fX1 = value;
                this.fWidth = this.fX2 - this.fX1;
            }
        }

        [Browsable(false)]
        public virtual int X2
        {
            get
            {
                return this.fX2;
            }
            set
            {
                this.fX2 = value;
                this.fWidth = this.fX2 - this.fX1;
            }
        }

        [Browsable(false)]
        public int Y1
        {
            get
            {
                return this.fY1;
            }
            set
            {
                this.fY1 = value;
                this.fHeight = this.fY2 - this.fY1;
            }
        }

        [Browsable(false)]
        public int Y2
        {
            get
            {
                return this.fY2;
            }
            set
            {
                this.fY2 = value;
                this.fHeight = this.fY2 - this.fY1;
            }
        }

        [Browsable(false)]
        public int Width
        {
            get
            {
                return this.fWidth;
            }
            set
            {
                this.fWidth = value;
                this.fX2 = this.fX1 + this.fWidth;
            }
        }

        [Browsable(false)]
        public int Height
        {
            get
            {
                return this.fHeight;
            }
            set
            {
                this.fHeight = value;
                this.fY2 = this.fY1 + this.fHeight;
            }
        }

        [Browsable(false)]
        public double XMin
        {
            get
            {
                if (this.fAxisBottom.Enabled && this.fAxisBottom.Zoomed)
                    return this.fAxisBottom.Min;
                else
                    return this.fXMin;
            }
            set
            {
                this.fXMin = value;
            }
        }

        [Browsable(false)]
        public double XMax
        {
            get
            {
                if (this.fAxisBottom.Enabled && this.fAxisBottom.Zoomed)
                    return this.fAxisBottom.Max;
                else
                    return this.fXMax;
            }
            set
            {
                this.fXMax = value;
            }
        }

        [Browsable(false)]
        public double YMin
        {
            get
            {
                if (this.fAxisLeft.Enabled && this.fAxisLeft.Zoomed)
                    return this.fAxisLeft.Min;
                else
                    return this.fYMin;
            }
            set
            {
                this.fYMin = value;
            }
        }

        [Browsable(false)]
        public double YMax
        {
            get
            {
                if (this.fAxisLeft.Enabled && this.fAxisLeft.Zoomed)
                    return this.fAxisLeft.Max;
                else
                    return this.fYMax;
            }
            set
            {
                this.fYMax = value;
            }
        }

        [Browsable(false)]
        public double XRangeMin
        {
            get
            {
                return this.fXMin;
            }
            set
            {
                this.fXMin = value;
            }
        }

        [Browsable(false)]
        public double XRangeMax
        {
            get
            {
                return this.fXMax;
            }
            set
            {
                this.fXMax = value;
            }
        }

        [Browsable(false)]
        public double YRangeMin
        {
            get
            {
                return this.fYMin;
            }
            set
            {
                this.fYMin = value;
            }
        }

        [Browsable(false)]
        public double YRangeMax
        {
            get
            {
                return this.fYMax;
            }
            set
            {
                this.fYMax = value;
            }
        }

        [Category("Margin")]
        [Description("")]
        public int MarginLeft
        {
            get
            {
                return this.fMarginLeft;
            }
            set
            {
                this.fMarginLeft = value;
            }
        }

        [Category("Margin")]
        [Description("")]
        public int MarginRight
        {
            get
            {
                return this.fMarginRight;
            }
            set
            {
                this.fMarginRight = value;
            }
        }

        [Category("Margin")]
        [Description("")]
        public int MarginTop
        {
            get
            {
                return this.fMarginTop;
            }
            set
            {
                this.fMarginTop = value;
            }
        }

        [Category("Margin")]
        [Description("")]
        public int MarginBottom
        {
            get
            {
                return this.fMarginBottom;
            }
            set
            {
                this.fMarginBottom = value;
            }
        }

        public string Name
        {
            get
            {
                return this.fName;
            }
            set
            {
                this.fName = value;
            }
        }

        [Browsable(false)]
        public TTitle Title
        {
            get
            {
                return this.fTitle;
            }
            set
            {
                this.fTitle = value;
            }
        }

        [Description("")]
        [Category("Title")]
        public bool TitleEnabled
        {
            get
            {
                return this.fTitleEnabled;
            }
            set
            {
                this.fTitleEnabled = value;
            }
        }

        [Description("")]
        [Category("Title")]
        public ArrayList TitleItems
        {
            get
            {
                return this.fTitle.Items;
            }
        }

        [Description("")]
        [Category("Title")]
        public bool TitleItemsEnabled
        {
            get
            {
                return this.fTitle.ItemsEnabled;
            }
            set
            {
                this.fTitle.ItemsEnabled = value;
            }
        }

        [Description("")]
        [Category("Title")]
        public string TitleText
        {
            get
            {
                return this.fTitle.Text;
            }
            set
            {
                this.fTitle.Text = value;
            }
        }

        [Description("")]
        [Category("Title")]
        public Font TitleFont
        {
            get
            {
                return this.fTitle.Font;
            }
            set
            {
                this.fTitle.Font = value;
            }
        }

        [Category("Title")]
        [Description("")]
        public Color TitleColor
        {
            get
            {
                return this.fTitle.Color;
            }
            set
            {
                this.fTitle.Color = value;
            }
        }

        [Description("Title offset alone X axis")]
        [Category("Title")]
        public int TitleOffsetX
        {
            get
            {
                return this.fTitleOffsetX;
            }
            set
            {
                this.fTitleOffsetX = value;
            }
        }

        [Category("Title")]
        [Description("Title offset alone Y axis")]
        public int TitleOffsetY
        {
            get
            {
                return this.fTitleOffsetY;
            }
            set
            {
                this.fTitleOffsetY = value;
            }
        }

        [Description("")]
        [Category("Title")]
        public ETitlePosition TitlePosition
        {
            get
            {
                return this.fTitle.Position;
            }
            set
            {
                this.fTitle.Position = value;
            }
        }

        [Description("")]
        [Category("Title")]
        public ETitleStrategy TitleStrategy
        {
            get
            {
                return this.fTitle.Strategy;
            }
            set
            {
                this.fTitle.Strategy = value;
            }
        }

        [Category("Color")]
        [Description("")]
        public Color BackColor
        {
            get
            {
                return this.fBackColor;
            }
            set
            {
                this.fBackColor = value;
            }
        }

        [Description("")]
        [Category("Color")]
        public Color ForeColor
        {
            get
            {
                return this.fForeColor;
            }
            set
            {
                this.fForeColor = value;
            }
        }

        [Browsable(false)]
        public ArrayList Primitives
        {
            get
            {
                return this.fPrimitives;
            }
            set
            {
                this.fPrimitives = value;
            }
        }

        [Browsable(false)]
        public Graphics Graphics
        {
            get
            {
                return this.fGraphics;
            }
            set
            {
                this.fGraphics = value;
            }
        }

        [Browsable(false)]
        public Axis AxisLeft => this.fAxisLeft;

        [Browsable(false)]
        public Axis AxisRight => this.fAxisRight;

        [Browsable(false)]
        public Axis AxisTop => this.fAxisTop;

        [Browsable(false)]
        public Axis AxisBottom => this.fAxisBottom;

        [Description("")]
        [Category("Grid")]
        public bool XGridEnabled
        {
            get
            {
                return this.fAxisLeft.GridEnabled;
            }
            set
            {
                this.fAxisLeft.GridEnabled = value;
            }
        }

        [Category("Grid")]
        [Description("")]
        public bool YGridEnabled
        {
            get
            {
                return this.fAxisBottom.GridEnabled;
            }
            set
            {
                this.fAxisBottom.GridEnabled = value;
            }
        }

        [Description("")]
        [Category("Grid")]
        public float XGridWidth
        {
            get
            {
                return this.fAxisLeft.GridWidth;
            }
            set
            {
                this.fAxisLeft.GridWidth = value;
            }
        }

        [Description("")]
        [Category("Grid")]
        public float YGridWidth
        {
            get
            {
                return this.fAxisBottom.GridWidth;
            }
            set
            {
                this.fAxisBottom.GridWidth = value;
            }
        }

        [Category("Grid")]
        [Description("")]
        public Color XGridColor
        {
            get
            {
                return this.fAxisLeft.GridColor;
            }
            set
            {
                this.fAxisLeft.GridColor = value;
            }
        }

        [Description("")]
        [Category("Grid")]
        public Color YGridColor
        {
            get
            {
                return this.fAxisBottom.GridColor;
            }
            set
            {
                this.fAxisBottom.GridColor = value;
            }
        }

        [Description("")]
        [Category("Grid")]
        public DashStyle XGridDashStyle
        {
            get
            {
                return this.fAxisLeft.GridDashStyle;
            }
            set
            {
                this.fAxisLeft.GridDashStyle = value;
            }
        }

        [Category("Grid")]
        [Description("")]
        public DashStyle YGridDashStyle
        {
            get
            {
                return this.fAxisBottom.GridDashStyle;
            }
            set
            {
                this.fAxisBottom.GridDashStyle = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public EAxisType XAxisType
        {
            get
            {
                return this.fAxisBottom.Type;
            }
            set
            {
                this.fAxisBottom.Type = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public EAxisPosition XAxisPosition
        {
            get
            {
                return this.fAxisBottom.Position;
            }
            set
            {
                this.fAxisBottom.Position = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public bool XAxisMajorTicksEnabled
        {
            get
            {
                return this.fAxisBottom.MajorTicksEnabled;
            }
            set
            {
                this.fAxisBottom.MajorTicksEnabled = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public bool XAxisMinorTicksEnabled
        {
            get
            {
                return this.fAxisBottom.MinorTicksEnabled;
            }
            set
            {
                this.fAxisBottom.MinorTicksEnabled = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public bool XAxisTitleEnabled
        {
            get
            {
                return this.fAxisBottom.TitleEnabled;
            }
            set
            {
                this.fAxisBottom.TitleEnabled = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public string XAxisTitle
        {
            get
            {
                return this.fAxisBottom.Title;
            }
            set
            {
                this.fAxisBottom.Title = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public Font XAxisTitleFont
        {
            get
            {
                return this.fAxisBottom.TitleFont;
            }
            set
            {
                this.fAxisBottom.TitleFont = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public Color XAxisTitleColor
        {
            get
            {
                return this.fAxisBottom.TitleColor;
            }
            set
            {
                this.fAxisBottom.TitleColor = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public int XAxisTitleOffset
        {
            get
            {
                return this.fAxisBottom.TitleOffset;
            }
            set
            {
                this.fAxisBottom.TitleOffset = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public EAxisTitlePosition XAxisTitlePosition
        {
            get
            {
                return this.fAxisBottom.TitlePosition;
            }
            set
            {
                this.fAxisBottom.TitlePosition = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public bool XAxisLabelEnabled
        {
            get
            {
                return this.fAxisBottom.LabelEnabled;
            }
            set
            {
                this.fAxisBottom.LabelEnabled = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public Font XAxisLabelFont
        {
            get
            {
                return this.fAxisBottom.LabelFont;
            }
            set
            {
                this.fAxisBottom.LabelFont = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public Color XAxisLabelColor
        {
            get
            {
                return this.fAxisBottom.LabelColor;
            }
            set
            {
                this.fAxisBottom.LabelColor = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public int XAxisLabelOffset
        {
            get
            {
                return this.fAxisBottom.LabelOffset;
            }
            set
            {
                this.fAxisBottom.LabelOffset = value;
            }
        }

        [Description("")]
        [Category("XAxis")]
        public string XAxisLabelFormat
        {
            get
            {
                return this.fAxisBottom.LabelFormat;
            }
            set
            {
                this.fAxisBottom.LabelFormat = value;
            }
        }

        [Category("XAxis")]
        [Description("")]
        public EAxisLabelAlignment XAxisLabelAlignment
        {
            get
            {
                return this.fAxisBottom.LabelAlignment;
            }
            set
            {
                this.fAxisBottom.LabelAlignment = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public EAxisType YAxisType
        {
            get
            {
                return this.fAxisLeft.Type;
            }
            set
            {
                this.fAxisLeft.Type = value;
                this.fAxisRight.Type = value;
            }
        }

        [Category("YAxis")]
        [Description("")]
        public EAxisPosition YAxisPosition
        {
            get
            {
                return this.fAxisLeft.Position;
            }
            set
            {
                this.fAxisLeft.Position = value;
            }
        }

        [Category("YAxis")]
        [Description("")]
        public bool YAxisMajorTicksEnabled
        {
            get
            {
                return this.fAxisLeft.MajorTicksEnabled;
            }
            set
            {
                this.fAxisLeft.MajorTicksEnabled = value;
                this.fAxisRight.MajorTicksEnabled = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public bool YAxisMinorTicksEnabled
        {
            get
            {
                return this.fAxisLeft.MinorTicksEnabled;
            }
            set
            {
                this.fAxisLeft.MinorTicksEnabled = value;
                this.fAxisRight.MinorTicksEnabled = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public bool YAxisTitleEnabled
        {
            get
            {
                return this.fAxisLeft.TitleEnabled;
            }
            set
            {
                this.fAxisLeft.TitleEnabled = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public string YAxisTitle
        {
            get
            {
                return this.fAxisLeft.Title;
            }
            set
            {
                this.fAxisLeft.Title = value;
                this.fAxisRight.Title = value;
            }
        }

        [Category("YAxis")]
        [Description("")]
        public Font YAxisTitleFont
        {
            get
            {
                return this.fAxisLeft.TitleFont;
            }
            set
            {
                this.fAxisLeft.TitleFont = value;
                this.fAxisRight.TitleFont = value;
            }
        }

        [Category("YAxis")]
        [Description("")]
        public Color YAxisTitleColor
        {
            get
            {
                return this.fAxisLeft.TitleColor;
            }
            set
            {
                this.fAxisLeft.TitleColor = value;
                this.fAxisRight.TitleColor = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public int YAxisTitleOffset
        {
            get
            {
                return this.fAxisLeft.TitleOffset;
            }
            set
            {
                this.fAxisLeft.TitleOffset = value;
                this.fAxisRight.TitleOffset = value;
            }
        }

        [Category("YAxis")]
        [Description("")]
        public EAxisTitlePosition YAxisTitlePosition
        {
            get
            {
                return this.fAxisLeft.TitlePosition;
            }
            set
            {
                this.fAxisLeft.TitlePosition = value;
                this.fAxisRight.TitlePosition = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public bool YAxisLabelEnabled
        {
            get
            {
                return this.fAxisLeft.LabelEnabled;
            }
            set
            {
                this.fAxisLeft.LabelEnabled = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public Font YAxisLabelFont
        {
            get
            {
                return this.fAxisLeft.LabelFont;
            }
            set
            {
                this.fAxisLeft.LabelFont = value;
                this.fAxisRight.LabelFont = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public Color YAxisLabelColor
        {
            get
            {
                return this.fAxisLeft.LabelColor;
            }
            set
            {
                this.fAxisLeft.LabelColor = value;
                this.fAxisRight.LabelColor = value;
            }
        }

        [Category("YAxis")]
        [Description("")]
        public int YAxisLabelOffset
        {
            get
            {
                return this.fAxisLeft.LabelOffset;
            }
            set
            {
                this.fAxisLeft.LabelOffset = value;
                this.fAxisRight.LabelOffset = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public string YAxisLabelFormat
        {
            get
            {
                return this.fAxisLeft.LabelFormat;
            }
            set
            {
                this.fAxisLeft.LabelFormat = value;
                this.fAxisRight.LabelFormat = value;
            }
        }

        [Description("")]
        [Category("YAxis")]
        public EAxisLabelAlignment YAxisLabelAlignment
        {
            get
            {
                return this.fAxisLeft.LabelAlignment;
            }
            set
            {
                this.fAxisLeft.LabelAlignment = value;
                this.fAxisRight.LabelAlignment = value;
            }
        }

        [Browsable(false)]
        public TLegend Legend => this.fLegend;

        [Category("Legend")]
        [Description("")]
        public bool LegendEnabled
        {
            get
            {
                return this.fLegendEnabled;
            }
            set
            {
                this.fLegendEnabled = value;
            }
        }

        [Category("Legend")]
        [Description("")]
        public ELegendPosition LegendPosition
        {
            get
            {
                return this.fLegendPosition;
            }
            set
            {
                this.fLegendPosition = value;
            }
        }

        [Description("")]
        [Category("Legend")]
        public int LegendOffsetX
        {
            get
            {
                return this.fLegendOffsetX;
            }
            set
            {
                this.fLegendOffsetX = value;
            }
        }

        [Category("Legend")]
        [Description("")]
        public int LegendOffsetY
        {
            get
            {
                return this.fLegendOffsetY;
            }
            set
            {
                this.fLegendOffsetY = value;
            }
        }

        [Description("")]
        [Category("Legend")]
        public bool LegendBorderEnabled
        {
            get
            {
                return this.fLegend.BorderEnabled;
            }
            set
            {
                this.fLegend.BorderEnabled = value;
            }
        }

        [Category("Legend")]
        [Description("")]
        public Color LegendBorderColor
        {
            get
            {
                return this.fLegend.BorderColor;
            }
            set
            {
                this.fLegend.BorderColor = value;
            }
        }

        [Description("")]
        [Category("Legend")]
        public Color LegendBackColor
        {
            get
            {
                return this.fLegend.BackColor;
            }
            set
            {
                this.fLegend.BackColor = value;
            }
        }

        [Category("Border")]
        [Description("")]
        public bool BorderEnabled
        {
            get
            {
                return this.fBorderEnabled;
            }
            set
            {
                this.fBorderEnabled = value;
            }
        }

        [Category("Border")]
        [Description("")]
        public Color BorderColor
        {
            get
            {
                return this.fBorderColor;
            }
            set
            {
                this.fBorderColor = value;
            }
        }

        [Description("")]
        [Category("Border")]
        public int BorderWidth
        {
            get
            {
                return this.fBorderWidth;
            }
            set
            {
                this.fBorderWidth = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseZoomEnabled
        {
            get
            {
                return this.fMouseZoomEnabled;
            }
            set
            {
                this.fMouseZoomEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseZoomXAxisEnabled
        {
            get
            {
                return this.fMouseZoomXAxisEnabled;
            }
            set
            {
                this.fMouseZoomXAxisEnabled = value;
            }
        }

        [Category("Mouse")]
        [Description("")]
        public bool MouseZoomYAxisEnabled
        {
            get
            {
                return this.fMouseZoomYAxisEnabled;
            }
            set
            {
                this.fMouseZoomYAxisEnabled = value;
            }
        }

        [Category("Mouse")]
        [Description("")]
        public bool MouseUnzoomEnabled
        {
            get
            {
                return this.fMouseUnzoomEnabled;
            }
            set
            {
                this.fMouseUnzoomEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseUnzoomXAxisEnabled
        {
            get
            {
                return this.fMouseUnzoomXAxisEnabled;
            }
            set
            {
                this.fMouseUnzoomXAxisEnabled = value;
            }
        }

        [Category("Mouse")]
        [Description("")]
        public bool MouseUnzoomYAxisEnabled
        {
            get
            {
                return this.fMouseUnzoomYAxisEnabled;
            }
            set
            {
                this.fMouseUnzoomYAxisEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseMoveContentEnabled
        {
            get
            {
                return this.fMouseMoveContentEnabled;
            }
            set
            {
                this.fMouseMoveContentEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseMovePrimitiveEnabled
        {
            get
            {
                return this.fMouseMovePrimitiveEnabled;
            }
            set
            {
                this.fMouseMovePrimitiveEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseDeletePrimitiveEnabled
        {
            get
            {
                return this.fMouseDeletePrimitiveEnabled;
            }
            set
            {
                this.fMouseDeletePrimitiveEnabled = value;
            }
        }

        [Category("Mouse")]
        [Description("")]
        public bool MousePadPropertiesEnabled
        {
            get
            {
                return this.fMousePadPropertiesEnabled;
            }
            set
            {
                this.fMousePadPropertiesEnabled = value;
            }
        }

        [Category("Mouse")]
        [Description("")]
        public bool MousePrimitivePropertiesEnabled
        {
            get
            {
                return this.fMousePrimitivePropertiesEnabled;
            }
            set
            {
                this.fMousePrimitivePropertiesEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public bool MouseContextMenuEnabled
        {
            get
            {
                return this.fMouseContextMenuEnabled;
            }
            set
            {
                this.fMouseContextMenuEnabled = value;
            }
        }

        [Category("Mouse")]
        [Description("Enable or disable mouse wheel")]
        public bool MouseWheelEnabled
        {
            get
            {
                return this.fMouseWheelEnabled;
            }
            set
            {
                this.fMouseWheelEnabled = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public double MouseWheelSensitivity
        {
            get
            {
                return this.fMouseWheelSensitivity;
            }
            set
            {
                this.fMouseWheelSensitivity = value;
            }
        }

        [Description("")]
        [Category("Mouse")]
        public EMouseWheelMode MouseWheelMode
        {
            get
            {
                return this.fMouseWheelMode;
            }
            set
            {
                this.fMouseWheelMode = value;
            }
        }

        [Browsable(false)]
        public IChartTransformation Transformation => this.fTransformation;

        [Category("Transformation")]
        [Description("")]
        public ETransformationType TransformationType
        {
            get
            {
                return this.fTransformationType;
            }
            set
            {
                this.fTransformationType = value;
                double Y1 = this.fXMin + this.CalculateRealQuantityOfTicks_Right(this.fXMin, this.fXMax);
                double Y2 = this.fAxisBottom.Min + this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Max);
                if (this.fTransformationType == ETransformationType.Empty)
                    this.fTransformation = (IChartTransformation)new TEmptyTransformation();
                if (this.fTransformationType == ETransformationType.Intraday)
                    this.fTransformation = (IChartTransformation)new TIntradayTransformation();
                this.fXMax = Y1 - this.CalculateNotInSessionTicks(this.fXMin, Y1);
                this.fAxisBottom.Max = Y2 - this.CalculateNotInSessionTicks(this.fAxisBottom.Min, Y2);
                this.Update();
            }
        }

        [Category("Transformation")]
        [Description("")]
        public bool SessionGridEnabled
        {
            get {
                return this.fTransformationType == ETransformationType.Intraday && ((TIntradayTransformation)Transformation).SessionGridEnabled;
            }
            set
            {
                if (this.fTransformationType != ETransformationType.Intraday)
                    return;
                ((TIntradayTransformation)this.Transformation).SessionGridEnabled = value;
            }
        }

        [Description("")]
        [Category("Transformation")]
        public Color SessionGridColor
        {
            get
            {
                return this.fSessionGridColor;
            }
            set
            {
                this.fSessionGridColor = value;
            }
        }

        [Category("Transformation")]
        [Description("")]
        public TimeSpan SessionStart
        {
            get {
                return this.fTransformationType == ETransformationType.Intraday ? new TimeSpan(((TIntradayTransformation)this.fTransformation).FirstSessionTick) : new TimeSpan(0, 0, 0, 0);
            }
            set
            {
                double Y1 = this.fXMin + this.CalculateRealQuantityOfTicks_Right(this.fXMin, this.fXMax);
                double Y2 = this.fAxisBottom.Min + this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Max);
                if (this.fTransformationType == ETransformationType.Intraday)
                    ((TIntradayTransformation)this.fTransformation).FirstSessionTick = value.Ticks;
                this.fXMax = Y1 - this.CalculateNotInSessionTicks(this.fXMin, Y1);
                this.fAxisBottom.Max = Y2 - this.CalculateNotInSessionTicks(this.fAxisBottom.Min, Y2);
                this.Update();
            }
        }

        [Description("")]
        [Category("Transformation")]
        public TimeSpan SessionEnd
        {
            get {
                return this.fTransformationType == ETransformationType.Intraday ? new TimeSpan(((TIntradayTransformation)this.fTransformation).LastSessionTick) : new TimeSpan(0, 24, 0, 0);
            }
            set
            {
                double Y1 = this.fXMin + this.CalculateRealQuantityOfTicks_Right(this.fXMin, this.fXMax);
                double Y2 = this.fAxisBottom.Min + this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Max);
                if (this.fTransformationType == ETransformationType.Intraday)
                    ((TIntradayTransformation)this.fTransformation).LastSessionTick = value.Ticks;
                this.fXMax = Y1 - this.CalculateNotInSessionTicks(this.fXMin, Y1);
                this.fAxisBottom.Max = Y2 - this.CalculateNotInSessionTicks(this.fAxisBottom.Min, Y2);
                this.Update();
            }
        }

        [Browsable(false)]
        public bool Monitored
        {
            get
            {
                return this.fMonitored;
            }
            set
            {
                this.fMonitored = value;
                if (this.fMonitored)
                    Pad.NewTick += this.OnNewTick;
                else
                    Pad.NewTick -= this.OnNewTick;
            }
        }

        [Browsable(false)]
        public int WindowSize
        {
            get
            {
                return this.fWindowSize;
            }
            set
            {
                this.fWindowSize = value;
            }
        }

        [Browsable(false)]
        public int UpdateInterval
        {
            get
            {
                return this.fUpdateInterval;
            }
            set
            {
                this.fUpdateInterval = value;
            }
        }

        public static event NewTickEventHandler NewTick;

        public event ZoomEventHandler Zoom;

        public Pad()
        {
            Init();
        }

        public Pad(Chart chart)
        {
            this.fChart = chart;
            Init();
        }

        public Pad(Chart chart, double x1, double y1, double x2, double y2)
        {
            this.fChart = chart;
            this.fCanvasX1 = x1;
            this.fCanvasX2 = x2;
            this.fCanvasY1 = y1;
            this.fCanvasY2 = y2;
            Init();
        }

        private Viewer GetViewer(object obj)
        {
            System.Type key = obj.GetType();
            Viewer viewer = null;
            for (; key !=null; key = key.BaseType)
            {
                if (this.viewers.TryGetValue(key, out viewer))
                    return Activator.CreateInstance(viewer.GetType()) as Viewer;
            }
            Console.WriteLine("No viewer exists for object with type " + obj.GetType());
            return null;
        }

        public void RegisterViewer(Viewer viewer)
        {
            try
            {
                this.viewers.Add(viewer.Type, viewer);
            }
            catch
            {
            }
        }

        public void Set(object obj, string name, object value)
        {
            var viewer = GetViewer(obj);
            viewer?.Set(obj, name, value);
        }

        public void ResetLastTickTime()
        {
            this.fLastTickTime = 0;
        }

        public void Init()
        {
            this.fPrimitives = new ArrayList();
            Chart.Pad = this;
            this.features3D = new TFeatures3D(this);
            BackColor = Color.LightGray;
            ForeColor = Color.White;
            this.fX1 = 0;
            this.fX2 = 1;
            this.fY1 = 0;
            this.fY2 = 1;
            this.fWidth = this.fChart.ClientSize.Width;
            this.fHeight = this.fChart.ClientSize.Height;
            this.fClientX = 10;
            this.fClientY = 10;
            this.fClientWidth = 0;
            this.fClientHeight = 0;
            this.fMarginLeft = 10;
            this.fMarginRight = 20;
            this.fMarginTop = 10;
            this.fMarginBottom = 10;
            this.fTitle = new TTitle(this, "");
            this.fTitleEnabled = true;
            this.fTitleOffsetX = 5;
            this.fTitleOffsetY = 5;
            this.fTransformation = new TIntradayTransformation();
            this.fTransformationType = ETransformationType.Empty;
            SessionGridColor = Color.Blue;
            this.fAxisLeft = new Axis(this, EAxisPosition.Left);
            this.fAxisRight = new Axis(this, EAxisPosition.Right);
            this.fAxisTop = new Axis(this, EAxisPosition.Top);
            this.fAxisBottom = new Axis(this, EAxisPosition.Bottom);
            this.fAxisRight.LabelEnabled = false;
            this.fAxisRight.TitleEnabled = false;
            this.fAxisTop.LabelEnabled = false;
            this.fAxisTop.TitleEnabled = false;
            this.fLegend = new TLegend(this);
            this.fLegendEnabled = false;
            this.fLegendPosition = ELegendPosition.TopRight;
            this.fLegendOffsetX = 5;
            this.fLegendOffsetY = 5;
            this.fBorderEnabled = true;
            this.fBorderColor = Color.Black;
            this.fBorderWidth = 1;
            SetRange(0.0, 100.0, 0.0, 100.0);
            this.fGraphics = null;
            this.fOnAxis = false;
            this.fOnPrimitive = false;
            this.fMouseDown = false;
            this.fMouseDownX = 0;
            this.fMouseDownY = 0;
            this.fOutlineEnabled = false;
            this.fWindowSize = 600;
            this.fLastTickTime = 0;
            this.fUpdateInterval = 1;
            this.fLastUpdateDateTime = DateTime.Now;
            this.Monitored = false;
            this.fUpdating = false;
            this.fMouseZoomEnabled = true;
            this.fMouseZoomXAxisEnabled = true;
            this.fMouseZoomYAxisEnabled = true;
            this.fMouseUnzoomEnabled = true;
            this.fMouseUnzoomXAxisEnabled = true;
            this.fMouseUnzoomYAxisEnabled = true;
            this.fMouseMoveContentEnabled = true;
            this.fMouseMovePrimitiveEnabled = true;
            this.fMouseDeletePrimitiveEnabled = true;
            this.fMousePadPropertiesEnabled = true;
            this.fMousePrimitivePropertiesEnabled = true;
            this.fMouseContextMenuEnabled = true;
            this.fMouseWheelEnabled = true;
            this.fMouseWheelSensitivity = 0.1;
            this.fMouseWheelMode = EMouseWheelMode.ZoomX;
        }

        private void InitContextMenu()
        {
            #if GTK
            if (this.primitiveContextMenu != null)
                return;
            this.primitiveContextMenu = new Menu();
            this.deleteMenuItem = new MenuItem("Delete");
            this.deleteMenuItem.Activated += OnDeleteMenuItemClick;
            this.propertiesMenuItem = new MenuItem("Properties");
            this.deleteMenuItem.Activated += OnPropertiesMenuItemClick;
            this.primitiveContextMenu.Append(this.deleteMenuItem);
            this.primitiveContextMenu.Append(new SeparatorMenuItem());
            this.primitiveContextMenu.Append(this.propertiesMenuItem);
            this.primitiveContextMenu.ShowAll();
            #else
            if (this.primitiveContextMenu != null)
                return;
            this.primitiveContextMenu = new ContextMenu();
            this.deleteMenuItem = new MenuItem();
            this.propertiesMenuItem = new MenuItem();
            MenuItem menuItem = new MenuItem();
            this.primitiveContextMenu.MenuItems.AddRange(new MenuItem[3]
            {
                this.deleteMenuItem,
                menuItem,
                this.propertiesMenuItem
            });
            this.deleteMenuItem.Index = 0;
            this.deleteMenuItem.Text = "Delete";
            this.deleteMenuItem.Click += new EventHandler(this.OnDeleteMenuItemClick);
            menuItem.Index = 1;
            menuItem.Text = "-";
            this.propertiesMenuItem.Index = 2;
            this.propertiesMenuItem.Text = "Properties";
            this.propertiesMenuItem.Click += new EventHandler(this.OnPropertiesMenuItemClick);
            #endif
        }

        public virtual void SetCanvas(double x1, double y1, double x2, double y2, int width, int height)
        {
            SetCanvas(x1, y1, x2, y2);
            SetCanvas(width, height);
        }

        public virtual void SetCanvas(double x1, double y1, double x2, double y2)
        {
            this.fCanvasX1 = x1;
            this.fCanvasX2 = x2;
            this.fCanvasY1 = y1;
            this.fCanvasY2 = y2;
        }

        public virtual void SetCanvas(int width, int height)
        {
            this.fX1 = (int)(width * this.fCanvasX1);
            this.fX2 = (int)(width * this.fCanvasX2);
            this.fY1 = (int)(height * this.fCanvasY1);
            this.fY2 = (int)(height * this.fCanvasY2);
            this.fWidth = this.fX2 - this.fX1;
            this.fHeight = this.fY2 - this.fY1;
        }

        public void SetRangeX(double xMin, double xMax)
        {
            this.fXMin = xMin;
            this.fXMax = xMax - CalculateNotInSessionTicks(xMin, xMax);
            this.fAxisBottom.SetRange(this.fXMin, this.fXMax);
            this.fAxisTop.SetRange(this.fXMin, this.fXMax);
            this.features3D.SetRangeX(this.fXMin, this.fXMax);
        }

        public void SetRangeX(DateTime xMin, DateTime xMax)
        {
            SetRangeX(xMin.Ticks, xMax.Ticks);
        }

        public void SetRangeY(double yMin, double yMax)
        {
            this.fYMin = yMin;
            this.fYMax = yMax;
            this.fAxisLeft.SetRange(this.fYMin, this.fYMax);
            this.fAxisRight.SetRange(this.fYMin, this.fYMax);
            this.features3D.SetRangeY(this.fYMin, this.fYMax);
        }

        public void SetRange(double xMin, double xMax, double yMin, double yMax)
        {
            SetRangeX(xMin, xMax);
            SetRangeY(yMin, yMax);
        }

        public void SetRange(DateTime xMin, DateTime xMax, double yMin, double yMax)
        {
            SetRange(xMin.Ticks, xMax.Ticks, yMin, yMax);
        }

        public void SetRange(string xMin, string xMax, double yMin, double yMax)
        {
            SetRange(DateTime.Parse(xMin).Ticks, DateTime.Parse(xMax).Ticks, yMin, yMax);
        }

        public bool IsInRange(double x, double y)
        {
            return XMin <= x && x <= XMin + CalculateRealQuantityOfTicks_Right(XMin, XMax) && YMin <= y && y <= YMax;
        }

        public void UnZoomX()
        {
            this.fAxisBottom.UnZoom();
            this.fAxisTop.UnZoom();
        }

        public void UnZoomY()
        {
            this.fAxisLeft.UnZoom();
            this.fAxisRight.UnZoom();
        }

        public void UnZoom()
        {
            this.fAxisBottom.SetRange(this.fXMin, this.fXMax);
            this.fAxisTop.SetRange(this.fXMin, this.fXMax);
            this.fAxisLeft.SetRange(this.fYMin, this.fYMax);
            this.fAxisRight.SetRange(this.fYMin, this.fYMax);
            this.fAxisBottom.Zoomed = false;
            this.fAxisTop.Zoomed = false;
            this.fAxisLeft.Zoomed = false;
            this.fAxisRight.Zoomed = false;
            if (this.fChart.GroupZoomEnabled)
                return;
            Update();
        }

        public double GetNextGridDivision(double firstTick, double prevMajor, int majorCount, EGridSize gridSize)
        {
            return this.fTransformation.GetNextGridDivision(firstTick, prevMajor, majorCount, gridSize);
        }

        public double CalculateRealQuantityOfTicks_Right(double x, double y)
        {
            return this.fTransformation.CalculateRealQuantityOfTicks_Right(x, y);
        }

        public double CalculateRealQuantityOfTicks_Left(double x, double y)
        {
            return this.fTransformation.CalculateRealQuantityOfTicks_Left(x, y);
        }

        public void GetFirstGridDivision(ref EGridSize gridSize, ref double min, ref double max, ref DateTime firstDateTime)
        {
            this.fTransformation.GetFirstGridDivision(ref gridSize, ref min, ref max, ref firstDateTime);
        }

        public double CalculateNotInSessionTicks(double x, double y)
        {
            return this.fTransformation.CalculateNotInSessionTicks(x, y);
        }

        public int ClientX(double worldX)
        {
            return (int)((double)this.fClientX + (worldX - XMin - CalculateNotInSessionTicks(XMin, worldX)) * (this.fClientWidth / (XMax - XMin)));
        }

        public int ClientY(double worldY)
        {
            return (int)((double)this.fClientY + (double)this.fClientHeight * (1.0 - (worldY - YMin) / (YMax - YMin)));
        }

        public int ClientX()
        {
            return this.fClientX;
        }

        public int ClientY()
        {
            return this.fClientY;
        }

        public int ClientHeight()
        {
            return this.fClientHeight;
        }

        public int ClientWidth()
        {
            return this.fClientWidth;
        }

        public double WorldX(int clientX)
        {
            return this.fAxisBottom.Min + CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, XMin + (double)(clientX - this.fClientX) / (double)this.fClientWidth * (XMax - XMin));
        }

        public double WorldY(int clientY)
        {
            return this.YMin + (1.0 - (double)(clientY - this.fClientY) / (double)this.fClientHeight) * (YMax - YMin);
        }

        public Viewer Add(object obj)
        {
            if (obj is IDrawable)
            {
                this.fPrimitives.Add((IDrawable) obj);
                return null;
            }
            else
            {
                var viewer = GetViewer(obj);
                if (viewer == null)
                    throw new Exception("There is no viewer for " + obj.GetType());
                this.objectViewers.Add(new ObjectViewer(obj, viewer));
                return viewer;
            }
        }

        public Viewer Insert(int index, object obj)
        {
            if (obj is IDrawable)
            {
                this.fPrimitives.Add((IDrawable) obj);
                return null;
            }
            else
            {
                var viewer = GetViewer(obj);
                if (viewer == null)
                    throw new Exception("There is no viewer for " + obj.GetType());
                this.objectViewers.Insert(index, new ObjectViewer(obj, viewer));
                return viewer;
            }
        }

        public void Remove(object obj)
        {
            if (obj is IDrawable)
            {
                this.fPrimitives.Remove(obj);
            }
            else
            {
                foreach (var objectViewer in this.objectViewers.Where(objectViewer => objectViewer.Object == obj))
                {
                    this.objectViewers.Remove(objectViewer);
                    break;
                }
            }
        }

        public void Clear()
        {
            this.fPrimitives.Clear();
            this.fLegend.Items.Clear();
            this.objectViewers.Clear();
        }

        public static Graphics GetGraphics() => Chart.Pad?.Graphics;

        public virtual void Update()
        {
            if (this.fUpdating)
                return;
            this.fUpdating = true;
            this.fChart.UpdatePads();
            this.fUpdating = false;
        }

        public virtual void Update(Graphics graphics)
        {
            double val1_1 = double.MaxValue;
            double val1_2 = double.MinValue;
            double val1_3 = double.MaxValue;
            double val1_4 = double.MinValue;
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            try
            {
                foreach (IDrawable drawable in this.fPrimitives)
                {
                    if (drawable is Histogram)
                        flag3 = true;
                    if (drawable is IZoomable)
                    {
                        var zoomable = (IZoomable)drawable;
                        if (zoomable.IsPadRangeX())
                        {
                            var padRangeX = zoomable.GetPadRangeX(this);
                            if (padRangeX.IsValid)
                            {
                                val1_1 = Math.Min(val1_1, padRangeX.Min);
                                val1_2 = Math.Max(val1_2, padRangeX.Max);
                                flag1 = true;
                            }
                        }
                        if (zoomable.IsPadRangeY())
                        {
                            double max = this.fAxisBottom.Max;
                            double num = this.fXMax;
                            this.fAxisBottom.Max = this.fAxisBottom.Min + this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Max);
                            this.fXMax = this.fAxisBottom.Max;
                            var padRangeY = zoomable.GetPadRangeY(this);
                            if (padRangeY.IsValid)
                            {
                                val1_3 = Math.Min(val1_3, padRangeY.Min);
                                val1_4 = Math.Max(val1_4, padRangeY.Max);
                                flag2 = true;
                            }
                            this.fAxisBottom.Max = max;
                            this.fXMax = num;
                        }
                    }
                }
                foreach (var objectViewer in this.objectViewers)
                {
                    if (objectViewer.Viewer.IsZoomable)
                    {
                        var padRangeX = objectViewer.Viewer.GetPadRangeX(objectViewer.Object, this);
                        if (padRangeX != null && padRangeX.IsValid)
                        {
                            val1_1 = Math.Min(val1_1, padRangeX.Min);
                            val1_2 = Math.Max(val1_2, padRangeX.Max);
                            flag1 = true;
                        }
                        var padRangeY = objectViewer.Viewer.GetPadRangeY(objectViewer.Object, this);
                        if (padRangeY != null)
                        {
                            double max = this.fAxisBottom.Max;
                            double num = this.fXMax;
                            this.fAxisBottom.Max = this.fAxisBottom.Min + this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Max);
                            this.fXMax = this.fAxisBottom.Max;
                            if (padRangeY.IsValid)
                            {
                                val1_3 = Math.Min(val1_3, padRangeY.Min);
                                val1_4 = Math.Max(val1_4, padRangeY.Max);
                                if (Math.Round(val1_3, 6) == 0.0 && Math.Round(val1_4, 6) == 0.0)
                                {
                                    val1_3 = -1.0;
                                    val1_4 = 1.0;
                                }
                                flag2 = true;
                            }
                            this.fAxisBottom.Max = max;
                            this.fXMax = num;
                        }
                    }
                }
            }
            catch
            {
            }
            if (!flag2 && !flag3)
            {
                flag2 = true;
                val1_4 = 1.0;
                val1_3 = -1.0;
            }
            if (flag1)
                SetRangeX(val1_1 - (val1_2 - val1_1) / 20.0, val1_2 + (val1_2 - val1_1) / 20.0);
            if (flag2)
                SetRangeY(val1_3 - (val1_4 - val1_3) / 20.0, val1_4 + (val1_4 - val1_3) / 20.0);
            this.fGraphics = graphics;
            this.titleHeight = 0;
            this.axisBottomHeight = 0;
            this.axisTopHeight = 0;
            this.axisRightWidth = 0;
            this.axisLeftWidth = 0;
            if (this.fTitleEnabled)
            {
                switch (this.fTitle.Position)
                {
                    case ETitlePosition.Left:
                        this.titleHeight = this.Title.Height + this.fTitleOffsetY;
                        break;
                    case ETitlePosition.Right:
                        this.titleHeight = this.Title.Height + this.fTitleOffsetY;
                        break;
                    case ETitlePosition.Centre:
                        this.titleHeight = this.Title.Height + this.fTitleOffsetY;
                        break;
                    case ETitlePosition.InsideLeft:
                        this.titleHeight = 0;
                        break;
                    case ETitlePosition.InsideRight:
                        this.titleHeight = 0;
                        break;
                    case ETitlePosition.InsideCentre:
                        this.titleHeight = 0;
                        break;
                }
            }
            if (this.fAxisBottom.Enabled)
                this.axisBottomHeight = this.fAxisBottom.Height;
            if (this.fAxisTop.Enabled)
                this.axisTopHeight = this.fAxisTop.Height;
            if (this.fAxisRight.Enabled)
                this.axisRightWidth = this.fAxisRight.Width;
            if (this.fAxisLeft.Enabled)
                this.axisLeftWidth = this.fAxisLeft.Width;
            PaintAll(graphics);
        }

        public void PaintAll(Graphics graphics)
        {
            this.fGraphics = graphics;
            this.fGraphics.Clip = new Region(new Rectangle(this.fX1, this.fY1, this.fWidth + 1, this.fHeight + 1));
            this.fGraphics.FillRectangle(new SolidBrush(BackColor), this.fX1, this.fY1, this.fWidth, this.fHeight);
            if (this.fBorderEnabled)
            {
                int height = this.fHeight;
                int width = this.fWidth;
                int num1 = this.fChart.ClientRectangle.Height - this.fY1 - 1;
                int num2 = this.fChart.ClientRectangle.Width - this.fX1 - 1;
                if (this.fHeight > num1)
                    height = num1;
                if (this.fWidth > num2)
                    width = num2;
                this.fGraphics.DrawRectangle(new Pen(this.fBorderColor)
                {
                    Width = (float)this.fBorderWidth
                }, this.fX1, this.fY1, width, height);
            }
            this.fClientX = this.fX1 + this.axisLeftWidth + this.fMarginLeft;
            this.fClientY = this.fY1 + this.titleHeight + this.axisTopHeight + this.fMarginTop;
            this.fClientWidth = this.fWidth - this.axisLeftWidth - this.axisRightWidth - this.fMarginLeft - this.fMarginRight;
            this.fClientHeight = this.fHeight - this.titleHeight - this.axisTopHeight - this.axisBottomHeight - this.fMarginTop - this.fMarginBottom;
            if (this.fClientWidth != 0 && this.fClientHeight != 0)
                this.fGraphics.FillRectangle(new LinearGradientBrush(new RectangleF(this.fClientX, this.fClientY, this.fClientWidth, this.fClientHeight), Color.FromArgb((int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue), Color.FromArgb(200, 200, 200), LinearGradientMode.Vertical), this.fClientX, this.fClientY, this.fClientWidth, this.fClientHeight);
            if (this.fAxisBottom.Enabled)
            {
                this.fAxisBottom.SetLocation(this.fClientX, this.fClientY + this.fClientHeight, this.fClientX + this.fClientWidth, this.fClientY + this.fClientHeight);
                this.fAxisBottom.Paint();
            }
            if (this.fAxisLeft.Enabled)
            {
                this.fGraphics.Clip = new Region(new Rectangle(this.fX1, this.fY1, this.fWidth, this.fHeight));
                this.fAxisLeft.SetLocation(this.fClientX, this.fClientY, this.fClientX, this.fClientY + this.fClientHeight);
                this.fAxisLeft.Paint();
            }
            if (this.fAxisTop.Enabled)
            {
                this.fAxisTop.SetLocation(this.fClientX, this.fClientY, this.fClientX + this.fClientWidth, this.fClientY);
                this.fAxisTop.Paint();
            }
            if (this.fAxisRight.Enabled)
            {
                this.fAxisRight.SetLocation(this.fClientX + this.fClientWidth, this.fClientY, this.fClientX + this.fClientWidth, this.fClientY + this.fClientHeight);
                this.fAxisRight.Paint();
            }
            this.fGraphics.Clip = new Region(new Rectangle(this.fClientX + 1, this.fClientY + 1, this.fClientWidth - 1, this.fClientHeight - 1));
            try
            {
                foreach (IDrawable drawable in this.fPrimitives)
                    drawable.Paint(this, XMin, XMin + CalculateRealQuantityOfTicks_Right(XMin, XMax), YMin, YMax);
            }
            catch
            {
            }
            foreach (var objectViewer in this.objectViewers)
                objectViewer.Viewer.Paint(objectViewer.Object, this);
            if (this.fOutlineEnabled)
                this.fGraphics.DrawRectangle(new Pen(Color.Green), this.fOutlineRectangle);
            if (this.fTitleEnabled)
            {
                switch (this.fTitle.Position)
                {
                    case ETitlePosition.Left:
                        this.fGraphics.Clip = new Region(new Rectangle(this.fX1, this.fY1, this.fWidth, this.fHeight));
                        this.fTitle.Y = this.fY1 + this.fMarginTop;
                        this.fTitle.X = this.fClientX + this.fTitleOffsetX;
                        break;
                    case ETitlePosition.Right:
                        this.fGraphics.Clip = new Region(new Rectangle(this.fX1, this.fY1, this.fWidth, this.fHeight));
                        this.fTitle.Y = this.fY1 + this.fMarginTop;
                        this.fTitle.X = this.fClientX + this.fClientWidth - this.fTitle.Width - this.fTitleOffsetX;
                        break;
                    case ETitlePosition.Centre:
                        this.fGraphics.Clip = new Region(new Rectangle(this.fX1, this.fY1, this.fWidth, this.fHeight));
                        this.fTitle.Y = this.fY1 + this.fMarginTop;
                        this.fTitle.X = this.fClientX + this.fClientWidth / 2 - this.fTitle.Width / 2 + this.fTitleOffsetX;
                        break;
                    case ETitlePosition.InsideLeft:
                        this.fTitle.Y = this.fClientY + this.fTitleOffsetY;
                        this.fTitle.X = this.fClientX + this.fTitleOffsetX;
                        this.fGraphics.FillRectangle((Brush)new SolidBrush(this.fForeColor), this.fTitle.X, this.fTitle.Y, this.fTitle.Width, this.fTitle.Height);
                        break;
                    case ETitlePosition.InsideRight:
                        this.fTitle.Y = this.fClientY + this.fTitleOffsetY;
                        this.fTitle.X = this.fClientX + this.fClientWidth - this.fTitle.Width - this.fTitleOffsetX;
                        this.fGraphics.FillRectangle((Brush)new SolidBrush(this.fForeColor), this.fTitle.X, this.fTitle.Y, this.fTitle.Width, this.fTitle.Height);
                        break;
                    case ETitlePosition.InsideCentre:
                        this.fTitle.Y = this.fClientY + this.fTitleOffsetY;
                        this.fTitle.X = this.fClientX + this.fClientWidth / 2 - this.fTitle.Width / 2 + this.fTitleOffsetX;
                        this.fGraphics.FillRectangle((Brush)new SolidBrush(this.fForeColor), this.fTitle.X, this.fTitle.Y, this.fTitle.Width, this.fTitle.Height);
                        break;
                }
                this.fTitle.Paint();
            }
            if (!this.fLegendEnabled)
                return;
            switch (this.fLegendPosition)
            {
                case ELegendPosition.TopRight:
                    this.fLegend.X = this.fClientX + this.fClientWidth - this.fLegendOffsetX - this.fLegend.Width;
                    this.fLegend.Y = this.fClientY + this.fLegendOffsetY;
                    break;
                case ELegendPosition.TopLeft:
                    this.fLegend.X = this.fClientX + this.fLegendOffsetX;
                    this.fLegend.Y = this.fClientY + this.fLegendOffsetY;
                    break;
                case ELegendPosition.BottomRight:
                    this.fLegend.X = this.fClientX + this.fClientWidth - this.fLegendOffsetX - this.fLegend.Width;
                    this.fLegend.Y = this.fClientY + this.fClientHeight - this.fLegendOffsetY - this.fLegend.Height;
                    break;
                case ELegendPosition.BottomLeft:
                    this.fLegend.X = this.fClientX + this.fLegendOffsetX;
                    this.fLegend.Y = this.fClientY + this.fClientHeight - this.fLegendOffsetY - this.fLegend.Height;
                    break;
            }
            this.fLegend.Paint();
        }

        public void DrawLine(Pen pen, double x1, double y1, double x2, double y2, bool doTransform)
        {
            if (doTransform)
                this.fGraphics.DrawLine(pen, ClientX(x1), ClientY(y1), ClientX(x2), ClientY(y2));
            else
                this.fGraphics.DrawLine(pen, (int)x1, (int)y1, (int)x2, (int)y2);
        }

        public void DrawVerticalTick(Pen pen, double x, double y, int length)
        {
        }

        public void DrawHorizontalTick(Pen pen, double x, double y, int length)
        {
            this.fGraphics.DrawLine(pen, (int)x, ClientY(y), (int)x + length, ClientY(y));
        }

        public void DrawVerticalGrid(Pen pen, double x)
        {
            this.fGraphics.DrawLine(pen, ClientX(x), this.fClientY, ClientX(x), this.fClientY + this.fClientHeight);
        }

        public void DrawHorizontalGrid(Pen pen, double y)
        {
            this.fGraphics.DrawLine(pen, this.fClientX, ClientY(y), this.fClientX + this.fClientWidth, ClientY(y));
        }

        public void DrawLine(Pen pen, double x1, double y1, double x2, double y2)
        {
            DrawLine(pen, x1, y1, x2, y2, true);
        }

        public void DrawRectangle(Pen pen, double x, double y, int width, int height)
        {
            this.fGraphics.DrawRectangle(pen, ClientX(x), ClientY(y), width, height);
        }

        public void DrawEllipse(Pen pen, double x, double y, int width, int height)
        {
            this.fGraphics.DrawEllipse(pen, ClientX(x), ClientY(y), width, height);
        }

        public void DrawBeziers(Pen pen, PointF[] pts)
        {
            var points = new Point[pts.Length];
            for (var i = 0; i < pts.Length; ++i)
            {
                var p = pts[i];
                points[i] = new Point(this.ClientX(p.X), this.ClientY(p.Y));
            }
            this.fGraphics.DrawBeziers(pen, points);
        }

        public void DrawText(string text, Font font, Brush brush, int x, int y)
        {
            this.fGraphics.DrawString(text, font, brush, x, y);
        }

        private bool IsInsideClient(int x, int y)
        {
            return this.fClientX < x && x < this.fClientX + this.fClientWidth && this.fClientY < y && y < this.fClientY + this.fClientHeight;
        }

        public virtual void MouseMove(MouseEventArgs evnt)
        {
            try
            {
                if (!this.fMouseDown)
                {
                    double num1 = (this.fXMax - this.fXMin) / 100.0;
                    double num2 = (this.fYMax - this.fYMin) / 100.0;
                    double X = this.WorldX(evnt.X);
                    double Y = this.WorldY(evnt.Y);
                    string str = "";
                    this.fSelectedPrimitive = null;
                    this.fSelectedPrimitiveDistance = null;
                    this.fOnPrimitive = false;
                    foreach (IDrawable drawable in this.fPrimitives)
                    {
                        TDistance tdistance = drawable.Distance(X, Y);
                        if (tdistance != null && tdistance.dX < num1 && tdistance.dY < num2)
                        {
                            if (drawable.ToolTipEnabled)
                            {
                                if (str != "")
                                    str = str + "\n\n";
                                str = str + tdistance.ToolTipText;
                            }
                            this.fOnPrimitive = true;
                            this.fSelectedPrimitive = drawable;
                            this.fSelectedPrimitiveDistance = tdistance;
                        }
                    }
                }
                if (this.fMouseMovePrimitiveEnabled && this.fMouseDown && (this.fMouseDownButton == MouseButtons.Left && this.fOnPrimitive) && this.fSelectedPrimitive is IMovable)
                {
                    double num1 = this.WorldX(evnt.X);
                    double num2 = this.WorldY(evnt.Y);
                    ((IMovable)this.fSelectedPrimitive).Move(this.fSelectedPrimitiveDistance.X, this.fSelectedPrimitiveDistance.Y, num1 - this.fSelectedPrimitiveDistance.X, num2 - this.fSelectedPrimitiveDistance.Y);
                    this.fSelectedPrimitiveDistance.X = num1;
                    this.fSelectedPrimitiveDistance.Y = num2;
                    this.fOnPrimitive = true;
                    this.Update();
                }
                if (this.fMouseZoomEnabled && this.fMouseDown && (this.fMouseDownButton == MouseButtons.Left && !this.fOnPrimitive))
                {
                    int num1 = Math.Abs(this.fMouseDownX - evnt.X);
                    int num2 = Math.Abs(this.fMouseDownY - evnt.Y);
                    int num3 = this.fMouseDownX >= evnt.X ? evnt.X : this.fMouseDownX;
                    int num4 = this.fMouseDownY >= evnt.Y ? evnt.Y : this.fMouseDownY;
                    this.fOutlineRectangle.X = num3;
                    this.fOutlineRectangle.Y = num4;
                    this.fOutlineRectangle.Width = num1;
                    this.fOutlineRectangle.Height = num2;
                    Update();
                }
                if (this.fMouseMoveContentEnabled && this.fMouseDown && this.fMouseDownButton == MouseButtons.Right)
                {
                    double num1 = (double)(this.fMouseDownX - evnt.X) / (double)this.fClientWidth * (this.XMax - this.XMin);
                    double num2 = this.WorldY(this.fMouseDownY) - this.WorldY(evnt.Y);
                    double num3 = num1 <= 0.0 ? this.CalculateRealQuantityOfTicks_Left(this.fAxisBottom.Min, this.fAxisBottom.Min + num1) : this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Min + num1);
                    this.fMouseDownX = evnt.X;
                    this.fMouseDownY = evnt.Y;
                    this.fAxisBottom.SetRange(this.fAxisBottom.Min + num3, this.fAxisBottom.Max + num3);
                    this.fAxisTop.SetRange(this.fAxisTop.Min + num3, this.fAxisTop.Max + num3);
                    this.fAxisLeft.SetRange(this.fAxisLeft.Min + num2, this.fAxisLeft.Max + num2);
                    this.fAxisRight.SetRange(this.fAxisRight.Min + num2, this.fAxisRight.Max + num2);
                    this.fAxisBottom.Zoomed = true;
                    this.fAxisTop.Zoomed = true;
                    this.fAxisLeft.Zoomed = true;
                    this.fAxisRight.Zoomed = true;
                    if (!this.fChart.GroupZoomEnabled)
                        this.Update();
                    this.EmitZoom(true);
                }
                else
                {
                    this.fOnAxis = false;
                    this.fAxisLeft.MouseMove(evnt);
                    this.fAxisBottom.MouseMove(evnt);
                    if (this.fAxisLeft.X1 - 10.0 <= (double)evnt.X && this.fAxisLeft.X1 >= (double)evnt.X && (this.fAxisLeft.Y1 <= (double)evnt.Y && this.fAxisLeft.Y2 >= (double)evnt.Y))
                        this.fOnAxis = true;
                    if (this.fAxisBottom.X1 <= (double)evnt.X && this.fAxisBottom.X2 >= (double)evnt.X && (this.fAxisBottom.Y1 <= (double)evnt.Y && this.fAxisBottom.Y1 + 10.0 >= (double)evnt.Y))
                        this.fOnAxis = true;
                    if (this.fOnAxis || this.fOnPrimitive)
                    {
                        #if GTK
                        this.fChart.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Hand1);
                        #else
//                        if (Cursor.Current == Cursors.Hand)
//                            return;
                        Cursor.Current = Cursors.Hand;
                        #endif
                    }
                    else
                    {
                        #if GTK
                        this.fChart.GdkWindow.Cursor = null;
                        #else
//                        if (Cursor.Current == Cursors.Default)
//                            return;
                        Cursor.Current = Cursors.Default;
                        #endif
                    }
                }
            }
            catch
            {
            }
        }

        public virtual void MouseWheel(MouseEventArgs evnt)
        {
            if (!this.fMouseWheelEnabled)
                return;
            double min = this.fAxisBottom.Min;
            double max = this.fAxisBottom.Max;
            switch (this.fMouseWheelMode)
            {
                case EMouseWheelMode.MoveX:
                    double num1 = (double)evnt.Delta / 120.0 * (this.fAxisBottom.Max - this.fAxisBottom.Min) * this.fMouseWheelSensitivity;
                    double num2 = num1 <= 0.0 ? this.CalculateRealQuantityOfTicks_Left(this.fAxisBottom.Min, this.fAxisBottom.Min + num1) : this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Min + num1);
                    this.fAxisBottom.SetRange(this.fAxisBottom.Min + num2, this.fAxisBottom.Max + num2);
                    this.fAxisTop.SetRange(this.fAxisTop.Min + num2, this.fAxisTop.Max + num2);
                    this.fAxisBottom.Zoomed = true;
                    this.fAxisTop.Zoomed = true;
                    EmitZoom(true);
                    break;
                case EMouseWheelMode.MoveY:
                    double num3 = (double)evnt.Delta / 120.0 * (this.fYMax - this.fYMin) * this.fMouseWheelSensitivity;
                    this.fAxisLeft.SetRange(this.fAxisLeft.Min + num3, this.fAxisLeft.Max + num3);
                    this.fAxisRight.SetRange(this.fAxisRight.Min + num3, this.fAxisRight.Max + num3);
                    this.fAxisLeft.Zoomed = true;
                    this.fAxisRight.Zoomed = true;
                    EmitZoom(true);
                    break;
                case EMouseWheelMode.ZoomX:
                    double num4 = (double)evnt.Delta / 120.0 * (this.fAxisBottom.Max - this.fAxisBottom.Min) * this.fMouseWheelSensitivity;
                    double num5 = num4 <= 0.0 ? this.CalculateRealQuantityOfTicks_Left(this.fAxisBottom.Min, this.fAxisBottom.Min + num4) : this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Min + num4);
                    double num6 = num5 - num4;
                    this.fAxisBottom.SetRange(this.fAxisBottom.Min + num5, this.fAxisBottom.Max + num6);
                    this.fAxisTop.SetRange(this.fAxisTop.Min + num5, this.fAxisTop.Max + num6);
                    this.fAxisBottom.Zoomed = true;
                    this.fAxisTop.Zoomed = true;
                    EmitZoom(true);
                    break;
                case EMouseWheelMode.ZoomY:
                    double num7 = (double)evnt.Delta / 120.0 * (this.fYMax - this.fYMin) * this.fMouseWheelSensitivity;
                    this.fAxisLeft.SetRange(this.fAxisLeft.Min + num7, this.fAxisLeft.Max);
                    this.fAxisRight.SetRange(this.fAxisRight.Min + num7, this.fAxisRight.Max);
                    this.fAxisLeft.Zoomed = true;
                    this.fAxisRight.Zoomed = true;
                    EmitZoom(true);
                    break;
                case EMouseWheelMode.Zoom:
                    double num8 = this.fAxisBottom.Min + this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Max);
                    double num9 = (double)evnt.Delta / 120.0 * (num8 - this.fAxisBottom.Min) * this.fMouseWheelSensitivity;
                    double num10 = (double)evnt.Delta / 120.0 * (this.fYMax - this.fYMin) * this.fMouseWheelSensitivity;
                    double num11 = this.WorldX(evnt.X);
                    double num12 = this.WorldY(evnt.Y);
                    double num13 = (num11 - this.fAxisBottom.Min) / (num8 - this.fAxisBottom.Min) * num9;
                    double num14 = (num8 - num11) / (num8 - this.fAxisBottom.Min) * num9;
                    double num15 = (num12 - this.fYMin) / (this.fYMax - this.fYMin) * num10;
                    double num16 = (this.fYMax - num12) / (this.fYMax - this.fYMin) * num10;
                    double num17 = num13 <= 0.0 ? this.CalculateRealQuantityOfTicks_Left(this.fAxisBottom.Min, this.fAxisBottom.Min + num13) : this.CalculateRealQuantityOfTicks_Right(this.fAxisBottom.Min, this.fAxisBottom.Min + num13);
                    double num18 = -num17 + num13 + num14;
                    this.fAxisBottom.SetRange(this.fAxisBottom.Min + num17, this.fAxisBottom.Max - num18);
                    this.fAxisTop.SetRange(this.fAxisTop.Min + num17, this.fAxisTop.Max - num18);
                    this.fAxisLeft.SetRange(this.fAxisLeft.Min + num15, this.fAxisLeft.Max - num16);
                    this.fAxisRight.SetRange(this.fAxisRight.Min + num15, this.fAxisRight.Max - num16);
                    this.fAxisBottom.Zoomed = true;
                    this.fAxisTop.Zoomed = true;
                    this.fAxisLeft.Zoomed = true;
                    this.fAxisRight.Zoomed = true;
                    EmitZoom(true);
                    break;
            }
            if (this.fChart.GroupZoomEnabled)
                return;
            Update();
        }

        public virtual void MouseDown(MouseEventArgs evnt)
        {
            if (IsInsideClient(evnt.X, evnt.Y))
            {
                this.fMouseDown = true;
                this.fMouseDownX = evnt.X;
                this.fMouseDownY = evnt.Y;
                this.fMouseDownButton = evnt.Button;
                if (this.fMouseZoomEnabled && this.fMouseDownButton == MouseButtons.Left && this.fSelectedPrimitive == null)
                    this.fOutlineEnabled = true;
                if (this.fMouseContextMenuEnabled && this.fMouseDownButton == MouseButtons.Right && this.fOnPrimitive)
                {
                    InitContextMenu();
                    #if !GTK
                    this.deleteMenuItem.Text = "Delete " + this.fSelectedPrimitive.GetType().Name;
                    #endif
                }
            }
            this.fAxisLeft.MouseDown(evnt);
            this.fAxisBottom.MouseDown(evnt);
        }

        public virtual void MouseUp(MouseEventArgs evnt)
        {
            if (this.fMouseZoomEnabled && this.fMouseDown && this.fMouseDownButton == MouseButtons.Left && !this.fOnPrimitive)
            {
                this.fOutlineEnabled = false;
                if (Math.Abs(this.fMouseDownX - evnt.X) > 2 && Math.Abs(this.fMouseDownY - evnt.Y) > 2)
                {
                    double num1 = this.WorldX(this.fMouseDownX);
                    double num2 = this.WorldX(evnt.X);
                    double num3 = this.WorldY(this.fMouseDownY);
                    double num4 = this.WorldY(evnt.Y);
                    double num5;
                    double Y;
                    if (num1 < num2)
                    {
                        num5 = num1;
                        Y = num2;
                    }
                    else
                    {
                        num5 = num2;
                        Y = num1;
                    }
                    double Min;
                    double Max1;
                    if (num3 < num4)
                    {
                        Min = num3;
                        Max1 = num4;
                    }
                    else
                    {
                        Min = num4;
                        Max1 = num3;
                    }
                    double Max2 = Y - this.CalculateNotInSessionTicks(num5, Y);
                    this.fAxisBottom.SetRange(num5, Max2);
                    this.fAxisTop.SetRange(num5, Max2);
                    this.fAxisLeft.SetRange(Min, Max1);
                    this.fAxisRight.SetRange(Min, Max1);
                    this.fAxisBottom.Zoomed = true;
                    this.fAxisTop.Zoomed = true;
                    this.fAxisLeft.Zoomed = true;
                    this.fAxisRight.Zoomed = true;
                    if (!this.fChart.GroupZoomEnabled)
                        this.Update();
                    this.EmitZoom(true);
                }
                this.fMouseDown = false;
            }
            else
            {
                this.fAxisLeft.MouseUp(evnt);
                this.fAxisBottom.MouseUp(evnt);
                this.fMouseDown = false;
            }
        }

        public virtual void DoubleClick(int x, int y)
        {
            if (IsInsideClient(x, y))
            {
                if (this.fOnPrimitive)
                {
                    if (!this.fMousePrimitivePropertiesEnabled)
                        return;
                    new PadProperyForm(this.fSelectedPrimitive, this).ShowDialog();
                }
                else
                {
                    if (!this.fMouseUnzoomEnabled || !this.AxisLeft.Zoomed && !this.AxisBottom.Zoomed)
                        return;
                    this.fOutlineEnabled = false;
                    if (!this.fChart.GroupZoomEnabled)
                        this.UnZoom();
                    this.EmitZoom(false);
                }
            }
            else
            {
                if (this.fMousePadPropertiesEnabled)
                    new PadProperyForm(this, this).ShowDialog();
            }
        }

        public static void EmitNewTick(DateTime datetime)
        {
            if (Pad.NewTick != null)
                Pad.NewTick(null, new NewTickEventArgs(datetime));
        }

        private void OnNewTick(object sender, NewTickEventArgs args)
        {
            if (!this.fMonitored)
                return;
            int num1 = args.DateTime.Hour * 60 * 60 + args.DateTime.Minute * 60 + args.DateTime.Second;
            if (num1 - this.fLastTickTime < this.fUpdateInterval)
                return;
            DateTime dateTime = args.DateTime;
            double XMin = (double)dateTime.AddSeconds((double)-this.fWindowSize).Ticks;
            double num2 = (double)dateTime.Ticks;
            this.SetRangeX(XMin, num2 + (num2 - XMin) / 20.0);
            if ((DateTime.Now.Ticks - this.fLastUpdateDateTime.Ticks) / 1000000L > 1)
            {
                if (!this.fChart.GroupZoomEnabled)
                    this.Update();
                EmitZoom(true);
                this.fLastUpdateDateTime = DateTime.Now;
            }
            this.fLastTickTime = num1;
        }

        public void EmitZoom(bool zoom)
        {
            Zoom?.Invoke(null, new ZoomEventArgs(this.XMin, this.XMax, this.YMin, this.YMax, zoom));
        }

        private void OnDeleteMenuItemClick(object sender, EventArgs e)
        {
            this.fPrimitives.Remove(this.fSelectedPrimitive);
            this.Update();
        }

        private void OnPropertiesMenuItemClick(object sender, EventArgs e)
        {
            new PadProperyForm(this.fSelectedPrimitive, this).ShowDialog();
        }

        private class ObjectViewer
        {
            public object Object { get; set; }

            public Viewer Viewer { get; set; }

            public ObjectViewer(object obj, Viewer viewer)
            {
                Object = obj;
                Viewer = viewer;
            }
        }

        [Serializable]
        private class TFeatures3D
        {
            private Pad pad;
            private TAxes2D axes2D;
            public Axis[] Axes;
            private bool active;
            public object View;

            public bool Active
            {
                get
                {
                    return this.active;
                }
                set
                {
                    this.active = value;
                    if (value)
                    {
                        this.axes2D.SetFor3D();
                        this.pad.ForeColor = this.pad.BackColor;
                        this.pad.AntiAliasingEnabled = true;
                    }
                    else
                        this.axes2D.Restore();
                }
            }

            public TFeatures3D(Pad pad)
            {
                this.pad = pad;
                this.axes2D = new Pad.TFeatures3D.TAxes2D(pad);
                this.Axes = new Axis[] { new Axis(pad), new Axis(pad), new Axis(pad) };
                for (int i = 0; i < this.Axes.Length; ++i)
                {
                    this.Axes[i].Max = 1;
                    this.Axes[i].Min = 0;
                }
            }

            public void SetRangeX(double xMin, double xMax)
            {
                this.Axes[0].SetRange(xMin, xMax);
            }

            public void SetRangeY(double yMin, double yMax)
            {
                this.Axes[1].SetRange(yMin, yMax);
            }

            public void SetRangeZ(double zMin, double zMax)
            {
                this.Axes[2].SetRange(zMin, zMax);
            }

            public void SetRange(double xMin, double xMax, double yMin, double yMax)
            {
                SetRangeX(xMin, xMax);
                SetRangeY(yMin, yMax);
            }

            [Serializable]
            private class TAxes2D
            {
                private Pad pad;
                private bool saved;
                private Axis top;
                private Axis bottom;
                private Axis left;
                private Axis right;

                public TAxes2D(Pad pad)
                {
                    this.pad = pad;
                    this.top = new Axis(pad);
                    this.bottom = new Axis(pad);
                    this.left = new Axis(pad);
                    this.right = new Axis(pad);
                }

                private void Copy(Axis dst, Axis src)
                {
                    dst.LabelEnabled = src.LabelEnabled;
                    dst.MajorTicksEnabled = src.MajorTicksEnabled;
                    dst.MinorTicksEnabled = src.MinorTicksEnabled;
                    dst.GridEnabled = src.GridEnabled;
                    dst.MinorGridEnabled = src.MinorGridEnabled;
                    dst.SetRange(src.Min, src.Max);
                    dst.Enabled = src.Enabled;
                }

                private void SetFor3D(Axis a)
                {
                    a.LabelEnabled = false;
                    a.MajorTicksEnabled = false;
                    a.MinorTicksEnabled = false;
                    a.GridEnabled = false;
                    a.MinorGridEnabled = false;
                    a.SetRange(0, 1);
                    a.Enabled = false;
                }

                public void Save()
                {
                    Copy(this.top, this.pad.fAxisTop);
                    Copy(this.bottom, this.pad.fAxisBottom);
                    Copy(this.left, this.pad.fAxisLeft);
                    Copy(this.right, this.pad.fAxisRight);
                    this.saved = true;
                }

                public void SetFor3D()
                {
                    Save();
                    SetFor3D(this.pad.AxisTop);
                    SetFor3D(this.pad.AxisBottom);
                    SetFor3D(this.pad.AxisLeft);
                    SetFor3D(this.pad.AxisRight);
                }

                public void Restore()
                {
                    if (!this.saved)
                        return;
                    Copy(this.pad.fAxisTop, this.top);
                    Copy(this.pad.fAxisBottom, this.bottom);
                    Copy(this.pad.fAxisLeft, this.left);
                    Copy(this.pad.fAxisRight, this.right);
                }
            }
        }
    }

    [Serializable]
    public class PadList : IList
    {
        private ArrayList list = new ArrayList();

        public bool IsReadOnly
        {
            get
            {
                return this.list.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return this.list.IsFixedSize;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.list.IsSynchronized;
            }
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.list.SyncRoot;
            }
        }

        public Pad this[int index]
        {
            get
            {
                return this.list[index] as Pad;
            }
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        void IList.Insert(int index, object value)
        {
        }

        void IList.Remove(object value)
        {
            this.Remove(value as Pad);
        }

        bool IList.Contains(object value)
        {
            return this.list.Contains(value);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as Pad);
        }

        int IList.Add(object value)
        {
            return this.Add(value as Pad);
        }

        public void CopyTo(Array array, int index)
        {
            this.list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int Add(Pad pad)
        {
            return this.list.Add(pad);
        }

        public void Remove(Pad pad)
        {
            this.list.Remove(pad);
        }

        public int IndexOf(Pad pad)
        {
            return this.list.IndexOf(pad);
        }
    }

    public class PadRange
    {
        public double Min;
        public double Max;
        protected bool isValid;

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public PadRange(double min, double max)
        {
            Min = min;
            Max = max;
            isValid = max - min > double.Epsilon;
        }
    }

#if GTK
    public class PadProperyForm : Form
    {
        private object obj;
        private Pad pad;
        public PadProperyForm(object obj, Pad pad)
        {
            InitializeComponent();
            this.obj = obj;
            this.pad = pad;
            Text = string.Format("{0}  properties", obj.GetType().Name);
        }

        private void InitializeComponent()
        {
        }

        public int ShowDialog()
        {
            return 0;
            //throw new System.NotImplementedException();
        }
    }
#else
    public class PadProperyForm : Form
    {
        private object obj;
        private Pad pad;
        private PropertyGrid propertyGrid;

        public PadProperyForm(object obj, Pad pad)
        {
            InitializeComponent();
            this.obj = obj;
            this.pad = pad;
            Text = string.Format("{0}  properties", obj.GetType().Name);
            this.propertyGrid.SelectedObject = obj;
        }

        private void InitializeComponent()
        {

            ResourceManager resourceManager = new ResourceManager(typeof(PadProperyForm));
            this.propertyGrid = new PropertyGrid();
            this.SuspendLayout();
            this.propertyGrid.CommandsVisibleIfAvailable = true;
            this.propertyGrid.Dock = DockStyle.Fill;
            this.propertyGrid.LargeButtons = false;
            this.propertyGrid.LineColor = SystemColors.ScrollBar;
            this.propertyGrid.Location = new Point(0, 0);
            this.propertyGrid.Name = "PropertyGrid";
            this.propertyGrid.Size = new Size(336, 381);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.Text = "propertyGrid1";
            this.propertyGrid.ViewBackColor = SystemColors.Window;
            this.propertyGrid.ViewForeColor = SystemColors.WindowText;
            this.propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.PropertyGrid_PropertyValueChanged);
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(336, 381);
            this.Controls.Add(this.propertyGrid);
            this.Icon = (Icon)resourceManager.GetObject("$this.Icon");
            this.Name = "PadProperyForm";
            this.Text = "Pad properties";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
        }

        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.pad.Update();
        }

    }
#endif
}
