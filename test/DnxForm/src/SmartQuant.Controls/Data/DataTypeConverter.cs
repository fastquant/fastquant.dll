// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataTypeConverter
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;

namespace SmartQuant.Controls.Data
{
  public static class DataTypeConverter
  {
    public static string Convert(byte dataType, BarType? barType = null, long? barSize = null)
    {
      switch (dataType)
      {
        case 2:
          return "Bid";
        case 3:
          return "Ask";
        case 4:
          return "Trade";
        case 5:
          return "Quote";
        case 6:
          if (!barType.HasValue)
            return "Bar";
          if (!barSize.HasValue)
            return string.Format("Bar {0}", (object) barType.Value);
          return string.Format("Bar {0} {1}", (object) barType.Value, (object) barSize.Value);
        default:
          return string.Format("DataType #{0}", (object) dataType);
      }
    }

    public static string Convert(DataSeries series)
    {
      byte dataType = DataSeriesNameHelper.GetDataType(series);
      if ((int) dataType != 6)
        return DataTypeConverter.Convert(dataType, new BarType?(), new long?());
      BarType barType;
      long barSize;
      if (DataSeriesNameHelper.TryGetBarTypeSize(series, out barType, out barSize))
        return DataTypeConverter.Convert(dataType, new BarType?(barType), new long?(barSize));
      return string.Empty;
    }
  }
}
