// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Portfolios.CorrelationMatrix
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using SmartQuant.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
    public class CorrelationMatrixViewItem : ListViewItem
    {
        private List<TimeSeries> series;

        public CorrelationMatrixViewItem(string name, List<TimeSeries> series)
          : base(new string[series.Count + 1])
        {
            this.series = series;
            this.SubItems[0].Text = name;
            this.UseItemStyleForSubItems = false;
            this.Update();
        }

        public void Update()
        {
            if (this.series.Count <= 0)
                return;
            bool flag = false;
            TimeSeries timeSeries1 = this.series[0];
            for (int index = 1; index < this.series.Count && !flag; ++index)
            {
                TimeSeries timeSeries2 = timeSeries1;
                TimeSeries timeSeries3 = this.series[index];
                int num = Math.Min(timeSeries2.Count, timeSeries3.Count);
                while (timeSeries2.Count != timeSeries3.Count)
                {
                    if (timeSeries2.Count > num)
                    {
                        timeSeries2 = this.GetEqualSeries(timeSeries2, timeSeries3);
                        num = Math.Min(timeSeries2.Count, timeSeries3.Count);
                    }
                    if (timeSeries3.Count > num)
                    {
                        timeSeries3 = this.GetEqualSeries(timeSeries3, timeSeries2);
                        num = Math.Min(timeSeries2.Count, timeSeries3.Count);
                    }
                }
                double correlation = this.GetCorrelation(timeSeries2, timeSeries3);
                this.SubItems[index].Text = correlation.ToString("F2");
                this.SubItems[index].BackColor = double.IsNaN(correlation) ? this.GetColor(0.0) : this.GetColor(correlation);
                if (this.series[index] == timeSeries1)
                    flag = true;
            }
        }

        private double GetAvg(TimeSeries values)
        {
            double num = 0.0;
            if (values.Count > 0)
            {
                for (int index = 0; index < values.Count; ++index)
                    num += values[index];
                num /= (double)values.Count;
            }
            return num;
        }

        private double GetStdDev(TimeSeries values)
        {
            double num = 0.0;
            if (values.Count > 1)
            {
                double avg = this.GetAvg(values);
                for (int index = 0; index < values.Count; ++index)
                    num += Math.Pow(values[index] - avg, 2.0);
                num = Math.Sqrt(num / (double)(values.Count - 1));
            }
            return num;
        }

        private double GetCovariance(TimeSeries values1, TimeSeries values2)
        {
            double num1 = 0.0;
            int num2 = Math.Min(values1.Count, values2.Count);
            if (num2 > 1)
            {
                double avg1 = this.GetAvg(values1);
                double avg2 = this.GetAvg(values2);
                for (int index = num2 - 1; index >= 0; --index)
                    num1 += (values1[index] - avg1) * (values2[index] - avg2);
                num1 /= (double)(num2 - 1);
            }
            return num1;
        }

        private double GetCorrelation(TimeSeries values1, TimeSeries values2)
        {
            double num = 0.0;
            if (Math.Min(values1.Count, values2.Count) > 1)
                num = this.GetCovariance(values1, values2) / (this.GetStdDev(values1) * this.GetStdDev(values2));
            return num;
        }

        private TimeSeries GetEqualSeries(TimeSeries values1, TimeSeries values2)
        {
            TimeSeries timeSeries = new TimeSeries();
            for (int index = 0; index < values2.Count; ++index)
            {
                DateTime dateTime = values1.GetDateTime(index);
                if (values2.Contains(dateTime))
                    timeSeries.Add(dateTime, values1[index]);
            }
            return timeSeries;
        }

        private Color GetColor(double correlation)
        {
            double num1 = Math.Floor((correlation + 1.0) * 60.0 - 1.0);
            double num2 = 0.5;
            double num3 = 1.0;
            int num4 = Convert.ToInt32(Math.Floor(num1 / 60.0)) % 6;
            double num5 = num1 / 60.0 - Math.Floor(num1 / 60.0);
            double num6 = num3 * (double)byte.MaxValue;
            int num7 = Convert.ToInt32(num6);
            int num8 = Convert.ToInt32(num6 * (1.0 - num2));
            int num9 = Convert.ToInt32(num6 * (1.0 - num5 * num2));
            int num10 = Convert.ToInt32(num6 * (1.0 - (1.0 - num5) * num2));
            if (num4 == 0)
                return Color.FromArgb((int)byte.MaxValue, num7, num10, num8);
            if (num4 == 1)
                return Color.FromArgb((int)byte.MaxValue, num9, num7, num8);
            if (num4 == 2)
                return Color.FromArgb((int)byte.MaxValue, num8, num7, num10);
            if (num4 == 3)
                return Color.FromArgb((int)byte.MaxValue, num8, num9, num7);
            if (num4 == 4)
                return Color.FromArgb((int)byte.MaxValue, num10, num8, num7);
            return Color.FromArgb((int)byte.MaxValue, num7, num8, num9);
        }
    }

    public class CorrelationMatrix : UserControl
  {
    private string name;
    private SmartQuant.Portfolio portfolio;
    private Dictionary<string, int> indexes;
    private string statisticsName;
    private IContainer components;
    private ComboBox cmbChooseStatistics;
    private Label label1;
    private ListViewNB ltvMatrix;
    private Panel panel1;

    public CorrelationMatrix()
    {
      this.InitializeComponent();
    }

    public void OnInit(string name)
    {
      this.name = name;
      if (Framework.Current.PortfolioManager[name] != null)
        this.portfolio = Framework.Current.PortfolioManager[name];
      this.cmbChooseStatistics.Items.Clear();
      this.cmbChooseStatistics.Items.Add((object) "Daily Return %");
      this.cmbChooseStatistics.Items.Add((object) "Drawdown %");
      this.cmbChooseStatistics.SelectedIndex = 0;
      this.UpdateGUI();
    }

    public void UpdateGUI()
    {
      if (Framework.Current.PortfolioManager[this.name] != null)
        this.portfolio = Framework.Current.PortfolioManager[this.name];
      this.indexes = new Dictionary<string, int>();
      if (this.portfolio == null)
        return;
      this.ltvMatrix.BeginUpdate();
      this.ltvMatrix.Items.Clear();
      this.ltvMatrix.Columns.Clear();
      int count = this.portfolio.Children.Count;
      if (count > 0)
      {
        string[] items = new string[count + 1];
        List<List<TimeSeries>> list1 = new List<List<TimeSeries>>();
        for (int index1 = 0; index1 < count; ++index1)
        {
          items[index1 + 1] = this.portfolio.Children[index1].Name;
          List<TimeSeries> list2 = new List<TimeSeries>();
          list2.Add(this.GetTimeSeries(this.portfolio.Children[index1].Statistics));
          for (int index2 = 0; index2 < count; ++index2)
            list2.Add(this.GetTimeSeries(this.portfolio.Children[index2].Statistics));
          list1.Add(list2);
        }
        for (int index = 0; index < items.Length; ++index)
        {
          this.ltvMatrix.Columns.Add("");
          this.ltvMatrix.Columns[index].Width = 125;
        }
        this.ltvMatrix.Items.Add(new ListViewItem(items));
        for (int index = 0; index < list1.Count; ++index)
          this.ltvMatrix.Items.Add((ListViewItem) new CorrelationMatrixViewItem(items[index + 1], list1[index]));
      }
      this.ltvMatrix.EndUpdate();
    }

    private TimeSeries GetTimeSeries(PortfolioStatistics portfolioStatistics)
    {
      foreach (PortfolioStatisticsItem portfolioStatisticsItem in portfolioStatistics.Items)
      {
        if (portfolioStatisticsItem.Name == this.statisticsName)
          return portfolioStatisticsItem.TotalValues;
      }
      return (TimeSeries) null;
    }

    private void cmbChooseStatistics_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.statisticsName = this.cmbChooseStatistics.Items[this.cmbChooseStatistics.SelectedIndex].ToString();
      this.UpdateGUI();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.cmbChooseStatistics = new ComboBox();
      this.label1 = new Label();
      this.ltvMatrix = new ListViewNB();
      this.panel1 = new Panel();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      this.cmbChooseStatistics.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbChooseStatistics.FormattingEnabled = true;
      this.cmbChooseStatistics.Location = new Point(109, 13);
      this.cmbChooseStatistics.Name = "cmbChooseStatistics";
      this.cmbChooseStatistics.Size = new Size(251, 21);
      this.cmbChooseStatistics.TabIndex = 0;
      this.cmbChooseStatistics.SelectedIndexChanged += new EventHandler(this.cmbChooseStatistics_SelectedIndexChanged);
      this.label1.AutoSize = true;
      this.label1.Location = new Point(14, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(89, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Choose statistics:";
      this.ltvMatrix.Dock = DockStyle.Fill;
      this.ltvMatrix.Location = new Point(0, 43);
      this.ltvMatrix.Name = "ltvMatrix";
      this.ltvMatrix.Size = new Size(699, 446);
      this.ltvMatrix.TabIndex = 2;
      this.ltvMatrix.UseCompatibleStateImageBehavior = false;
      this.ltvMatrix.View = View.Details;
      this.panel1.Controls.Add((Control) this.cmbChooseStatistics);
      this.panel1.Controls.Add((Control) this.label1);
      this.panel1.Dock = DockStyle.Top;
      this.panel1.Location = new Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(699, 43);
      this.panel1.TabIndex = 4;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.ltvMatrix);
      this.Controls.Add((Control) this.panel1);
      this.Name = "CorrelationMatrix";
      this.Size = new Size(699, 489);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
    }
  }
}
