// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.CustomBarSeriesMenuItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class CustomBarSeriesMenuItem : BarSeriesMenuItem
  {
    public override bool CreateSeries
    {
      get
      {
        NewBarSeriesForm newBarSeriesForm = new NewBarSeriesForm();
        bool flag;
        if (newBarSeriesForm.ShowDialog() == DialogResult.OK)
        {
          this.barType = newBarSeriesForm.BarType;
          this.barSize = newBarSeriesForm.BarSize;
          flag = true;
        }
        else
          flag = false;
        newBarSeriesForm.Dispose();
        return flag;
      }
    }

    public CustomBarSeriesMenuItem()
    {
      this.Text = "Custom...";
    }
  }
}
