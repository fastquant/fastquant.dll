using SmartQuant;
using SmartQuant.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
    class ChildrenStatisticsViewItem : ListViewItem
    {
        private const string NetProfit = "Net Profit";
        private const string GrossProfit = "Gross Profit";
        private const string GrossLoss = "Gross Loss";
        private const string AvgDrawdown = "Average Drawdown %";
        private const string MaxDrawdown = "Maximum Drawdown %";
        private const string ProfitFactor = "Profit Factor";
        private const string RecoveryFactor = "Recovery Factor";

        public SmartQuant.Portfolio Portfolio { get; private set; }

        public ChildrenStatisticsViewItem(SmartQuant.Portfolio portfolio)
          : base(new string[8])
        {
            this.Portfolio = portfolio;
            this.SubItems[0].Text = this.Portfolio.Name;
            this.UseItemStyleForSubItems = false;
            this.Update();
        }

        public void Update()
        {
            for (int index = 0; index < this.Portfolio.Statistics.Items.Count; ++index)
            {
                PortfolioStatisticsItem statistics = this.Portfolio.Statistics.Items[index];
                switch (statistics.Name)
                {
                    case "Net Profit":
                        this.UpdateSubItem(1, statistics);
                        break;
                    case "Gross Profit":
                        this.UpdateSubItem(2, statistics);
                        break;
                    case "Gross Loss":
                        this.UpdateSubItem(3, statistics);
                        break;
                    case "Average Drawdown %":
                        this.UpdateSubItem(4, statistics);
                        break;
                    case "Maximum Drawdown %":
                        this.UpdateSubItem(5, statistics);
                        break;
                    case "Profit Factor":
                        this.UpdateSubItem(6, statistics);
                        break;
                    case "Recovery Factor":
                        this.UpdateSubItem(7, statistics);
                        break;
                }
            }
        }

        private void UpdateSubItem(int index, PortfolioStatisticsItem statistics)
        {
            this.SubItems[index].Text = statistics.TotalValue.ToString(statistics.Format);
            if (statistics.TotalValue < 0.0)
                this.SubItems[index].ForeColor = Color.Red;
            else
                this.SubItems[index].ForeColor = Color.Black;
        }
    }

    public class ChildrenStatistics : UserControl
  {
    private string name;
    private SmartQuant.Portfolio portfolio;
    private Dictionary<string, int> indexes;
    private IContainer components;
    private ListViewNB ltvChldStatistics;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;
    private ColumnHeader columnHeader6;
    private ColumnHeader columnHeader7;
    private ColumnHeader columnHeader8;

    public ChildrenStatistics()
    {
      this.InitializeComponent();
    }

    public void OnInit(string name)
    {
      this.name = name;
      this.portfolio = Framework.Current.PortfolioManager[name];
      this.indexes = new Dictionary<string, int>();
      if (this.portfolio == null)
        return;
      this.ltvChldStatistics.BeginUpdate();
      this.ltvChldStatistics.Items.Clear();
      for (int index = 0; index < this.portfolio.Children.Count; ++index)
        this.ltvChldStatistics.Items.Add((ListViewItem) new ChildrenStatisticsViewItem(this.portfolio.Children[index]));
      this.ltvChldStatistics.EndUpdate();
    }

    public void UpdateGUI()
    {
      if (this.portfolio == null)
        this.OnInit(this.name);
      this.portfolio = Framework.Current.PortfolioManager[this.name];
      if (this.portfolio == null)
        return;
      this.ltvChldStatistics.BeginUpdate();
      foreach (ListViewItem listViewItem in this.ltvChldStatistics.Items)
      {
        ChildrenStatisticsViewItem statisticsViewItem = listViewItem as ChildrenStatisticsViewItem;
        if (statisticsViewItem != null)
          statisticsViewItem.Update();
      }
      this.ltvChldStatistics.EndUpdate();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.ltvChldStatistics = new ListViewNB();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.columnHeader5 = new ColumnHeader();
      this.columnHeader6 = new ColumnHeader();
      this.columnHeader7 = new ColumnHeader();
      this.columnHeader8 = new ColumnHeader();
      this.SuspendLayout();
      this.ltvChldStatistics.Columns.AddRange(new ColumnHeader[8]
      {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4,
        this.columnHeader5,
        this.columnHeader6,
        this.columnHeader7,
        this.columnHeader8
      });
      this.ltvChldStatistics.Dock = DockStyle.Fill;
      this.ltvChldStatistics.Location = new Point(0, 0);
      this.ltvChldStatistics.Name = "ltvChldStatistics";
      this.ltvChldStatistics.Size = new Size(851, 386);
      this.ltvChldStatistics.TabIndex = 0;
      this.ltvChldStatistics.UseCompatibleStateImageBehavior = false;
      this.ltvChldStatistics.View = View.Details;
      this.columnHeader1.Text = "Portfolio";
      this.columnHeader1.Width = 140;
      this.columnHeader2.Text = "Net Profit";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 80;
      this.columnHeader3.Text = "Gross Profit";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 80;
      this.columnHeader4.Text = "Gross Loss";
      this.columnHeader4.TextAlign = HorizontalAlignment.Right;
      this.columnHeader4.Width = 80;
      this.columnHeader5.Text = "Avg Drawdown %";
      this.columnHeader5.TextAlign = HorizontalAlignment.Right;
      this.columnHeader5.Width = 100;
      this.columnHeader6.Text = "Max Drawdown %";
      this.columnHeader6.TextAlign = HorizontalAlignment.Right;
      this.columnHeader6.Width = 110;
      this.columnHeader7.Text = "Profit Factor";
      this.columnHeader7.TextAlign = HorizontalAlignment.Right;
      this.columnHeader7.Width = 90;
      this.columnHeader8.Text = "Recovery Factor";
      this.columnHeader8.TextAlign = HorizontalAlignment.Right;
      this.columnHeader8.Width = 100;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.ltvChldStatistics);
      this.Name = "ChildrenStatistics";
      this.Size = new Size(851, 386);
      this.ResumeLayout(false);
    }
  }
}
