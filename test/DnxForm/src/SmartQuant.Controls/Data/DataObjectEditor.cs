using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class DataObjectEditor : Form
  {
    protected int instrumentId;
    protected byte providerId;
    private IContainer components;
    private Panel panel;
    protected Label label1;
    protected DateTimePicker dtpDateTime;
    protected Button btnCancel;
    protected Button btnOk;
    protected GroupBox groupBox1;

    protected virtual string ObjectName
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public DataObjectEditor()
    {
      this.InitializeComponent();
    }

    protected virtual void OnInit(SmartQuant.DataObject dataObject, int decimalPlaces)
    {
      throw new NotImplementedException();
    }

    public virtual SmartQuant.DataObject GetDataObject()
    {
      throw new NotImplementedException();
    }

    public void Init(SmartQuant.DataObject dataObject, DateTime dateTime, string priceFormat, int instrumentId)
    {
      this.instrumentId = instrumentId;
      int decimalPlaces = 2;
      try
      {
        decimalPlaces = int.Parse(priceFormat[1].ToString());
      }
      catch
      {
      }
      if (dataObject == null)
      {
        this.dtpDateTime.Value = dateTime;
        this.Text = string.Format("New {0}", (object) this.ObjectName);
      }
      else
      {
        this.dtpDateTime.Value = dateTime;
        this.dtpDateTime.Enabled = false;
        this.Text = string.Format("Edit {0}", (object) this.ObjectName);
      }
      this.OnInit(dataObject, decimalPlaces);
    }

    protected void SetNumericUpDownRange<T>(NumericUpDown control)
    {
      if (typeof (T) == typeof (double))
      {
        control.Minimum = new Decimal(-1, -1, -1, true, (byte) 0);
        control.Maximum = new Decimal(-1, -1, -1, false, (byte) 0);
      }
      if (typeof (T) == typeof (int))
      {
        control.Minimum = new Decimal(int.MinValue);
        control.Maximum = new Decimal(int.MaxValue);
      }
      if (typeof (T) == typeof (long))
      {
        control.Minimum = new Decimal(long.MinValue);
        control.Maximum = new Decimal(long.MaxValue);
      }
      if (typeof (T) == typeof (sbyte))
      {
        control.Minimum = new Decimal((int) sbyte.MinValue);
        control.Maximum = new Decimal((int) sbyte.MaxValue);
      }
      if (!(typeof (T) == typeof (byte)))
        return;
      control.Minimum = new Decimal(0);
      control.Maximum = new Decimal((int) byte.MaxValue);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.panel = new Panel();
      this.btnCancel = new Button();
      this.btnOk = new Button();
      this.groupBox1 = new GroupBox();
      this.dtpDateTime = new DateTimePicker();
      this.label1 = new Label();
      this.panel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      this.panel.Controls.Add((Control) this.btnCancel);
      this.panel.Controls.Add((Control) this.btnOk);
      this.panel.Dock = DockStyle.Bottom;
      this.panel.Location = new Point(4, 289);
      this.panel.Name = "panel";
      this.panel.Size = new Size(263, 40);
      this.panel.TabIndex = 0;
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Location = new Point(140, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(56, 22);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnOk.DialogResult = DialogResult.OK;
      this.btnOk.Location = new Point(78, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new Size(56, 22);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      this.groupBox1.Controls.Add((Control) this.dtpDateTime);
      this.groupBox1.Controls.Add((Control) this.label1);
      this.groupBox1.Dock = DockStyle.Fill;
      this.groupBox1.Location = new Point(4, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(263, 289);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.dtpDateTime.Format = DateTimePickerFormat.Short;
      this.dtpDateTime.Location = new Point(86, 16);
      this.dtpDateTime.Name = "dtpDateTime";
      this.dtpDateTime.Size = new Size(124, 20);
      this.dtpDateTime.TabIndex = 1;
      this.label1.Location = new Point(16, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(71, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "DateTime";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      this.AcceptButton = (IButtonControl) this.btnOk;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ClientSize = new Size(271, 329);
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.panel);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DataObjectEditor";
      this.Padding = new Padding(4, 0, 4, 0);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "DataObjectEditor";
      this.panel.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
