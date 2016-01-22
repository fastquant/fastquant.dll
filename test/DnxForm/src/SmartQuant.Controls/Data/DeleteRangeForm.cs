// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DeleteRangeForm
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class DeleteRangeForm : Form
  {
    private IContainer components;
    private Label label1;
    private DateTimePicker dtpDateTimeFrom;
    private Button btnCancel;
    private Button btnOk;
    private Label label2;
    private DateTimePicker dtpDateTimeTo;
    private NumericUpDown nudMillisecondsFrom;
    private NumericUpDown nudMillisecondsTo;

    internal DateTime From
    {
      get
      {
        return this.dtpDateTimeFrom.Value.AddMilliseconds((double) (int) this.nudMillisecondsFrom.Value);
      }
    }

    internal DateTime To
    {
      get
      {
        return this.dtpDateTimeTo.Value.AddMilliseconds((double) (int) this.nudMillisecondsTo.Value);
      }
    }

    public DeleteRangeForm()
    {
      this.InitializeComponent();
      DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
      string str = string.Format("{0} {1}", (object) currentInfo.ShortDatePattern, (object) currentInfo.LongTimePattern);
      if (str.Contains(".fff"))
        str = str.Replace(".fff", "");
      this.dtpDateTimeFrom.CustomFormat = str;
      this.dtpDateTimeTo.CustomFormat = str;
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      if (this.From > this.To)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, "\"From\" DateTime can not be greater than \"To\" dateTime", "", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
      else
        this.DialogResult = DialogResult.OK;
    }

    internal void SetRange(DateTime startDate, DateTime stopDate)
    {
      this.dtpDateTimeFrom.Value = startDate;
      this.dtpDateTimeTo.Value = stopDate;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (DeleteRangeForm));
      this.label1 = new Label();
      this.dtpDateTimeFrom = new DateTimePicker();
      this.btnCancel = new Button();
      this.btnOk = new Button();
      this.label2 = new Label();
      this.dtpDateTimeTo = new DateTimePicker();
      this.nudMillisecondsFrom = new NumericUpDown();
      this.nudMillisecondsTo = new NumericUpDown();
      this.nudMillisecondsFrom.BeginInit();
      this.nudMillisecondsTo.BeginInit();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Location = new Point(11, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(30, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "From";
      this.dtpDateTimeFrom.Format = DateTimePickerFormat.Custom;
      this.dtpDateTimeFrom.Location = new Point(47, 10);
      this.dtpDateTimeFrom.Name = "dtpDateTimeFrom";
      this.dtpDateTimeFrom.Size = new Size(160, 20);
      this.dtpDateTimeFrom.TabIndex = 7;
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Location = new Point(185, 68);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(72, 24);
      this.btnCancel.TabIndex = 6;
      this.btnCancel.Text = "Cancel";
      this.btnOk.Location = new Point(107, 68);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new Size(72, 24);
      this.btnOk.TabIndex = 5;
      this.btnOk.Text = "Ok";
      this.btnOk.Click += new EventHandler(this.btnOk_Click);
      this.label2.AutoSize = true;
      this.label2.Location = new Point(11, 42);
      this.label2.Name = "label2";
      this.label2.Size = new Size(20, 13);
      this.label2.TabIndex = 10;
      this.label2.Text = "To";
      this.dtpDateTimeTo.Format = DateTimePickerFormat.Custom;
      this.dtpDateTimeTo.Location = new Point(47, 36);
      this.dtpDateTimeTo.Name = "dtpDateTimeTo";
      this.dtpDateTimeTo.Size = new Size(160, 20);
      this.dtpDateTimeTo.TabIndex = 9;
      this.nudMillisecondsFrom.Location = new Point(213, 10);
      NumericUpDown numericUpDown1 = this.nudMillisecondsFrom;
      int[] bits1 = new int[4];
      bits1[0] = 999;
      Decimal num1 = new Decimal(bits1);
      numericUpDown1.Maximum = num1;
      this.nudMillisecondsFrom.Name = "nudMillisecondsFrom";
      this.nudMillisecondsFrom.Size = new Size(38, 20);
      this.nudMillisecondsFrom.TabIndex = 11;
      this.nudMillisecondsTo.Location = new Point(213, 35);
      NumericUpDown numericUpDown2 = this.nudMillisecondsTo;
      int[] bits2 = new int[4];
      bits2[0] = 999;
      Decimal num2 = new Decimal(bits2);
      numericUpDown2.Maximum = num2;
      this.nudMillisecondsTo.Name = "nudMillisecondsTo";
      this.nudMillisecondsTo.Size = new Size(38, 20);
      this.nudMillisecondsTo.TabIndex = 12;
      this.AcceptButton = (IButtonControl) this.btnOk;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ClientSize = new Size(269, 104);
      this.Controls.Add((Control) this.nudMillisecondsTo);
      this.Controls.Add((Control) this.nudMillisecondsFrom);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.dtpDateTimeTo);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.dtpDateTimeFrom);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnOk);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DeleteRangeForm";
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Delete Range";
      this.nudMillisecondsFrom.EndInit();
      this.nudMillisecondsTo.EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
