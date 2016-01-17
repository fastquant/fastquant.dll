using SmartQuant;
using SmartQuant.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
    class FillViewItem : ListViewItem
    {
        public Fill Fill { get; private set; }

        public FillViewItem(Fill fill)
          : base(new string[5])
        {
            this.Fill = fill;
            this.SubItems[0].Text = fill.DateTime.ToString();
            this.SubItems[3].Text = fill.Price.ToString(fill.Instrument.PriceFormat);
            this.SubItems[4].Text = fill.Qty.ToString();
            this.ImageIndex = 2;
            this.BackColor = Color.FromArgb((int)byte.MaxValue, (int)byte.MaxValue, 230);
        }
    }

    public class Composition : UserControl
  {
    private string name;
    private SmartQuant.Portfolio portfolio;
    private Portfolio portfolioControl;
    private IContainer components;
    private GroupBox groupBox2;
    private ListViewNB ltvTransactions;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;
    private GroupBox groupBox3;
    private ListViewNB ltvPortfolio;
    private ColumnHeader columnHeader10;
    private ColumnHeader columnHeader11;
    private ColumnHeader columnHeader12;
    private ColumnHeader columnHeader13;
    private GroupBox groupBox1;
    private ListViewNB ltvPositions;
    private ColumnHeader columnHeader6;
    private ColumnHeader columnHeader7;
    private ColumnHeader columnHeader8;
    private ColumnHeader columnHeader9;
    private ColumnHeader columnHeader14;
    private ColumnHeader columnHeader15;
    private ColumnHeader columnHeader16;

    public Dictionary<Position, PositionViewItem> PositionViewItems { get; private set; }

    public List<ListViewItem> TransactionsViewItems { get; private set; }

    public Composition()
    {
      this.InitializeComponent();
      this.PositionViewItems = new Dictionary<Position, PositionViewItem>();
      this.TransactionsViewItems = new List<ListViewItem>();
    }

    public void OnInit(string name, Portfolio portfolioControl)
    {
      this.name = name;
      this.portfolioControl = portfolioControl;
      this.portfolio = Framework.Current.PortfolioManager[name];
      this.PositionViewItems.Clear();
      this.ltvPositions.Items.Clear();
      this.TransactionsViewItems.Clear();
      this.ltvTransactions.VirtualListSize = this.TransactionsViewItems.Count;
      this.ltvPortfolio.Items.Clear();
      this.ltvPortfolio.Items.Add(new ListViewItem(new string[this.ltvPortfolio.Columns.Count]));
    }

    public void UpdateGUI()
    {
      this.portfolio = Framework.Current.PortfolioManager[this.name];
      if (this.portfolio == null)
        return;
      this.ltvPortfolio.BeginUpdate();
      string format = "F2";
      this.ltvPortfolio.Items[0].SubItems[0].Text = CurrencyId.GetName(this.portfolio.Account.CurrencyId);
      this.ltvPortfolio.Items[0].SubItems[1].Text = this.portfolio.AccountValue.ToString(format);
      this.ltvPortfolio.Items[0].SubItems[2].Text = this.portfolio.PositionValue.ToString(format);
      this.ltvPortfolio.Items[0].SubItems[3].Text = this.portfolio.Value.ToString(format);
      this.ltvPortfolio.EndUpdate();
      this.ltvTransactions.VirtualListSize = this.TransactionsViewItems.Count;
    }

    public void AddPosition(Position position)
    {
      PositionViewItem positionViewItem = new PositionViewItem(position);
      this.PositionViewItems.Add(position, positionViewItem);
      this.ltvPositions.Items.Insert(0, (ListViewItem) positionViewItem);
    }

    public void UpdatePosition(Position position)
    {
      this.PositionViewItems[position].Update();
    }

    public void RemovePosition(Position position)
    {
      this.PositionViewItems[position].Remove();
      this.PositionViewItems.Remove(position);
    }

    private void ltvPositions_SelectedIndexChanged(object sender, EventArgs e)
    {
      object obj = (object) null;
      if (this.ltvPositions.SelectedItems.Count > 0)
        obj = (object) (this.ltvPositions.SelectedItems[0] as PositionViewItem).Position;
      this.portfolioControl.UpdatePropertyObject(obj, false);
    }

    private void ltvTransactions_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
      e.Item = this.TransactionsViewItems[e.ItemIndex];
    }

    private void ltvTransactions_SelectedIndexChanged(object sender, EventArgs e)
    {
      object obj = (object) null;
      if (this.ltvTransactions.SelectedIndices.Count > 0)
      {
        obj = (object) this.TransactionsViewItems[this.ltvTransactions.SelectedIndices[0]];
        if (obj is TransactionViewItem)
          obj = (object) (obj as TransactionViewItem).Transaction;
        else if (obj is FillViewItem)
          obj = (object) (obj as FillViewItem).Fill;
      }
      this.portfolioControl.UpdatePropertyObject(obj, false);
    }

    private void ltvTransactions_MouseClick(object sender, MouseEventArgs e)
    {
      TransactionViewItem tvi = this.ltvTransactions.GetItemAt(e.X, e.Y) as TransactionViewItem;
      if (tvi == null || e.X >= 20)
        return;
      if (tvi.ImageIndex == 0)
      {
        this.Expand(tvi);
      }
      else
      {
        if (tvi.ImageIndex != 1)
          return;
        this.Collapse(tvi);
      }
    }

    private void ltvTransactions_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      TransactionViewItem tvi = this.ltvTransactions.GetItemAt(e.X, e.Y) as TransactionViewItem;
      if (tvi == null)
        return;
      if (tvi.ImageIndex == 0)
      {
        this.Expand(tvi);
      }
      else
      {
        if (tvi.ImageIndex != 1)
          return;
        this.Collapse(tvi);
      }
    }

    private void ltvTransactions_KeyDown(object sender, KeyEventArgs e)
    {
      if (this.ltvTransactions.SelectedIndices.Count <= 0)
        return;
      TransactionViewItem tvi = this.TransactionsViewItems[this.ltvTransactions.SelectedIndices[0]] as TransactionViewItem;
      if (tvi == null)
        return;
      if (tvi.ImageIndex == 0 && (e.KeyData == Keys.Return || e.KeyData == Keys.Right))
      {
        this.Expand(tvi);
      }
      else
      {
        if (tvi.ImageIndex != 1 || e.KeyData != Keys.Return && e.KeyData != Keys.Left)
          return;
        this.Collapse(tvi);
      }
    }

    private void Expand(TransactionViewItem tvi)
    {
      for (int index = 0; index < tvi.Transaction.Fills.Count; ++index)
        this.TransactionsViewItems.Insert(tvi.Index + index + 1, (ListViewItem) new FillViewItem(tvi.Transaction.Fills[index]));
      tvi.ImageIndex = 1;
      this.ltvTransactions.VirtualListSize = this.TransactionsViewItems.Count;
    }

    private void Collapse(TransactionViewItem tvi)
    {
      for (int index = 0; index < tvi.Transaction.Fills.Count; ++index)
        this.TransactionsViewItems.RemoveAt(tvi.Index + 1);
      tvi.ImageIndex = 0;
      this.ltvTransactions.VirtualListSize = this.TransactionsViewItems.Count;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.groupBox2 = new GroupBox();
      this.ltvTransactions = new ListViewNB();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.columnHeader5 = new ColumnHeader();
      this.columnHeader14 = new ColumnHeader();
      this.groupBox3 = new GroupBox();
      this.ltvPortfolio = new ListViewNB();
      this.columnHeader10 = new ColumnHeader();
      this.columnHeader11 = new ColumnHeader();
      this.columnHeader12 = new ColumnHeader();
      this.columnHeader13 = new ColumnHeader();
      this.groupBox1 = new GroupBox();
      this.ltvPositions = new ListViewNB();
      this.columnHeader6 = new ColumnHeader();
      this.columnHeader7 = new ColumnHeader();
      this.columnHeader8 = new ColumnHeader();
      this.columnHeader9 = new ColumnHeader();
      this.columnHeader15 = new ColumnHeader();
      this.columnHeader16 = new ColumnHeader();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      this.groupBox2.Controls.Add((Control) this.ltvTransactions);
      this.groupBox2.Dock = DockStyle.Fill;
      this.groupBox2.Location = new Point(0, 117);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(745, 312);
      this.groupBox2.TabIndex = 5;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Transactions";
      this.ltvTransactions.Columns.AddRange(new ColumnHeader[8]
      {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4,
        this.columnHeader5,
        this.columnHeader15,
        this.columnHeader16,
        this.columnHeader14
      });
      this.ltvTransactions.Dock = DockStyle.Fill;
      this.ltvTransactions.FullRowSelect = true;
      this.ltvTransactions.GridLines = true;
      this.ltvTransactions.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.ltvTransactions.LabelWrap = false;
      this.ltvTransactions.Location = new Point(3, 16);
      this.ltvTransactions.Name = "ltvTransactions";
      this.ltvTransactions.ShowGroups = false;
      this.ltvTransactions.ShowItemToolTips = true;
      this.ltvTransactions.Size = new Size(739, 293);
      this.ltvTransactions.TabIndex = 0;
      this.ltvTransactions.UseCompatibleStateImageBehavior = false;
      this.ltvTransactions.View = View.Details;
      this.ltvTransactions.VirtualMode = true;
      this.ltvTransactions.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this.ltvTransactions_RetrieveVirtualItem);
      this.ltvTransactions.SelectedIndexChanged += new EventHandler(this.ltvTransactions_SelectedIndexChanged);
      this.ltvTransactions.KeyDown += new KeyEventHandler(this.ltvTransactions_KeyDown);
      this.ltvTransactions.MouseClick += new MouseEventHandler(this.ltvTransactions_MouseClick);
      this.ltvTransactions.MouseDoubleClick += new MouseEventHandler(this.ltvTransactions_MouseDoubleClick);
      this.columnHeader1.Text = "DateTime";
      this.columnHeader1.Width = 126;
      this.columnHeader2.Text = "Instrument";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 88;
      this.columnHeader3.Text = "Side";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 62;
      this.columnHeader4.Text = "Price";
      this.columnHeader4.TextAlign = HorizontalAlignment.Right;
      this.columnHeader4.Width = 68;
      this.columnHeader5.Text = "Qty";
      this.columnHeader5.TextAlign = HorizontalAlignment.Right;
      this.columnHeader5.Width = 65;
      this.columnHeader14.Text = "Text";
      this.columnHeader14.Width = 250;
      this.groupBox3.Controls.Add((Control) this.ltvPortfolio);
      this.groupBox3.Dock = DockStyle.Bottom;
      this.groupBox3.Location = new Point(0, 429);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new Size(745, 81);
      this.groupBox3.TabIndex = 6;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Value";
      this.ltvPortfolio.Columns.AddRange(new ColumnHeader[4]
      {
        this.columnHeader10,
        this.columnHeader11,
        this.columnHeader12,
        this.columnHeader13
      });
      this.ltvPortfolio.Dock = DockStyle.Fill;
      this.ltvPortfolio.GridLines = true;
      this.ltvPortfolio.Location = new Point(3, 16);
      this.ltvPortfolio.Name = "ltvPortfolio";
      this.ltvPortfolio.Size = new Size(739, 62);
      this.ltvPortfolio.TabIndex = 0;
      this.ltvPortfolio.UseCompatibleStateImageBehavior = false;
      this.ltvPortfolio.View = View.Details;
      this.columnHeader10.Text = "Currency";
      this.columnHeader10.Width = 80;
      this.columnHeader11.Text = "Account";
      this.columnHeader11.TextAlign = HorizontalAlignment.Right;
      this.columnHeader11.Width = 101;
      this.columnHeader12.Text = "Position";
      this.columnHeader12.TextAlign = HorizontalAlignment.Right;
      this.columnHeader12.Width = 100;
      this.columnHeader13.Text = "Portfolio";
      this.columnHeader13.TextAlign = HorizontalAlignment.Right;
      this.columnHeader13.Width = 100;
      this.groupBox1.Controls.Add((Control) this.ltvPositions);
      this.groupBox1.Dock = DockStyle.Top;
      this.groupBox1.Location = new Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(745, 117);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Positions";
      this.ltvPositions.Columns.AddRange(new ColumnHeader[4]
      {
        this.columnHeader6,
        this.columnHeader7,
        this.columnHeader8,
        this.columnHeader9
      });
      this.ltvPositions.Dock = DockStyle.Fill;
      this.ltvPositions.FullRowSelect = true;
      this.ltvPositions.GridLines = true;
      this.ltvPositions.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.ltvPositions.LabelWrap = false;
      this.ltvPositions.Location = new Point(3, 16);
      this.ltvPositions.Name = "ltvPositions";
      this.ltvPositions.ShowGroups = false;
      this.ltvPositions.ShowItemToolTips = true;
      this.ltvPositions.Size = new Size(739, 98);
      this.ltvPositions.TabIndex = 0;
      this.ltvPositions.UseCompatibleStateImageBehavior = false;
      this.ltvPositions.View = View.Details;
      this.ltvPositions.SelectedIndexChanged += new EventHandler(this.ltvPositions_SelectedIndexChanged);
      this.columnHeader6.Text = "Instrument";
      this.columnHeader6.Width = 94;
      this.columnHeader7.Text = "Amount";
      this.columnHeader7.TextAlign = HorizontalAlignment.Right;
      this.columnHeader7.Width = 79;
      this.columnHeader8.Text = "Bought";
      this.columnHeader8.TextAlign = HorizontalAlignment.Right;
      this.columnHeader8.Width = 72;
      this.columnHeader9.Text = "Sold";
      this.columnHeader9.TextAlign = HorizontalAlignment.Right;
      this.columnHeader9.Width = 65;
      this.columnHeader15.Text = "Value";
      this.columnHeader15.TextAlign = HorizontalAlignment.Right;
      this.columnHeader15.Width = 68;
      this.columnHeader16.Text = "Cost";
      this.columnHeader16.TextAlign = HorizontalAlignment.Right;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.groupBox2);
      this.Controls.Add((Control) this.groupBox3);
      this.Controls.Add((Control) this.groupBox1);
      this.Name = "Composition";
      this.Size = new Size(745, 510);
      this.groupBox2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
