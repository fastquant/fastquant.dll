// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.QuoteEditor
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class QuoteEditor : DataObjectEditor
  {
    private IContainer components;
    private NumericUpDown nudAsk;
    private NumericUpDown nudBidSize;
    private NumericUpDown nudBid;
    private Label label5;
    private Label label4;
    private Label label3;
    private Label label2;
    private NumericUpDown nudAskSize;

    protected override string ObjectName
    {
      get
      {
        return "Quote";
      }
    }

    public QuoteEditor()
    {
      this.InitializeComponent();
      this.SetNumericUpDownRange<double>(this.nudBid);
      this.SetNumericUpDownRange<int>(this.nudBidSize);
      this.SetNumericUpDownRange<double>(this.nudAsk);
      this.SetNumericUpDownRange<int>(this.nudAskSize);
      DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
      string str = string.Format("{0} {1}", (object) currentInfo.ShortDatePattern, (object) currentInfo.LongTimePattern);
      if (str.Contains(".fff"))
        str = str.Replace(".fff", "");
      this.dtpDateTime.CustomFormat = str;
    }

    protected override void OnInit(SmartQuant.DataObject dataObject, int decimalPlaces)
    {
      if (dataObject != null)
      {
        Quote quote = (Quote) dataObject;
        this.nudBid.Value = (Decimal) quote.Bid.Price;
        this.nudBidSize.Value = (Decimal) quote.Bid.Size;
        this.nudAsk.Value = (Decimal) quote.Ask.Price;
        this.nudAskSize.Value = (Decimal) quote.Ask.Size;
      }
      this.nudBid.DecimalPlaces = decimalPlaces;
      this.nudAsk.DecimalPlaces = decimalPlaces;
    }

    public override SmartQuant.DataObject GetDataObject()
    {
      return (SmartQuant.DataObject) new Quote(new Bid(this.dtpDateTime.Value, this.providerId, this.instrumentId, (double) this.nudBid.Value, (int) this.nudBidSize.Value), new Ask(this.dtpDateTime.Value, this.providerId, this.instrumentId, (double) this.nudAsk.Value, (int) this.nudAskSize.Value));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.label2 = new Label();
      this.label3 = new Label();
      this.label4 = new Label();
      this.label5 = new Label();
      this.nudBid = new NumericUpDown();
      this.nudBidSize = new NumericUpDown();
      this.nudAsk = new NumericUpDown();
      this.nudAskSize = new NumericUpDown();
      this.groupBox1.SuspendLayout();
      this.nudBid.BeginInit();
      this.nudBidSize.BeginInit();
      this.nudAsk.BeginInit();
      this.nudAskSize.BeginInit();
      this.SuspendLayout();
      this.dtpDateTime.Format = DateTimePickerFormat.Custom;
      this.dtpDateTime.Location = new Point(88, 16);
      this.dtpDateTime.Size = new Size(174, 20);
      this.btnCancel.Location = new Point(148, 8);
      this.btnOk.Location = new Point(86, 8);
      this.groupBox1.Controls.Add((Control) this.nudAskSize);
      this.groupBox1.Controls.Add((Control) this.nudAsk);
      this.groupBox1.Controls.Add((Control) this.nudBidSize);
      this.groupBox1.Controls.Add((Control) this.nudBid);
      this.groupBox1.Controls.Add((Control) this.label5);
      this.groupBox1.Controls.Add((Control) this.label4);
      this.groupBox1.Controls.Add((Control) this.label3);
      this.groupBox1.Controls.Add((Control) this.label2);
      this.groupBox1.Size = new Size(278, 154);
      this.groupBox1.Controls.SetChildIndex((Control) this.label2, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label1, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.dtpDateTime, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label3, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label4, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label5, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudBid, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudBidSize, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudAsk, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudAskSize, 0);
      this.label2.Location = new Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new Size(55, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "Bid";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      this.label3.Location = new Point(16, 72);
      this.label3.Name = "label3";
      this.label3.Size = new Size(55, 20);
      this.label3.TabIndex = 3;
      this.label3.Text = "Bid Size";
      this.label3.TextAlign = ContentAlignment.MiddleLeft;
      this.label4.Location = new Point(16, 96);
      this.label4.Name = "label4";
      this.label4.Size = new Size(55, 20);
      this.label4.TabIndex = 4;
      this.label4.Text = "Ask";
      this.label4.TextAlign = ContentAlignment.MiddleLeft;
      this.label5.Location = new Point(16, 120);
      this.label5.Name = "label5";
      this.label5.Size = new Size(55, 20);
      this.label5.TabIndex = 5;
      this.label5.Text = "Ask Size";
      this.label5.TextAlign = ContentAlignment.MiddleLeft;
      this.nudBid.Location = new Point(88, 48);
      this.nudBid.Name = "nudBid";
      this.nudBid.Size = new Size(84, 20);
      this.nudBid.TabIndex = 6;
      this.nudBid.TextAlign = HorizontalAlignment.Right;
      this.nudBid.ThousandsSeparator = true;
      this.nudBidSize.Location = new Point(88, 72);
      this.nudBidSize.Name = "nudBidSize";
      this.nudBidSize.Size = new Size(84, 20);
      this.nudBidSize.TabIndex = 7;
      this.nudBidSize.TextAlign = HorizontalAlignment.Right;
      this.nudBidSize.ThousandsSeparator = true;
      this.nudAsk.Location = new Point(88, 96);
      this.nudAsk.Name = "nudAsk";
      this.nudAsk.Size = new Size(84, 20);
      this.nudAsk.TabIndex = 8;
      this.nudAsk.TextAlign = HorizontalAlignment.Right;
      this.nudAsk.ThousandsSeparator = true;
      this.nudAskSize.Location = new Point(88, 120);
      this.nudAskSize.Name = "nudAskSize";
      this.nudAskSize.Size = new Size(84, 20);
      this.nudAskSize.TabIndex = 9;
      this.nudAskSize.TextAlign = HorizontalAlignment.Right;
      this.nudAskSize.ThousandsSeparator = true;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.ClientSize = new Size(286, 194);
      this.Name = "QuoteEditor";
      this.groupBox1.ResumeLayout(false);
      this.nudBid.EndInit();
      this.nudBidSize.EndInit();
      this.nudAsk.EndInit();
      this.nudAskSize.EndInit();
      this.ResumeLayout(false);
    }
  }
}
