// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.Import.Historical.ImportHistoricalData
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using SmartQuant.Controls;
//using SmartQuant.Controls.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data.Import.Historical
{
  public class ImportHistoricalData : FrameworkControl
  {
    private IHistoricalDataProvider provider;
    private HashSet<Instrument> instruments;
    private Dictionary<string, ImportTaskViewItem> taskItems;
    private List<HistoricalDataRequest> requests;
    private HashSet<string> workingRequests;
    private Dictionary<string, Quote> lastQuotes;
    private IContainer components;
    private ToolStrip toolStrip1;
    private StatusStrip statusStrip1;
    private GroupBox gbxSettings;
    private ListView ltvImportTasks;
    private ColumnHeader columnHeader1;
    private ToolStripButton tsbInstrumentAdd;
    private ToolStripButton tsbInstrumentRemove;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton tsbImport_Start;
    private ToolStripButton tsbImport_Stop;
    private DateTimePicker dtpTo;
    private DateTimePicker dtpFrom;
    private Label label3;
    private Label label2;
    private ComboBox cbxDataType;
    private Label label1;
    private NumericUpDown nudBarSize;
    private Label label4;
    private BackgroundWorker worker;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ToolStripStatusLabel tsiInfo;

    public ImportHistoricalData()
    {
      this.InitializeComponent();
      this.instruments = new HashSet<Instrument>();
      this.taskItems = new Dictionary<string, ImportTaskViewItem>();
      this.requests = new List<HistoricalDataRequest>();
      this.workingRequests = new HashSet<string>();
      this.lastQuotes = new Dictionary<string, Quote>();
      this.cbxDataType.BeginUpdate();
      this.cbxDataType.Items.Clear();
      this.cbxDataType.Items.Add((object) new DataTypeItem((byte) 4));
      this.cbxDataType.Items.Add((object) new DataTypeItem((byte) 5));
      foreach (BarType barType in Enum.GetValues(typeof (BarType)))
        this.cbxDataType.Items.Add((object) new DataTypeItem((byte) 6, new BarType?(barType)));
      this.cbxDataType.SelectedIndex = 0;
      this.cbxDataType.EndUpdate();
      this.nudBarSize.Minimum = new Decimal(0);
      this.nudBarSize.Maximum = new Decimal(-1, -1, -1, false, (byte) 0);
      this.nudBarSize.Value = new Decimal(60);
      string str = CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern;
      if (str.Contains(".fff"))
        str = str.Replace(".fff", "");
      this.dtpFrom.CustomFormat = str;
      this.dtpTo.CustomFormat = str;
      this.dtpFrom.Value = DateTime.Today.AddDays(-1.0);
      this.dtpTo.Value = DateTime.Today;
    }

    protected override void OnInit()
    {
      this.provider = (IHistoricalDataProvider) this.args[0];
      this.framework.EventManager.Dispatcher.HistoricalData += new HistoricalDataEventHandler(this.Dispatcher_HistoricalData);
      this.framework.EventManager.Dispatcher.HistoricalDataEnd += new HistoricalDataEndEventHandler(this.Dispatcher_HistoricalDataEnd);
    }

    protected override void OnClosing(CancelEventArgs args)
    {
      this.framework.EventManager.Dispatcher.HistoricalData -= new HistoricalDataEventHandler(this.Dispatcher_HistoricalData);
      this.framework.EventManager.Dispatcher.HistoricalDataEnd -= new HistoricalDataEndEventHandler(this.Dispatcher_HistoricalDataEnd);
    }

    private void tsbInstrumentAdd_Click(object sender, EventArgs e)
    {
      int num = (int) MessageBox.Show((IWin32Window) this, "Drag-drop instruments from Instruments window.", this.GetMessageBoxCaption(), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
    }

    private void tsbInstrumentRemove_Click(object sender, EventArgs e)
    {
      List<ImportTaskViewItem> list = new List<ImportTaskViewItem>();
      foreach (ImportTaskViewItem importTaskViewItem in this.ltvImportTasks.SelectedItems)
        list.Add(importTaskViewItem);
      if (list.Count == 0)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, "No instrument(s) selected.", this.GetMessageBoxCaption(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else
      {
        if (MessageBox.Show((IWin32Window) this, "Do you really want to remove selected instrument(s) ?", this.GetMessageBoxCaption(), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
          return;
        foreach (ImportTaskViewItem importTaskViewItem in list)
        {
          importTaskViewItem.Remove();
          this.instruments.Remove(importTaskViewItem.Task.Instrument);
        }
      }
    }

    private void tsbImport_Start_Click(object sender, EventArgs e)
    {
      if (this.instruments.Count == 0)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, "Please, add instrument(s).", this.GetMessageBoxCaption(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else
      {
        this.tsbInstrumentAdd.Enabled = false;
        this.tsbInstrumentRemove.Enabled = false;
        this.tsbImport_Start.Enabled = false;
        this.tsbImport_Stop.Enabled = true;
        this.gbxSettings.Enabled = false;
        this.taskItems.Clear();
        this.requests.Clear();
        this.workingRequests.Clear();
        this.lastQuotes.Clear();
        foreach (ImportTaskViewItem importTaskViewItem in this.ltvImportTasks.Items)
        {
          HistoricalDataRequest historicalDataRequest = new HistoricalDataRequest();
          historicalDataRequest.RequestId = Guid.NewGuid().ToString();
          historicalDataRequest.Instrument = importTaskViewItem.Task.Instrument;
          historicalDataRequest.DataType = ((DataTypeItem) this.cbxDataType.SelectedItem).DataType;
          historicalDataRequest.BarType = ((DataTypeItem) this.cbxDataType.SelectedItem).BarType;
          if (historicalDataRequest.BarType.HasValue)
            historicalDataRequest.BarSize = new long?((long) this.nudBarSize.Value);
          historicalDataRequest.DateTime1 = this.dtpFrom.Value;
          historicalDataRequest.DateTime2 = this.dtpTo.Value;
          importTaskViewItem.Task.State = ImportTaskState.Pending;
          importTaskViewItem.Task.Count = 0;
          importTaskViewItem.Task.TotalNum = 0;
          importTaskViewItem.Task.Message = string.Empty;
          this.taskItems.Add(historicalDataRequest.RequestId, importTaskViewItem);
          this.requests.Add(historicalDataRequest);
        }
        this.worker.RunWorkerAsync((object) this.requests);
      }
    }

    private void tsbImport_Stop_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show((IWin32Window) this, "Do you really want to stop the process?", this.GetMessageBoxCaption(), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        return;
      this.tsbImport_Stop.Enabled = false;
      this.requests.Clear();
      this.worker.CancelAsync();
      string[] array;
      lock (this.workingRequests)
      {
        array = new string[this.workingRequests.Count];
        this.workingRequests.CopyTo(array);
      }
      foreach (string requestId in array)
        this.provider.Cancel(requestId);
    }

    private void cbxDataType_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.nudBarSize.Enabled = (int) ((DataTypeItem) this.cbxDataType.SelectedItem).DataType == 6;
    }

    private void ltvImportTasks_DragOver(object sender, DragEventArgs e)
    {
      if (this.worker.IsBusy || !e.Data.GetDataPresent(typeof (InstrumentList)))
        return;
      e.Effect = DragDropEffects.Copy;
    }

    private void ltvImportTasks_DragDrop(object sender, DragEventArgs e)
    {
      if (this.worker.IsBusy || !e.Data.GetDataPresent(typeof (InstrumentList)))
        return;
      foreach (Instrument instrument in (InstrumentList) e.Data.GetData(typeof (InstrumentList)))
        this.AddInstrument(instrument);
    }

    private void AddInstrument(Instrument instrument)
    {
      if (!this.instruments.Add(instrument))
        return;
      this.ltvImportTasks.Items.Add((ListViewItem) new ImportTaskViewItem(new ImportTask(instrument)));
    }

    private void Dispatcher_HistoricalData(object sender, HistoricalDataEventArgs args)
    {
      HistoricalData data = args.Data;
      ImportTaskViewItem importTaskViewItem;
      if (!this.taskItems.TryGetValue(data.RequestId, out importTaskViewItem))
        return;
      foreach (SmartQuant.DataObject dataObject in data.Objects)
      {
        if (dataObject is Quote)
        {
          Quote quote1 = (Quote) dataObject;
          Quote quote2;
          lock (this.lastQuotes)
          {
            if (!this.lastQuotes.TryGetValue(data.RequestId, out quote2))
            {
              quote2 = new Quote(new Bid(), new Ask());
              this.lastQuotes.Add(data.RequestId, quote2);
            }
          }
          if (quote1.Bid.Price != quote2.Bid.Price || quote1.Bid.Size != quote2.Bid.Size)
            this.framework.DataManager.Save(importTaskViewItem.Task.Instrument, (SmartQuant.DataObject) new Bid(quote1.Bid), SaveMode.Add);
          if (quote1.Ask.Price != quote2.Ask.Price || quote1.Ask.Size != quote2.Ask.Size)
            this.framework.DataManager.Save(importTaskViewItem.Task.Instrument, (SmartQuant.DataObject) new Ask(quote1.Ask), SaveMode.Add);
          quote2.Bid.Price = quote1.Bid.Price;
          quote2.Bid.Size = quote1.Bid.Size;
          quote2.Ask.Price = quote1.Ask.Price;
          quote2.Ask.Size = quote1.Ask.Size;
        }
        else
          this.framework.DataManager.Save(importTaskViewItem.Task.Instrument, dataObject, SaveMode.Add);
      }
      importTaskViewItem.Task.TotalNum = data.TotalNum;
      importTaskViewItem.Task.Count += data.Objects.Length;
    }

    private void Dispatcher_HistoricalDataEnd(object sender, HistoricalDataEndEventArgs args)
    {
      HistoricalDataEnd end = args.End;
      ImportTaskViewItem importTaskViewItem;
      if (!this.taskItems.TryGetValue(end.RequestId, out importTaskViewItem))
        return;
      switch (end.Result)
      {
        case RequestResult.Completed:
          importTaskViewItem.Task.State = ImportTaskState.Completed;
          break;
        case RequestResult.Cancelled:
          importTaskViewItem.Task.State = ImportTaskState.Cancelled;
          break;
        case RequestResult.Error:
          importTaskViewItem.Task.State = ImportTaskState.Error;
          break;
      }
      importTaskViewItem.Task.Message = end.Text;
      lock (this.workingRequests)
        this.workingRequests.Remove(end.RequestId);
    }

    private void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.UpdateThread), (object) cancellationTokenSource.Token);
      int num = 0;
      while (!this.worker.CancellationPending)
      {
        if (this.workingRequests.Count == 1)
          Thread.Sleep(TimeSpan.FromMilliseconds(1.0));
        else if (num < this.requests.Count)
        {
          HistoricalDataRequest historicalDataRequest = this.requests[num++];
          WaitCallback callBack = (WaitCallback) (obj => this.provider.Send((HistoricalDataRequest) obj));
          lock (this.workingRequests)
            this.workingRequests.Add(historicalDataRequest.RequestId);
          this.taskItems[historicalDataRequest.RequestId].Task.State = ImportTaskState.Processing;
          ThreadPool.QueueUserWorkItem(callBack, (object) historicalDataRequest);
        }
        else
          goto label_13;
      }
      e.Cancel = true;
label_13:
      while (this.workingRequests.Count > 0)
        Thread.Sleep(TimeSpan.FromMilliseconds(1.0));
      cancellationTokenSource.Cancel();
    }

    private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      this.InvokeAction((Action) (() =>
      {
        if (e.Error != null)
          this.tsiInfo.Text = e.Error.Message;
        else if (e.Cancelled)
          this.tsiInfo.Text = "Cancelled";
        else
          this.tsiInfo.Text = "Completed";
        this.tsbInstrumentAdd.Enabled = true;
        this.tsbInstrumentRemove.Enabled = true;
        this.tsbImport_Start.Enabled = true;
        this.tsbImport_Stop.Enabled = false;
        this.gbxSettings.Enabled = true;
      }));
    }

    private void UpdateThread(object obj)
    {
      CancellationToken cancellationToken = (CancellationToken) obj;
      while (!cancellationToken.IsCancellationRequested)
      {
        this.UpdateAllItems();
        Thread.Sleep(TimeSpan.FromSeconds(1.0));
      }
      this.UpdateAllItems();
    }

    private void UpdateAllItems()
    {
      foreach (ImportTaskViewItem importTaskViewItem in this.taskItems.Values)
      {
        ImportTaskViewItem item = importTaskViewItem;
        this.InvokeAction((Action) (() => item.Update()));
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
      this.toolStrip1 = new ToolStrip();
      this.tsbInstrumentAdd = new ToolStripButton();
      this.tsbInstrumentRemove = new ToolStripButton();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.tsbImport_Start = new ToolStripButton();
      this.tsbImport_Stop = new ToolStripButton();
      this.statusStrip1 = new StatusStrip();
      this.tsiInfo = new ToolStripStatusLabel();
      this.gbxSettings = new GroupBox();
      this.nudBarSize = new NumericUpDown();
      this.label4 = new Label();
      this.dtpTo = new DateTimePicker();
      this.dtpFrom = new DateTimePicker();
      this.label3 = new Label();
      this.label2 = new Label();
      this.cbxDataType = new ComboBox();
      this.label1 = new Label();
      this.ltvImportTasks = new ListView();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.worker = new BackgroundWorker();
      this.toolStrip1.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.gbxSettings.SuspendLayout();
      this.nudBarSize.BeginInit();
      this.SuspendLayout();
      this.toolStrip1.Items.AddRange(new ToolStripItem[5]
      {
        (ToolStripItem) this.tsbInstrumentAdd,
        (ToolStripItem) this.tsbInstrumentRemove,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.tsbImport_Start,
        (ToolStripItem) this.tsbImport_Stop
      });
      this.toolStrip1.Location = new Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new Size(583, 25);
      this.toolStrip1.TabIndex = 0;
      this.toolStrip1.Text = "toolStrip1";
      this.tsbInstrumentAdd.DisplayStyle = ToolStripItemDisplayStyle.Image;
     // this.tsbInstrumentAdd.Image = (Image) Resources.instrument_add;
      this.tsbInstrumentAdd.ImageTransparentColor = Color.Magenta;
      this.tsbInstrumentAdd.Name = "tsbInstrumentAdd";
      this.tsbInstrumentAdd.Size = new Size(23, 22);
      this.tsbInstrumentAdd.Text = "Add instrument(s)";
      this.tsbInstrumentAdd.Click += new EventHandler(this.tsbInstrumentAdd_Click);
      this.tsbInstrumentRemove.DisplayStyle = ToolStripItemDisplayStyle.Image;
    //  this.tsbInstrumentRemove.Image = (Image) Resources.instrument_delete;
      this.tsbInstrumentRemove.ImageTransparentColor = Color.Magenta;
      this.tsbInstrumentRemove.Name = "tsbInstrumentRemove";
      this.tsbInstrumentRemove.Size = new Size(23, 22);
      this.tsbInstrumentRemove.Text = "Remove instrument(s)";
      this.tsbInstrumentRemove.Click += new EventHandler(this.tsbInstrumentRemove_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(6, 25);
      this.tsbImport_Start.DisplayStyle = ToolStripItemDisplayStyle.Image;
   //   this.tsbImport_Start.Image = (Image) Resources.import_start;
      this.tsbImport_Start.ImageTransparentColor = Color.Magenta;
      this.tsbImport_Start.Name = "tsbImport_Start";
      this.tsbImport_Start.Size = new Size(23, 22);
      this.tsbImport_Start.Text = "Start";
      this.tsbImport_Start.Click += new EventHandler(this.tsbImport_Start_Click);
      this.tsbImport_Stop.DisplayStyle = ToolStripItemDisplayStyle.Image;
      this.tsbImport_Stop.Enabled = false;
    //  this.tsbImport_Stop.Image = (Image) Resources.import_stop;
      this.tsbImport_Stop.ImageTransparentColor = Color.Magenta;
      this.tsbImport_Stop.Name = "tsbImport_Stop";
      this.tsbImport_Stop.Size = new Size(23, 22);
      this.tsbImport_Stop.Text = "Stop";
      this.tsbImport_Stop.Click += new EventHandler(this.tsbImport_Stop_Click);
      this.statusStrip1.Items.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.tsiInfo
      });
      this.statusStrip1.Location = new Point(0, 366);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new Size(583, 22);
      this.statusStrip1.SizingGrip = false;
      this.statusStrip1.TabIndex = 1;
      this.statusStrip1.Text = "statusStrip1";
      this.tsiInfo.AutoToolTip = true;
      this.tsiInfo.Name = "tsiInfo";
      this.tsiInfo.Size = new Size(568, 17);
      this.tsiInfo.Spring = true;
      this.tsiInfo.TextAlign = ContentAlignment.MiddleLeft;
      this.gbxSettings.Controls.Add((Control) this.nudBarSize);
      this.gbxSettings.Controls.Add((Control) this.label4);
      this.gbxSettings.Controls.Add((Control) this.dtpTo);
      this.gbxSettings.Controls.Add((Control) this.dtpFrom);
      this.gbxSettings.Controls.Add((Control) this.label3);
      this.gbxSettings.Controls.Add((Control) this.label2);
      this.gbxSettings.Controls.Add((Control) this.cbxDataType);
      this.gbxSettings.Controls.Add((Control) this.label1);
      this.gbxSettings.Dock = DockStyle.Top;
      this.gbxSettings.Location = new Point(0, 25);
      this.gbxSettings.Name = "gbxSettings";
      this.gbxSettings.Size = new Size(583, 79);
      this.gbxSettings.TabIndex = 2;
      this.gbxSettings.TabStop = false;
      this.gbxSettings.Text = "Settings";
      this.nudBarSize.Location = new Point(80, 48);
      this.nudBarSize.Name = "nudBarSize";
      this.nudBarSize.Size = new Size(100, 20);
      this.nudBarSize.TabIndex = 7;
      this.nudBarSize.TextAlign = HorizontalAlignment.Right;
      this.nudBarSize.ThousandsSeparator = true;
      this.label4.Location = new Point(16, 48);
      this.label4.Name = "label4";
      this.label4.Size = new Size(58, 20);
      this.label4.TabIndex = 6;
      this.label4.Text = "Bar size";
      this.label4.TextAlign = ContentAlignment.MiddleLeft;
      this.dtpTo.Format = DateTimePickerFormat.Custom;
      this.dtpTo.Location = new Point(264, 48);
      this.dtpTo.Name = "dtpTo";
      this.dtpTo.Size = new Size(200, 20);
      this.dtpTo.TabIndex = 5;
      this.dtpFrom.Format = DateTimePickerFormat.Custom;
      this.dtpFrom.Location = new Point(264, 24);
      this.dtpFrom.Name = "dtpFrom";
      this.dtpFrom.Size = new Size(200, 20);
      this.dtpFrom.TabIndex = 4;
      this.label3.Location = new Point(220, 48);
      this.label3.Name = "label3";
      this.label3.Size = new Size(40, 20);
      this.label3.TabIndex = 3;
      this.label3.Text = "To";
      this.label3.TextAlign = ContentAlignment.MiddleLeft;
      this.label2.Location = new Point(220, 24);
      this.label2.Name = "label2";
      this.label2.Size = new Size(40, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "From";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      this.cbxDataType.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbxDataType.FormattingEnabled = true;
      this.cbxDataType.Location = new Point(80, 24);
      this.cbxDataType.Name = "cbxDataType";
      this.cbxDataType.Size = new Size(100, 21);
      this.cbxDataType.TabIndex = 1;
      this.cbxDataType.SelectedIndexChanged += new EventHandler(this.cbxDataType_SelectedIndexChanged);
      this.label1.Location = new Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new Size(58, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Data type";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      this.ltvImportTasks.AllowDrop = true;
      this.ltvImportTasks.Columns.AddRange(new ColumnHeader[4]
      {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4
      });
      this.ltvImportTasks.Dock = DockStyle.Fill;
      this.ltvImportTasks.FullRowSelect = true;
      this.ltvImportTasks.GridLines = true;
      this.ltvImportTasks.HeaderStyle = ColumnHeaderStyle.Nonclickable;
      this.ltvImportTasks.HideSelection = false;
      this.ltvImportTasks.LabelWrap = false;
      this.ltvImportTasks.Location = new Point(0, 104);
      this.ltvImportTasks.Name = "ltvImportTasks";
      this.ltvImportTasks.ShowGroups = false;
      this.ltvImportTasks.ShowItemToolTips = true;
      this.ltvImportTasks.Size = new Size(583, 262);
      this.ltvImportTasks.TabIndex = 3;
      this.ltvImportTasks.UseCompatibleStateImageBehavior = false;
      this.ltvImportTasks.View = View.Details;
      this.ltvImportTasks.DragDrop += new DragEventHandler(this.ltvImportTasks_DragDrop);
      this.ltvImportTasks.DragOver += new DragEventHandler(this.ltvImportTasks_DragOver);
      this.columnHeader1.Text = "Instrument";
      this.columnHeader1.Width = 97;
      this.columnHeader2.Text = "State";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 93;
      this.columnHeader3.Text = "Count";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 173;
      this.columnHeader4.Text = "Message";
      this.columnHeader4.Width = 187;
      this.worker.WorkerSupportsCancellation = true;
      this.worker.DoWork += new DoWorkEventHandler(this.worker_DoWork);
      this.worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.ltvImportTasks);
      this.Controls.Add((Control) this.gbxSettings);
      this.Controls.Add((Control) this.statusStrip1);
      this.Controls.Add((Control) this.toolStrip1);
      this.Name = "ImportHistoricalData";
      this.Size = new Size(583, 388);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.gbxSettings.ResumeLayout(false);
      this.nudBarSize.EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
