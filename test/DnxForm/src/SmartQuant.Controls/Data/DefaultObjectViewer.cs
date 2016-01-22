// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DefaultObjectViewer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class DefaultObjectViewer : DataObjectViewer
  {
    private string dateTimeFormat;

    protected override ColumnHeader[] GetCustomColumnHeaders()
    {
      return new ColumnHeader[2]
      {
        this.CreateColumnHeader("DateTime", 144, HorizontalAlignment.Left),
        this.CreateColumnHeader("", 400, HorizontalAlignment.Left)
      };
    }

    protected override string[] GetCustomSubItems(int index)
    {
      SmartQuant.DataObject dataObject = this.dataSeries[(long) index];
      if (dataObject == null)
        return new string[2]
        {
          "-",
          "-"
        };
      return new string[2]
      {
        dataObject.DateTime.ToString(this.dateTimeFormat),
        dataObject.ToString()
      };
    }

    public override DataObjectEditor GetEditor()
    {
      return (DataObjectEditor) null;
    }
  }
}
