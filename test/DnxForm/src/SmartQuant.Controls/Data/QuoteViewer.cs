// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.QuoteViewer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class QuoteViewer : DataObjectViewer
  {
    private string dateTimeFormat;

    protected override ColumnHeader[] GetCustomColumnHeaders()
    {
      return new List<ColumnHeader>()
      {
        this.CreateColumnHeader("DateTime", 128, HorizontalAlignment.Right),
        this.CreateColumnHeader("BidSize", 64, HorizontalAlignment.Right),
        this.CreateColumnHeader("Bid", 64, HorizontalAlignment.Right),
        this.CreateColumnHeader("Ask", 64, HorizontalAlignment.Right),
        this.CreateColumnHeader("AskSize", 64, HorizontalAlignment.Right)
      }.ToArray();
    }

    protected override string[] GetCustomSubItems(int index)
    {
      Quote quote = (Quote) this.dataSeries[(long) index];
      return new string[5]
      {
        quote.DateTime.ToString(this.dateTimeFormat),
        quote.Bid.Size.ToString("n0"),
        quote.Bid.Price.ToString(this.priceFormat),
        quote.Ask.Price.ToString(this.priceFormat),
        quote.Ask.Size.ToString("n0")
      };
    }

    public override DataObjectEditor GetEditor()
    {
      return (DataObjectEditor) new QuoteEditor();
    }
  }
}
