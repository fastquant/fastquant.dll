// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.TickViewer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class TickViewer : DataObjectViewer
  {
    private byte tickTypeId;
    private string dateTimeFormat;

    public TickViewer(byte tickTypeId)
    {
      this.tickTypeId = tickTypeId;
    }

    protected override ColumnHeader[] GetCustomColumnHeaders()
    {
      List<ColumnHeader> list = new List<ColumnHeader>();
      list.Add(this.CreateColumnHeader("DateTime", 144, HorizontalAlignment.Left));
      list.Add(this.CreateColumnHeader("Provider", 64, HorizontalAlignment.Right));
      list.Add(this.CreateColumnHeader("Price", 64, HorizontalAlignment.Right));
      list.Add(this.CreateColumnHeader("Size", 64, HorizontalAlignment.Right));
      if ((int) this.tickTypeId == 4)
        list.Add(this.CreateColumnHeader("Direction", 64, HorizontalAlignment.Right));
      return list.ToArray();
    }

    protected override string[] GetCustomSubItems(int index)
    {
      List<string> list = new List<string>();
      Tick tick = (Tick) this.dataSeries[(long) index];
      if (tick == null)
      {
        list.Add("-");
        list.Add("-");
        list.Add("-");
        list.Add("-");
        if ((int) this.tickTypeId == 4)
          list.Add("-");
      }
      else
      {
        list.Add(tick.DateTime.ToString(this.dateTimeFormat));
        list.Add(tick.ProviderId.ToString());
        list.Add(tick.Price.ToString(this.priceFormat));
        list.Add(tick.Size.ToString("n0"));
        if ((int) this.tickTypeId == 4)
          list.Add(((Trade) tick).Direction.ToString());
      }
      return list.ToArray();
    }

    public override DataObjectEditor GetEditor()
    {
      TickEditor tickEditor = new TickEditor();
      tickEditor.SetTickTypeId(this.tickTypeId);
      return (DataObjectEditor) tickEditor;
    }
  }
}
