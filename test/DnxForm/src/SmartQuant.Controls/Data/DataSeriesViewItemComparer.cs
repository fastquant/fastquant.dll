// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataSeriesViewItemComparer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System;
using System.Collections;

namespace SmartQuant.Controls.Data
{
  internal class DataSeriesViewItemComparer : IComparer
  {
    private int[] sortOrders;
    private int column;

    public DataSeriesViewItemComparer()
    {
      this.sortOrders = new int[4]
      {
        1,
        -1,
        -1,
        -1
      };
      this.column = 0;
    }

    public void SortByColumn(int column)
    {
      this.column = column;
      this.sortOrders[column] *= -1;
    }

    public int Compare(object x, object y)
    {
      InstrumentDataSeries series1 = ((InstrumentDataSeriesViewItem) x).Series;
      InstrumentDataSeries series2 = ((InstrumentDataSeriesViewItem) y).Series;
      int num = 0;
      switch (this.column)
      {
        case 0:
          num = string.Compare(series1.Instrument.Symbol, series2.Instrument.Symbol);
          break;
        case 1:
          num = series1.DataSeries.Count.CompareTo(series2.DataSeries.Count);
          break;
        case 2:
          num = DateTime.Compare(series1.DataSeries.Count == 0L ? DateTime.MinValue : series1.DataSeries.DateTime1, series2.DataSeries.Count == 0L ? DateTime.MinValue : series2.DataSeries.DateTime1);
          break;
        case 3:
          num = DateTime.Compare(series1.DataSeries.Count == 0L ? DateTime.MinValue : series1.DataSeries.DateTime1, series2.DataSeries.Count == 0L ? DateTime.MinValue : series2.DataSeries.DateTime1);
          break;
      }
      return num * this.sortOrders[this.column];
    }
  }
}
