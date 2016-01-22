// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataTypeItemComparer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System.Collections.Generic;

namespace SmartQuant.Controls.Data
{
  internal class DataTypeItemComparer : IComparer<DataTypeItem>
  {
    private Dictionary<byte, int> sortOrders;

    public DataTypeItemComparer()
    {
      this.sortOrders = new Dictionary<byte, int>();
      for (int index = 0; index <= (int) byte.MaxValue; ++index)
        this.sortOrders.Add((byte) index, index);
    }

    public int Compare(DataTypeItem x, DataTypeItem y)
    {
      if ((int) x.DataType != 6 || (int) y.DataType != 6)
        return this.sortOrders[x.DataType].CompareTo(this.sortOrders[y.DataType]);
      if (x.BarType.Value == y.BarType.Value)
        return x.BarSize.Value.CompareTo(y.BarSize.Value);
      return this.sortOrders[(byte) x.BarType.Value].CompareTo(this.sortOrders[(byte) y.BarType.Value]);
    }
  }
}
