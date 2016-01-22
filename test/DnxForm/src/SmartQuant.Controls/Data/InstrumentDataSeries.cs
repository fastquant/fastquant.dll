// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.InstrumentDataSeries
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;

namespace SmartQuant.Controls.Data
{
  internal class InstrumentDataSeries
  {
    public Instrument Instrument { get; private set; }

    public DataSeries DataSeries { get; private set; }

    public DataTypeItem DataTypeItem { get; private set; }

    public InstrumentDataSeries(Instrument instrument, DataSeries dataSeries, DataTypeItem dataTypeItem)
    {
      this.Instrument = instrument;
      this.DataSeries = dataSeries;
      this.DataTypeItem = dataTypeItem;
    }
  }
}
