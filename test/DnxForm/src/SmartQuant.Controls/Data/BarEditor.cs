using SmartQuant;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class BarEditor : DataObjectEditor
  {
    private BarType barType;
    private long barSize;
    private IContainer components;
    private Label label7;
    private Label label6;
    private Label label5;
    private Label label4;
    private Label label3;
    private Label label2;
    private TextBox tbxBarType;
    private NumericUpDown nudOpen;
    private NumericUpDown nudVolume;
    private NumericUpDown nudClose;
    private NumericUpDown nudLow;
    private NumericUpDown nudHigh;
    private DateTimePicker dtpEnd;
    private Label label8;
    private NumericUpDown nudOpenInt;
    private Label label9;

    protected override string ObjectName
    {
      get
      {
        return "Bar";
      }
    }

    public BarEditor()
    {
      this.InitializeComponent();
      this.SetNumericUpDownRange<double>(this.nudOpen);
      this.SetNumericUpDownRange<double>(this.nudHigh);
      this.SetNumericUpDownRange<double>(this.nudLow);
      this.SetNumericUpDownRange<double>(this.nudClose);
      this.SetNumericUpDownRange<long>(this.nudVolume);
      this.SetNumericUpDownRange<long>(this.nudOpenInt);
      DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
      string str = string.Format("{0} {1}", (object) currentInfo.ShortDatePattern, (object) currentInfo.LongTimePattern);
      if (str.Contains(".fff"))
        str = str.Replace(".fff", "");
      this.dtpDateTime.CustomFormat = str;
      this.dtpEnd.CustomFormat = str;
    }

    public void InitBarSettings(BarType barType, long barSize)
    {
      this.barType = barType;
      this.barSize = barSize;
    }

    protected override void OnInit(SmartQuant.DataObject dataObject, int decimalPlaces)
    {
      if (dataObject != null)
      {
        Bar bar = (Bar) dataObject;
        this.dtpDateTime.Value = bar.OpenDateTime;
        this.dtpEnd.Value = bar.CloseDateTime;
        this.nudOpen.Value = (Decimal) bar.Open;
        this.nudHigh.Value = (Decimal) bar.High;
        this.nudLow.Value = (Decimal) bar.Low;
        this.nudClose.Value = (Decimal) bar.Close;
        this.nudVolume.Value = (Decimal) bar.Volume;
        this.nudOpenInt.Value = (Decimal) bar.OpenInt;
        this.dtpDateTime.Enabled = false;
        this.dtpEnd.Enabled = false;
      }
      else
      {
        DateTime now = DateTime.Now;
        this.dtpDateTime.Value = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        this.dtpDateTime.Enabled = true;
        if (this.barType == BarType.Time)
        {
          this.dtpEnd.Value = this.dtpDateTime.Value.AddSeconds((double) this.barSize);
          this.dtpEnd.Enabled = false;
        }
        else
        {
          this.dtpEnd.Value = this.dtpDateTime.Value;
          this.dtpEnd.Enabled = true;
        }
      }
      this.tbxBarType.Text = string.Format("{0} {1}", (object) this.barType, (object) this.barSize);
      this.nudOpen.DecimalPlaces = decimalPlaces;
      this.nudHigh.DecimalPlaces = decimalPlaces;
      this.nudLow.DecimalPlaces = decimalPlaces;
      this.nudClose.DecimalPlaces = decimalPlaces;
    }

    public override SmartQuant.DataObject GetDataObject()
    {
      return (SmartQuant.DataObject) new Bar(this.dtpDateTime.Value, this.dtpEnd.Value, this.instrumentId, this.barType, this.barSize, (double) this.nudOpen.Value, (double) this.nudHigh.Value, (double) this.nudLow.Value, (double) this.nudClose.Value, (long) this.nudVolume.Value, (long) this.nudOpenInt.Value);
    }

    private void dtpDateTime_ValueChanged(object sender, EventArgs e)
    {
      if (this.barType != BarType.Time)
        return;
      this.dtpEnd.Value = this.dtpDateTime.Value.AddSeconds((double) this.barSize);
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
      this.label6 = new Label();
      this.label7 = new Label();
      this.tbxBarType = new TextBox();
      this.nudOpen = new NumericUpDown();
      this.nudHigh = new NumericUpDown();
      this.nudLow = new NumericUpDown();
      this.nudClose = new NumericUpDown();
      this.nudVolume = new NumericUpDown();
      this.label8 = new Label();
      this.dtpEnd = new DateTimePicker();
      this.label9 = new Label();
      this.nudOpenInt = new NumericUpDown();
      this.groupBox1.SuspendLayout();
      this.nudOpen.BeginInit();
      this.nudHigh.BeginInit();
      this.nudLow.BeginInit();
      this.nudClose.BeginInit();
      this.nudVolume.BeginInit();
      this.nudOpenInt.BeginInit();
      this.SuspendLayout();
      this.label1.Text = "Begin";
      this.dtpDateTime.Format = DateTimePickerFormat.Custom;
      this.dtpDateTime.Location = new Point(88, 16);
      this.dtpDateTime.Size = new Size(160, 20);
      this.dtpDateTime.ValueChanged += new EventHandler(this.dtpDateTime_ValueChanged);
      this.btnOk.Location = new Point(76, 8);
      this.groupBox1.Controls.Add((Control) this.nudOpenInt);
      this.groupBox1.Controls.Add((Control) this.label9);
      this.groupBox1.Controls.Add((Control) this.dtpEnd);
      this.groupBox1.Controls.Add((Control) this.label8);
      this.groupBox1.Controls.Add((Control) this.nudVolume);
      this.groupBox1.Controls.Add((Control) this.nudClose);
      this.groupBox1.Controls.Add((Control) this.nudLow);
      this.groupBox1.Controls.Add((Control) this.nudHigh);
      this.groupBox1.Controls.Add((Control) this.nudOpen);
      this.groupBox1.Controls.Add((Control) this.tbxBarType);
      this.groupBox1.Controls.Add((Control) this.label7);
      this.groupBox1.Controls.Add((Control) this.label6);
      this.groupBox1.Controls.Add((Control) this.label5);
      this.groupBox1.Controls.Add((Control) this.label4);
      this.groupBox1.Controls.Add((Control) this.label3);
      this.groupBox1.Controls.Add((Control) this.label2);
      this.groupBox1.Size = new Size(263, 249);
      this.groupBox1.Controls.SetChildIndex((Control) this.label2, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label1, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.dtpDateTime, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label3, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label4, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label5, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label6, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label7, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.tbxBarType, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudOpen, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudHigh, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudLow, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudClose, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudVolume, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label8, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.dtpEnd, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.label9, 0);
      this.groupBox1.Controls.SetChildIndex((Control) this.nudOpenInt, 0);
      this.label2.Location = new Point(16, 72);
      this.label2.Name = "label2";
      this.label2.Size = new Size(58, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "Bar Type";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      this.label3.Location = new Point(16, 96);
      this.label3.Name = "label3";
      this.label3.Size = new Size(47, 20);
      this.label3.TabIndex = 3;
      this.label3.Text = "Open";
      this.label3.TextAlign = ContentAlignment.MiddleLeft;
      this.label4.Location = new Point(16, 120);
      this.label4.Name = "label4";
      this.label4.Size = new Size(47, 20);
      this.label4.TabIndex = 4;
      this.label4.Text = "High";
      this.label4.TextAlign = ContentAlignment.MiddleLeft;
      this.label5.Location = new Point(16, 144);
      this.label5.Name = "label5";
      this.label5.Size = new Size(47, 20);
      this.label5.TabIndex = 5;
      this.label5.Text = "Low";
      this.label5.TextAlign = ContentAlignment.MiddleLeft;
      this.label6.Location = new Point(16, 168);
      this.label6.Name = "label6";
      this.label6.Size = new Size(47, 20);
      this.label6.TabIndex = 6;
      this.label6.Text = "Close";
      this.label6.TextAlign = ContentAlignment.MiddleLeft;
      this.label7.Location = new Point(16, 192);
      this.label7.Name = "label7";
      this.label7.Size = new Size(47, 20);
      this.label7.TabIndex = 7;
      this.label7.Text = "Volume";
      this.label7.TextAlign = ContentAlignment.MiddleLeft;
      this.tbxBarType.Location = new Point(88, 72);
      this.tbxBarType.Name = "tbxBarType";
      this.tbxBarType.ReadOnly = true;
      this.tbxBarType.Size = new Size(94, 20);
      this.tbxBarType.TabIndex = 8;
      this.nudOpen.Location = new Point(88, 96);
      this.nudOpen.Name = "nudOpen";
      this.nudOpen.Size = new Size(94, 20);
      this.nudOpen.TabIndex = 9;
      this.nudOpen.TextAlign = HorizontalAlignment.Right;
      this.nudOpen.ThousandsSeparator = true;
      this.nudHigh.Location = new Point(88, 120);
      this.nudHigh.Name = "nudHigh";
      this.nudHigh.Size = new Size(94, 20);
      this.nudHigh.TabIndex = 10;
      this.nudHigh.TextAlign = HorizontalAlignment.Right;
      this.nudHigh.ThousandsSeparator = true;
      this.nudLow.Location = new Point(88, 144);
      this.nudLow.Name = "nudLow";
      this.nudLow.Size = new Size(94, 20);
      this.nudLow.TabIndex = 11;
      this.nudLow.TextAlign = HorizontalAlignment.Right;
      this.nudLow.ThousandsSeparator = true;
      this.nudClose.Location = new Point(88, 168);
      this.nudClose.Name = "nudClose";
      this.nudClose.Size = new Size(94, 20);
      this.nudClose.TabIndex = 12;
      this.nudClose.TextAlign = HorizontalAlignment.Right;
      this.nudClose.ThousandsSeparator = true;
      this.nudVolume.Location = new Point(88, 192);
      this.nudVolume.Name = "nudVolume";
      this.nudVolume.Size = new Size(94, 20);
      this.nudVolume.TabIndex = 13;
      this.nudVolume.TextAlign = HorizontalAlignment.Right;
      this.nudVolume.ThousandsSeparator = true;
      this.label8.Location = new Point(16, 40);
      this.label8.Name = "label8";
      this.label8.Size = new Size(58, 20);
      this.label8.TabIndex = 14;
      this.label8.Text = "End";
      this.label8.TextAlign = ContentAlignment.MiddleLeft;
      this.dtpEnd.Format = DateTimePickerFormat.Custom;
      this.dtpEnd.Location = new Point(88, 40);
      this.dtpEnd.Name = "dtpEnd";
      this.dtpEnd.Size = new Size(160, 20);
      this.dtpEnd.TabIndex = 15;
      this.label9.Location = new Point(16, 216);
      this.label9.Name = "label9";
      this.label9.Size = new Size(47, 20);
      this.label9.TabIndex = 16;
      this.label9.Text = "OpenInt";
      this.label9.TextAlign = ContentAlignment.MiddleLeft;
      this.nudOpenInt.Location = new Point(88, 216);
      this.nudOpenInt.Name = "nudOpenInt";
      this.nudOpenInt.Size = new Size(94, 20);
      this.nudOpenInt.TabIndex = 17;
      this.nudOpenInt.TextAlign = HorizontalAlignment.Right;
      this.nudOpenInt.ThousandsSeparator = true;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.ClientSize = new Size(271, 289);
      this.Name = "BarEditor";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.nudOpen.EndInit();
      this.nudHigh.EndInit();
      this.nudLow.EndInit();
      this.nudClose.EndInit();
      this.nudVolume.EndInit();
      this.nudOpenInt.EndInit();
      this.ResumeLayout(false);
    }
  }
}
