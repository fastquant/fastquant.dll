// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.Import.Historical.DataTypeItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant.Controls.Data;

namespace SmartQuant.Controls.Data.Import.Historical
{
  internal class DataTypeItem
  {
    public byte DataType { get; private set; }

    public SmartQuant.BarType? BarType { get; private set; }

    public DataTypeItem(byte dataType, SmartQuant.BarType? barType)
    {
      this.DataType = dataType;
      this.BarType = barType;
    }

    public DataTypeItem(byte dataType)
      : this(dataType, new SmartQuant.BarType?())
    {
    }

    public override string ToString()
    {
      return DataTypeConverter.Convert(this.DataType, this.BarType, new long?());
    }
  }
}
