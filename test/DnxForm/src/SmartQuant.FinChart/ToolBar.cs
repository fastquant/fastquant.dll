using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;

#if GTK
using Compatibility.Gtk;
#else
using System.Windows.Forms;
#endif

namespace SmartQuant.FinChart
{
    public class ToolBar : UserControl
    {
        private Chart chart;
        private ToolTip toolTip;
        private ImageList imageList;
        private ToolBarButton toolBarButton3;
        private ToolBarButton toolBarButton6;
        private ToolBarButton toolBarButton9;
        private System.Windows.Forms.ToolBar chartToolBar;
        private ToolBarButton btnCursor;
        private ToolBarButton btnCrosshair;
        private ToolBarButton btnZoomIn;
        private ToolBarButton btnZoomOut;
        private ToolBarButton btnLinear;
        private ToolBarButton btnLog;
        private ToolBarButton btnCandle;
        private ToolBarButton btnBar;
        private ToolBarButton btnLine;
        private ToolBarButton toolBarButton2;
        private ToolBarButton btnWholeRange;
        private ToolBarButton btnTrailing;
        private ToolBarButton btnFixed;
        private ToolBarButton btnPnF;
        private IContainer components;

        public Chart Chart
        {
            get
            {
                return this.chart;
            }
            set
            {
                Disconnect();
                this.chart = value;
                Connect();
                ChangeActionType();
                ChangeBarSeriesStyle();
                ChangeUpdateStyle();
                ChangeVolumeVisible();
                ChangeScaleStyle();
            }
        }

        public ToolBar()
        {
            InitializeComponent();
        }

        public ToolBar(Chart chart) : this()
        {
            this.Chart = chart;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            var resourceManager = new ResourceManager(typeof(ToolBar));
            this.chartToolBar = new System.Windows.Forms.ToolBar();
            this.btnCursor = new ToolBarButton();
            this.btnCrosshair = new ToolBarButton();
            this.toolBarButton3 = new ToolBarButton();
            this.btnZoomIn = new ToolBarButton();
            this.btnZoomOut = new ToolBarButton();
            this.toolBarButton6 = new ToolBarButton();
            this.btnLinear = new ToolBarButton();
            this.btnLog = new ToolBarButton();
            this.toolBarButton9 = new ToolBarButton();
            this.btnWholeRange = new ToolBarButton();
            this.btnTrailing = new ToolBarButton();
            this.btnFixed = new ToolBarButton();
            this.toolBarButton2 = new ToolBarButton();
            this.btnCandle = new ToolBarButton();
            this.btnBar = new ToolBarButton();
            this.btnLine = new ToolBarButton();
            this.btnPnF = new ToolBarButton();
            this.imageList = new ImageList(this.components);
            this.toolTip = new ToolTip(this.components);
            this.SuspendLayout();
            this.chartToolBar.Appearance = ToolBarAppearance.Flat;
            this.chartToolBar.Buttons.AddRange(new ToolBarButton[17]
            {
                this.btnCursor,
                this.btnCrosshair,
                this.toolBarButton3,
                this.btnZoomIn,
                this.btnZoomOut,
                this.toolBarButton6,
                this.btnLinear,
                this.btnLog,
                this.toolBarButton9,
                this.btnWholeRange,
                this.btnTrailing,
                this.btnFixed,
                this.toolBarButton2,
                this.btnCandle,
                this.btnBar,
                this.btnLine,
                this.btnPnF
            });
            this.chartToolBar.Dock = DockStyle.Fill;
            this.chartToolBar.DropDownArrows = true;
            this.chartToolBar.ImageList = this.imageList;
            this.chartToolBar.Location = new Point(0, 0);
            this.chartToolBar.Name = "chartToolBar";
            this.chartToolBar.ShowToolTips = true;
            this.chartToolBar.Size = new Size(408, 28);
            this.chartToolBar.TabIndex = 0;
            this.chartToolBar.ButtonClick += new ToolBarButtonClickEventHandler(this.chartToolBar_ButtonClick);
            this.btnCursor.ImageIndex = 7;
            this.btnCursor.ToolTipText = "Cusror";
            this.btnCrosshair.ImageIndex = 8;
            this.btnCrosshair.ToolTipText = "Crosshair";
            this.toolBarButton3.Style = ToolBarButtonStyle.Separator;
            this.btnZoomIn.ImageIndex = 5;
            this.btnZoomIn.ToolTipText = "Zoom In";
            this.btnZoomOut.ImageIndex = 6;
            this.btnZoomOut.ToolTipText = "Zoom Out";
            this.toolBarButton6.Style = ToolBarButtonStyle.Separator;
            this.btnLinear.ImageIndex = 9;
            this.btnLinear.ToolTipText = "Linear Scale";
            this.btnLog.ImageIndex = 10;
            this.btnLog.ToolTipText = "Log Scale";
            this.toolBarButton9.Style = ToolBarButtonStyle.Separator;
            this.btnWholeRange.ImageIndex = 17;
            this.btnWholeRange.ToolTipText = "Whole range mode";
            this.btnWholeRange.Visible = false;
            this.btnTrailing.ImageIndex = 19;
            this.btnTrailing.ToolTipText = "Trailing mode";
            this.btnFixed.ImageIndex = 18;
            this.btnFixed.ToolTipText = "Fixed mode";
            this.toolBarButton2.Style = ToolBarButtonStyle.Separator;
            this.btnCandle.ImageIndex = 11;
            this.btnCandle.ToolTipText = "Candle";
            this.btnBar.ImageIndex = 12;
            this.btnBar.ToolTipText = "Bar";
            this.btnLine.ImageIndex = 13;
            this.btnLine.ToolTipText = "Line";
            this.btnPnF.ImageIndex = 20;
            this.btnPnF.ToolTipText = "Point And Figure";
            this.imageList.ImageSize = new Size(16, 16);
            this.imageList.ImageStream = (ImageListStreamer)resourceManager.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.Transparent;
            this.Controls.Add((Control)this.chartToolBar);
            this.Name = "ToolBar";
            this.Size = new Size(408, 32);
            this.ResumeLayout(false);
        }

