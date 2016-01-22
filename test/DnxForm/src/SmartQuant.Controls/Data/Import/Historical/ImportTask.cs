// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.Import.Historical.ImportTask
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

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
