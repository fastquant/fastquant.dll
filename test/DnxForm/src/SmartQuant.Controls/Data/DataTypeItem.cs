// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataTypeItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

namespace SmartQuant.Controls.Data
{
  internal class DataTypeItem
  {
    public byte DataType { get; private set; }

    public SmartQuant.BarType? BarType { get; private set; }

    public long? BarSize { get; private set; }

    public DataTypeItem(byte dataType, SmartQuant.BarType? barType = null, long? barSize = null)
    {
      this.DataType = dataType;
      this.BarType = barType;
      this.BarSize = barSize;
    }

    public override string ToString()
    {
      return DataTypeConverter.Convert(this.DataType, this.BarType, this.BarSize);
    }
  }
}
