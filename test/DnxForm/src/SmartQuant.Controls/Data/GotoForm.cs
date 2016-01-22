// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.GotoForm
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
  internal class GotoForm : Form
  {
    private Button btnOk;
    private Button btnCancel;
    private DateTimePicker dtpDateTime;
    private Label label1;
    private NumericUpDown nudMilliseconds;
    private Container components;

    internal DateTime Result
    {
      get
      {
        return this.dtpDateTime.Value.AddMilliseconds((double) (int) this.nudMilliseconds.Value);
      }
    }

    public GotoForm()
    {
      this.InitializeComponent();
      DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
      string str = string.Format("{0} {1}", (object) currentInfo.ShortDatePattern, (object) currentInfo.LongTimePattern);
      if (str.Contains(".fff"))
        str = str.Replace(".fff", "");
      this.dtpDateTime.CustomFormat = str;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (GotoForm));
      this.btnOk = new Button();
      this.btnCancel = new Button();
      this.dtpDateTime = new DateTimePicker();
      this.label1 = new Label();
      this.nudMilliseconds = new NumericUpDown();
      this.nudMilliseconds.BeginInit();
      this.SuspendLayout();
      this.btnOk.DialogResult = DialogResult.OK;
      this.btnOk.Location = new Point(128, 49);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new Size(72, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "Ok";
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Location = new Point(206, 49);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(72, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.dtpDateTime.Format = DateTimePickerFormat.Custom;
      this.dtpDateTime.Location = new Point(71, 12);
      this.dtpDateTime.Name = "dtpDateTime";
      this.dtpDateTime.Size = new Size(160, 20);
      this.dtpDateTime.TabIndex = 3;
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(53, 13);
      this.label1.TabIndex = 4;
      this.label1.Text = "DateTime";
      this.nudMilliseconds.Location = new Point(237, 12);
      NumericUpDown numericUpDown = this.nudMilliseconds;
      int[] bits = new int[4];
      bits[0] = 999;
      Decimal num = new Decimal(bits);
      numericUpDown.Maximum = num;
      this.nudMilliseconds.Name = "nudMilliseconds";
      this.nudMilliseconds.Size = new Size(38, 20);
      this.nudMilliseconds.TabIndex = 5;
      this.AcceptButton = (IButtonControl) this.btnOk;
      this.AutoScaleBaseSize = new Size(5, 13);
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ClientSize = new Size(290, 85);
      this.Controls.Add((Control) this.nudMilliseconds);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.dtpDateTime);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnOk);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "GotoForm";
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Go To Date/Time";
      this.nudMilliseconds.EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    internal void SetRange(DateTime minDateTime, DateTime maxDateTime)
    {
      this.dtpDateTime.Value = minDateTime;
    }

    internal void SetInitialValue(DateTime dateTime)
    {
      this.dtpDateTime.Value = dateTime;
      this.nudMilliseconds.Value = (Decimal) dateTime.Millisecond;
    }
  }
}
