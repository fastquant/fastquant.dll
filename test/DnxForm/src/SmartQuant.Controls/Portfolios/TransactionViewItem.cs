// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Portfolios.TransactionViewItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
  class TransactionViewItem : ListViewItem
  {
    public Transaction Transaction { get; private set; }

    public TransactionViewItem(Transaction transaction)
      : base(new string[8])
    {
      this.Transaction = transaction;
      this.SubItems[0].Text = transaction.Fills[0].DateTime.ToString();
      this.SubItems[1].Text = transaction.Instrument.Symbol;
      this.SubItems[2].Text = transaction.Side.ToString();
      this.SubItems[3].Text = transaction.Price.ToString(transaction.Instrument.PriceFormat);
      this.SubItems[4].Text = transaction.Qty.ToString();
      this.SubItems[5].Text = transaction.Value.ToString("F2");
      this.SubItems[6].Text = transaction.Commission.ToString();
      this.SubItems[7].Text = transaction.Text;
      if (this.Transaction.Fills.Count > 1)
        this.ImageIndex = 0;
      else
        this.ImageIndex = 2;
      if (this.Transaction.IsDone)
        this.BackColor = Color.FromArgb(220, (int) byte.MaxValue, 220);
      else
        this.BackColor = Color.FromArgb((int) byte.MaxValue, 230, 230);
    }
  }
}
