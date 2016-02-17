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
