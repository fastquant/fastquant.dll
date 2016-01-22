// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.TickEditor
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
  internal class TickEditor : DataObjectEditor
  {
    private byte tickTypeId;
    private NumericUpDown nudDirection;
    private IContainer components;
    private NumericUpDown nudPrice;
    private Label label3;
    private Label label2;
    private NumericUpDown nudSize;
    private NumericUpDown nudProvider;
    private Label label4;

    protected override string ObjectName
    {
      get
      {
        switch (this.tickTypeId)
        {
          case 2:
            return "Bid";
          case 3:
            return "Ask";
          case 4:
            return "Trade";
          default:
            return this.tickTypeId.ToString();
        }
      }
    }

    public TickEditor()
    {
      this.InitializeComponent();
      this.tickTypeId = (byte) 1;
      this.SetNumericUpDownRange<byte>(this.nudProvider);
      this.SetNumericUpDownRange<double>(this.nudPrice);
      this.SetNumericUpDownRange<int>(this.nudSize);
      DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
      string str = string.Format("{0} {1}", (object) currentInfo.ShortDatePattern, (object) currentInfo.LongTimePattern);
      if (str.Contains(".fff"))
        str = str.Replace(".fff", "");
      this.dtpDateTime.CustomFormat = str;
    }

    public void SetTickTypeId(byte tickTypeId)
    {
      this.tickTypeId = tickTypeId;
      if ((int) tickTypeId != 4)
        return;
      this.Height += 24;
      Label label = new Label();
      label.Location = new Point(16, 112);
      label.Size = new Size(58, 20);
      label.TextAlign = ContentAlignment.MiddleLeft;
      label.Text = "Direction";
      this.groupBox1.Controls.Add((Control) label);
      this.nudDirection = new NumericUpDown();
      this.nudDirection.Location = new Point(86, 112);
      this.nudDirection.Size = new Size(90, 20);
      this.nudDirection.TextAlign = HorizontalAlignment.Right;
      this.SetNumericUpDownRange<sbyte>(this.nudDirection);
      this.nudDirection.Value = new Decimal(-1);
      this.groupBox1.Controls.Add((Control) this.nudDirection);
    }

    public override SmartQuant.DataObject GetDataObject()
    {
      Tick tick = (Tick) null;
      switch (this.tickTypeId)
      {
        case 2:
          tick = (Tick) new Bid();
          break;
        case 3:
          tick = (Tick) new Ask();
          break;
        case 4:
          tick = (Tick) new Trade();
          break;
      }
      tick.DateTime = this.dtpDateTime.Value;
      tick.InstrumentId = this.instrumentId;
      tick.ProviderId = (byte) this.nudProvider.Value;
      tick.Price = (double) this.nudPrice.Value;
      tick.Size = (int) this.nudSize.Value;
      if ((int) this.tickTypeId == 4)
        ((Trade) tick).Direction = (sbyte) this.nudDirection.Value;
      return (SmartQuant.DataObject) tick;
    }

    protected override void OnInit(SmartQuant.DataObject dataObject, int decimalPlaces)
    {
      if (dataObject != null)
      {
        Tick tick = (Tick) dataObject;
        this.nudProvider.Value = (Decimal) tick.ProviderId;
        this.nudPrice.Value = (Decimal) tick.Price;
        this.nudSize.Value = (Decimal) tick.Size;
        if ((int) this.tickTypeId == 4)
          this.nudDirection.Value = (Decimal) ((Trade) tick).Direction;
      }
      this.nudPrice.DecimalPlaces = decimalPlaces;
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
      this.nudPrice = new NumericUpDown();
      this.nudSize = new NumericUpDown();
      this.label4 = new Label();
      this.nudProvider = new NumericUpDown();
      this.groupBox1.SuspendLayout();
      this.nudPrice.BeginInit();
      this.nudSize.BeginInit();
      this.nudProvider.BeginInit();
      this.SuspendLayout();
      this.label1.Size = new Size(64, 20);
      this.dtpDateTime.Format = DateTimePickerFormat.Custom;
      this.dtpDateTime.Size = new Size(151, 20);
      this.groupBox1.Controls.Add((Control) this.nudProvider);
      this.groupBox1.Controls.Add((Control) this.label4);
      this.groupBox1.Controls.Add((Control) this.nudSize);
      this.groupBox1.Controls.Add((Control) this.nudPrice);
      this.groupBox1.Controls.Add((Control) this.label3);
      this.groupBox1.Controls.Add((Control) this.label2);
      this.groupBox1.Size = new Size((int) byte.MaxValue, 126);
      this.groupBox1.Controls.SetChildIndex((Control) this.label2, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label1, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.dtpDateTime, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label3, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudPrice, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudSize, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label4, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudProvider, 0);
      this.label2.Location = new Point(16, 64);
      this.label2.Name = "label2";
      this.label2.Size = new Size(58, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "Price";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      this.label3.Location = new Point(16, 88);
      this.label3.Name = "label3";
      this.label3.Size = new Size(58, 20);
      this.label3.TabIndex = 3;
      this.label3.Text = "Size";
      this.label3.TextAlign = ContentAlignment.MiddleLeft;
      this.nudPrice.Location = new Point(86, 64);
      this.nudPrice.Name = "nudPrice";
      this.nudPrice.Size = new Size(90, 20);
      this.nudPrice.TabIndex = 4;
      this.nudPrice.TextAlign = HorizontalAlignment.Right;
      this.nudPrice.ThousandsSeparator = true;
      this.nudSize.Location = new Point(86, 88);
      this.nudSize.Name = "nudSize";
      this.nudSize.Size = new Size(90, 20);
      this.nudSize.TabIndex = 5;
      this.nudSize.TextAlign = HorizontalAlignment.Right;
      this.nudSize.ThousandsSeparator = true;
      this.label4.Location = new Point(16, 40);
      this.label4.Name = "label4";
      this.label4.Size = new Size(58, 20);
      this.label4.TabIndex = 6;
      this.label4.Text = "Provider";
      this.label4.TextAlign = ContentAlignment.MiddleLeft;
      this.nudProvider.Location = new Point(86, 40);
      this.nudProvider.Name = "nudProvider";
      this.nudProvider.Size = new Size(90, 20);
      this.nudProvider.TabIndex = 7;
      this.nudProvider.TextAlign = HorizontalAlignment.Right;
      this.nudProvider.ThousandsSeparator = true;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.ClientSize = new Size(263, 166);
      this.Name = "TickEditor";
      this.groupBox1.ResumeLayout(false);
      this.nudPrice.EndInit();
      this.nudSize.EndInit();
      this.nudProvider.EndInit();
      this.ResumeLayout(false);
    }
  }
}
