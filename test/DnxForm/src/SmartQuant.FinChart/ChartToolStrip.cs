using System;
using System.ComponentModel;
using System.Drawing;

#if GTK
using Compatibility.Gtk;

#else
using System.Windows.Forms;
#endif

namespace SmartQuant.FinChart
{
    #if !GTK
    public class ChartToolStrip : UserControl
  {
    private Chart chart;
    private IContainer components;
    private ToolStripButton tsbZoomIn;
    private ToolStripButton tsbZoomOut;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripButton tsbLinear;
    private ToolStrip toolStrip;
    private ToolStripButton tsbCrosshair;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton tsbLog;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripButton tsbTrailing;
    private ToolStripButton tsbFixed;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripButton tsbCandle;
    private ToolStripButton tsbBar;
    private ToolStripButton tsbLine;
    private ToolStripButton tsbPnF;
    private ToolStripButton tsbCursor;

    public Chart Chart
    {
      get
      {
        return this.chart;
      }
      set
      {
        this.Disconnect();
        this.chart = value;
        this.Connect();
        this.ChangeActionType();
        this.ChangeBarSeriesStyle();
        this.ChangeUpdateStyle();
        this.ChangeVolumeVisible();
        this.ChangeScaleStyle();
      }
    }

    public ChartToolStrip()
    {
      this.InitializeComponent();
    }

    private void tsbCursor_Click(object sender, EventArgs e)
    {
      this.chart.ActionType = ChartActionType.None;
    }

    private void tsbCrosshair_Click(object sender, EventArgs e)
    {
      this.chart.ActionType = ChartActionType.Cross;
    }

    private void tsbZoomIn_Click(object sender, EventArgs e)
    {
      this.chart.ZoomIn();
    }

    private void tsbZoomOut_Click(object sender, EventArgs e)
    {
      this.chart.ZoomOut();
    }

    private void tsbLinear_Click(object sender, EventArgs e)
    {
      this.chart.ScaleStyle = PadScaleStyle.Arith;
    }

    private void tsbLog_Click(object sender, EventArgs e)
    {
      this.chart.ScaleStyle = PadScaleStyle.Log;
    }

    private void tsbTrailing_Click(object sender, EventArgs e)
    {
      this.chart.UpdateStyle = ChartUpdateStyle.Trailing;
    }

    private void tsbFixed_Click(object sender, EventArgs e)
    {
      this.chart.UpdateStyle = ChartUpdateStyle.Fixed;
    }

    private void tsbCandle_Click(object sender, EventArgs e)
    {
      this.chart.BarSeriesStyle = BSStyle.Candle;
    }

    private void tsbBar_Click(object sender, EventArgs e)
    {
      this.chart.BarSeriesStyle = BSStyle.Bar;
    }

    private void tsbLine_Click(object sender, EventArgs e)
    {
      this.chart.BarSeriesStyle = BSStyle.Line;
    }

    private void tsbPnF_Click(object sender, EventArgs e)
    {
      this.chart.BarSeriesStyle = BSStyle.PointAndFigure;
      this.chart.DrawItems = false;
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
        this.tsbFixed.Checked = true;
        this.tsbTrailing.Checked = false;
      }
      if (this.chart.UpdateStyle == ChartUpdateStyle.Trailing)
      {
        this.tsbFixed.Checked = false;
        this.tsbTrailing.Checked = true;
      }
      if (this.chart.UpdateStyle != ChartUpdateStyle.WholeRange)
        return;
      this.tsbFixed.Checked = false;
      this.tsbTrailing.Checked = false;
    }

    private void ChangeVolumeVisible()
    {
    }

    private void ChangeBarSeriesStyle()
    {
      if (this.chart.BarSeriesStyle == BSStyle.Bar)
      {
        this.tsbBar.Checked = true;
        this.tsbCandle.Checked = false;
        this.tsbLine.Checked = false;
        this.tsbPnF.Checked = false;
      }
      if (this.chart.BarSeriesStyle == BSStyle.Candle)
      {
        this.tsbBar.Checked = false;
        this.tsbCandle.Checked = true;
        this.tsbLine.Checked = false;
        this.tsbPnF.Checked = false;
      }
      if (this.chart.BarSeriesStyle == BSStyle.Line)
      {
        this.tsbBar.Checked = false;
        this.tsbCandle.Checked = false;
        this.tsbLine.Checked = true;
        this.tsbPnF.Checked = false;
      }
      if (this.chart.BarSeriesStyle != BSStyle.PointAndFigure)
        return;
      this.tsbBar.Checked = false;
      this.tsbCandle.Checked = false;
      this.tsbLine.Checked = false;
      this.tsbPnF.Checked = true;
    }