        private void Candle()
        {
            this.chart.BarSeriesStyle = BSStyle.Candle;
        }

        private void Bar()
        {
            this.chart.BarSeriesStyle = BSStyle.Bar;
        }

        private void Line()
        {
            this.chart.BarSeriesStyle = BSStyle.Line;
        }

        private void Kagi()
        {
        }

        private void LineBreak()
        {
        }

        private void Renko()
        {
        }

        private void Linear()
        {
            this.chart.ScaleStyle = PadScaleStyle.Arith;
        }

        private void Log()
        {
            this.chart.ScaleStyle = PadScaleStyle.Log;
        }

        private void Crosshair()
        {
            this.chart.ActionType = ChartActionType.Cross;
        }

        private void Pointer()
        {
            this.chart.ActionType = ChartActionType.None;
        }

        private void ZoomIn()
        {
            this.chart.ZoomIn();
        }

        private void ZoomOut()
        {
            this.chart.ZoomOut();
        }

        private void WholeRange()
        {
            this.chart.UpdateStyle = ChartUpdateStyle.WholeRange;
        }

        private void Trailing()
        {
            this.chart.UpdateStyle = ChartUpdateStyle.Trailing;
        }

        private void Fixed()
        {
            this.chart.UpdateStyle = ChartUpdateStyle.Fixed;
        }

        private void PnF()
        {
            this.chart.BarSeriesStyle = BSStyle.PointAndFigure;
            this.chart.DrawItems = false;
        }

