// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.BarSeriesMenuItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class BarSeriesMenuItem : ToolStripMenuItem
  {
    protected BarType barType;
    protected long barSize;

    public BarType BarType
    {
      get
      {
        return this.barType;
      }
    }

    public long BarSize
    {
      get
      {
        return this.barSize;
      }
    }

    public virtual bool CreateSeries
    {
      get
      {
        return true;
      }
    }

    public BarSeriesMenuItem(BarType barType, long barSize)
    {
      this.barType = barType;
      this.barSize = barSize;
      this.Text = DataTypeConverter.Convert((byte) 6, new BarType?(barType), new long?(barSize));
    }

    protected BarSeriesMenuItem()
    {
    }
  }
}
