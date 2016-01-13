using System;
using System.Windows.Forms;
using SmartQuant;
using System.Threading;
using SmartQuant.Controls;
using System.Drawing;
using SmartQuant.FinChart;
using System.Drawing.Drawing2D;

namespace Demo
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer timer;
        private Portfolio portfolio;

        public MainForm()
        {
            InitComponent();
            var f = new Framework("Demo");
            f.IsDisposable = false;
            f.GroupDispatcher = new GroupDispatcher(f);
            this.barChart.Init(f, null, null);
            this.barChart2.Init(f, null, null);
            this.barChart.ResumeUpdates();
            this.barChart2.ResumeUpdates();

            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 500;
            this.timer.Tick += new EventHandler((sender, e) =>
            {
                try
                {
                    this.timer.Stop();
                    barChart.UpdateGUI();
                    barChart2.UpdateGUI();
                    this.timer.Interval = 500;
                    this.timer.Enabled = true;
                }
                catch (Exception)
                {
                }
            });
            this.timer.Start();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            new Thread(new ThreadStart(() =>
            {
                Scenario scenario = new Backtest(Framework.Current);
                scenario.Run();
                Reset();
            })).Start();
        }

        private void Reset()
        {
            Invoke((System.Action)delegate
            {
                this.portfolio = Framework.Current.PortfolioManager.Portfolios.GetByIndex(0);
                if (this.portfolio == null)
                    return;
                PortfolioPerformance performance = this.portfolio.Performance;
                this.chart3.Reset();
                this.chart3.SetMainSeries(performance.EquitySeries, false, Color.White);
                this.chart3.AddPad();
                this.chart3.DrawSeries(performance.DrawdownSeries, 2, Color.White, SimpleDSStyle.Line, SearchOption.ExactFirst, SmoothingMode.HighSpeed);
                this.chart3.UpdateStyle = ChartUpdateStyle.WholeRange;
                performance.Updated += new EventHandler((sender, e) =>
                {
                    this.chart3.OnItemAdedd(this.portfolio.Performance.EquitySeries.LastDateTime);
                });
            });
        }
    }
}
