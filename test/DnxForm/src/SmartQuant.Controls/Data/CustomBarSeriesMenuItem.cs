using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
    internal class CustomBarSeriesMenuItem : BarSeriesMenuItem
    {
        public override bool CreateSeries
        {
            get
            {
                var form = new NewBarSeriesForm();
                bool flag;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.barType = form.BarType;
                    this.barSize = form.BarSize;
                    flag = true;
                }
                else
                    flag = false;
                form.Dispose();
                return flag;
            }
        }

        public CustomBarSeriesMenuItem()
        {
            Text = "Custom...";
        }
    }
}
