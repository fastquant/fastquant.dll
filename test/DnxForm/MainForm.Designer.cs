using SmartQuant.Controls.BarChart;
using SmartQuant.FinChart;
using System.Windows.Forms;

namespace Demo
{
    partial class MainForm
    {
        private void InitComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.barChart = new SmartQuant.Controls.BarChart.BarChart();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.barChart2 = new SmartQuant.Controls.BarChart.BarChart2();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.chart3 = new SmartQuant.FinChart.Chart();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(624, 362);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.barChart);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(616, 336);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Chart";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // barChart
            // 
            this.barChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChart.Location = new System.Drawing.Point(3, 3);
            this.barChart.Name = "barChart";
            this.barChart.Size = new System.Drawing.Size(610, 330);
            this.barChart.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.barChart2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(616, 336);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Chart(Gapless)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // barChart2
            // 
            this.barChart2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barChart2.Location = new System.Drawing.Point(3, 3);
            this.barChart2.Name = "barChart2";
            this.barChart2.Size = new System.Drawing.Size(610, 330);
            this.barChart2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.chart3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(616, 336);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Performance";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // chart3
            // 
            this.chart3.ActionType = ChartActionType.Cross;
            this.chart3.AutoScroll = true;
            this.chart3.AutoSize = true;
            this.chart3.BarSeriesStyle = BSStyle.Candle;
            this.chart3.BorderColor = System.Drawing.Color.Gray;
            this.chart3.BottomAxisGridColor = System.Drawing.Color.LightGray;
            this.chart3.BottomAxisLabelColor = System.Drawing.Color.LightGray;
            this.chart3.CanvasColor = System.Drawing.Color.MidnightBlue;
            this.chart3.ChartBackColor = System.Drawing.Color.MidnightBlue;
            this.chart3.ContextMenuEnabled = true;
            this.chart3.CrossColor = System.Drawing.Color.DarkGray;
            this.chart3.DateTipRectangleColor = System.Drawing.Color.LightGray;
            this.chart3.DateTipTextColor = System.Drawing.Color.Black;
            this.chart3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart3.DrawItems = false;
            this.chart3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chart3.ItemTextColor = System.Drawing.Color.LightGray;
            this.chart3.LabelDigitsCount = 2;
            this.chart3.Location = new System.Drawing.Point(3, 3);
            this.chart3.MinNumberOfBars = 125;
            this.chart3.Name = "chart3";
            this.chart3.PrimitiveDeleteImage = null;
            this.chart3.PrimitivePropertiesImage = null;
            this.chart3.RightAxesFontSize = 7;
            this.chart3.RightAxisGridColor = System.Drawing.Color.DimGray;
            this.chart3.RightAxisMajorTicksColor = System.Drawing.Color.LightGray;
            this.chart3.RightAxisMinorTicksColor = System.Drawing.Color.LightGray;
            this.chart3.RightAxisTextColor = System.Drawing.Color.LightGray;
            this.chart3.ScaleStyle = SmartQuant.FinChart.PadScaleStyle.Arith;
            this.chart3.SelectedFillHighlightColor = System.Drawing.Color.LightBlue;
            this.chart3.SelectedItemTextColor = System.Drawing.Color.Yellow;
            this.chart3.SessionEnd = System.TimeSpan.Parse("00:00:00");
            this.chart3.SessionGridColor = System.Drawing.Color.Empty;
            this.chart3.SessionGridEnabled = false;
            this.chart3.SessionStart = System.TimeSpan.Parse("00:00:00");
            this.chart3.Size = new System.Drawing.Size(610, 330);
            this.chart3.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            this.chart3.SplitterColor = System.Drawing.Color.LightGray;
            this.chart3.TabIndex = 0;
            this.chart3.UpdateStyle = SmartQuant.FinChart.ChartUpdateStyle.Trailing;
            this.chart3.ValTipRectangleColor = System.Drawing.Color.LightGray;
            this.chart3.ValTipTextColor = System.Drawing.Color.Black;
            this.chart3.VolumePadVisible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "SmartQuant Charts Demo";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private BarChart barChart;
        private BarChart2 barChart2;
        private Chart chart3;
        private TabPage tabPage3;
    }
}

