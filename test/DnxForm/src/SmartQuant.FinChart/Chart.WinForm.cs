using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.FinChart
{
    public partial class Chart : UserControl
	{
        private HScrollBar scrollBar;
        internal ToolTip ToolTip;

        private void InitializeComponent()
        {
            this.components = new Container();
            this.scrollBar = new HScrollBar();
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            this.UpdateStyles();
            this.ToolTip = new ToolTip(this.components);
            this.SuspendLayout();
            this.scrollBar.Dock = DockStyle.Bottom;
            this.scrollBar.TabIndex = 0;
            this.scrollBar.Scroll += OnScrollBarScroll;
            this.scrollBar.Minimum = 0;
            this.Controls.Add(this.scrollBar);
            this.MouseDown += OnChartMouseDown;
            this.MouseLeave += OnChartMouseLeave;
            this.MouseUp += OnChartMouseUp;
            this.MouseWheel += OnChartMouseWheel;
            this.ResumeLayout(false);
        }
	}
}