    private void ChangeActionType()
    {
      if (this.chart.ActionType == ChartActionType.Cross)
      {
        this.tsbCrosshair.Checked = true;
        this.tsbCursor.Checked = false;
      }
      if (this.chart.ActionType != ChartActionType.None)
        return;
      this.tsbCrosshair.Checked = false;
      this.tsbCursor.Checked = true;
    }

    private void ChangeScaleStyle()
    {
      if (this.chart.ScaleStyle == PadScaleStyle.Arith)
      {
        this.tsbLinear.Checked = true;
        this.tsbLog.Checked = false;
      }
      if (this.chart.ScaleStyle != PadScaleStyle.Log)
        return;
      this.tsbLinear.Checked = false;
      this.tsbLog.Checked = true;
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

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (ChartToolStrip));
      this.tsbZoomIn = new ToolStripButton();
      this.tsbZoomOut = new ToolStripButton();
      this.toolStripSeparator2 = new ToolStripSeparator();
      this.tsbLinear = new ToolStripButton();
      this.toolStrip = new ToolStrip();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.tsbCrosshair = new ToolStripButton();
      this.tsbLog = new ToolStripButton();
      this.tsbBar = new ToolStripButton();
      this.tsbCandle = new ToolStripButton();
      this.tsbPnF = new ToolStripButton();
      this.tsbLine = new ToolStripButton();
      this.tsbTrailing = new ToolStripButton();
      this.toolStripSeparator3 = new ToolStripSeparator();
      this.toolStripSeparator4 = new ToolStripSeparator();
      this.tsbFixed = new ToolStripButton();
      this.tsbCursor = new ToolStripButton();
      this.toolStrip.SuspendLayout();
      this.SuspendLayout();
      this.tsbZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbZoomIn.Image = (Image) componentResourceManager.GetObject("tsbZoomIn.Image");
      this.tsbZoomIn.ImageTransparentColor = Color.Magenta;
      this.tsbZoomIn.Name = "tsbZoomIn";
      this.tsbZoomIn.Size = new Size(23, 22);
      this.tsbZoomIn.Text = "Zoom In";
      this.tsbZoomIn.Click += new EventHandler(this.tsbZoomIn_Click);
      this.tsbZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbZoomOut.Image = (Image) componentResourceManager.GetObject("tsbZoomOut.Image");
      this.tsbZoomOut.ImageTransparentColor = Color.Magenta;
      this.tsbZoomOut.Name = "tsbZoomOut";
      this.tsbZoomOut.Size = new Size(23, 22);
      this.tsbZoomOut.Text = "Zoom Out";
      this.tsbZoomOut.Click += new EventHandler(this.tsbZoomOut_Click);
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new Size(6, 25);
      this.tsbLinear.Checked = true;
      this.tsbLinear.CheckState = CheckState.Checked;
      this.tsbLinear.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbLinear.Image = (Image) componentResourceManager.GetObject("tsbLinear.Image");
      this.tsbLinear.ImageTransparentColor = Color.Magenta;
      this.tsbLinear.Name = "tsbLinear";
      this.tsbLinear.Size = new Size(23, 22);
      this.tsbLinear.Text = "Linear Scale";
      this.tsbLinear.Click += new EventHandler(this.tsbLinear_Click);
      this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
      this.toolStrip.Items.AddRange(new ToolStripItem[16]
      {
        (ToolStripItem) this.tsbCursor,
        (ToolStripItem) this.tsbCrosshair,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.tsbZoomIn,
        (ToolStripItem) this.tsbZoomOut,
        (ToolStripItem) this.toolStripSeparator2,
        (ToolStripItem) this.tsbLinear,
        (ToolStripItem) this.tsbLog,
        (ToolStripItem) this.toolStripSeparator3,
        (ToolStripItem) this.tsbTrailing,
        (ToolStripItem) this.tsbFixed,
        (ToolStripItem) this.toolStripSeparator4,
        (ToolStripItem) this.tsbCandle,
        (ToolStripItem) this.tsbBar,
        (ToolStripItem) this.tsbLine,
        (ToolStripItem) this.tsbPnF
      });
      this.toolStrip.Location = new Point(0, 0);
      this.toolStrip.Name = "toolStrip";
      this.toolStrip.Size = new Size(302, 25);
      this.toolStrip.TabIndex = 0;
      this.toolStrip.Text = "toolStrip1";
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(6, 25);
      this.tsbCrosshair.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbCrosshair.Image = (Image) componentResourceManager.GetObject("tsbCrosshair.Image");
      this.tsbCrosshair.ImageTransparentColor = Color.Magenta;
      this.tsbCrosshair.Name = "tsbCrosshair";
      this.tsbCrosshair.Size = new Size(23, 22);
      this.tsbCrosshair.Text = "Crosshair";
      this.tsbCrosshair.Click += new EventHandler(this.tsbCrosshair_Click);
      this.tsbLog.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbLog.Image = (Image) componentResourceManager.GetObject("tsbLog.Image");
      this.tsbLog.ImageTransparentColor = Color.Magenta;
      this.tsbLog.Name = "tsbLog";
      this.tsbLog.Size = new Size(23, 22);
      this.tsbLog.Text = "Log Scale";
      this.tsbLog.Click += new EventHandler(this.tsbLog_Click);
      this.tsbBar.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbBar.Image = (Image) componentResourceManager.GetObject("tsbBar.Image");
      this.tsbBar.ImageTransparentColor = Color.Magenta;
      this.tsbBar.Name = "tsbBar";
      this.tsbBar.Size = new Size(23, 22);
      this.tsbBar.Text = "toolStripButton2";
      this.tsbBar.ToolTipText = "Bar";
      this.tsbBar.Click += new EventHandler(this.tsbBar_Click);
      this.tsbCandle.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbCandle.Image = (Image) componentResourceManager.GetObject("tsbCandle.Image");
      this.tsbCandle.ImageTransparentColor = Color.Magenta;
      this.tsbCandle.Name = "tsbCandle";
      this.tsbCandle.Size = new Size(23, 22);
      this.tsbCandle.Text = "toolStripButton1";
      this.tsbCandle.ToolTipText = "Candle";
      this.tsbCandle.Click += new EventHandler(this.tsbCandle_Click);
      this.tsbPnF.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbPnF.Image = (Image) componentResourceManager.GetObject("tsbPnF.Image");
      this.tsbPnF.ImageTransparentColor = Color.Magenta;
      this.tsbPnF.Name = "tsbPnF";
      this.tsbPnF.Size = new Size(23, 22);
      this.tsbPnF.Text = "toolStripButton4";
      this.tsbPnF.ToolTipText = "Point & Figure";
      this.tsbPnF.Click += new EventHandler(this.tsbPnF_Click);
      this.tsbLine.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbLine.Image = (Image) componentResourceManager.GetObject("tsbLine.Image");
      this.tsbLine.ImageTransparentColor = Color.Magenta;
      this.tsbLine.Name = "tsbLine";
      this.tsbLine.Size = new Size(23, 22);
      this.tsbLine.Text = "toolStripButton3";
      this.tsbLine.ToolTipText = "Line";
      this.tsbLine.Click += new EventHandler(this.tsbLine_Click);
      this.tsbTrailing.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbTrailing.Image = (Image) componentResourceManager.GetObject("tsbTrailing.Image");
      this.tsbTrailing.ImageTransparentColor = Color.Magenta;
      this.tsbTrailing.Name = "tsbTrailing";
      this.tsbTrailing.Size = new Size(23, 22);
      this.tsbTrailing.Text = "toolStripButton1";
      this.tsbTrailing.ToolTipText = "Trailing Mode";
      this.tsbTrailing.Click += new EventHandler(this.tsbTrailing_Click);
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new Size(6, 25);
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new Size(6, 25);
      this.tsbFixed.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbFixed.Image = (Image) componentResourceManager.GetObject("tsbFixed.Image");
      this.tsbFixed.ImageTransparentColor = Color.Magenta;
      this.tsbFixed.Name = "tsbFixed";
      this.tsbFixed.Size = new Size(23, 22);
      this.tsbFixed.Text = "toolStripButton2";
      this.tsbFixed.ToolTipText = "Fixed Mode";
      this.tsbFixed.Click += new EventHandler(this.tsbFixed_Click);
      this.tsbCursor.Checked = true;
      this.tsbCursor.CheckState = CheckState.Checked;
      this.tsbCursor.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbCursor.Image = (Image) componentResourceManager.GetObject("tsbCursor.Image");
      this.tsbCursor.ImageTransparentColor = Color.Magenta;
      this.tsbCursor.Name = "tsbCursor";
      this.tsbCursor.Size = new Size(23, 22);
      this.tsbCursor.ToolTipText = "Cursor";
      this.tsbCursor.Click += new EventHandler(this.tsbCursor_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.toolStrip);
      this.Name = "ChartToolStrip";
      this.Size = new Size(302, 26);
      this.toolStrip.ResumeLayout(false);
      this.toolStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
    #endif
}
