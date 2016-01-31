using SmartQuant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data.Account
{
    class PositionKey : AccountDataKey
    {
        public PositionKey(SmartQuant.AccountData data) : base(data)
        {
            this.key = $"{GetFieldAsString("Symbol")} {GetFieldAsString("Maturity")} {GetFieldAsString("PutOrCall")} {GetFieldAsString("Strike")}";
        }
    }

    class OrderKey : AccountDataKey
    {
        public OrderKey(SmartQuant.AccountData data) : base(data)
        {
            this.key = GetFieldAsString("OrderID");
        }
    }

    internal class AccountDataViewer : UserControl
    {
        private Dictionary<string, Dictionary<string, ListViewItem>> accounts;
        private Dictionary<string, Dictionary<string, ListViewItem>> positions;
        private Dictionary<string, Dictionary<string, ListViewItem>> orders;
        private IContainer components;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox cbxAccount;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private ListView ltvDetails;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private ListView ltvPositions;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private ColumnHeader columnHeader6;
        private ColumnHeader columnHeader7;
        private ListView ltvOrders;
        private ColumnHeader columnHeader8;
        private ColumnHeader columnHeader9;
        private ColumnHeader columnHeader10;
        private ColumnHeader columnHeader11;
        private ColumnHeader columnHeader12;
        private ColumnHeader columnHeader13;
        private ColumnHeader columnHeader14;
        private ColumnHeader columnHeader15;

        public AccountDataViewer()
        {
            this.InitializeComponent();
            this.accounts = new Dictionary<string, Dictionary<string, ListViewItem>>();
            this.positions = new Dictionary<string, Dictionary<string, ListViewItem>>();
            this.orders = new Dictionary<string, Dictionary<string, ListViewItem>>();
        }

        public void Clear()
        {
            this.accounts.Clear();
            this.positions.Clear();
            this.orders.Clear();
            this.cbxAccount.Items.Clear();
            this.ltvDetails.Items.Clear();
            this.ltvPositions.Items.Clear();
            this.ltvOrders.Items.Clear();
        }

        public void OnData(SmartQuant.AccountData data)
        {
            if (!this.cbxAccount.Items.Contains((object)data.Account))
            {
                this.cbxAccount.Items.Add((object)data.Account);
                this.accounts.Add(data.Account, new Dictionary<string, ListViewItem>());
                this.positions.Add(data.Account, new Dictionary<string, ListViewItem>());
                this.orders.Add(data.Account, new Dictionary<string, ListViewItem>());
                if (this.cbxAccount.SelectedIndex < 0)
                    this.cbxAccount.SelectedIndex = 0;
            }
            switch (data.Type)
            {
                case AccountDataType.AccountValue:
                    this.AddValue(data);
                    break;
                case AccountDataType.Position:
                    this.AddPosition(data);
                    break;
                case AccountDataType.Order:
                    this.AddOrder(data);
                    break;
            }
        }

        private void AddValue(SmartQuant.AccountData data)
        {
            Dictionary<string, ListViewItem> dictionary = this.accounts[data.Account];
            foreach (AccountDataField accountDataField in data.Fields)
            {
                string key = string.Format("{0}:{1}", (object)accountDataField.Name, (object)accountDataField.Currency);
                ListViewItem listViewItem = (ListViewItem)null;
                if (!dictionary.TryGetValue(key, out listViewItem))
                {
                    listViewItem = new ListViewItem(new string[3]);
                    listViewItem.Name = key;
                    listViewItem.SubItems[0].Text = accountDataField.Name;
                    listViewItem.SubItems[1].Text = accountDataField.Currency;
                    dictionary.Add(key, listViewItem);
                }
                listViewItem.SubItems[2].Text = this.ValueToString(accountDataField.Value);
            }
            if (!(data.Account == this.cbxAccount.SelectedItem as string))
                return;
            this.UpdateListView(data.Account, AccountDataViewer.ListType.Accounts);
        }

        private void AddPosition(SmartQuant.AccountData data)
        {
            Dictionary<string, ListViewItem> dictionary = this.positions[data.Account];
            string key = new PositionKey(data).ToString();
            ListViewItem listViewItem = (ListViewItem)null;
            if (!dictionary.TryGetValue(key, out listViewItem))
            {
                listViewItem = new ListViewItem(new string[4]);
                listViewItem.Name = key;
                listViewItem.SubItems[0].Text = key;
                dictionary.Add(key, listViewItem);
            }
            this.UpdateSubItem(listViewItem, 1, data, "Qty");
            this.UpdateSubItem(listViewItem, 2, data, "LongQty");
            this.UpdateSubItem(listViewItem, 3, data, "ShortQty");
            if (!(data.Account == this.cbxAccount.SelectedItem as string))
                return;
            this.UpdateListView(data.Account, AccountDataViewer.ListType.Positions);
        }

        private void AddOrder(SmartQuant.AccountData data)
        {
            Dictionary<string, ListViewItem> dictionary = this.orders[data.Account];
            string key = new OrderKey(data).ToString();
            ListViewItem listViewItem = (ListViewItem)null;
            if (!dictionary.TryGetValue(key, out listViewItem))
            {
                listViewItem = new ListViewItem(new string[8]);
                listViewItem.Name = key;
                listViewItem.SubItems[0].Text = key;
                dictionary.Add(key, listViewItem);
            }
            this.UpdateSubItem(listViewItem, 1, data, "Symbol");
            this.UpdateSubItem(listViewItem, 2, data, "OrderSide");
            this.UpdateSubItem(listViewItem, 3, data, "OrderType");
            this.UpdateSubItem(listViewItem, 4, data, "Qty");
            this.UpdateSubItem(listViewItem, 5, data, "Price");
            this.UpdateSubItem(listViewItem, 6, data, "StopPx");
            this.UpdateSubItem(listViewItem, 7, data, "OrderStatus");
            if (!(data.Account == this.cbxAccount.SelectedItem as string))
                return;
            this.UpdateListView(data.Account, AccountDataViewer.ListType.Orders);
        }

        private void UpdateSubItem(ListViewItem item, int index, SmartQuant.AccountData data, string name)
        {
            object obj = data.Fields[name];
            if (obj == null)
                return;
            item.SubItems[index].Text = obj.ToString();
        }

        private void UpdateListView(string account, AccountDataViewer.ListType type)
        {
            ListView listView = (ListView)null;
            Dictionary<string, ListViewItem> dictionary = (Dictionary<string, ListViewItem>)null;
            switch (type)
            {
                case AccountDataViewer.ListType.Accounts:
                    listView = this.ltvDetails;
                    dictionary = this.accounts[account];
                    break;
                case AccountDataViewer.ListType.Positions:
                    listView = this.ltvPositions;
                    dictionary = this.positions[account];
                    break;
                case AccountDataViewer.ListType.Orders:
                    listView = this.ltvOrders;
                    dictionary = this.orders[account];
                    break;
            }
            listView.BeginUpdate();
            listView.Items.Clear();
            foreach (ListViewItem listViewItem in dictionary.Values)
                listView.Items.Add(listViewItem);
            listView.EndUpdate();
        }

        private void cbxAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbxAccount.SelectedIndex < 0)
                return;
            string account = this.cbxAccount.SelectedItem as string;
            this.UpdateListView(account, AccountDataViewer.ListType.Accounts);
            this.UpdateListView(account, AccountDataViewer.ListType.Positions);
            this.UpdateListView(account, AccountDataViewer.ListType.Orders);
        }

        private string ValueToString(object obj)
        {
            if (obj is object[])
                return string.Format("{0}", (object)string.Join(",", (object[])obj));
            return string.Format("{0}", obj);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.toolStrip1 = new ToolStrip();
            this.toolStripLabel1 = new ToolStripLabel();
            this.cbxAccount = new ToolStripComboBox();
            this.tabControl1 = new TabControl();
            this.tabPage1 = new TabPage();
            this.ltvDetails = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.columnHeader2 = new ColumnHeader();
            this.columnHeader3 = new ColumnHeader();
            this.tabPage2 = new TabPage();
            this.ltvPositions = new ListView();
            this.columnHeader4 = new ColumnHeader();
            this.columnHeader5 = new ColumnHeader();
            this.columnHeader6 = new ColumnHeader();
            this.columnHeader7 = new ColumnHeader();
            this.tabPage3 = new TabPage();
            this.ltvOrders = new ListView();
            this.columnHeader8 = new ColumnHeader();
            this.columnHeader9 = new ColumnHeader();
            this.columnHeader10 = new ColumnHeader();
            this.columnHeader11 = new ColumnHeader();
            this.columnHeader12 = new ColumnHeader();
            this.columnHeader13 = new ColumnHeader();
            this.columnHeader14 = new ColumnHeader();
            this.columnHeader15 = new ColumnHeader();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            this.toolStrip1.Items.AddRange(new ToolStripItem[2]
            {
        (ToolStripItem) this.toolStripLabel1,
        (ToolStripItem) this.cbxAccount
            });
            this.toolStrip1.Location = new Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new Size(567, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new Size(52, 22);
            this.toolStripLabel1.Text = "Account";
            this.cbxAccount.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxAccount.Name = "cbxAccount";
            this.cbxAccount.Size = new Size(121, 25);
            this.cbxAccount.SelectedIndexChanged += new EventHandler(this.cbxAccount_SelectedIndexChanged);
            this.tabControl1.Controls.Add((Control)this.tabPage1);
            this.tabControl1.Controls.Add((Control)this.tabPage2);
            this.tabControl1.Controls.Add((Control)this.tabPage3);
            this.tabControl1.Dock = DockStyle.Fill;
            this.tabControl1.Location = new Point(0, 25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new Size(567, 443);
            this.tabControl1.TabIndex = 1;
            this.tabPage1.Controls.Add((Control)this.ltvDetails);
            this.tabPage1.Location = new Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new Padding(3);
            this.tabPage1.Size = new Size(559, 417);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Details";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.ltvDetails.Columns.AddRange(new ColumnHeader[3]
            {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3
            });
            this.ltvDetails.Dock = DockStyle.Fill;
            this.ltvDetails.FullRowSelect = true;
            this.ltvDetails.GridLines = true;
            this.ltvDetails.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.ltvDetails.LabelWrap = false;
            this.ltvDetails.Location = new Point(3, 3);
            this.ltvDetails.Name = "ltvDetails";
            this.ltvDetails.ShowGroups = false;
            this.ltvDetails.ShowItemToolTips = true;
            this.ltvDetails.Size = new Size(553, 411);
            this.ltvDetails.TabIndex = 0;
            this.ltvDetails.UseCompatibleStateImageBehavior = false;
            this.ltvDetails.View = View.Details;
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 119;
            this.columnHeader2.Text = "Currency";
            this.columnHeader2.TextAlign = HorizontalAlignment.Right;
            this.columnHeader2.Width = 123;
            this.columnHeader3.Text = "Value";
            this.columnHeader3.TextAlign = HorizontalAlignment.Right;
            this.columnHeader3.Width = 128;
            this.tabPage2.Controls.Add((Control)this.ltvPositions);
            this.tabPage2.Location = new Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new Padding(3);
            this.tabPage2.Size = new Size(559, 417);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Positions";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.ltvPositions.Columns.AddRange(new ColumnHeader[4]
            {
        this.columnHeader4,
        this.columnHeader5,
        this.columnHeader6,
        this.columnHeader7
            });
            this.ltvPositions.Dock = DockStyle.Top;
            this.ltvPositions.FullRowSelect = true;
            this.ltvPositions.GridLines = true;
            this.ltvPositions.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.ltvPositions.HideSelection = false;
            this.ltvPositions.LabelWrap = false;
            this.ltvPositions.Location = new Point(3, 3);
            this.ltvPositions.MultiSelect = false;
            this.ltvPositions.Name = "ltvPositions";
            this.ltvPositions.ShowGroups = false;
            this.ltvPositions.ShowItemToolTips = true;
            this.ltvPositions.Size = new Size(553, 161);
            this.ltvPositions.TabIndex = 0;
            this.ltvPositions.UseCompatibleStateImageBehavior = false;
            this.ltvPositions.View = View.Details;
            this.columnHeader4.Text = "Instrument";
            this.columnHeader4.Width = 113;
            this.columnHeader5.Text = "Position";
            this.columnHeader5.TextAlign = HorizontalAlignment.Right;
            this.columnHeader5.Width = 91;
            this.columnHeader6.Text = "Bought";
            this.columnHeader6.TextAlign = HorizontalAlignment.Right;
            this.columnHeader6.Width = 80;
            this.columnHeader7.Text = "Sold";
            this.columnHeader7.TextAlign = HorizontalAlignment.Right;
            this.columnHeader7.Width = 84;
            this.tabPage3.Controls.Add((Control)this.ltvOrders);
            this.tabPage3.Location = new Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new Size(559, 417);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Orders";
            this.tabPage3.UseVisualStyleBackColor = true;
            this.ltvOrders.Columns.AddRange(new ColumnHeader[8]
            {
        this.columnHeader8,
        this.columnHeader9,
        this.columnHeader10,
        this.columnHeader11,
        this.columnHeader12,
        this.columnHeader13,
        this.columnHeader14,
        this.columnHeader15
            });
            this.ltvOrders.Dock = DockStyle.Top;
            this.ltvOrders.FullRowSelect = true;
            this.ltvOrders.GridLines = true;
            this.ltvOrders.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.ltvOrders.HideSelection = false;
            this.ltvOrders.LabelWrap = false;
            this.ltvOrders.Location = new Point(0, 0);
            this.ltvOrders.MultiSelect = false;
            this.ltvOrders.Name = "ltvOrders";
            this.ltvOrders.ShowGroups = false;
            this.ltvOrders.ShowItemToolTips = true;
            this.ltvOrders.Size = new Size(559, 158);
            this.ltvOrders.TabIndex = 0;
            this.ltvOrders.UseCompatibleStateImageBehavior = false;
            this.ltvOrders.View = View.Details;
            this.columnHeader8.Text = "OrderID";
            this.columnHeader8.Width = 64;
            this.columnHeader9.Text = "Symbol";
            this.columnHeader9.Width = 82;
            this.columnHeader10.Text = "Side";
            this.columnHeader10.TextAlign = HorizontalAlignment.Right;
            this.columnHeader11.Text = "Type";
            this.columnHeader11.TextAlign = HorizontalAlignment.Right;
            this.columnHeader12.Text = "Qty";
            this.columnHeader12.TextAlign = HorizontalAlignment.Right;
            this.columnHeader13.Text = "Price";
            this.columnHeader13.TextAlign = HorizontalAlignment.Right;
            this.columnHeader14.Text = "StopPx";
            this.columnHeader14.TextAlign = HorizontalAlignment.Right;
            this.columnHeader15.Text = "Status";
            this.columnHeader15.TextAlign = HorizontalAlignment.Right;
            this.columnHeader15.Width = 74;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add((Control)this.tabControl1);
            this.Controls.Add((Control)this.toolStrip1);
            this.Name = "AccountDataViewer";
            this.Size = new Size(567, 468);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private enum ListType
        {
            Accounts = 1,
            Positions = 2,
            Orders = 3,
        }
    }
}
