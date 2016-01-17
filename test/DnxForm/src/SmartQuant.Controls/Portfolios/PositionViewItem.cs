using SmartQuant;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
  public class PositionViewItem : ListViewItem
  {
    public new Position Position { get; private set; }

    public PositionViewItem(Position position)
      : base(new string[4])
    {
      this.Position = position;
      this.SubItems[0].Text = position.Instrument.Symbol;
      this.Update();
    }

    public void Update()
    {
      this.SubItems[1].Text = this.Position.Amount.ToString();
      this.SubItems[2].Text = this.Position.QtyBought.ToString();
      this.SubItems[3].Text = this.Position.QtySold.ToString();
    }
  }
}
