// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Portfolios.PerformanceWindow
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using SmartQuant.FinChart;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
  public class PerformanceWindow : UserControl
  {
    private string name;
    private SmartQuant.Portfolio portfolio;
    private IContainer components;
    private Chart chart;

    public SmartQuant.Portfolio Portfolio
    {
      get
      {
        return this.portfolio;
      }
    }

    public PerformanceWindow()
    {
      this.InitializeComponent();
    }

    public void OnInit(string name)
    {
      this.name = name;
      this.Reset();
    }

    public void UpdateGUI()
    {
      this.Invoke((Action) (() => this.Reset()));
    }

    private void Reset()
    {
      this.portfolio = Framework.Current.PortfolioManager.Portfolios[this.name];
      if (this.portfolio == null)
        return;
      PortfolioPerformance performance = this.portfolio.Performance;
      this.chart.Reset();
      this.chart.SetMainSeries((ISeries) performance.EquitySeries, false, Color.White);
      this.chart.AddPad();
      this.chart.DrawSeries(performance.DrawdownSeries, 2, Color.White, SimpleDSStyle.Line, SearchOption.ExactFirst, SmoothingMode.HighSpeed);
      this.chart.UpdateStyle = ChartUpdateStyle.WholeRange;
      performance.Updated += new EventHandler(this.performance_Updated);
    }

    private void performance_Updated(object sender, EventArgs e)
    {
      this.chart.OnItemAdded();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.chart = new Chart();
      this.SuspendLayout();
      this.chart.ActionType = ChartActionType.Cross;
      this.chart.AllowDrop = true;
      this.chart.AutoScroll = true;
      this.chart.BackColor = Color.MidnightBlue;
      this.chart.BarSeriesStyle = BSStyle.Candle;
      this.chart.BorderColor = Color.Gray;
      this.chart.BottomAxisGridColor = Color.LightGray;
      this.chart.BottomAxisLabelColor = Color.LightGray;
      this.chart.CanvasColor = Color.MidnightBlue;
      this.chart.ChartBackColor = Color.MidnightBlue;
      this.chart.ContextMenuEnabled = false;
      this.chart.CrossColor = Color.DarkGray;
      this.chart.DateTipRectangleColor = Color.LightGray;
      this.chart.DateTipTextColor = Color.Black;
      this.chart.Dock = DockStyle.Fill;
      this.chart.DrawItems = false;
      this.chart.Font = new Font("Microsoft Sans Serif", 7f, FontStyle.Regular, GraphicsUnit.Point, (byte) 204);
      this.chart.ItemTextColor = Color.LightGray;
      this.chart.LabelDigitsCount = 2;
      this.chart.Location = new Point(0, 0);
      this.chart.MinNumberOfBars = 125;
      this.chart.Name = "chart";
      this.chart.PrimitiveDeleteImage = (Image) null;
      this.chart.PrimitivePropertiesImage = (Image) null;
      this.chart.RightAxesFontSize = 7;
      this.chart.RightAxisGridColor = Color.DimGray;
      this.chart.RightAxisMajorTicksColor = Color.LightGray;
      this.chart.RightAxisMinorTicksColor = Color.LightGray;
      this.chart.RightAxisTextColor = Color.LightGray;
      this.chart.ScaleStyle = PadScaleStyle.Arith;
      this.chart.SelectedFillHighlightColor = Color.FromArgb(100, 173, 216, 230);
      this.chart.SelectedItemTextColor = Color.Yellow;
      this.chart.SessionEnd = TimeSpan.Parse("00:00:00");
      this.chart.SessionGridColor = Color.Empty;
      this.chart.SessionGridEnabled = false;
      this.chart.SessionStart = TimeSpan.Parse("00:00:00");
      this.chart.Size = new Size(931, 432);
      this.chart.SmoothingMode = SmoothingMode.HighSpeed;
      this.chart.SplitterColor = Color.LightGray;
      this.chart.TabIndex = 0;
      this.chart.UpdateStyle = ChartUpdateStyle.WholeRange;
      this.chart.ValTipRectangleColor = Color.LightGray;
      this.chart.ValTipTextColor = Color.Black;
      this.chart.VolumePadVisible = false;
      this.Controls.Add((Control) this.chart);
      this.Name = "PerformanceWindow";
      this.Size = new Size(931, 432);
      this.ResumeLayout(false);
    }
  }
}
