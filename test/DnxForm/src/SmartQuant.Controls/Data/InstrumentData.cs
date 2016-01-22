using SmartQuant;
using SmartQuant.Controls;
//using SmartQuant.Controls.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  public class InstrumentData : FrameworkControl
  {
    private Instrument instrument;
    private IContainer components;
    private SplitContainer splitContainer1;
    private TabControl tabControl2;
    private TabPage tabData;
    private ImageList images;
    private ListViewNB ltvDataSeries;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private DataSeriesViewer dataSeriesViewer;
    private ContextMenuStrip ctxDataSeries;
    private ToolStripMenuItem ctxDataSeries_Clear;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem ctxDataSeries_Delete;
    private ToolStripMenuItem ctxDataSeries_Refresh;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem ctxDataSeries_New;
    private ToolStripMenuItem ctxDataSeries_New_Bar;
    private ToolStripMenuItem ctxDataSeries_New_Trade;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem ctxDataSeries_New_Quote;
    private ToolStripMenuItem toolStripMenuItem1;
    private ToolStripMenuItem ctxDataSeries_ExportCSV;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripMenuItem ctxDataSeries_CompressBars;
    private ToolStripSeparator toolStripSeparator5;
    private ToolStrip toolStrip1;
    private ToolStripButton tsbRefresh;
    private ToolStripMenuItem ctxDataSeries_Dump;

    public event EventHandler<DataSeriesListEventArgs> CompressBars;

    public event EventHandler<DataSeriesListEventArgs> ExportToCSV;

    public InstrumentData()
    {
      this.InitializeComponent();
    }

    protected override void OnInit()
    {
      this.instrument = (Instrument) this.args[0];
      this.InitDataSeriesList();
      this.InitDataSeriesViewer();
      this.Text = string.Format("Data [{0}]", (object) this.instrument.Symbol);
    }

    private void InitDataSeriesList()
    {
      this.ltvDataSeries.BeginUpdate();
      this.ltvDataSeries.Items.Clear();
      foreach (DataSeries dataSeries in this.framework.DataManager.GetDataSeriesList(this.instrument, (string) null))
      {
        byte dataType = DataSeriesNameHelper.GetDataType(dataSeries);
        if ((int) dataType == 6)
        {
          BarType barType;
          long barSize;
          if (DataSeriesNameHelper.TryGetBarTypeSize(dataSeries, out barType, out barSize))
            this.ltvDataSeries.Items.Add((ListViewItem) new DataSeriesViewItem(dataSeries, dataType, new BarType?(barType), new long?(barSize)));
        }
        else
          this.ltvDataSeries.Items.Add((ListViewItem) new DataSeriesViewItem(dataSeries, dataType, new BarType?(), new long?()));
      }
      this.ltvDataSeries.EndUpdate();
    }

    private void InitDataSeriesViewer()
    {
      this.dataSeriesViewer.SetInstrument(this.instrument);
      this.dataSeriesViewer.SetDataSeries((DataSeries) null, new byte?());
    }

    private void ltvDataSeries_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.ltvDataSeries.SelectedItems.Count == 1)
      {
        DataSeriesViewItem dataSeriesViewItem = this.ltvDataSeries.SelectedItems[0] as DataSeriesViewItem;
        this.dataSeriesViewer.SetDataSeries(dataSeriesViewItem.DataSeries, new byte?(dataSeriesViewItem.DataType));
      }
      else
        this.dataSeriesViewer.SetDataSeries((DataSeries) null, new byte?());
    }

    private void tsbRefresh_Click(object sender, EventArgs e)
    {
      foreach (DataSeries dataSeries in this.framework.DataManager.GetDataSeriesList(this.instrument, (string) null))
        dataSeries.Refresh();
      this.InitDataSeriesList();
      this.InitDataSeriesViewer();
    }

    private void ctxDataSeries_Opening(object sender, CancelEventArgs e)
    {
      switch (this.ltvDataSeries.SelectedItems.Count)
      {
        case 0:
          this.ctxDataSeries_New.Enabled = true;
          this.ctxDataSeries_Refresh.Enabled = true;
          this.ctxDataSeries_Dump.Enabled = false;
          this.ctxDataSeries_ExportCSV.Enabled = false;
          this.ctxDataSeries_CompressBars.Enabled = false;
          this.ctxDataSeries_Clear.Enabled = false;
          this.ctxDataSeries_Delete.Enabled = false;
          break;
        case 1:
          this.ctxDataSeries_New.Enabled = true;
          this.ctxDataSeries_Refresh.Enabled = true;
          this.ctxDataSeries_Dump.Enabled = true;
          this.ctxDataSeries_ExportCSV.Enabled = true;
          this.ctxDataSeries_CompressBars.Enabled = true;
          this.ctxDataSeries_Clear.Enabled = true;
          this.ctxDataSeries_Delete.Enabled = true;
          break;
        default:
          this.ctxDataSeries_New.Enabled = true;
          this.ctxDataSeries_Refresh.Enabled = true;
          this.ctxDataSeries_Dump.Enabled = false;
          this.ctxDataSeries_ExportCSV.Enabled = true;
          this.ctxDataSeries_CompressBars.Enabled = false;
          this.ctxDataSeries_Clear.Enabled = true;
          this.ctxDataSeries_Delete.Enabled = true;
          break;
      }
    }

    private void ctxDataSeries_Refresh_Click(object sender, EventArgs e)
    {
      this.InitDataSeriesList();
      this.InitDataSeriesViewer();
    }

    private void ctxDataSeries_Dump_Click(object sender, EventArgs e)
    {
      ((DataSeriesViewItem) this.ltvDataSeries.SelectedItems[0]).DataSeries.Dump();
    }

    private void ctxDataSeries_ExportCSV_Click(object sender, EventArgs e)
    {
      List<DataSeries> list = new List<DataSeries>();
      foreach (DataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
      {
        switch (DataSeriesNameHelper.GetDataType(dataSeriesViewItem.DataSeries))
        {
          case 2:
          case 3:
          case 4:
          case 5:
          case 6:
            list.Add(dataSeriesViewItem.DataSeries);
            continue;
          default:
            int num = (int) MessageBox.Show((IWin32Window) this, string.Format("Cannot export {0} series to CSV format.", (object) DataTypeConverter.Convert(dataSeriesViewItem.DataSeries)), "Export To CSV", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            continue;
        }
      }
      if (list.Count <= 0 || this.ExportToCSV == null)
        return;
      this.ExportToCSV((object) this, new DataSeriesListEventArgs(list.ToArray()));
    }

    private void ctxDataSeries_CompressBars_Click(object sender, EventArgs e)
    {
      DataSeries dataSeries = (this.ltvDataSeries.SelectedItems[0] as DataSeriesViewItem).DataSeries;
      byte dataType = DataSeriesNameHelper.GetDataType(dataSeries);
      switch (dataType)
      {
        case 2:
        case 3:
        case 4:
        case 5:
        case 6:
          if (this.CompressBars == null)
            break;
          this.CompressBars((object) this, new DataSeriesListEventArgs(new DataSeries[1]
          {
            dataSeries
          }));
          break;
        default:
          int num = (int) MessageBox.Show((IWin32Window) this, string.Format("Cannot compress bars from {0} series.", (object) DataTypeConverter.Convert(dataType, new BarType?(), new long?())), "Compress Bars", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          break;
      }
    }

    private void ctxDataSeries_Clear_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show((IWin32Window) this, "Do you really want to clear selected series?", "Clear Series", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      foreach (DataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
        dataSeriesViewItem.DataSeries.Clear();
      this.InitDataSeriesList();
      this.InitDataSeriesViewer();
    }

    private void ctxDataSeries_Delete_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show((IWin32Window) this, "Do you really want to delete selected series?", "Delete Series", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
        return;
      foreach (DataSeriesViewItem dataSeriesViewItem in this.ltvDataSeries.SelectedItems)
        Framework.Current.DataManager.DeleteDataSeries(dataSeriesViewItem.DataSeries.Name);
      this.InitDataSeriesList();
      this.InitDataSeriesViewer();
    }

    private void ctxDataSeries_New_Bar_DropDownOpening(object sender, EventArgs e)
    {
      this.ctxDataSeries_New_Bar.DropDownItems.Clear();
      this.ctxDataSeries_New_Bar.DropDownItems.AddRange(new ToolStripItem[13]
      {
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 60L),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 300L),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 600L),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 1800L),
        (ToolStripItem) new ToolStripSeparator(),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 3600L),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 10800L),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Time, 21600L),
        (ToolStripItem) new ToolStripSeparator(),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Tick, 50L),
        (ToolStripItem) new BarSeriesMenuItem(BarType.Tick, 100L),
        (ToolStripItem) new ToolStripSeparator(),
        (ToolStripItem) new CustomBarSeriesMenuItem()
      });
    }

    private void ctxDataSeries_New_Bar_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      BarSeriesMenuItem barSeriesMenuItem = (BarSeriesMenuItem) e.ClickedItem;
      this.ctxDataSeries.Close();
      if (!barSeriesMenuItem.CreateSeries)
        return;
      this.NewDataSeries((byte) 6, new BarType?(barSeriesMenuItem.BarType), new long?(barSeriesMenuItem.BarSize));
    }

    private void ctxDataSeries_New_Trade_Click(object sender, EventArgs e)
    {
      this.NewDataSeries((byte) 4, new BarType?(), new long?());
    }

    private void ctxDataSeries_New_Quote_Click(object sender, EventArgs e)
    {
      this.NewDataSeries((byte) 5, new BarType?(), new long?());
    }

    private void NewDataSeries(byte dataType, BarType? barType, long? barSize)
    {
      string name = (int) dataType == 6 ? DataSeriesNameHelper.GetName(this.instrument, barType.Value, barSize.Value) : DataSeriesNameHelper.GetName(this.instrument, dataType);
      if (this.framework.DataServer.GetDataSeries(name) == null)
      {
        this.framework.DataServer.AddDataSeries(name);
        this.InitDataSeriesList();
        this.InitDataSeriesViewer();
      }
      else
      {
        int num = (int) MessageBox.Show((IWin32Window) this, "The series already exists.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (InstrumentData));
      this.splitContainer1 = new SplitContainer();
      this.ltvDataSeries = new ListViewNB();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.ctxDataSeries = new ContextMenuStrip(this.components);
      this.ctxDataSeries_New = new ToolStripMenuItem();
      this.ctxDataSeries_New_Bar = new ToolStripMenuItem();
      this.toolStripMenuItem1 = new ToolStripMenuItem();
      this.ctxDataSeries_New_Trade = new ToolStripMenuItem();
      this.ctxDataSeries_New_Quote = new ToolStripMenuItem();
      this.toolStripSeparator3 = new ToolStripSeparator();
      this.ctxDataSeries_Refresh = new ToolStripMenuItem();
      this.toolStripSeparator2 = new ToolStripSeparator();
      this.ctxDataSeries_Dump = new ToolStripMenuItem();
      this.ctxDataSeries_ExportCSV = new ToolStripMenuItem();
      this.toolStripSeparator4 = new ToolStripSeparator();
      this.ctxDataSeries_CompressBars = new ToolStripMenuItem();
      this.toolStripSeparator5 = new ToolStripSeparator();
      this.ctxDataSeries_Clear = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.ctxDataSeries_Delete = new ToolStripMenuItem();
      this.images = new ImageList(this.components);
      this.tabControl2 = new TabControl();
      this.tabData = new TabPage();
      this.dataSeriesViewer = new DataSeriesViewer();
      this.toolStrip1 = new ToolStrip();
      this.tsbRefresh = new ToolStripButton();
      this.splitContainer1.BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.ctxDataSeries.SuspendLayout();
      this.tabControl2.SuspendLayout();
      this.tabData.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      this.splitContainer1.Dock = DockStyle.Fill;
      this.splitContainer1.Location = new Point(0, 25);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = Orientation.Horizontal;
      this.splitContainer1.Panel1.Controls.Add((Control) this.ltvDataSeries);
      this.splitContainer1.Panel1.Padding = new Padding(4);
      this.splitContainer1.Panel2.Controls.Add((Control) this.tabControl2);
      this.splitContainer1.Panel2.Padding = new Padding(4);
      this.splitContainer1.Size = new Size(584, 442);
      this.splitContainer1.SplitterDistance = 114;
      this.splitContainer1.TabIndex = 0;
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
      this.ltvDataSeries.Location = new Point(4, 4);
      this.ltvDataSeries.Name = "ltvDataSeries";
      this.ltvDataSeries.ShowGroups = false;
      this.ltvDataSeries.Size = new Size(576, 106);
      this.ltvDataSeries.SmallImageList = this.images;
      this.ltvDataSeries.Sorting = SortOrder.Ascending;
      this.ltvDataSeries.TabIndex = 1;
      this.ltvDataSeries.UseCompatibleStateImageBehavior = false;
      this.ltvDataSeries.View = View.Details;
      this.ltvDataSeries.SelectedIndexChanged += new EventHandler(this.ltvDataSeries_SelectedIndexChanged);
      this.columnHeader1.Text = "Data Series";
      this.columnHeader1.Width = 112;
      this.columnHeader2.Text = "Object Count";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 96;
      this.columnHeader3.Text = "First DateTime";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 144;
      this.columnHeader4.Text = "Last DateTime";
      this.columnHeader4.TextAlign = HorizontalAlignment.Right;
      this.columnHeader4.Width = 144;
      this.ctxDataSeries.Items.AddRange(new ToolStripItem[12]
      {
        (ToolStripItem) this.ctxDataSeries_New,
        (ToolStripItem) this.toolStripSeparator3,
        (ToolStripItem) this.ctxDataSeries_Refresh,
        (ToolStripItem) this.toolStripSeparator2,
        (ToolStripItem) this.ctxDataSeries_Dump,
        (ToolStripItem) this.ctxDataSeries_ExportCSV,
        (ToolStripItem) this.toolStripSeparator4,
        (ToolStripItem) this.ctxDataSeries_CompressBars,
        (ToolStripItem) this.toolStripSeparator5,
        (ToolStripItem) this.ctxDataSeries_Clear,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.ctxDataSeries_Delete
      });
      this.ctxDataSeries.Name = "ctxDataSeries";
      this.ctxDataSeries.Size = new Size(162, 210);
      this.ctxDataSeries.Opening += new CancelEventHandler(this.ctxDataSeries_Opening);
      this.ctxDataSeries_New.DropDownItems.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) this.ctxDataSeries_New_Bar,
        (ToolStripItem) this.ctxDataSeries_New_Trade,
        (ToolStripItem) this.ctxDataSeries_New_Quote
      });
      this.ctxDataSeries_New.Name = "ctxDataSeries_New";
      this.ctxDataSeries_New.Size = new Size(161, 22);
      this.ctxDataSeries_New.Text = "New Data Series";
      this.ctxDataSeries_New_Bar.DropDownItems.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.toolStripMenuItem1
      });
      this.ctxDataSeries_New_Bar.Name = "ctxDataSeries_New_Bar";
      this.ctxDataSeries_New_Bar.Size = new Size(152, 22);
      this.ctxDataSeries_New_Bar.Text = "Bar";
      this.ctxDataSeries_New_Bar.DropDownOpening += new EventHandler(this.ctxDataSeries_New_Bar_DropDownOpening);
      this.ctxDataSeries_New_Bar.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.ctxDataSeries_New_Bar_DropDownItemClicked);
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new Size(152, 22);
      this.toolStripMenuItem1.Text = "(Empty)";
      this.ctxDataSeries_New_Trade.Name = "ctxDataSeries_New_Trade";
      this.ctxDataSeries_New_Trade.Size = new Size(152, 22);
      this.ctxDataSeries_New_Trade.Text = "Trade";
      this.ctxDataSeries_New_Trade.Click += new EventHandler(this.ctxDataSeries_New_Trade_Click);
      this.ctxDataSeries_New_Quote.Name = "ctxDataSeries_New_Quote";
      this.ctxDataSeries_New_Quote.Size = new Size(152, 22);
      this.ctxDataSeries_New_Quote.Text = "Quote";
      this.ctxDataSeries_New_Quote.Click += new EventHandler(this.ctxDataSeries_New_Quote_Click);
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new Size(158, 6);
      this.ctxDataSeries_Refresh.Name = "ctxDataSeries_Refresh";
      this.ctxDataSeries_Refresh.Size = new Size(161, 22);
      this.ctxDataSeries_Refresh.Text = "Refresh";
      this.ctxDataSeries_Refresh.Click += new EventHandler(this.ctxDataSeries_Refresh_Click);
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new Size(158, 6);
      this.ctxDataSeries_Dump.Name = "ctxDataSeries_Dump";
      this.ctxDataSeries_Dump.Size = new Size(161, 22);
      this.ctxDataSeries_Dump.Text = "Dump";
      this.ctxDataSeries_Dump.Click += new EventHandler(this.ctxDataSeries_Dump_Click);
      this.ctxDataSeries_ExportCSV.Name = "ctxDataSeries_ExportCSV";
      this.ctxDataSeries_ExportCSV.Size = new Size(161, 22);
      this.ctxDataSeries_ExportCSV.Text = "Export To CSV...";
      this.ctxDataSeries_ExportCSV.Click += new EventHandler(this.ctxDataSeries_ExportCSV_Click);
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new Size(158, 6);
      this.ctxDataSeries_CompressBars.Name = "ctxDataSeries_CompressBars";
      this.ctxDataSeries_CompressBars.Size = new Size(161, 22);
      this.ctxDataSeries_CompressBars.Text = "Compress Bars...";
      this.ctxDataSeries_CompressBars.Click += new EventHandler(this.ctxDataSeries_CompressBars_Click);
      this.toolStripSeparator5.Name = "toolStripSeparator5";
      this.toolStripSeparator5.Size = new Size(158, 6);
      this.ctxDataSeries_Clear.Name = "ctxDataSeries_Clear";
      this.ctxDataSeries_Clear.Size = new Size(161, 22);
      this.ctxDataSeries_Clear.Text = "Clear";
      this.ctxDataSeries_Clear.Click += new EventHandler(this.ctxDataSeries_Clear_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(158, 6);
      this.ctxDataSeries_Delete.Name = "ctxDataSeries_Delete";
      this.ctxDataSeries_Delete.Size = new Size(161, 22);
      this.ctxDataSeries_Delete.Text = "Delete";
      this.ctxDataSeries_Delete.Click += new EventHandler(this.ctxDataSeries_Delete_Click);
      this.images.ImageStream = (ImageListStreamer) componentResourceManager.GetObject("images.ImageStream");
      this.images.TransparentColor = Color.Transparent;
      this.images.Images.SetKeyName(0, "data.png");
      this.tabControl2.Controls.Add((Control) this.tabData);
      this.tabControl2.Dock = DockStyle.Fill;
      this.tabControl2.Location = new Point(4, 4);
      this.tabControl2.Name = "tabControl2";
      this.tabControl2.SelectedIndex = 0;
      this.tabControl2.Size = new Size(576, 316);
      this.tabControl2.TabIndex = 0;
      this.tabData.Controls.Add((Control) this.dataSeriesViewer);
      this.tabData.Location = new Point(4, 22);
      this.tabData.Name = "tabData";
      this.tabData.Padding = new Padding(3);
      this.tabData.Size = new Size(568, 290);
      this.tabData.TabIndex = 0;
      this.tabData.Text = "Data";
      this.tabData.UseVisualStyleBackColor = true;
      this.dataSeriesViewer.Dock = DockStyle.Fill;
      this.dataSeriesViewer.Location = new Point(3, 3);
      this.dataSeriesViewer.Name = "dataSeriesViewer";
      this.dataSeriesViewer.Size = new Size(562, 284);
      this.dataSeriesViewer.TabIndex = 0;
      this.toolStrip1.Items.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.tsbRefresh
      });
      this.toolStrip1.Location = new Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new Size(584, 25);
      this.toolStrip1.TabIndex = 1;
      this.toolStrip1.Text = "toolStrip1";
      this.tsbRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
   //   this.tsbRefresh.Image = (Image) Resources.refresh;
      this.tsbRefresh.ImageTransparentColor = Color.Magenta;
      this.tsbRefresh.Name = "tsbRefresh";
      this.tsbRefresh.Size = new Size(23, 22);
      this.tsbRefresh.Text = "Refresh";
      this.tsbRefresh.Click += new EventHandler(this.tsbRefresh_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.splitContainer1);
      this.Controls.Add((Control) this.toolStrip1);
      this.Name = "InstrumentData";
      this.Size = new Size(584, 467);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ctxDataSeries.ResumeLayout(false);
      this.tabControl2.ResumeLayout(false);
      this.tabData.ResumeLayout(false);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
