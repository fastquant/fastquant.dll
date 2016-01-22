// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataManager
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using SmartQuant.Controls;
//using SmartQuant.Controls.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  public class DataManager : FrameworkControl
  {
    private SortedList<DataTypeItem, List<InstrumentDataSeries>> seriesLists;
    private DataSeriesViewItemComparer itemComparer;
    private IContainer components;
    private TabControl tabControl1;
    private TabPage tabPage1;
    private TreeView trvDataTypes;
    private Splitter splitter1;
    private TabControl tabControl2;
    private TabPage tabDataSeries;
    private ListViewNB ltvDataSeries;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ImageList imgDataTypes;
    private ImageList imgDataSeries;
    private ToolStrip toolStrip;
    private ToolStripButton tsbRefresh;
    private ContextMenuStrip ctxDataSeries;
    private ToolStripMenuItem ctxDataSeries_Export;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem ctxDataSeries_Clear;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem ctxDataSeries_Delete;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem ctxDataSeries_CompressBars;

    public event EventHandler<DataSeriesListEventArgs> CompressBars;

    public event EventHandler<DataSeriesListEventArgs> ExportToCSV;

    public DataManager()
    {
      this.InitializeComponent();
      this.seriesLists = new SortedList<DataTypeItem, List<InstrumentDataSeries>>((IComparer<DataTypeItem>) new DataTypeItemComparer());
      this.itemComparer = new DataSeriesViewItemComparer();
    }

    protected override void OnInit()
    {
      this.LoadLayout();
      this.Init();
    }

    private void trvDataTypes_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
        return;
      this.trvDataTypes.SelectedNode = this.trvDataTypes.GetNodeAt(e.Location);
    }

    private void trvDataTypes_AfterSelect(object sender, TreeViewEventArgs e)
    {
      DataTypeNode dataTypeNode = this.trvDataTypes.SelectedNode as DataTypeNode;
      if (dataTypeNode == null)
        this.UpdateDataSeriesList((DataTypeItem) null);
      else
        this.UpdateDataSeriesList(dataTypeNode.Item);
    }

    private void tsbRefresh_Click(object sender, EventArgs e)
    {
      this.Init();
    }

    private void ctxDataSeries_Opening(object sender, CancelEventArgs e)
    {
      if (this.ltvDataSeries.SelectedItems.Count == 0)
      {
        this.ctxDataSeries_Export.Enabled = false;
        this.ctxDataSeries_CompressBars.Enabled = false;
        this.ctxDataSeries_Clear.Enabled = false;
        this.ctxDataSeries_Delete.Enabled = false;
      }
      else
      {
        byte dataType = (this.ltvDataSeries.SelectedItems[0] as InstrumentDataSeriesViewItem).Series.DataTypeItem.DataType;
        this.ctxDataSeries_Export.Enabled = (int) dataType != 7;
        this.ctxDataSeries_CompressBars.Enabled = (int) dataType != 7;
        this.ctxDataSeries_Clear.Enabled = true;
        this.ctxDataSeries_Delete.Enabled = true;
      }
    }

    private void ctxDataSeries_Export_Click(object sender, EventArgs e)
    {
      List<DataSeries> list = new List<DataSeries>();
      foreach (InstrumentDataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
        list.Add(dataSeriesViewItem.Series.DataSeries);
      if (this.ExportToCSV == null)
        return;
      this.ExportToCSV((object) this, new DataSeriesListEventArgs(list.ToArray()));
    }

    private void ctxDataSeries_CompressBars_Click(object sender, EventArgs e)
    {
      List<DataSeries> list = new List<DataSeries>();
      foreach (InstrumentDataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
        list.Add(dataSeriesViewItem.Series.DataSeries);
      if (this.CompressBars == null)
        return;
      this.CompressBars((object) this, new DataSeriesListEventArgs(list.ToArray()));
    }

    private void ctxDataSeries_Clear_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show((IWin32Window) this, "Do you really want to clear selected series?", "Clear Series", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      foreach (InstrumentDataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
      {
        dataSeriesViewItem.Series.DataSeries.Clear();
        dataSeriesViewItem.UpdateValues();
      }
    }

    private void ctxDataSeries_Delete_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show((IWin32Window) this, "Do you really want to delete selected series?", "Delete Series", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      foreach (InstrumentDataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
      {
        this.framework.DataServer.DeleteDataSeries(dataSeriesViewItem.Series.DataSeries.Name);
        dataSeriesViewItem.Remove();
        List<InstrumentDataSeries> list = this.seriesLists[dataSeriesViewItem.Series.DataTypeItem];
        list.Remove(dataSeriesViewItem.Series);
        if (list.Count == 0)
        {
          foreach (DataTypeNode dataTypeNode in this.trvDataTypes.Nodes)
          {
            if (dataTypeNode.Item == dataSeriesViewItem.Series.DataTypeItem)
            {
              dataTypeNode.Remove();
              break;
            }
          }
        }
      }
    }

    private void ltvDataSeries_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      this.itemComparer.SortByColumn(e.Column);
      this.ltvDataSeries.Sort();
    }

    private void ltvDataSeries_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
    {
      this.SaveLayout();
    }

    private void Init()
    {
      this.seriesLists.Clear();
      foreach (Instrument instrument in this.framework.InstrumentManager.Instruments)
      {
        foreach (DataSeries dataSeries in this.framework.DataManager.GetDataSeriesList(instrument, (string) null))
        {
          byte dataType = DataSeriesNameHelper.GetDataType(dataSeries);
          DataTypeItem dataTypeItem = (DataTypeItem) null;
          if ((int) dataType == 6)
          {
            BarType barType;
            long barSize;
            if (DataSeriesNameHelper.TryGetBarTypeSize(dataSeries, out barType, out barSize))
              dataTypeItem = new DataTypeItem(dataType, new BarType?(barType), new long?(barSize));
          }
          else
            dataTypeItem = new DataTypeItem(dataType, new BarType?(), new long?());
          if (dataTypeItem != null)
          {
            List<InstrumentDataSeries> list;
            if (!this.seriesLists.TryGetValue(dataTypeItem, out list))
            {
              list = new List<InstrumentDataSeries>();
              this.seriesLists.Add(dataTypeItem, list);
            }
            list.Add(new InstrumentDataSeries(instrument, dataSeries, dataTypeItem));
          }
        }
      }
      string str = this.trvDataTypes.SelectedNode == null ? (string) null : this.trvDataTypes.SelectedNode.Text;
      HashSet<string> hashSet = new HashSet<string>();
      foreach (ListViewItem listViewItem in this.ltvDataSeries.SelectedItems)
        hashSet.Add(listViewItem.Text);
      this.trvDataTypes.BeginUpdate();
      this.trvDataTypes.Nodes.Clear();
      foreach (DataTypeItem dataTypeItem in (IEnumerable<DataTypeItem>) this.seriesLists.Keys)
        this.trvDataTypes.Nodes.Add((TreeNode) new DataTypeNode(dataTypeItem));
      this.trvDataTypes.EndUpdate();
      this.UpdateDataSeriesList((DataTypeItem) null);
      if (str == null)
        return;
      foreach (TreeNode treeNode in this.trvDataTypes.Nodes)
      {
        if (treeNode.Text == str)
        {
          this.trvDataTypes.SelectedNode = treeNode;
          if (hashSet.Count <= 0)
            break;
          IEnumerator enumerator = this.ltvDataSeries.Items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              ListViewItem listViewItem = (ListViewItem) enumerator.Current;
              if (hashSet.Contains(listViewItem.Text))
                listViewItem.Selected = true;
            }
            break;
          }
          finally
          {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable != null)
              disposable.Dispose();
          }
        }
      }
    }

    private void UpdateDataSeriesList(DataTypeItem item)
    {
      this.Cursor = Cursors.WaitCursor;
      this.ltvDataSeries.BeginUpdate();
      this.ltvDataSeries.Items.Clear();
      this.ltvDataSeries.Groups.Clear();
      this.ltvDataSeries.ListViewItemSorter = (IComparer) null;
      if (item == null)
      {
        this.tabDataSeries.ResetText();
      }
      else
      {
        List<InstrumentDataSeries> list = this.seriesLists[item];
        foreach (InstrumentDataSeries series in list)
        {
          ListViewItem listViewItem = (ListViewItem) new InstrumentDataSeriesViewItem(series);
          this.ltvDataSeries.Items.Add(listViewItem);
          ListViewGroup group = this.ltvDataSeries.Groups[series.Instrument.Type.ToString()];
          if (group == null)
          {
            group = new ListViewGroup(series.Instrument.Type.ToString(), series.Instrument.Type.ToString());
            this.ltvDataSeries.Groups.Add(group);
          }
          listViewItem.Group = group;
        }
        this.tabDataSeries.Text = string.Format("{0} - {1:n0} instruments", (object) item, (object) list.Count);
      }
      this.ltvDataSeries.ListViewItemSorter = (IComparer) this.itemComparer;
      this.ltvDataSeries.EndUpdate();
      this.Cursor = Cursors.Default;
    }

    private void SaveLayout()
    {
      List<int> list = new List<int>();
      foreach (ColumnHeader columnHeader in this.ltvDataSeries.Columns)
        list.Add(columnHeader.Width);
    }

    private void LoadLayout()
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (DataManager));
      this.tabControl1 = new TabControl();
      this.tabPage1 = new TabPage();
      this.trvDataTypes = new TreeView();
      this.imgDataTypes = new ImageList(this.components);
      this.splitter1 = new Splitter();
      this.tabControl2 = new TabControl();
      this.tabDataSeries = new TabPage();
      this.ltvDataSeries = new ListViewNB();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.ctxDataSeries = new ContextMenuStrip(this.components);
      this.ctxDataSeries_Export = new ToolStripMenuItem();
      this.toolStripSeparator3 = new ToolStripSeparator();
      this.ctxDataSeries_CompressBars = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.ctxDataSeries_Clear = new ToolStripMenuItem();
      this.toolStripSeparator2 = new ToolStripSeparator();
      this.ctxDataSeries_Delete = new ToolStripMenuItem();
      this.imgDataSeries = new ImageList(this.components);
      this.toolStrip = new ToolStrip();
      this.tsbRefresh = new ToolStripButton();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabControl2.SuspendLayout();
      this.tabDataSeries.SuspendLayout();
      this.ctxDataSeries.SuspendLayout();
      this.toolStrip.SuspendLayout();
      this.SuspendLayout();
      this.tabControl1.Controls.Add((Control) this.tabPage1);
      this.tabControl1.Dock = DockStyle.Left;
      this.tabControl1.Location = new Point(0, 25);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new Size(140, 375);
      this.tabControl1.TabIndex = 0;
      this.tabPage1.Controls.Add((Control) this.trvDataTypes);
      this.tabPage1.Location = new Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(132, 349);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Data Type";
      this.tabPage1.UseVisualStyleBackColor = true;
      this.trvDataTypes.Dock = DockStyle.Fill;
      this.trvDataTypes.HideSelection = false;
      this.trvDataTypes.ImageIndex = 0;
      this.trvDataTypes.ImageList = this.imgDataTypes;
      this.trvDataTypes.Location = new Point(3, 3);
      this.trvDataTypes.Name = "trvDataTypes";
      this.trvDataTypes.SelectedImageIndex = 0;
      this.trvDataTypes.ShowLines = false;
      this.trvDataTypes.ShowRootLines = false;
      this.trvDataTypes.Size = new Size(126, 343);
      this.trvDataTypes.TabIndex = 0;
      this.trvDataTypes.AfterSelect += new TreeViewEventHandler(this.trvDataTypes_AfterSelect);
      this.trvDataTypes.MouseDown += new MouseEventHandler(this.trvDataTypes_MouseDown);
      this.imgDataTypes.ImageStream = (ImageListStreamer) componentResourceManager.GetObject("imgDataTypes.ImageStream");
      this.imgDataTypes.TransparentColor = Color.Transparent;
      this.imgDataTypes.Images.SetKeyName(0, "data_type.png");
      this.splitter1.Location = new Point(140, 25);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new Size(4, 375);
      this.splitter1.TabIndex = 1;
      this.splitter1.TabStop = false;
      this.tabControl2.Controls.Add((Control) this.tabDataSeries);
      this.tabControl2.Dock = DockStyle.Fill;
      this.tabControl2.Location = new Point(144, 25);
      this.tabControl2.Name = "tabControl2";
      this.tabControl2.SelectedIndex = 0;
      this.tabControl2.Size = new Size(448, 375);
      this.tabControl2.TabIndex = 2;
      this.tabDataSeries.Controls.Add((Control) this.ltvDataSeries);
      this.tabDataSeries.Location = new Point(4, 22);
      this.tabDataSeries.Name = "tabDataSeries";
      this.tabDataSeries.Padding = new Padding(3);
      this.tabDataSeries.Size = new Size(440, 349);
      this.tabDataSeries.TabIndex = 0;
      this.tabDataSeries.Text = "Data Series";
      this.tabDataSeries.UseVisualStyleBackColor = true;
      this.ltvDataSeries.Columns.AddRange(new ColumnHeader[4]
      {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4
      });
      this.ltvDataSeries.ContextMenuStrip = this.ctxDataSeries;
      this.ltvDataSeries.Dock = DockStyle.Fill;
      this.ltvDataSeries.FullRowSelect = true;
      this.ltvDataSeries.GridLines = true;
      this.ltvDataSeries.HideSelection = false;
      this.ltvDataSeries.Location = new Point(3, 3);
      this.ltvDataSeries.Name = "ltvDataSeries";
      this.ltvDataSeries.ShowItemToolTips = true;
      this.ltvDataSeries.Size = new Size(434, 343);
      this.ltvDataSeries.SmallImageList = this.imgDataSeries;
      this.ltvDataSeries.TabIndex = 0;
      this.ltvDataSeries.UseCompatibleStateImageBehavior = false;
      this.ltvDataSeries.View = View.Details;
      this.ltvDataSeries.ColumnClick += new ColumnClickEventHandler(this.ltvDataSeries_ColumnClick);
      this.ltvDataSeries.ColumnWidthChanged += new ColumnWidthChangedEventHandler(this.ltvDataSeries_ColumnWidthChanged);
      this.columnHeader1.Text = "Instrument";
      this.columnHeader1.Width = 79;
      this.columnHeader2.Text = "Count";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 86;
      this.columnHeader3.Text = "DateTime1";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 114;
      this.columnHeader4.Text = "DateTime2";
      this.columnHeader4.TextAlign = HorizontalAlignment.Right;
      this.columnHeader4.Width = 118;
      this.ctxDataSeries.Items.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.ctxDataSeries_Export,
        (ToolStripItem) this.toolStripSeparator3,
        (ToolStripItem) this.ctxDataSeries_CompressBars,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.ctxDataSeries_Clear,
        (ToolStripItem) this.toolStripSeparator2,
        (ToolStripItem) this.ctxDataSeries_Delete
      });
      this.ctxDataSeries.Name = "ctxDataSeries";
      this.ctxDataSeries.Size = new Size(162, 110);
      this.ctxDataSeries.Opening += new CancelEventHandler(this.ctxDataSeries_Opening);
      this.ctxDataSeries_Export.Name = "ctxDataSeries_Export";
      this.ctxDataSeries_Export.Size = new Size(161, 22);
      this.ctxDataSeries_Export.Text = "Export To CSV...";
      this.ctxDataSeries_Export.Click += new EventHandler(this.ctxDataSeries_Export_Click);
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new Size(158, 6);
      this.ctxDataSeries_CompressBars.Name = "ctxDataSeries_CompressBars";
      this.ctxDataSeries_CompressBars.Size = new Size(161, 22);
      this.ctxDataSeries_CompressBars.Text = "Compress Bars...";
      this.ctxDataSeries_CompressBars.Click += new EventHandler(this.ctxDataSeries_CompressBars_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(158, 6);
      this.ctxDataSeries_Clear.Name = "ctxDataSeries_Clear";
      this.ctxDataSeries_Clear.Size = new Size(161, 22);
      this.ctxDataSeries_Clear.Text = "Clear";
      this.ctxDataSeries_Clear.Click += new EventHandler(this.ctxDataSeries_Clear_Click);
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new Size(158, 6);
      this.ctxDataSeries_Delete.Name = "ctxDataSeries_Delete";
      this.ctxDataSeries_Delete.Size = new Size(161, 22);
      this.ctxDataSeries_Delete.Text = "Delete";
      this.ctxDataSeries_Delete.Click += new EventHandler(this.ctxDataSeries_Delete_Click);
      this.imgDataSeries.ImageStream = (ImageListStreamer) componentResourceManager.GetObject("imgDataSeries.ImageStream");
      this.imgDataSeries.TransparentColor = Color.Transparent;
      this.imgDataSeries.Images.SetKeyName(0, "data.png");
      this.toolStrip.Items.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.tsbRefresh
      });
      this.toolStrip.Location = new Point(0, 0);
      this.toolStrip.Name = "toolStrip";
      this.toolStrip.Size = new Size(592, 25);
      this.toolStrip.TabIndex = 3;
      this.toolStrip.Text = "toolStrip1";
      this.tsbRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
   //   this.tsbRefresh.Image = (Image) Resources.refresh;
      this.tsbRefresh.ImageTransparentColor = Color.Magenta;
      this.tsbRefresh.Name = "tsbRefresh";
      this.tsbRefresh.Size = new Size(23, 22);
      this.tsbRefresh.Text = "Refresh";
      this.tsbRefresh.Click += new EventHandler(this.tsbRefresh_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.tabControl2);
      this.Controls.Add((Control) this.splitter1);
      this.Controls.Add((Control) this.tabControl1);
      this.Controls.Add((Control) this.toolStrip);
      this.Name = "DataManager";
      this.Size = new Size(592, 400);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.tabDataSeries.ResumeLayout(false);
      this.ctxDataSeries.ResumeLayout(false);
      this.toolStrip.ResumeLayout(false);
      this.toolStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
