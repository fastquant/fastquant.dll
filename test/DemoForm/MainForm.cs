using System;
using System.Windows.Forms;
using SmartQuant;
using System.Threading;
using SmartQuant.Controls;
using System.Drawing;
using SmartQuant.FinChart;
using System.Drawing.Drawing2D;
using SmartQuant.Shared;
using System.ComponentModel;
//using SmartQuant.Controls.Portfolios;

namespace Demo
{
    public partial class MainForm : Form
    {
        private IContainer components;
        private System.Windows.Forms.Timer timer;
        private Portfolio portfolio;
        private DataLoader dataLoader;
        private SmartQuant.Controls.Portfolios.Portfolio portfolioWindow;


        public MainForm()
        {
            InitComponent();
            SuspendLayout();
 
            this.orderManager = new SmartQuant.Controls.Orders.OrderManagerWindow();
            this.orderManager.Dock = DockStyle.Fill;
            this.orderManager.Name = "OrderManager";
            this.accountData = new SmartQuant.Controls.Data.Account.AccountData();
            this.accountData.Dock = DockStyle.Fill;
            this.accountData.Location = new Point(0, 0);
            this.accountData.Name = "Account";

            this.portfolioWindow = new SmartQuant.Controls.Portfolios.Portfolio();
            this.portfolioWindow.Dock = DockStyle.Fill;
            this.portfolioWindow.Name = "portfolio";

            TabPage tpage = null;
            tpage = new TabPage();
            tpage.Controls.Add(this.orderManager);
            tpage.Name = this.orderManager.Name;
            tpage.Text = this.orderManager.Name;
            this.tabControl1.Controls.Add(tpage);
            tpage = new TabPage();
            tpage.Controls.Add(this.accountData);
            tpage.Name = this.accountData.Name;
            tpage.Text = this.accountData.Name;
            this.tabControl1.Controls.Add(tpage);

            tpage = new TabPage();
            tpage.Controls.Add(this.portfolioWindow);
            tpage.Name = this.portfolioWindow.Name;
            tpage.Text = this.portfolioWindow.Name;
            this.tabControl1.Controls.Add(tpage);

            ResumeLayout();

            var f = new Framework("Demo");
            f.IsDisposable = false;
            f.GroupDispatcher = new GroupDispatcher(f);
            this.dataLoader = new DataLoader(f);

            this.barChart.Init(f, null, null);
            this.barChart.ResumeUpdates();

            this.barChart2.Init(f, null, null);
            this.barChart2.ResumeUpdates();

            this.orderManager.Init(this.dataLoader.OrderManagerQueue, true);

            this.timer = new System.Windows.Forms.Timer();
            this.timer.Interval = 500;
            this.timer.Tick += (sender, e) =>
            {
                try
                {
                    this.timer.Stop();
                    this.barChart.UpdateGUI();
                    this.barChart2.UpdateGUI();
                    this.orderManager.UpdateGUI();

                    this.timer.Interval = 500;
                    this.timer.Enabled = true;
                }
                catch (Exception)
                {
                }
            };
            this.timer.Start();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            new Thread(() =>
            {
                var scenario = new Backtest(Framework.Current);
                scenario.Run();
                Reset();
            }).Start();
        }

        private void Reset()
        {
            Invoke((Action)delegate
            {
                this.portfolio = Framework.Current.PortfolioManager.Portfolios.GetByIndex(0);
                if (this.portfolio == null)
                    return;
                var performance = this.portfolio.Performance;
                this.chart3.Reset();
                this.chart3.SetMainSeries(performance.EquitySeries, false, Color.White);
                this.chart3.AddPad();
                this.chart3.DrawSeries(performance.DrawdownSeries, 2, Color.White, SimpleDSStyle.Line, SearchOption.ExactFirst, SmoothingMode.HighSpeed);
                this.chart3.UpdateStyle = ChartUpdateStyle.WholeRange;
                performance.Updated += (sender, e) => this.chart3.OnItemAdded();
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.components?.Dispose();
            base.Dispose(disposing);
        }
    }
}
