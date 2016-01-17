using System.Windows.Forms;

namespace SmartQuant.Controls
{
  public class ListViewNB : ListView
  {
    public ListViewNB()
    {
      this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
      this.SetStyle(ControlStyles.EnableNotifyMessage, true);
    }

    protected override void OnNotifyMessage(System.Windows.Forms.Message m)
    {
      if (m.Msg == 20)
        return;
      base.OnNotifyMessage(m);
    }
  }
}
