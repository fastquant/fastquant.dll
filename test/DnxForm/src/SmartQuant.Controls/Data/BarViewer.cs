// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.BarViewer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class BarViewer : DataObjectViewer
  {
    protected override ColumnHeader[] GetCustomColumnHeaders()
    {
      return new List<ColumnHeader>()
      {
        this.CreateColumnHeader("Date", 80, HorizontalAlignment.Right),
        this.CreateColumnHeader("Time", 160, HorizontalAlignment.Right),
        this.CreateColumnHeader("Open", 80, HorizontalAlignment.Right),
        this.CreateColumnHeader("High", 80, HorizontalAlignment.Right),
        this.CreateColumnHeader("Low", 80, HorizontalAlignment.Right),
        this.CreateColumnHeader("Close", 80, HorizontalAlignment.Right),
        this.CreateColumnHeader("Volume", 80, HorizontalAlignment.Right),
        this.CreateColumnHeader("OpenInt", 80, HorizontalAlignment.Right)
      }.ToArray();
    }

    protected override string[] GetCustomSubItems(int index)
    {
      Bar bar = (Bar) this.dataSeries[(long) index];
      if (bar == null)
        return new string[8]
        {
          "-",
          "-",
          "-",
          "-",
          "-",
          "-",
          "-",
          "-"
        };
      string str1;
      string str2;
      if (bar.OpenDateTime.Date == bar.CloseDateTime.Date)
      {
        str1 = string.Format("{0:d}", (object) bar.DateTime);
        str2 = string.Format("{0:HH:mm:ss} - {1:HH:mm:ss}", (object) bar.OpenDateTime, (object) bar.CloseDateTime);
      }
      else
      {
        str1 = "";
        str2 = string.Format("{0:d} {0:HH:mm:ss} - {1:d} {1:HH:mm:ss}", (object) bar.OpenDateTime, (object) bar.CloseDateTime);
      }
      return new string[8]
      {
        str1,
        str2,
        bar.Open.ToString(this.priceFormat),
        bar.High.ToString(this.priceFormat),
        bar.Low.ToString(this.priceFormat),
        bar.Close.ToString(this.priceFormat),
        bar.Volume.ToString("n0"),
        bar.OpenInt.ToString("n0")
      };
    }

    public override DataObjectEditor GetEditor()
    {
      BarEditor barEditor = new BarEditor();
      BarType barType;
      long barSize;
      if (!DataSeriesNameHelper.TryGetBarTypeSize(this.dataSeries, out barType, out barSize))
      {
        barType = BarType.Time;
        barSize = 0L;
      }
      barEditor.InitBarSettings(barType, barSize);
      return (DataObjectEditor) barEditor;
    }
  }
}
