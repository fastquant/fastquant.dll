using SmartQuant;

namespace SmartQuant.Controls.Data.Import.Historical
{
  internal class ImportTask
  {
    public Instrument Instrument { get; private set; }

    public ImportTaskState State { get; set; }

    public int Count { get; set; }

    public int TotalNum { get; set; }

    public string Message { get; set; }

    public ImportTask(Instrument instrument)
    {
      this.Instrument = instrument;
      this.State = ImportTaskState.Pending;
      this.Count = 0;
      this.TotalNum = 0;
      this.Message = string.Empty;
    }
  }
}
