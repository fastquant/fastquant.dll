using SmartQuant;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.TradingTools
{
    internal class ModifyOrderForm : Form
    {
        private IContainer components;
        private GroupBox groupBox1;
        private Label label1;
        private Label label3;
        private Label label2;
        private NumericUpDown nudQty;
        private NumericUpDown nudStopPrice;
        private NumericUpDown nudLimitPrice;
        private Button btnModify;
        private Button btnCancel;

        public double LimitPrice => (double)this.nudLimitPrice.Value;

        public double StopPrice => (double)this.nudStopPrice.Value;

        public int Qty => (int)this.nudQty.Value;

        public ModifyOrderForm()
        {
            InitializeComponent();
            this.nudLimitPrice.Minimum = new Decimal(-1, -1, -1, true, 0);
            this.nudLimitPrice.Maximum = new Decimal(-1, -1, -1, false, 0);
            this.nudStopPrice.Minimum = new Decimal(-1, -1, -1, true, 0);
            this.nudStopPrice.Maximum = new Decimal(-1, -1, -1, false, 0);
        }

        public void Init(Order order)
        {
            int digits = GetDigits(order.Instrument);
            if (order.Instrument.TickSize != 0.0)
            {
                this.nudLimitPrice.Increment = (Decimal)order.Instrument.TickSize;
                this.nudStopPrice.Increment = (Decimal)order.Instrument.TickSize;
            }
            else
            {
                Decimal num = (Decimal)Math.Pow(0.1, digits);
                this.nudLimitPrice.Increment = num;
                this.nudStopPrice.Increment = num;
            }
            int num1 = this.calculateDecimals(this.nudLimitPrice.Increment);
            if (num1 > digits)
            {
                this.nudLimitPrice.DecimalPlaces = num1;
                this.nudStopPrice.DecimalPlaces = num1;
            }
            else
            {
                this.nudLimitPrice.DecimalPlaces = digits;
                this.nudStopPrice.DecimalPlaces = digits;
            }
            this.nudLimitPrice.Enabled = false;
            this.nudStopPrice.Enabled = false;
            switch (order.Type)
            {
                case OrderType.Market:
                    this.nudLimitPrice.Value = (Decimal)order.Price;
                    this.nudStopPrice.Value = (Decimal)order.StopPx;
                    this.nudQty.Value = (Decimal)order.Qty;
                    this.Text = $"{order.Instrument.Symbol} - {order.Side} {order.Type} [Modify]";
                    break;
                case OrderType.Stop:
                    this.nudStopPrice.Enabled = true;
                    goto case OrderType.Market;
                case OrderType.Limit:
                    this.nudLimitPrice.Enabled = true;
                    goto case OrderType.Market;
                case OrderType.StopLimit:
                    this.nudLimitPrice.Enabled = true;
                    this.nudStopPrice.Enabled = true;
                    goto case OrderType.Market;
                default:
                    throw new NotSupportedException("Not supported order type - {order.Type}");
            }
        }

        private int GetDigits(Instrument instrument)
        {
            int num = 5;
            if (instrument.PriceFormat.Contains("F"))
            {
                string s = instrument.PriceFormat.Substring(1);
                num = s.Length == 0 ? 0 : int.Parse(s);
            }
            return num;
        }

        private int calculateDecimals(Decimal value)
        {
            int num = 0;
            while (value * (Decimal)Math.Pow(10.0, num) - Math.Truncate(value * (Decimal)Math.Pow(10.0, (double)num)) > new Decimal(0))
                ++num;
            return num;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupBox1 = new GroupBox();
            this.nudQty = new NumericUpDown();
            this.nudStopPrice = new NumericUpDown();
            this.nudLimitPrice = new NumericUpDown();
            this.label3 = new Label();
            this.label2 = new Label();
            this.label1 = new Label();
            this.btnModify = new Button();
            this.btnCancel = new Button();
            this.groupBox1.SuspendLayout();
            this.nudQty.BeginInit();
            this.nudStopPrice.BeginInit();
            this.nudLimitPrice.BeginInit();
            this.SuspendLayout();
            this.groupBox1.Controls.Add((Control)this.nudQty);
            this.groupBox1.Controls.Add((Control)this.nudStopPrice);
            this.groupBox1.Controls.Add((Control)this.nudLimitPrice);
            this.groupBox1.Controls.Add((Control)this.label3);
            this.groupBox1.Controls.Add((Control)this.label2);
            this.groupBox1.Controls.Add((Control)this.label1);
            this.groupBox1.Location = new Point(16, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(206, 115);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Order Details";
            this.nudQty.Location = new Point(97, 76);
            NumericUpDown numericUpDown1 = this.nudQty;
            int[] bits1 = new int[4];
            bits1[0] = 1000000000;
            Decimal num1 = new Decimal(bits1);
            numericUpDown1.Maximum = num1;
            this.nudQty.Name = "nudQty";
            this.nudQty.Size = new Size(90, 20);
            this.nudQty.TabIndex = 5;
            this.nudQty.TextAlign = HorizontalAlignment.Right;
            this.nudQty.ThousandsSeparator = true;
            this.nudStopPrice.Location = new Point(97, 48);
            NumericUpDown numericUpDown2 = this.nudStopPrice;
            int[] bits2 = new int[4];
            bits2[0] = 1000000;
            Decimal num2 = new Decimal(bits2);
            numericUpDown2.Maximum = num2;
            this.nudStopPrice.Name = "nudStopPrice";
            this.nudStopPrice.Size = new Size(90, 20);
            this.nudStopPrice.TabIndex = 4;
            this.nudStopPrice.TextAlign = HorizontalAlignment.Right;
            this.nudStopPrice.ThousandsSeparator = true;
            this.nudLimitPrice.Location = new Point(97, 20);
            NumericUpDown numericUpDown3 = this.nudLimitPrice;
            int[] bits3 = new int[4];
            bits3[0] = 1000000;
            Decimal num3 = new Decimal(bits3);
            numericUpDown3.Maximum = num3;
            this.nudLimitPrice.Name = "nudLimitPrice";
            this.nudLimitPrice.Size = new Size(90, 20);
            this.nudLimitPrice.TabIndex = 3;
            this.nudLimitPrice.TextAlign = HorizontalAlignment.Right;
            this.nudLimitPrice.ThousandsSeparator = true;
            this.label3.Location = new Point(16, 76);
            this.label3.Name = "label3";
            this.label3.Size = new Size(74, 22);
            this.label3.TabIndex = 2;
            this.label3.Text = "Qty";
            this.label3.TextAlign = ContentAlignment.MiddleLeft;
            this.label2.Location = new Point(16, 48);
            this.label2.Name = "label2";
            this.label2.Size = new Size(74, 22);
            this.label2.TabIndex = 1;
            this.label2.Text = "Stop Price";
            this.label2.TextAlign = ContentAlignment.MiddleLeft;
            this.label1.Location = new Point(16, 20);
            this.label1.Name = "label1";
            this.label1.Size = new Size(74, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Limit Price";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;
            this.btnModify.DialogResult = DialogResult.OK;
            this.btnModify.Location = new Point(46, 140);
            this.btnModify.Name = "btnModify";
            this.btnModify.Size = new Size(67, 24);
            this.btnModify.TabIndex = 1;
            this.btnModify.Text = "Modify";
            this.btnModify.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(125, 140);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(67, 24);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.AcceptButton = (IButtonControl)this.btnModify;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = (IButtonControl)this.btnCancel;
            this.ClientSize = new Size(240, 177);
            this.Controls.Add((Control)this.btnCancel);
            this.Controls.Add((Control)this.btnModify);
            this.Controls.Add((Control)this.groupBox1);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifyOrderForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Modify Order";
            this.groupBox1.ResumeLayout(false);
            this.nudQty.EndInit();
            this.nudStopPrice.EndInit();
            this.nudLimitPrice.EndInit();
            this.ResumeLayout(false);
        }
    }
}
