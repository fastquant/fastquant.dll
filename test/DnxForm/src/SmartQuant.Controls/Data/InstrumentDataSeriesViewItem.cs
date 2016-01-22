// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.InstrumentDataSeriesViewItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class InstrumentDataSeriesViewItem : ListViewItem
  {
    private InstrumentDataSeries series;

    public InstrumentDataSeries Series
    {
      get
      {
        return this.series;
      }
    }

    public InstrumentDataSeriesViewItem(InstrumentDataSeries series)
      : base(new string[4], 0)
    {
      this.series = series;
      this.UpdateValues();
    }

    public void UpdateValues()
    {
      this.SubItems[0].Text = this.series.Instrument.Symbol;
      this.SubItems[1].Text = this.series.DataSeries.Count.ToString("n0");
      if (this.series.DataSeries.Count > 0L)
      {
        this.SubItems[2].Text = this.series.DataSeries.DateTime1.ToString();
        this.SubItems[3].Text = this.series.DataSeries.DateTime2.ToString();
      }
      else
      {
        this.SubItems[2].Text = "-";
        this.SubItems[3].Text = "-";
      }
    }
  }
}
