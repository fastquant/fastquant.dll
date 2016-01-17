// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Orders.ExecutionReportViewItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Windows.Forms;

namespace SmartQuant.Controls.Orders
{
  internal class ExecutionReportViewItem : ListViewItem
  {
    private ExecutionReport report;

    internal ExecutionReport Report
    {
      get
      {
        return this.report;
      }
    }

    internal ExecutionReportViewItem(ExecutionReport report)
      : base(new string[14])
    {
      this.report = report;
      Order order = report.Order;
      this.ImageIndex = 1;
      string priceFormat = report.Instrument.PriceFormat;
      this.SubItems[0].Text = report.DateTime.ToString();
      this.SubItems[1].Text = "";
      this.SubItems[2].Text = report.ExecType.ToString();
      this.SubItems[3].Text = report.OrdStatus.ToString();
      this.SubItems[4].Text = report.Side.ToString();
      this.SubItems[5].Text = report.OrdType.ToString();
      this.SubItems[6].Text = report.Price.ToString(priceFormat);
      this.SubItems[7].Text = report.StopPx.ToString(priceFormat);
      this.SubItems[8].Text = report.OrdQty.ToString();
      this.SubItems[9].Text = report.CumQty.ToString();
      this.SubItems[10].Text = report.LeavesQty.ToString();
      this.SubItems[11].Text = report.LastQty.ToString();
      this.SubItems[12].Text = report.LastPx.ToString();
      this.SubItems[13].Text = report.Text;
    }
  }
}