        private void chartToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button == this.btnZoomIn)
                this.ZoomIn();
            if (e.Button == this.btnZoomOut)
                this.ZoomOut();
            if (e.Button == this.btnCrosshair)
                this.Crosshair();
            if (e.Button == this.btnCursor)
                this.Pointer();
            if (e.Button == this.btnLinear)
                this.Linear();
            if (e.Button == this.btnLog)
                this.Log();
            if (e.Button == this.btnCandle)
                this.Candle();
            if (e.Button == this.btnBar)
                this.Bar();
            if (e.Button == this.btnLine)
                this.Line();
            if (e.Button == this.btnWholeRange)
                this.WholeRange();
            if (e.Button == this.btnTrailing)
                this.Trailing();
            if (e.Button == this.btnFixed)
                this.Fixed();
            if (e.Button != this.btnPnF)
                return;
            this.PnF();
        }

        private void Disconnect()
        {
            if (this.chart == null)
                return;
            this.chart.ActionTypeChanged -= new EventHandler(this.chart_ActionTypeChanged);
            this.chart.UpdateStyleChanged -= new EventHandler(this.chart_UpdateStyleChanged);
            this.chart.VolumeVisibleChanged -= new EventHandler(this.chart_VolumeVisibleChanged);
            this.chart.BarSeriesStyleChanged -= new EventHandler(this.chart_BarSeriesStyleChanged);
            this.chart.ScaleStyleChanged -= new EventHandler(this.chart_ScaleStyleChanged);
        }

        private void Connect()
        {
            if (this.chart == null)
                return;
            this.chart.ActionTypeChanged += new EventHandler(this.chart_ActionTypeChanged);
            this.chart.UpdateStyleChanged += new EventHandler(this.chart_UpdateStyleChanged);
            this.chart.VolumeVisibleChanged += new EventHandler(this.chart_VolumeVisibleChanged);
            this.chart.BarSeriesStyleChanged += new EventHandler(this.chart_BarSeriesStyleChanged);
            this.chart.ScaleStyleChanged += new EventHandler(this.chart_ScaleStyleChanged);
        }

        private void ChangeUpdateStyle()
        {
            if (this.chart.UpdateStyle == ChartUpdateStyle.Fixed)
            {
                this.btnFixed.Pushed = true;
                this.btnTrailing.Pushed = false;
                this.btnWholeRange.Pushed = false;
            }
            if (this.chart.UpdateStyle == ChartUpdateStyle.Trailing)
            {
                this.btnFixed.Pushed = false;
                this.btnTrailing.Pushed = true;
                this.btnWholeRange.Pushed = false;
            }
            if (this.chart.UpdateStyle != ChartUpdateStyle.WholeRange)
                return;
            this.btnFixed.Pushed = false;
            this.btnTrailing.Pushed = false;
            this.btnWholeRange.Pushed = true;
        }

        private void ChangeVolumeVisible()
        {
        }

        private void ChangeBarSeriesStyle()
        {
            if (this.chart.BarSeriesStyle == BSStyle.Bar)
            {
                this.btnBar.Pushed = true;
                this.btnCandle.Pushed = false;
                this.btnLine.Pushed = false;
                this.btnPnF.Pushed = false;
            }
            if (this.chart.BarSeriesStyle == BSStyle.Candle)
            {
                this.btnBar.Pushed = false;
                this.btnCandle.Pushed = true;
                this.btnLine.Pushed = false;
                this.btnPnF.Pushed = false;
            }
            if (this.chart.BarSeriesStyle == BSStyle.Line)
            {
                this.btnBar.Pushed = false;
                this.btnCandle.Pushed = false;
                this.btnLine.Pushed = true;
                this.btnPnF.Pushed = false;
            }
            if (this.chart.BarSeriesStyle != BSStyle.PointAndFigure)
                return;
            this.btnBar.Pushed = false;
            this.btnCandle.Pushed = false;
            this.btnLine.Pushed = false;
            this.btnPnF.Pushed = true;
        }

        private void ChangeActionType()
        {
            if (this.chart.ActionType == ChartActionType.Cross)
            {
                this.btnCrosshair.Pushed = true;
                this.btnCursor.Pushed = false;
            }
            if (this.chart.ActionType != ChartActionType.None)
                return;
            this.btnCrosshair.Pushed = false;
            this.btnCursor.Pushed = true;
        }

        private void ChangeScaleStyle()
        {
            if (this.chart.ScaleStyle == PadScaleStyle.Arith)
            {
                this.btnLinear.Pushed = true;
                this.btnLog.Pushed = false;
            }
            if (this.chart.ScaleStyle != PadScaleStyle.Log)
                return;
            this.btnLinear.Pushed = false;
            this.btnLog.Pushed = true;
        }

        private void chart_UpdateStyleChanged(object sender, EventArgs e)
        {
            this.ChangeUpdateStyle();
        }

        private void chart_VolumeVisibleChanged(object sender, EventArgs e)
        {
            this.ChangeVolumeVisible();
        }

        private void chart_BarSeriesStyleChanged(object sender, EventArgs e)
        {
            this.ChangeBarSeriesStyle();
        }

        private void chart_ActionTypeChanged(object sender, EventArgs e)
        {
            this.ChangeActionType();
        }

        private void chart_ScaleStyleChanged(object sender, EventArgs e)
        {
            this.ChangeScaleStyle();
        }
    }
}
