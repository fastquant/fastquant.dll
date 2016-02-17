
using System.Windows.Forms;

namespace SmartQuant.Controls.Data.Import.Historical
{
  internal class ImportTaskViewItem : ListViewItem
  {
    public ImportTask Task { get; private set; }

    public ImportTaskViewItem(ImportTask task)
      : base(new string[4])
    {
      this.Task = task;
      this.SubItems[0].Text = task.Instrument.Symbol;
      this.Update();
    }

    public void Update()
    {
      string str = string.Empty;
      switch (this.Task.State)
      {
        case ImportTaskState.Processing:
          str = this.Task.TotalNum <= 0 ? string.Format("{0:n0}", (object) this.Task.Count) : string.Format("{0:n0} of {1:n0}", (object) this.Task.Count, (object) this.Task.TotalNum);
          break;
        case ImportTaskState.Completed:
        case ImportTaskState.Cancelled:
        case ImportTaskState.Error:
          str = string.Format("{0:n0}", (object) this.Task.Count);
          break;
      }
      this.SubItems[1].Text = this.Task.State.ToString();
      this.SubItems[2].Text = str;
      this.SubItems[3].Text = this.Task.Message;
    }
  }
}
