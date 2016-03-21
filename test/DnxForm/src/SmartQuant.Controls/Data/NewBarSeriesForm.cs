using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class NewBarSeriesForm : Form
  {
    private IContainer components;
    private Panel panel1;
    private GroupBox groupBox1;
    private Button btnCancel;
    private Button btnOk;
    private NumericUpDown nudBarSize;
    private ComboBox cbxBarTypes;
    private Label label2;
    private Label label1;

    public BarType BarType
    {
      get
      {
        return (BarType) this.cbxBarTypes.SelectedItem;
      }
    }

    public long BarSize
    {
      get
      {
        return (long) this.nudBarSize.Value;
      }
    }

    public NewBarSeriesForm()
    {
      this.InitializeComponent();
      this.cbxBarTypes.BeginUpdate();
      this.cbxBarTypes.Items.Clear();
      foreach (BarType barType in Enum.GetValues(typeof (BarType)))
        this.cbxBarTypes.Items.Add((object) barType);
      this.cbxBarTypes.SelectedItem = (object) BarType.Time;
      this.cbxBarTypes.EndUpdate();
      this.nudBarSize.Minimum = new Decimal(0);
      this.nudBarSize.Maximum = new Decimal(long.MaxValue);
      this.nudBarSize.Value = new Decimal(60);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
        this.components?.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.panel1 = new Panel();
      this.btnCancel = new Button();
      this.btnOk = new Button();
      this.groupBox1 = new GroupBox();
      this.nudBarSize = new NumericUpDown();
      this.cbxBarTypes = new ComboBox();
      this.label2 = new Label();
      this.label1 = new Label();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.nudBarSize.BeginInit();
      this.SuspendLayout();
      this.panel1.Controls.Add((Control) this.btnCancel);
      this.panel1.Controls.Add((Control) this.btnOk);
      this.panel1.Dock = DockStyle.Bottom;
      this.panel1.Location = new Point(4, 91);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(206, 35);
      this.panel1.TabIndex = 0;
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Location = new Point(143, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(56, 22);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnOk.DialogResult = DialogResult.OK;
      this.btnOk.Location = new Point(82, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new Size(56, 22);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      this.groupBox1.Controls.Add((Control) this.nudBarSize);
      this.groupBox1.Controls.Add((Control) this.cbxBarTypes);
      this.groupBox1.Controls.Add((Control) this.label2);
      this.groupBox1.Controls.Add((Control) this.label1);
      this.groupBox1.Dock = DockStyle.Fill;
      this.groupBox1.Location = new Point(4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(206, 87);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.nudBarSize.Location = new Point(80, 48);
      this.nudBarSize.Name = "nudBarSize";
      this.nudBarSize.Size = new Size(100, 20);
      this.nudBarSize.TabIndex = 3;
      this.nudBarSize.TextAlign = HorizontalAlignment.Right;
      this.nudBarSize.ThousandsSeparator = true;
      this.cbxBarTypes.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbxBarTypes.FormattingEnabled = true;
      this.cbxBarTypes.Location = new Point(80, 24);
      this.cbxBarTypes.Name = "cbxBarTypes";
      this.cbxBarTypes.Size = new Size(100, 21);
      this.cbxBarTypes.TabIndex = 2;
      this.label2.Location = new Point(16, 48);
      this.label2.Name = "label2";
      this.label2.Size = new Size(57, 20);
      this.label2.TabIndex = 1;
      this.label2.Text = "Bar Size";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      this.label1.Location = new Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new Size(57, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Bar Type";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      this.AcceptButton = (IButtonControl) this.btnOk;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ClientSize = new Size(214, 126);
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.panel1);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NewBarSeriesForm";
      this.Padding = new Padding(4, 4, 4, 0);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "New Bar Series";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.nudBarSize.EndInit();
      this.ResumeLayout(false);
    }
  }
}
