using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
    internal class BarSeriesMenuItem : ToolStripMenuItem
    {
        protected BarType barType;
        protected long barSize;

        public BarType BarType => this.barType;

        public long BarSize => this.barSize;

        public virtual bool CreateSeries => true;

        public BarSeriesMenuItem(BarType barType, long barSize)
        {
            this.barType = barType;
            this.barSize = barSize;
            Text = DataTypeConverter.Convert(DataObjectType.Bar, barType, barSize);
        }

        protected BarSeriesMenuItem()
        {
        }
    }
}
