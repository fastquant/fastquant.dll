// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataSeriesViewItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class DataSeriesViewItem : ListViewItem
  {
    private long objectsCount = -1;
    private DataSeries dataSeries;

    public DataSeries DataSeries
    {
      get
      {
        return this.dataSeries;
      }
    }

    public byte DataType { get; private set; }

    public SmartQuant.BarType? BarType { get; private set; }

    public long? BarSize { get; private set; }

    public DataSeriesViewItem(DataSeries dataSeries, byte dataType, SmartQuant.BarType? barType = null, long? barSize = null)
      : base(new string[4], 0)
    {
      this.dataSeries = dataSeries;
      this.DataType = dataType;
      this.BarType = barType;
      this.BarSize = barSize;
      this.Update();
    }

    public void Update()
    {
      if (this.dataSeries.Count == this.objectsCount)
        return;
      this.objectsCount = this.dataSeries.Count;
      this.SubItems[0].Text = DataTypeConverter.Convert(this.DataType, this.BarType, this.BarSize);
      this.SubItems[1].Text = this.dataSeries.Count.ToString("n0");
      if (this.dataSeries.Count > 0L)
      {
        this.SubItems[2].Text = this.dataSeries.DateTime1.ToString();
        this.SubItems[3].Text = this.dataSeries.DateTime2.ToString();
      }
      else
      {
        this.SubItems[2].Text = "-";
        this.SubItems[3].Text = "-";
      }
    }
  }
}
