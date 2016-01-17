using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
  class NewPortfolioForm : Form
  {
    private IContainer components;
    private Label label1;
    private TextBox tbxName;
    private Button btnOK;
    private Button btnCancel;

    public string PortfolioName
    {
      get
      {
        return this.tbxName.Text.Trim();
      }
    }

    public NewPortfolioForm()
    {
      this.InitializeComponent();
      this.UpdateOKButtonStatus();
    }

    private void tbxName_TextChanged(object sender, EventArgs e)
    {
      this.UpdateOKButtonStatus();
    }

    private void UpdateOKButtonStatus()
    {
      this.btnOK.Enabled = !string.IsNullOrEmpty(this.PortfolioName);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.label1 = new Label();
      this.tbxName = new TextBox();
      this.btnOK = new Button();
      this.btnCancel = new Button();
      this.SuspendLayout();
      this.label1.Location = new Point(20, 24);
      this.label1.Name = "label1";
      this.label1.Size = new Size(44, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      this.tbxName.Location = new Point(70, 24);
      this.tbxName.Name = "tbxName";
      this.tbxName.Size = new Size(191, 20);
      this.tbxName.TabIndex = 1;
      this.tbxName.TextChanged += new EventHandler(this.tbxName_TextChanged);
      this.btnOK.DialogResult = DialogResult.OK;
      this.btnOK.Location = new Point(82, 64);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(60, 24);
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Location = new Point(154, 64);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(60, 24);
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.AcceptButton = (IButtonControl) this.btnOK;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ClientSize = new Size(284, 102);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnOK);
      this.Controls.Add((Control) this.tbxName);
      this.Controls.Add((Control) this.label1);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NewPortfolioForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Add New Portfolio";
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
