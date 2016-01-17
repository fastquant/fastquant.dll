using SmartQuant;
using System.Windows.Forms;

namespace SmartQuant.Controls.Orders
{
  internal class ExecutionCommandViewItem : ListViewItem
  {
    private ExecutionCommand command;

    internal ExecutionCommand Command
    {
      get
      {
        return this.command;
      }
    }

    internal ExecutionCommandViewItem(ExecutionCommand command)
      : base(new string[15])
    {
      this.command = command;
      string priceFormat = this.Command.Instrument.PriceFormat;
      this.ImageIndex = 0;
      this.SubItems[0].Text = command.DateTime.ToString();
      this.SubItems[1].Text = command.Type.ToString();
      this.SubItems[2].Text = "";
      this.SubItems[3].Text = "";
      this.SubItems[4].Text = command.Side.ToString();
      this.SubItems[5].Text = command.OrdType.ToString();
      this.SubItems[6].Text = command.Price.ToString(priceFormat);
      this.SubItems[7].Text = command.StopPx.ToString(priceFormat);
      this.SubItems[8].Text = command.Qty.ToString();
      this.SubItems[9].Text = "";
      this.SubItems[10].Text = "";
      this.SubItems[11].Text = "";
      this.SubItems[12].Text = "";
      this.SubItems[13].Text = "";
      this.SubItems[14].Text = command.Text;
    }
  }
}
