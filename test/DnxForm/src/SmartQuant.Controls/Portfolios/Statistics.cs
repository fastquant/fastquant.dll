using SmartQuant;
using SmartQuant.Charting;
using SmartQuant.Controls;
using SmartQuant.FinChart;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
    public class StatisticsViewItem : ListViewItem
    {
        public PortfolioStatisticsItem Statistics { get; private set; }

        public StatisticsViewItem(PortfolioStatisticsItem statistics)
          : base(new string[4])
        {
            this.Statistics = statistics;
            this.UseItemStyleForSubItems = false;
            this.SubItems[0].Text = this.Statistics.Name;
            this.Update();
        }

        public void Update()
        {
            this.UpdateSubItem(this.Statistics.TotalValue, 1);
            this.UpdateSubItem(this.Statistics.LongValues, 2);
            this.UpdateSubItem(this.Statistics.ShortValues, 3);
        }

        private void UpdateSubItem(double value, int index)
        {
            if (this.Statistics.Type == 42 || this.Statistics.Type == 43)
                this.SubItems[index].Text = this.DateTimeValueToString((long)value);
            else
                this.SubItems[index].Text = value.ToString(this.Statistics.Format);
            if (value < 0.0)
                this.SubItems[index].ForeColor = Color.Red;
            else
                this.SubItems[index].ForeColor = Color.Black;
        }

        private void UpdateSubItem(TimeSeries values, int index)
        {
            this.UpdateSubItem(values.Count > 0 ? values.Last : 0.0, index);
            if (!(this.Statistics.Category == "Daily / Annual returns") || values.Count != 0)
                return;
            this.SubItems[index].Text = string.Empty;
        }

        private string DateTimeValueToString(long value)
        {
            string str = "";
            long ticks = value;
            if (ticks <= 0L)
                return "";
            TimeSpan timeSpan = new TimeSpan(ticks);
            if (timeSpan.Days != 0)
                str = str + timeSpan.Days.ToString() + " Days ";
            if (timeSpan.Hours != 0)
                str = str + timeSpan.Hours.ToString() + " Hours ";
            if (str.Length >= 15)
                return str.Substring(0, str.Length - 1);
            if (timeSpan.Minutes != 0)
                str = str + timeSpan.Minutes.ToString() + " Minutes ";
            if (str.Length >= 17)
                return str.Substring(0, str.Length - 1);
            if (timeSpan.Minutes != 0)
                str = str + timeSpan.Minutes.ToString() + " Seconds ";
            if (str.Length == 0)
                return str;
            return str.Substring(0, str.Length - 1);
        }
    }
    public class Statistics : UserControl
  {
    private string name;
    private SmartQuant.Portfolio portfolio;
    private PortfolioStatisticsItem selectedItem;
    private IContainer components;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader1;
    private ListViewNB ltvStatistics;
    private SmartQuant.FinChart.Chart chrtStatistics;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private TabControl tcStatistiscs;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TabPage tabPage3;
    private Splitter splitter1;
    private ListViewNB ltvData;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private SmartQuant.Charting.Chart chrtHist;

    public Statistics()
    {
      this.InitializeComponent();
    }

    public void OnInit(string name)
    {
      this.name = name;
      this.portfolio = Framework.Current.PortfolioManager[name];
      if (this.portfolio == null)
        return;
      this.ltvStatistics.BeginUpdate();
      this.ltvStatistics.Groups.Clear();
      this.ltvStatistics.Items.Clear();
      for (int index = 0; index < this.portfolio.Statistics.Items.Count; ++index)
      {
        PortfolioStatisticsItem statistics = this.portfolio.Statistics.Items[index];
        if (statistics.Show)
        {
          if (this.ltvStatistics.Groups[statistics.Category] == null)
            this.ltvStatistics.Groups.Add(statistics.Category, statistics.Category);
          StatisticsViewItem statisticsViewItem = new StatisticsViewItem(statistics);
          statisticsViewItem.Group = this.ltvStatistics.Groups[statistics.Category];
          this.ltvStatistics.Items.Add((ListViewItem) statisticsViewItem);
        }
      }
      this.ltvStatistics.EndUpdate();
    }

    public void UpdateGUI()
    {
      if (this.portfolio == null)
        this.OnInit(this.name);
      this.portfolio = Framework.Current.PortfolioManager[this.name];
      if (this.portfolio == null)
        return;
      this.ltvStatistics.BeginUpdate();
      foreach (ListViewItem listViewItem in this.ltvStatistics.Items)
      {
        StatisticsViewItem statisticsViewItem = listViewItem as StatisticsViewItem;
        if (statisticsViewItem != null)
          statisticsViewItem.Update();
      }
      this.ltvStatistics.EndUpdate();
    }

    private void Reset()
    {
      this.chrtStatistics.Reset();
      this.chrtStatistics.SetMainSeries((ISeries) this.selectedItem.TotalValues, false, Color.White);
      SmartQuant.Charting.Chart.Pad = this.chrtHist.Pads[0];
      SmartQuant.Charting.Chart.Pad.Clear();
      SmartQuant.Charting.Chart.Pad.Title.Items.Clear();
      SmartQuant.Charting.Chart.Pad.Legend.Items.Clear();
      TimeSeries totalValues = this.selectedItem.TotalValues;
      if (totalValues.Count > 1)
      {
        double min = totalValues.GetMin();
        double max = totalValues.GetMax();
        int NBins = 20;
        Histogram histogram = new Histogram(totalValues.Name, NBins, min, max);
        for (int index = 0; index < totalValues.Count; ++index)
          histogram.Add(totalValues[index]);
        histogram.Draw();
      }
      SmartQuant.Charting.Chart.Pad.Update();
      this.ltvData.Items.Clear();
      for (int index = 0; index < this.selectedItem.TotalValues.Count; ++index)
        this.ltvData.Items.Add(new ListViewItem(new string[2]
        {
          this.selectedItem.TotalValues.GetDateTime(index).ToString(),
          this.selectedItem.Type == 42 || this.selectedItem.Type == 43 ? this.DateTimeValueToString((long) this.selectedItem.TotalValues[index]) : this.selectedItem.TotalValues[index].ToString(this.selectedItem.Format)
        }));
    }

    private void ltvStatistics_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.ltvStatistics.SelectedIndices.Count <= 0)
        return;
      StatisticsViewItem statisticsViewItem = this.ltvStatistics.Items[this.ltvStatistics.SelectedIndices[0]] as StatisticsViewItem;
      if (statisticsViewItem == null)
        return;
      this.selectedItem = statisticsViewItem.Statistics;
      this.Invoke((Action) (() => this.Reset()));
    }

    private string DateTimeValueToString(long value)
    {
      string str = "";
      long ticks = value;
      if (ticks <= 0L)
        return "";
      TimeSpan timeSpan = new TimeSpan(ticks);
      if (timeSpan.Days != 0)
        str = str + timeSpan.Days.ToString() + " Days ";
      if (timeSpan.Hours != 0)
        str = str + timeSpan.Hours.ToString() + " Hours ";
      if (str.Length >= 15)
        return str.Substring(0, str.Length - 1);
      if (timeSpan.Minutes != 0)
        str = str + timeSpan.Minutes.ToString() + " Minutes ";
      if (str.Length >= 17)
        return str.Substring(0, str.Length - 1);
      if (timeSpan.Minutes != 0)
        str = str + timeSpan.Minutes.ToString() + " Seconds ";
      if (str.Length == 0)
        return str;
      return str.Substring(0, str.Length - 1);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader1 = new ColumnHeader();
      this.ltvStatistics = new ListViewNB();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.tcStatistiscs = new TabControl();
      this.tabPage1 = new TabPage();
      this.chrtStatistics = new SmartQuant.FinChart.Chart();
      this.tabPage2 = new TabPage();
      this.chrtHist = new SmartQuant.Charting.Chart();
      this.tabPage3 = new TabPage();
      this.ltvData = new ListViewNB();
      this.columnHeader5 = new ColumnHeader();
      this.columnHeader6 = new ColumnHeader();
      this.splitter1 = new Splitter();
      this.tcStatistiscs.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.SuspendLayout();
      this.columnHeader2.Text = "All Trades";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 140;
      this.columnHeader1.Text = "Name";
      this.columnHeader1.Width = 200;
      this.ltvStatistics.Columns.AddRange(new ColumnHeader[4]
      {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4
      });
      this.ltvStatistics.Dock = DockStyle.Fill;
      this.ltvStatistics.Location = new Point(0, 0);
      this.ltvStatistics.Name = "ltvStatistics";
      this.ltvStatistics.Size = new Size(526, 163);
      this.ltvStatistics.TabIndex = 1;
      this.ltvStatistics.UseCompatibleStateImageBehavior = false;
      this.ltvStatistics.View = View.Details;
      this.ltvStatistics.SelectedIndexChanged += new EventHandler(this.ltvStatistics_SelectedIndexChanged);
      this.columnHeader3.Text = "Long Trades";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 140;
      this.columnHeader4.Text = "Short Trades";
      this.columnHeader4.TextAlign = HorizontalAlignment.Right;
      this.columnHeader4.Width = 140;
      this.tcStatistiscs.Controls.Add((Control) this.tabPage1);
      this.tcStatistiscs.Controls.Add((Control) this.tabPage2);
      this.tcStatistiscs.Controls.Add((Control) this.tabPage3);
      this.tcStatistiscs.Dock = DockStyle.Bottom;
      this.tcStatistiscs.Location = new Point(0, 166);
      this.tcStatistiscs.Multiline = true;
      this.tcStatistiscs.Name = "tcStatistiscs";
      this.tcStatistiscs.SelectedIndex = 0;
      this.tcStatistiscs.Size = new Size(526, 203);
      this.tcStatistiscs.TabIndex = 4;
      this.tabPage1.Controls.Add((Control) this.chrtStatistics);
      this.tabPage1.Location = new Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(518, 177);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Chart";
      this.tabPage1.UseVisualStyleBackColor = true;
      this.chrtStatistics.ActionType = ChartActionType.None;
      this.chrtStatistics.AutoScroll = true;
      this.chrtStatistics.BarSeriesStyle = BSStyle.Candle;
      this.chrtStatistics.BorderColor = Color.Gray;
      this.chrtStatistics.BottomAxisGridColor = Color.LightGray;
      this.chrtStatistics.BottomAxisLabelColor = Color.LightGray;
      this.chrtStatistics.CanvasColor = Color.MidnightBlue;
      this.chrtStatistics.ChartBackColor = Color.MidnightBlue;
      this.chrtStatistics.ContextMenuEnabled = true;
      this.chrtStatistics.CrossColor = Color.DarkGray;
      this.chrtStatistics.DateTipRectangleColor = Color.LightGray;
      this.chrtStatistics.DateTipTextColor = Color.Black;
      this.chrtStatistics.Dock = DockStyle.Fill;
      this.chrtStatistics.DrawItems = false;
      this.chrtStatistics.Font = new Font("Microsoft Sans Serif", 7f, FontStyle.Regular, GraphicsUnit.Point, (byte) 204);
      this.chrtStatistics.ItemTextColor = Color.LightGray;
      this.chrtStatistics.LabelDigitsCount = 2;
      this.chrtStatistics.Location = new Point(3, 3);
      this.chrtStatistics.MinNumberOfBars = 125;
      this.chrtStatistics.Name = "chrtStatistics";
      this.chrtStatistics.PrimitiveDeleteImage = (Image) null;
      this.chrtStatistics.PrimitivePropertiesImage = (Image) null;
      this.chrtStatistics.RightAxesFontSize = 7;
      this.chrtStatistics.RightAxisGridColor = Color.DimGray;
      this.chrtStatistics.RightAxisMajorTicksColor = Color.LightGray;
      this.chrtStatistics.RightAxisMinorTicksColor = Color.LightGray;
      this.chrtStatistics.RightAxisTextColor = Color.LightGray;
      this.chrtStatistics.ScaleStyle = PadScaleStyle.Arith;
      this.chrtStatistics.SelectedFillHighlightColor = Color.FromArgb(100, 173, 216, 230);
      this.chrtStatistics.SelectedItemTextColor = Color.Yellow;
      this.chrtStatistics.SessionEnd = TimeSpan.Parse("00:00:00");
      this.chrtStatistics.SessionGridColor = Color.Empty;
      this.chrtStatistics.SessionGridEnabled = false;
      this.chrtStatistics.SessionStart = TimeSpan.Parse("00:00:00");
      this.chrtStatistics.Size = new Size(512, 171);
      this.chrtStatistics.SmoothingMode = SmoothingMode.Default;
      this.chrtStatistics.SplitterColor = Color.LightGray;
      this.chrtStatistics.TabIndex = 2;
      this.chrtStatistics.UpdateStyle = ChartUpdateStyle.WholeRange;
      this.chrtStatistics.ValTipRectangleColor = Color.LightGray;
      this.chrtStatistics.ValTipTextColor = Color.Black;
      this.chrtStatistics.VolumePadVisible = false;
      this.tabPage2.Controls.Add((Control) this.chrtHist);
      this.tabPage2.Location = new Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new Padding(3);
      this.tabPage2.Size = new Size(518, 177);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Histogram";
      this.tabPage2.UseVisualStyleBackColor = true;
      this.chrtHist.AntiAliasingEnabled = false;
      this.chrtHist.Dock = DockStyle.Fill;
      this.chrtHist.DoubleBufferingEnabled = true;
      this.chrtHist.FileName = (string) null;
      this.chrtHist.GroupLeftMarginEnabled = false;
      this.chrtHist.GroupRightMarginEnabled = false;
      this.chrtHist.GroupZoomEnabled = false;
      this.chrtHist.Location = new Point(3, 3);
      this.chrtHist.Name = "chrtHist";
      this.chrtHist.PadsForeColor = Color.White;
      this.chrtHist.PrintAlign = EPrintAlign.None;
      this.chrtHist.PrintHeight = 400;
      this.chrtHist.PrintLayout = EPrintLayout.Portrait;
      this.chrtHist.PrintWidth = 600;
      this.chrtHist.PrintX = 10;
      this.chrtHist.PrintY = 10;
      this.chrtHist.SessionEnd = TimeSpan.Parse("1.00:00:00");
      this.chrtHist.SessionGridColor = Color.Blue;
      this.chrtHist.SessionGridEnabled = false;
      this.chrtHist.SessionStart = TimeSpan.Parse("00:00:00");
      this.chrtHist.Size = new Size(512, 171);
      this.chrtHist.SmoothingEnabled = false;
      this.chrtHist.TabIndex = 0;
      this.chrtHist.TransformationType = ETransformationType.Empty;
      this.tabPage3.Controls.Add((Control) this.ltvData);
      this.tabPage3.Location = new Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new Size(518, 177);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Data";
      this.tabPage3.UseVisualStyleBackColor = true;
      this.ltvData.Columns.AddRange(new ColumnHeader[2]
      {
        this.columnHeader5,
        this.columnHeader6
      });
      this.ltvData.Dock = DockStyle.Fill;
      this.ltvData.Location = new Point(0, 0);
      this.ltvData.Name = "ltvData";
      this.ltvData.Size = new Size(518, 177);
      this.ltvData.TabIndex = 0;
      this.ltvData.UseCompatibleStateImageBehavior = false;
      this.ltvData.View = View.Details;
      this.columnHeader5.Text = "Date Tme";
      this.columnHeader5.Width = 150;
      this.columnHeader6.Text = "Total Value";
      this.columnHeader6.TextAlign = HorizontalAlignment.Right;
      this.columnHeader6.Width = 120;
      this.splitter1.Dock = DockStyle.Bottom;
      this.splitter1.Location = new Point(0, 163);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new Size(526, 3);
      this.splitter1.TabIndex = 5;
      this.splitter1.TabStop = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.ltvStatistics);
      this.Controls.Add((Control) this.splitter1);
      this.Controls.Add((Control) this.tcStatistiscs);
      this.Name = "Statistics";
      this.Size = new Size(526, 369);
      this.tcStatistiscs.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
