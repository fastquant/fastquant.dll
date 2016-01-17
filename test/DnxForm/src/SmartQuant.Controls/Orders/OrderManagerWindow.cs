using SmartQuant.Controls.TradingTools;
using SmartQuant.ExcelLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SmartQuant.Controls.Orders
{
  public class OrderManagerWindow : FrameworkControl
  {
    private bool autoScrollingEnabled = true;
    private OrderedDictionary allOrders = new OrderedDictionary();
    private OrderedDictionary workingOrders = new OrderedDictionary();
    private OrderedDictionary filledOrders = new OrderedDictionary();
    private OrderedDictionary cancelledOrders = new OrderedDictionary();
    private OrderedDictionary rejectedOrders = new OrderedDictionary();
    private OrderFactory orderFactory = new OrderFactory();
    private HashSet<OrderViewItem> lastUpdatedOrderList = new HashSet<OrderViewItem>();
    private PermanentQueue<Event> messageQueue;
    private object propertyObject;
    private bool freezeSelection;
    private Order reportedOrder;
    private IContainer components;
    private ContextMenuStrip ctxOrder;
    private ToolStripMenuItem ctxOrder_Cancel;
    private ColumnHeader columnHeader27;
    private ColumnHeader columnHeader36;
    private ColumnHeader columnHeader45;
    private ColumnHeader columnHeader46;
    private ColumnHeader columnHeader47;
    private ColumnHeader columnHeader48;
    private ColumnHeader columnHeader49;
    private ColumnHeader columnHeader50;
    private ColumnHeader columnHeader51;
    private ToolStripMenuItem ctxOrder_Modify;
    private ToolStripSeparator toolStripSeparator1;
    private Panel panel2;
    private TabControl tabReports;
    private TabPage tabPage6;
    private ListViewNB ltvReports;
    private ColumnHeader columnHeader52;
    private ColumnHeader columnHeader55;
    private ColumnHeader columnHeader59;
    private ColumnHeader columnHeader56;
    private ColumnHeader columnHeader57;
    private ColumnHeader columnHeader68;
    private ColumnHeader columnHeader58;
    private ColumnHeader columnHeader60;
    private ColumnHeader columnHeader53;
    private ColumnHeader columnHeader54;
    private ColumnHeader columnHeader66;
    private ColumnHeader columnHeader67;
    private ColumnHeader columnHeader69;
    private ColumnHeader columnHeader71;
    private Panel panel1;
    private TabControl tabOrders;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TabPage tabPage3;
    private TabPage tabPage4;
    private TabPage tabPage5;
    private Splitter splitter1;
    private DataGridView dgvOrders;
    private DataGridView dgvWorkingOrders;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
    private DataGridView dgvFilledOrders;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn15;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn16;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn17;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn18;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn19;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn20;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn21;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn22;
    private DataGridView dgvCancelledOrders;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn23;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn24;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn25;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn26;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn27;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn28;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn29;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn30;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn31;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn32;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn33;
    private DataGridView dgvRejectedOrders;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn34;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn35;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn36;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn37;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn38;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn39;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn40;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn41;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn42;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn43;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn44;
    private DataGridViewTextBoxColumn Column1;
    private DataGridViewTextBoxColumn Column2;
    private DataGridViewTextBoxColumn Column3;
    private DataGridViewTextBoxColumn Column4;
    private DataGridViewTextBoxColumn Column5;
    private DataGridViewTextBoxColumn Column6;
    private DataGridViewTextBoxColumn Column7;
    private DataGridViewTextBoxColumn Column8;
    private DataGridViewTextBoxColumn Column9;
    private DataGridViewTextBoxColumn Column10;
    private DataGridViewTextBoxColumn Column11;
    private Label lblOrdersSetttings;
    private Label lblReportsSettings;
    private ContextMenuStrip ctxOrdersSettings;
    private ToolStripMenuItem ctxOrdersSettings_ExportToCSV;
    private ToolStripMenuItem ctxOrdersSettings_ExportToXLS;
    private ToolStripSeparator ctxOrdersSettings_Separator1;
    private ToolStripMenuItem ctxOrdersSettings_AutoScrolling;
    private ContextMenuStrip ctxReportsSettings;
    private ToolStripMenuItem ctxReportsSettings_ExportToCSV;
    private ToolStripMenuItem ctxReportsSettings_ExportToXLS;

    public override object PropertyObject
    {
      get
      {
        return this.propertyObject;
      }
    }

    public OrderManagerWindow()
    {
      this.InitializeComponent();
      this.SetupDgvCtx(this.dgvOrders);
      this.SetupDgvCtx(this.dgvWorkingOrders);
      this.SetupDgvCtx(this.dgvFilledOrders);
      this.SetupDgvCtx(this.dgvCancelledOrders);
      this.SetupDgvCtx(this.dgvRejectedOrders);
    }

    public void Init(PermanentQueue<Event> messages, bool createOrders)
    {
      this.messageQueue = messages;
      this.messageQueue.AddReader((object) this);
    }

    protected override void OnInit()
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new MethodInvoker(base.OnInit));
      }
      else
      {
        this.Clear();
        this.UpdateGUI();
      }
    }

    protected override void OnClosing(CancelEventArgs args)
    {
      this.messageQueue.RemoveReader((object) this);
      base.OnClosing(args);
    }

    private void Clear()
    {
      this.dgvOrders.Rows.Clear();
      this.dgvWorkingOrders.Rows.Clear();
      this.dgvFilledOrders.Rows.Clear();
      this.dgvCancelledOrders.Rows.Clear();
      this.dgvRejectedOrders.Rows.Clear();
      this.dgvOrders.RowCount = 0;
      this.dgvWorkingOrders.RowCount = 0;
      this.dgvFilledOrders.RowCount = 0;
      this.dgvCancelledOrders.RowCount = 0;
      this.dgvRejectedOrders.RowCount = 0;
      this.reportedOrder = (Order) null;
      this.orderFactory.Reset();
      this.allOrders.Clear();
      this.workingOrders.Clear();
      this.cancelledOrders.Clear();
      this.rejectedOrders.Clear();
      this.filledOrders.Clear();
    }

    public void UpdateGUI()
    {
      if (FrameworkControl.UpdatedSuspened && this.framework.Mode != FrameworkMode.Realtime)
        return;
      Event[] eventArray = this.messageQueue.DequeueAll((object) this);
      if (eventArray == null)
        return;
      this.ltvReports.BeginUpdate();
      this.lastUpdatedOrderList.Clear();
      foreach (Event @event in eventArray)
      {
        switch (@event.TypeId)
        {
          case 13:
            this.ProcessExecutionReport(@event as ExecutionReport);
            break;
          case 14:
            this.ProcessExecutionCommand(@event as ExecutionCommand);
            break;
          case 130:
            this.Clear();
            break;
        }
      }
      foreach (OrderViewItem orderViewItem in this.lastUpdatedOrderList)
        this.UpdateStatus(orderViewItem);
      this.ltvReports.EndUpdate();
      this.UpdateIndexes();
    }

    private void UpdateIndexes()
    {
      this.freezeSelection = true;
      int num1 = this.dgvOrders.CurrentRow == null ? -1 : this.dgvOrders.CurrentRow.Index;
      int num2 = this.dgvWorkingOrders.CurrentRow == null ? -1 : this.dgvWorkingOrders.CurrentRow.Index;
      int num3 = this.dgvFilledOrders.CurrentRow == null ? -1 : this.dgvFilledOrders.CurrentRow.Index;
      int num4 = this.dgvCancelledOrders.CurrentRow == null ? -1 : this.dgvCancelledOrders.CurrentRow.Index;
      int num5 = this.dgvRejectedOrders.CurrentRow == null ? -1 : this.dgvRejectedOrders.CurrentRow.Index;
      this.UpdateDisplayIndex(this.dgvOrders, this.allOrders);
      this.UpdateDisplayIndex(this.dgvWorkingOrders, this.workingOrders);
      this.UpdateDisplayIndex(this.dgvFilledOrders, this.filledOrders);
      this.UpdateDisplayIndex(this.dgvCancelledOrders, this.cancelledOrders);
      this.UpdateDisplayIndex(this.dgvRejectedOrders, this.rejectedOrders);
      if (num1 == -1)
      {
        this.dgvOrders.ClearSelection();
        this.dgvOrders.CurrentCell = (DataGridViewCell) null;
      }
      if (num2 == -1)
      {
        this.dgvWorkingOrders.ClearSelection();
        this.dgvWorkingOrders.CurrentCell = (DataGridViewCell) null;
      }
      if (num3 == -1)
      {
        this.dgvFilledOrders.ClearSelection();
        this.dgvFilledOrders.CurrentCell = (DataGridViewCell) null;
      }
      if (num4 == -1)
      {
        this.dgvCancelledOrders.ClearSelection();
        this.dgvCancelledOrders.CurrentCell = (DataGridViewCell) null;
      }
      if (num5 == -1)
      {
        this.dgvRejectedOrders.ClearSelection();
        this.dgvRejectedOrders.CurrentCell = (DataGridViewCell) null;
      }
      this.freezeSelection = false;
    }

    private void UpdateDisplayIndex(DataGridView dgv, OrderedDictionary orders)
    {
      int num1 = orders.Count - dgv.RowCount;
      if (num1 == 0)
        return;
      dgv.RowCount = orders.Count;
      if (dgv.RowCount == 0)
        return;
      int num2 = 0;
      if (!this.autoScrollingEnabled)
      {
        num2 = dgv.FirstDisplayedScrollingRowIndex + num1;
        if (num2 < 0)
          num2 = 0;
        else if (num2 >= dgv.RowCount)
          num2 = this.dgvOrders.RowCount - 1;
      }
      dgv.FirstDisplayedScrollingRowIndex = num2;
    }

    private void ProcessExecutionCommand(ExecutionCommand executionCommand)
    {
      Order order = this.orderFactory.OnExecutionCommand(executionCommand);
      if (executionCommand.Type == ExecutionCommandType.Send)
        this.AddOrder(order);
      this.lastUpdatedOrderList.Add((OrderViewItem) this.allOrders[(object) order]);
      if (executionCommand.Order != this.reportedOrder)
        return;
      this.ltvReports.Items.Add((ListViewItem) new ExecutionCommandViewItem(executionCommand));
    }

    private void ProcessExecutionReport(ExecutionReport executionReport)
    {
      this.lastUpdatedOrderList.Add((OrderViewItem) this.allOrders[(object) this.orderFactory.OnExecutionReport(executionReport)]);
      if (executionReport.Order != this.reportedOrder)
        return;
      this.ltvReports.Items.Add((ListViewItem) new ExecutionReportViewItem(executionReport));
    }

    private void AddOrder(Order order)
    {
      OrderViewItem orderViewItem = new OrderViewItem(order);
      this.allOrders.Add((object) order, (object) orderViewItem);
    }

    private void UpdateStatus(OrderViewItem item)
    {
      try
      {
        Order order = item.Order;
        switch (order.Status)
        {
          case OrderStatus.Rejected:
            this.workingOrders.Remove((object) order);
            this.rejectedOrders.Add((object) order, (object) item);
            break;
          case OrderStatus.Filled:
            this.workingOrders.Remove((object) order);
            this.filledOrders.Add((object) order, (object) item);
            break;
          case OrderStatus.Cancelled:
            this.workingOrders.Remove((object) order);
            this.cancelledOrders.Add((object) order, (object) item);
            break;
          default:
            this.workingOrders.Add((object) order, (object) item);
            break;
        }
      }
      catch
      {
      }
    }

    private void ctxOrder_Cancel_Click(object sender, EventArgs e)
    {
      if (this.reportedOrder == null || this.reportedOrder.IsDone)
        return;
      this.framework.OrderManager.Cancel(this.reportedOrder);
    }

    private void ctxOrder_Modify_Click(object sender, EventArgs e)
    {
      if (this.reportedOrder == null || this.reportedOrder.IsDone)
        return;
      ModifyOrderForm modifyOrderForm = new ModifyOrderForm();
      modifyOrderForm.Init(this.reportedOrder);
      if (modifyOrderForm.ShowDialog((IWin32Window) this) == DialogResult.OK)
        this.framework.OrderManager.Replace(this.reportedOrder, modifyOrderForm.LimitPrice, modifyOrderForm.StopPrice, (double) modifyOrderForm.Qty);
      modifyOrderForm.Dispose();
    }

    private void ctxOrder_Opening(object sender, CancelEventArgs e)
    {
      if (this.reportedOrder != null && !this.reportedOrder.IsDone)
      {
        this.ctxOrder_Modify.Enabled = true;
        this.ctxOrder_Cancel.Enabled = true;
      }
      else
      {
        this.ctxOrder_Modify.Enabled = false;
        this.ctxOrder_Cancel.Enabled = false;
      }
    }

    private void dgvOrders_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
      if (this.freezeSelection)
        return;
      OrderViewItem orderViewItem = this.GetItem(sender, e.RowIndex);
      e.Value = orderViewItem[e.ColumnIndex];
    }

    private void dgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (this.freezeSelection)
        return;
      OrderViewItem orderViewItem = this.GetItem(sender, e.RowIndex);
      orderViewItem.Update();
      e.CellStyle.BackColor = orderViewItem.Color;
    }

    private void dgvOrders_SelectionChanged(object sender, EventArgs e)
    {
      if (this.freezeSelection)
        return;
      DataGridView dataGridView = (DataGridView) sender;
      if (dataGridView.SelectedRows.Count == 0)
        return;
      this.UpdateReports(this.GetItem((object) dataGridView, dataGridView.SelectedRows[0].Index).Order);
    }

    private void UpdateReports(Order order)
    {
      this.tabReports.TabPages[0].Text = string.Format("Reports - (Order Id = {0})", (object) order.Id);
      this.ltvReports.BeginUpdate();
      this.ltvReports.Items.Clear();
      foreach (ExecutionMessage executionMessage in order.Messages)
      {
        if (executionMessage is ExecutionReport)
          this.ltvReports.Items.Add((ListViewItem) new ExecutionReportViewItem(executionMessage as ExecutionReport));
        else
          this.ltvReports.Items.Add((ListViewItem) new ExecutionCommandViewItem(executionMessage as ExecutionCommand));
      }
      this.ltvReports.EndUpdate();
      this.reportedOrder = order;
      this.propertyObject = (object) this.reportedOrder;
      this.OnShowProperties(false);
    }

    private OrderViewItem GetItem(object sender, int index)
    {
      OrderViewItem orderViewItem = (OrderViewItem) null;
      if (sender == this.dgvOrders)
        orderViewItem = this.allOrders[this.allOrders.Count - 1 - index] as OrderViewItem;
      else if (sender == this.dgvWorkingOrders)
        orderViewItem = this.workingOrders[this.workingOrders.Count - 1 - index] as OrderViewItem;
      else if (sender == this.dgvFilledOrders)
        orderViewItem = this.filledOrders[this.filledOrders.Count - 1 - index] as OrderViewItem;
      else if (sender == this.dgvCancelledOrders)
        orderViewItem = this.cancelledOrders[this.cancelledOrders.Count - 1 - index] as OrderViewItem;
      else if (sender == this.dgvRejectedOrders)
        orderViewItem = this.rejectedOrders[this.rejectedOrders.Count - 1 - index] as OrderViewItem;
      return orderViewItem;
    }

    private void dgvOrders_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      DataGridView dataGridView = (DataGridView) sender;
      if (dataGridView.HitTest(e.X, e.Y).Type != DataGridViewHitTestType.None)
        return;
      dataGridView.ClearSelection();
      dataGridView.CurrentCell = (DataGridViewCell) null;
    }

    private void ExportXls(TabControl tabControl)
    {
      CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
      try
      {
        this.Cursor = Cursors.WaitCursor;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture.DateTimeFormat = currentCulture.DateTimeFormat;
        Excel excel = new Excel();
        excel.Workbooks.Add();
        Workbook workbook = excel.Workbooks[1];
        WorksheetList worksheets = workbook.Worksheets;
        for (int index = 0; index < tabControl.TabCount; ++index)
        {
          TabPage tabPage = tabControl.TabPages[index];
          Control control = tabPage.Controls[0];
          if (control is ListView)
          {
            worksheets.AddLast();
            Worksheet sheet = worksheets[index + 1];
            sheet.Name = tabPage.Text;
            this.CopyDataToWorksheet(sheet, new ListView[1]
            {
              control as ListView
            });
          }
          else if (control is DataGridView)
          {
            worksheets.AddLast();
            Worksheet sheet = worksheets[index + 1];
            sheet.Name = tabPage.Text;
            this.CopyDataToWorksheet(sheet, new DataGridView[1]
            {
              control as DataGridView
            });
          }
        }
        workbook.Worksheets[1].Activate();
        excel.Visible = true;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, string.Format("An error occured while exporting results. Possible, MS Office is not installed.{0}{0}{1}", (object) Environment.NewLine, (object) ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = currentCulture;
        this.Cursor = Cursors.Default;
      }
    }

    private void CopyDataToWorksheet(Worksheet sheet, ListView[] ltvList)
    {
      int num = 2;
      int count = ltvList[0].Columns.Count;
      for (int index = 0; index < count; ++index)
      {
        ColumnHeader columnHeader = ltvList[0].Columns[index];
        Range range = sheet.GetRange(1, index + 1);
        range.Bold = true;
        range.Value = (object) columnHeader.Text;
      }
      foreach (ListView listView in ltvList)
      {
        for (int index1 = 0; index1 < listView.Items.Count; ++index1)
        {
          ListViewItem listViewItem = listView.Items[index1];
          for (int index2 = 0; index2 < listViewItem.SubItems.Count && index2 < count; ++index2)
          {
            Range range = sheet.GetRange(index1 + num, index2 + 1);
            if (listViewItem.SubItems[index2].Font.Italic)
              range.Italic = true;
            if (listViewItem.SubItems[index2].Font.Bold)
              range.Bold = true;
            if (listViewItem.SubItems[index2].Font.Underline)
              range.Underline = true;
            range.Value = (object) listViewItem.SubItems[index2].Text;
          }
        }
        num += listView.Items.Count;
      }
    }

    private void CopyDataToWorksheet(Worksheet sheet, DataGridView[] dgvList)
    {
      int num = 2;
      int count = dgvList[0].Columns.Count;
      for (int index = 0; index < count; ++index)
      {
        DataGridViewColumn dataGridViewColumn = dgvList[0].Columns[index];
        Range range = sheet.GetRange(1, index + 1);
        range.Bold = true;
        range.Value = (object) dataGridViewColumn.HeaderText;
      }
      foreach (DataGridView dataGridView in dgvList)
      {
        for (int index1 = 0; index1 < dataGridView.Rows.Count; ++index1)
        {
          DataGridViewRow dataGridViewRow = dataGridView.Rows[index1];
          for (int index2 = 0; index2 < dataGridViewRow.Cells.Count && index2 < count; ++index2)
          {
            Range range = sheet.GetRange(index1 + num, index2 + 1);
            Font font = dataGridViewRow.Cells[index2].Style.Font ?? dataGridViewRow.Cells[index2].InheritedStyle.Font;
            if (font.Italic)
              range.Italic = true;
            if (font.Bold)
              range.Bold = true;
            if (font.Underline)
              range.Underline = true;
            range.Value = (object) dataGridViewRow.Cells[index2].Value.ToString();
          }
        }
        num += dataGridView.Rows.Count;
      }
    }

    private void ExportCsv(TabControl tabControl)
    {
      CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
      try
      {
        this.Cursor = Cursors.WaitCursor;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture.DateTimeFormat = currentCulture.DateTimeFormat;
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.DefaultExt = "*.csv";
        saveFileDialog.Filter = "CSV Files|*.csv|All Files|*.*";
        saveFileDialog.Title = "Save Worksheet";
        if (saveFileDialog.ShowDialog((IWin32Window) this) != DialogResult.OK)
          return;
        StreamWriter sw = new StreamWriter(saveFileDialog.FileName);
        for (int index = 0; index < tabControl.TabCount; ++index)
        {
          TabPage tabPage = tabControl.TabPages[index];
          Control control = tabPage.Controls[0];
          if (control is ListView)
          {
            sw.WriteLine("Tab[" + tabPage.Text + "]");
            this.WriteDataToCsv(sw, new ListView[1]
            {
              control as ListView
            });
          }
          else if (control is DataGridView)
          {
            sw.WriteLine("Tab[" + tabPage.Text + "]");
            this.WriteDataToCsv(sw, new DataGridView[1]
            {
              control as DataGridView
            });
          }
        }
        sw.Close();
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, string.Format("An error occured while exporting results. {0}{0}{1}", (object) Environment.NewLine, (object) ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = currentCulture;
        this.Cursor = Cursors.Default;
      }
    }

    private void WriteDataToCsv(StreamWriter sw, ListView[] ltvList)
    {
      string listSeparator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
      int count = ltvList[0].Columns.Count;
      for (int index = 0; index < count; ++index)
      {
        ColumnHeader columnHeader = ltvList[0].Columns[index];
        sw.Write(columnHeader.Text);
        if (index < count - 1)
          sw.Write(listSeparator);
        else
          sw.Write("\r\n");
      }
      foreach (ListView listView in ltvList)
      {
        for (int index1 = 0; index1 < listView.Items.Count; ++index1)
        {
          ListViewItem listViewItem = listView.Items[index1];
          for (int index2 = 0; index2 < listViewItem.SubItems.Count && index2 < count; ++index2)
          {
            sw.Write(listViewItem.SubItems[index2].Text);
            if (index2 < count - 1)
              sw.Write(listSeparator);
            else
              sw.Write("\r\n");
          }
        }
      }
    }

    private void WriteDataToCsv(StreamWriter sw, DataGridView[] dgvList)
    {
      string listSeparator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
      int count = dgvList[0].Columns.Count;
      for (int index = 0; index < count; ++index)
      {
        DataGridViewColumn dataGridViewColumn = dgvList[0].Columns[index];
        sw.Write(dataGridViewColumn.HeaderText);
        if (index < count - 1)
          sw.Write(listSeparator);
        else
          sw.Write("\r\n");
      }
      foreach (DataGridView dataGridView in dgvList)
      {
        for (int index1 = 0; index1 < dataGridView.Rows.Count; ++index1)
        {
          DataGridViewRow dataGridViewRow = dataGridView.Rows[index1];
          for (int index2 = 0; index2 < dataGridViewRow.Cells.Count && index2 < count; ++index2)
          {
            sw.Write(dataGridViewRow.Cells[index2].Value);
            if (index2 < count - 1)
              sw.Write(listSeparator);
            else
              sw.Write("\r\n");
          }
        }
      }
    }

    private void SetupDgvCtx(DataGridView dgv)
    {
      dgv.Columns[0].HeaderCell.ContextMenuStrip = new ContextMenuStrip();
      dgv.Columns[0].HeaderCell.ContextMenuStrip.Opening += new CancelEventHandler(this.ctxFirstColumn_Opening);
      for (int index = 1; index < dgv.Columns.Count; ++index)
        this.SetupHeaderCtx(dgv.Columns[index]);
      for (int index = 0; index < dgv.Columns.Count; ++index)
        dgv.Columns[index].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }

    private void SetupHeaderCtx(DataGridViewColumn column)
    {
      column.HeaderCell.ContextMenuStrip = new ContextMenuStrip();
      ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
      toolStripMenuItem.Text = "Hide";
      toolStripMenuItem.Tag = (object) column;
      toolStripMenuItem.Click += new EventHandler(this.ctxHeaders_Hide_Click);
      column.HeaderCell.ContextMenuStrip.Items.Add((ToolStripItem) toolStripMenuItem);
    }

    private void ctxFirstColumn_Opening(object sender, CancelEventArgs e)
    {
      ContextMenuStrip contextMenuStrip = (ContextMenuStrip) sender;
      contextMenuStrip.Items.Clear();
      foreach (DataGridViewColumn dataGridViewColumn in (IEnumerable<DataGridViewColumn>) this.GetColumns((DataGridView) contextMenuStrip.SourceControl))
      {
        ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
        toolStripMenuItem.Text = dataGridViewColumn.HeaderText;
        toolStripMenuItem.Checked = dataGridViewColumn.Visible;
        toolStripMenuItem.Tag = (object) dataGridViewColumn;
        toolStripMenuItem.Click += new EventHandler(this.ctxFirstColumn_Item_Click);
        contextMenuStrip.Items.Add((ToolStripItem) toolStripMenuItem);
      }
      e.Cancel = contextMenuStrip.Items.Count == 0;
    }

    private void ctxFirstColumn_Item_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem) sender;
      ((DataGridViewBand) toolStripMenuItem.Tag).Visible = !toolStripMenuItem.Checked;
    }

    private void ctxHeaders_Hide_Click(object sender, EventArgs e)
    {
      ((DataGridViewBand) ((ToolStripItem) sender).Tag).Visible = false;
    }

    private IList<DataGridViewColumn> GetColumns(DataGridView dgv)
    {
      SortedList<int, DataGridViewColumn> sortedList = new SortedList<int, DataGridViewColumn>();
      for (int index = 1; index < dgv.ColumnCount; ++index)
      {
        DataGridViewColumn dataGridViewColumn = dgv.Columns[index];
        sortedList.Add(dataGridViewColumn.DisplayIndex, dataGridViewColumn);
      }
      return sortedList.Values;
    }

    private void ltvReports_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.ltvReports.SelectedIndices.Count == 0)
        return;
      if (this.ltvReports.SelectedItems[0] is ExecutionCommandViewItem)
        this.propertyObject = (object) (this.ltvReports.SelectedItems[0] as ExecutionCommandViewItem).Command;
      if (this.ltvReports.SelectedItems[0] is ExecutionReportViewItem)
        this.propertyObject = (object) (this.ltvReports.SelectedItems[0] as ExecutionReportViewItem).Report;
      this.OnShowProperties(false);
    }

    private void ctxOrdersSettings_ExportToCSV_Click(object sender, EventArgs e)
    {
      this.ExportCsv(this.tabOrders);
    }

    private void ctxOrdersSettings_ExportToXLS_Click(object sender, EventArgs e)
    {
      this.ExportXls(this.tabOrders);
    }

    private void ctxOrdersSettings_AutoScrolling_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem) sender;
      toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
      this.autoScrollingEnabled = toolStripMenuItem.Checked;
    }

    private void ctxReportsSettings_ExportToCSV_Click(object sender, EventArgs e)
    {
      this.ExportCsv(this.tabReports);
    }

    private void ctxReportsSettings_ExportToXLS_Click(object sender, EventArgs e)
    {
      this.ExportXls(this.tabReports);
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
      this.ctxOrder = new ContextMenuStrip(this.components);
      this.ctxOrder_Modify = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.ctxOrder_Cancel = new ToolStripMenuItem();
      this.columnHeader27 = new ColumnHeader();
      this.columnHeader36 = new ColumnHeader();
      this.columnHeader45 = new ColumnHeader();
      this.columnHeader46 = new ColumnHeader();
      this.columnHeader47 = new ColumnHeader();
      this.columnHeader48 = new ColumnHeader();
      this.columnHeader49 = new ColumnHeader();
      this.columnHeader50 = new ColumnHeader();
      this.columnHeader51 = new ColumnHeader();
      this.panel2 = new Panel();
      this.lblReportsSettings = new Label();
      this.ctxReportsSettings = new ContextMenuStrip(this.components);
      this.ctxReportsSettings_ExportToCSV = new ToolStripMenuItem();
      this.ctxReportsSettings_ExportToXLS = new ToolStripMenuItem();
      this.tabReports = new TabControl();
      this.tabPage6 = new TabPage();
      this.ltvReports = new ListViewNB();
      this.columnHeader52 = new ColumnHeader();
      this.columnHeader55 = new ColumnHeader();
      this.columnHeader59 = new ColumnHeader();
      this.columnHeader56 = new ColumnHeader();
      this.columnHeader57 = new ColumnHeader();
      this.columnHeader68 = new ColumnHeader();
      this.columnHeader58 = new ColumnHeader();
      this.columnHeader60 = new ColumnHeader();
      this.columnHeader53 = new ColumnHeader();
      this.columnHeader54 = new ColumnHeader();
      this.columnHeader66 = new ColumnHeader();
      this.columnHeader67 = new ColumnHeader();
      this.columnHeader69 = new ColumnHeader();
      this.columnHeader71 = new ColumnHeader();
      this.ctxOrdersSettings = new ContextMenuStrip(this.components);
      this.ctxOrdersSettings_ExportToCSV = new ToolStripMenuItem();
      this.ctxOrdersSettings_ExportToXLS = new ToolStripMenuItem();
      this.ctxOrdersSettings_Separator1 = new ToolStripSeparator();
      this.ctxOrdersSettings_AutoScrolling = new ToolStripMenuItem();
      this.panel1 = new Panel();
      this.lblOrdersSetttings = new Label();
      this.tabOrders = new TabControl();
      this.tabPage1 = new TabPage();
      this.dgvOrders = new DataGridView();
      this.Column1 = new DataGridViewTextBoxColumn();
      this.Column2 = new DataGridViewTextBoxColumn();
      this.Column3 = new DataGridViewTextBoxColumn();
      this.Column4 = new DataGridViewTextBoxColumn();
      this.Column5 = new DataGridViewTextBoxColumn();
      this.Column6 = new DataGridViewTextBoxColumn();
      this.Column7 = new DataGridViewTextBoxColumn();
      this.Column8 = new DataGridViewTextBoxColumn();
      this.Column9 = new DataGridViewTextBoxColumn();
      this.Column10 = new DataGridViewTextBoxColumn();
      this.Column11 = new DataGridViewTextBoxColumn();
      this.tabPage2 = new TabPage();
      this.dgvWorkingOrders = new DataGridView();
      this.dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn10 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn11 = new DataGridViewTextBoxColumn();
      this.tabPage3 = new TabPage();
      this.dgvFilledOrders = new DataGridView();
      this.dataGridViewTextBoxColumn12 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn13 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn14 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn15 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn16 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn17 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn18 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn19 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn20 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn21 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn22 = new DataGridViewTextBoxColumn();
      this.tabPage4 = new TabPage();
      this.dgvCancelledOrders = new DataGridView();
      this.dataGridViewTextBoxColumn23 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn24 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn25 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn26 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn27 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn28 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn29 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn30 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn31 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn32 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn33 = new DataGridViewTextBoxColumn();
      this.tabPage5 = new TabPage();
      this.dgvRejectedOrders = new DataGridView();
      this.dataGridViewTextBoxColumn34 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn35 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn36 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn37 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn38 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn39 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn40 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn41 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn42 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn43 = new DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn44 = new DataGridViewTextBoxColumn();
      this.splitter1 = new Splitter();
      this.ctxOrder.SuspendLayout();
      this.panel2.SuspendLayout();
      this.ctxReportsSettings.SuspendLayout();
      this.tabReports.SuspendLayout();
      this.tabPage6.SuspendLayout();
      this.ctxOrdersSettings.SuspendLayout();
      this.panel1.SuspendLayout();
      this.tabOrders.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((ISupportInitialize) this.dgvOrders).BeginInit();
      this.tabPage2.SuspendLayout();
      ((ISupportInitialize) this.dgvWorkingOrders).BeginInit();
      this.tabPage3.SuspendLayout();
      ((ISupportInitialize) this.dgvFilledOrders).BeginInit();
      this.tabPage4.SuspendLayout();
      ((ISupportInitialize) this.dgvCancelledOrders).BeginInit();
      this.tabPage5.SuspendLayout();
      ((ISupportInitialize) this.dgvRejectedOrders).BeginInit();
      this.SuspendLayout();
      this.ctxOrder.Items.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) this.ctxOrder_Modify,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.ctxOrder_Cancel
      });
      this.ctxOrder.Name = "ctxOrder";
      this.ctxOrder.Size = new Size(122, 54);
      this.ctxOrder.Opening += new CancelEventHandler(this.ctxOrder_Opening);
      this.ctxOrder_Modify.Enabled = false;
      this.ctxOrder_Modify.Name = "ctxOrder_Modify";
      this.ctxOrder_Modify.Size = new Size(121, 22);
      this.ctxOrder_Modify.Text = "Modify...";
      this.ctxOrder_Modify.Click += new EventHandler(this.ctxOrder_Modify_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(118, 6);
      this.ctxOrder_Cancel.Name = "ctxOrder_Cancel";
      this.ctxOrder_Cancel.Size = new Size(121, 22);
      this.ctxOrder_Cancel.Text = "Cancel";
      this.ctxOrder_Cancel.Click += new EventHandler(this.ctxOrder_Cancel_Click);
      this.columnHeader27.Text = "DateTime";
      this.columnHeader27.Width = 116;
      this.columnHeader36.Text = "Symbol";
      this.columnHeader36.TextAlign = HorizontalAlignment.Right;
      this.columnHeader45.Text = "Side";
      this.columnHeader45.TextAlign = HorizontalAlignment.Right;
      this.columnHeader46.Text = "Type";
      this.columnHeader46.TextAlign = HorizontalAlignment.Right;
      this.columnHeader47.Text = "Qty";
      this.columnHeader47.TextAlign = HorizontalAlignment.Right;
      this.columnHeader48.Text = "Avg. Price";
      this.columnHeader48.TextAlign = HorizontalAlignment.Right;
      this.columnHeader48.Width = 73;
      this.columnHeader49.Text = "Price";
      this.columnHeader49.TextAlign = HorizontalAlignment.Right;
      this.columnHeader50.Text = "Stop Price";
      this.columnHeader50.TextAlign = HorizontalAlignment.Right;
      this.columnHeader50.Width = 72;
      this.columnHeader51.Text = "Status";
      this.columnHeader51.TextAlign = HorizontalAlignment.Right;
      this.panel2.Controls.Add((Control) this.lblReportsSettings);
      this.panel2.Controls.Add((Control) this.tabReports);
      this.panel2.Dock = DockStyle.Bottom;
      this.panel2.Location = new Point(0, 369);
      this.panel2.Name = "panel2";
      this.panel2.Size = new Size(743, 151);
      this.panel2.TabIndex = 2;
      this.lblReportsSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.lblReportsSettings.ContextMenuStrip = this.ctxReportsSettings;
 //     this.lblReportsSettings.Image = (Image) Resources.settings;
      this.lblReportsSettings.Location = new Point(723, 0);
      this.lblReportsSettings.Name = "lblReportsSettings";
      this.lblReportsSettings.Size = new Size(16, 16);
      this.lblReportsSettings.TabIndex = 3;
      this.ctxReportsSettings.Items.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.ctxReportsSettings_ExportToCSV,
        (ToolStripItem) this.ctxReportsSettings_ExportToXLS
      });
      this.ctxReportsSettings.Name = "ctxReportsSettings";
      this.ctxReportsSettings.Size = new Size(153, 70);
      this.ctxReportsSettings_ExportToCSV.Name = "ctxReportsSettings_ExportToCSV";
      this.ctxReportsSettings_ExportToCSV.Size = new Size(152, 22);
      this.ctxReportsSettings_ExportToCSV.Text = "ExportToCSV";
      this.ctxReportsSettings_ExportToCSV.Click += new EventHandler(this.ctxReportsSettings_ExportToCSV_Click);
      this.ctxReportsSettings_ExportToXLS.Name = "ctxReportsSettings_ExportToXLS";
      this.ctxReportsSettings_ExportToXLS.Size = new Size(152, 22);
      this.ctxReportsSettings_ExportToXLS.Text = "ExportToXLS";
      this.ctxReportsSettings_ExportToXLS.Click += new EventHandler(this.ctxReportsSettings_ExportToXLS_Click);
      this.tabReports.Controls.Add((Control) this.tabPage6);
      this.tabReports.Dock = DockStyle.Fill;
      this.tabReports.Location = new Point(0, 0);
      this.tabReports.Name = "tabReports";
      this.tabReports.SelectedIndex = 0;
      this.tabReports.Size = new Size(743, 151);
      this.tabReports.TabIndex = 5;
      this.tabPage6.Controls.Add((Control) this.ltvReports);
      this.tabPage6.Location = new Point(4, 22);
      this.tabPage6.Name = "tabPage6";
      this.tabPage6.Size = new Size(735, 125);
      this.tabPage6.TabIndex = 0;
      this.tabPage6.Text = "Reports";
      this.tabPage6.UseVisualStyleBackColor = true;
      this.ltvReports.BorderStyle = BorderStyle.None;
      this.ltvReports.Columns.AddRange(new ColumnHeader[14]
      {
        this.columnHeader52,
        this.columnHeader55,
        this.columnHeader59,
        this.columnHeader56,
        this.columnHeader57,
        this.columnHeader68,
        this.columnHeader58,
        this.columnHeader60,
        this.columnHeader53,
        this.columnHeader54,
        this.columnHeader66,
        this.columnHeader67,
        this.columnHeader69,
        this.columnHeader71
      });
      this.ltvReports.Dock = DockStyle.Fill;
      this.ltvReports.FullRowSelect = true;
      this.ltvReports.GridLines = true;
      this.ltvReports.HideSelection = false;
      this.ltvReports.Location = new Point(0, 0);
      this.ltvReports.MultiSelect = false;
      this.ltvReports.Name = "ltvReports";
      this.ltvReports.ShowGroups = false;
      this.ltvReports.ShowItemToolTips = true;
      this.ltvReports.Size = new Size(735, 125);
      this.ltvReports.TabIndex = 0;
      this.ltvReports.UseCompatibleStateImageBehavior = false;
      this.ltvReports.View = View.Details;
      this.ltvReports.SelectedIndexChanged += new EventHandler(this.ltvReports_SelectedIndexChanged);
      this.columnHeader52.Text = "TransactTime";
      this.columnHeader52.Width = 97;
      this.columnHeader55.Text = "Command";
      this.columnHeader55.TextAlign = HorizontalAlignment.Center;
      this.columnHeader55.Width = 74;
      this.columnHeader59.Text = "ExecType";
      this.columnHeader59.TextAlign = HorizontalAlignment.Center;
      this.columnHeader59.Width = 75;
      this.columnHeader56.Text = "Status";
      this.columnHeader56.TextAlign = HorizontalAlignment.Center;
      this.columnHeader56.Width = 67;
      this.columnHeader57.Text = "Side";
      this.columnHeader57.TextAlign = HorizontalAlignment.Center;
      this.columnHeader57.Width = 73;
      this.columnHeader68.Text = "Type";
      this.columnHeader68.TextAlign = HorizontalAlignment.Right;
      this.columnHeader68.Width = 71;
      this.columnHeader58.Text = "Price";
      this.columnHeader58.TextAlign = HorizontalAlignment.Right;
      this.columnHeader58.Width = 64;
      this.columnHeader60.Text = "StopPx";
      this.columnHeader60.TextAlign = HorizontalAlignment.Right;
      this.columnHeader60.Width = 63;
      this.columnHeader53.Text = "Qty";
      this.columnHeader53.TextAlign = HorizontalAlignment.Right;
      this.columnHeader53.Width = 100;
      this.columnHeader54.Text = "CumQty";
      this.columnHeader54.TextAlign = HorizontalAlignment.Right;
      this.columnHeader66.Text = "LeavesQty";
      this.columnHeader66.TextAlign = HorizontalAlignment.Right;
      this.columnHeader67.Text = "LastQty";
      this.columnHeader67.TextAlign = HorizontalAlignment.Right;
      this.columnHeader69.Text = "LastPx";
      this.columnHeader69.TextAlign = HorizontalAlignment.Right;
      this.columnHeader71.Text = "Text";
      this.ctxOrdersSettings.Items.AddRange(new ToolStripItem[4]
      {
        (ToolStripItem) this.ctxOrdersSettings_ExportToCSV,
        (ToolStripItem) this.ctxOrdersSettings_ExportToXLS,
        (ToolStripItem) this.ctxOrdersSettings_Separator1,
        (ToolStripItem) this.ctxOrdersSettings_AutoScrolling
      });
      this.ctxOrdersSettings.Name = "ctxSettings";
      this.ctxOrdersSettings.Size = new Size(147, 76);
      this.ctxOrdersSettings_ExportToCSV.Name = "ctxOrdersSettings_ExportToCSV";
      this.ctxOrdersSettings_ExportToCSV.Size = new Size(146, 22);
      this.ctxOrdersSettings_ExportToCSV.Text = "ExportToCSV";
      this.ctxOrdersSettings_ExportToCSV.Click += new EventHandler(this.ctxOrdersSettings_ExportToCSV_Click);
      this.ctxOrdersSettings_ExportToXLS.Name = "ctxOrdersSettings_ExportToXLS";
      this.ctxOrdersSettings_ExportToXLS.Size = new Size(146, 22);
      this.ctxOrdersSettings_ExportToXLS.Text = "ExportToXLS";
      this.ctxOrdersSettings_ExportToXLS.Click += new EventHandler(this.ctxOrdersSettings_ExportToXLS_Click);
      this.ctxOrdersSettings_Separator1.Name = "ctxOrdersSettings_Separator1";
      this.ctxOrdersSettings_Separator1.Size = new Size(143, 6);
      this.ctxOrdersSettings_AutoScrolling.Checked = true;
      this.ctxOrdersSettings_AutoScrolling.CheckState = CheckState.Checked;
      this.ctxOrdersSettings_AutoScrolling.Name = "ctxOrdersSettings_AutoScrolling";
      this.ctxOrdersSettings_AutoScrolling.Size = new Size(146, 22);
      this.ctxOrdersSettings_AutoScrolling.Text = "AutoScrolling";
      this.ctxOrdersSettings_AutoScrolling.Click += new EventHandler(this.ctxOrdersSettings_AutoScrolling_Click);
      this.panel1.Controls.Add((Control) this.lblOrdersSetttings);
      this.panel1.Controls.Add((Control) this.tabOrders);
      this.panel1.Dock = DockStyle.Fill;
      this.panel1.Location = new Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(743, 369);
      this.panel1.TabIndex = 6;
      this.lblOrdersSetttings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.lblOrdersSetttings.ContextMenuStrip = this.ctxOrdersSettings;
 //     this.lblOrdersSetttings.Image = (Image) Resources.settings;
      this.lblOrdersSetttings.Location = new Point(723, 0);
      this.lblOrdersSetttings.Name = "lblOrdersSetttings";
      this.lblOrdersSetttings.Size = new Size(16, 16);
      this.lblOrdersSetttings.TabIndex = 1;
      this.tabOrders.Controls.Add((Control) this.tabPage1);
      this.tabOrders.Controls.Add((Control) this.tabPage2);
      this.tabOrders.Controls.Add((Control) this.tabPage3);
      this.tabOrders.Controls.Add((Control) this.tabPage4);
      this.tabOrders.Controls.Add((Control) this.tabPage5);
      this.tabOrders.Dock = DockStyle.Fill;
      this.tabOrders.Location = new Point(0, 0);
      this.tabOrders.Name = "tabOrders";
      this.tabOrders.SelectedIndex = 0;
      this.tabOrders.Size = new Size(743, 369);
      this.tabOrders.TabIndex = 2;
      this.tabPage1.Controls.Add((Control) this.dgvOrders);
      this.tabPage1.Location = new Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(735, 343);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "All";
      this.tabPage1.UseVisualStyleBackColor = true;
      this.dgvOrders.AllowUserToAddRows = false;
      this.dgvOrders.AllowUserToDeleteRows = false;
      this.dgvOrders.AllowUserToOrderColumns = true;
      this.dgvOrders.AllowUserToResizeRows = false;
      this.dgvOrders.BackgroundColor = SystemColors.Window;
      this.dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
      this.dgvOrders.ColumnHeadersHeight = 21;
      this.dgvOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dgvOrders.Columns.AddRange((DataGridViewColumn) this.Column1, (DataGridViewColumn) this.Column2, (DataGridViewColumn) this.Column3, (DataGridViewColumn) this.Column4, (DataGridViewColumn) this.Column5, (DataGridViewColumn) this.Column6, (DataGridViewColumn) this.Column7, (DataGridViewColumn) this.Column8, (DataGridViewColumn) this.Column9, (DataGridViewColumn) this.Column10, (DataGridViewColumn) this.Column11);
      this.dgvOrders.ContextMenuStrip = this.ctxOrder;
      this.dgvOrders.Dock = DockStyle.Fill;
      this.dgvOrders.Location = new Point(3, 3);
      this.dgvOrders.MultiSelect = false;
      this.dgvOrders.Name = "dgvOrders";
      this.dgvOrders.ReadOnly = true;
      this.dgvOrders.RowHeadersVisible = false;
      this.dgvOrders.RowHeadersWidth = 30;
      this.dgvOrders.RowTemplate.Height = 18;
      this.dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dgvOrders.ShowCellErrors = false;
      this.dgvOrders.ShowEditingIcon = false;
      this.dgvOrders.ShowRowErrors = false;
      this.dgvOrders.Size = new Size(729, 337);
      this.dgvOrders.TabIndex = 0;
      this.dgvOrders.VirtualMode = true;
      this.dgvOrders.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvOrders_CellFormatting);
      this.dgvOrders.CellValueNeeded += new DataGridViewCellValueEventHandler(this.dgvOrders_CellValueNeeded);
      this.dgvOrders.SelectionChanged += new EventHandler(this.dgvOrders_SelectionChanged);
      this.dgvOrders.MouseUp += new MouseEventHandler(this.dgvOrders_MouseUp);
      this.Column1.FillWeight = 120f;
      this.Column1.HeaderText = "DateTime";
      this.Column1.MinimumWidth = 30;
      this.Column1.Name = "Column1";
      this.Column1.ReadOnly = true;
      this.Column1.Width = 120;
      this.Column2.HeaderText = "Provider";
      this.Column2.MinimumWidth = 30;
      this.Column2.Name = "Column2";
      this.Column2.ReadOnly = true;
      this.Column2.Width = 60;
      this.Column3.HeaderText = "Symbol";
      this.Column3.MinimumWidth = 30;
      this.Column3.Name = "Column3";
      this.Column3.ReadOnly = true;
      this.Column3.Width = 60;
      this.Column4.HeaderText = "Side";
      this.Column4.MinimumWidth = 30;
      this.Column4.Name = "Column4";
      this.Column4.ReadOnly = true;
      this.Column4.Width = 60;
      this.Column5.HeaderText = "Type";
      this.Column5.MinimumWidth = 30;
      this.Column5.Name = "Column5";
      this.Column5.ReadOnly = true;
      this.Column5.Width = 60;
      this.Column6.HeaderText = "Qty";
      this.Column6.MinimumWidth = 30;
      this.Column6.Name = "Column6";
      this.Column6.ReadOnly = true;
      this.Column6.Width = 60;
      this.Column7.HeaderText = "Avg. Price";
      this.Column7.MinimumWidth = 30;
      this.Column7.Name = "Column7";
      this.Column7.ReadOnly = true;
      this.Column7.Width = 60;
      this.Column8.HeaderText = "Price";
      this.Column8.MinimumWidth = 30;
      this.Column8.Name = "Column8";
      this.Column8.ReadOnly = true;
      this.Column8.Width = 60;
      this.Column9.HeaderText = "Stop Price";
      this.Column9.MinimumWidth = 30;
      this.Column9.Name = "Column9";
      this.Column9.ReadOnly = true;
      this.Column9.Width = 60;
      this.Column10.HeaderText = "Status";
      this.Column10.MinimumWidth = 30;
      this.Column10.Name = "Column10";
      this.Column10.ReadOnly = true;
      this.Column10.Width = 60;
      this.Column11.HeaderText = "Text";
      this.Column11.MinimumWidth = 30;
      this.Column11.Name = "Column11";
      this.Column11.ReadOnly = true;
      this.tabPage2.Controls.Add((Control) this.dgvWorkingOrders);
      this.tabPage2.Location = new Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new Padding(3);
      this.tabPage2.Size = new Size(735, 343);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Working";
      this.tabPage2.UseVisualStyleBackColor = true;
      this.dgvWorkingOrders.AllowUserToAddRows = false;
      this.dgvWorkingOrders.AllowUserToDeleteRows = false;
      this.dgvWorkingOrders.AllowUserToOrderColumns = true;
      this.dgvWorkingOrders.AllowUserToResizeRows = false;
      this.dgvWorkingOrders.BackgroundColor = SystemColors.Window;
      this.dgvWorkingOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
      this.dgvWorkingOrders.ColumnHeadersHeight = 21;
      this.dgvWorkingOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dgvWorkingOrders.Columns.AddRange((DataGridViewColumn) this.dataGridViewTextBoxColumn1, (DataGridViewColumn) this.dataGridViewTextBoxColumn2, (DataGridViewColumn) this.dataGridViewTextBoxColumn3, (DataGridViewColumn) this.dataGridViewTextBoxColumn4, (DataGridViewColumn) this.dataGridViewTextBoxColumn5, (DataGridViewColumn) this.dataGridViewTextBoxColumn6, (DataGridViewColumn) this.dataGridViewTextBoxColumn7, (DataGridViewColumn) this.dataGridViewTextBoxColumn8, (DataGridViewColumn) this.dataGridViewTextBoxColumn9, (DataGridViewColumn) this.dataGridViewTextBoxColumn10, (DataGridViewColumn) this.dataGridViewTextBoxColumn11);
      this.dgvWorkingOrders.ContextMenuStrip = this.ctxOrder;
      this.dgvWorkingOrders.Dock = DockStyle.Fill;
      this.dgvWorkingOrders.Location = new Point(3, 3);
      this.dgvWorkingOrders.MultiSelect = false;
      this.dgvWorkingOrders.Name = "dgvWorkingOrders";
      this.dgvWorkingOrders.ReadOnly = true;
      this.dgvWorkingOrders.RowHeadersVisible = false;
      this.dgvWorkingOrders.RowHeadersWidth = 30;
      this.dgvWorkingOrders.RowTemplate.Height = 18;
      this.dgvWorkingOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dgvWorkingOrders.ShowCellErrors = false;
      this.dgvWorkingOrders.ShowEditingIcon = false;
      this.dgvWorkingOrders.ShowRowErrors = false;
      this.dgvWorkingOrders.Size = new Size(729, 337);
      this.dgvWorkingOrders.TabIndex = 1;
      this.dgvWorkingOrders.VirtualMode = true;
      this.dgvWorkingOrders.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvOrders_CellFormatting);
      this.dgvWorkingOrders.CellValueNeeded += new DataGridViewCellValueEventHandler(this.dgvOrders_CellValueNeeded);
      this.dgvWorkingOrders.SelectionChanged += new EventHandler(this.dgvOrders_SelectionChanged);
      this.dgvWorkingOrders.MouseUp += new MouseEventHandler(this.dgvOrders_MouseUp);
      this.dataGridViewTextBoxColumn1.FillWeight = 120f;
      this.dataGridViewTextBoxColumn1.HeaderText = "DateTime";
      this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
      this.dataGridViewTextBoxColumn1.ReadOnly = true;
      this.dataGridViewTextBoxColumn1.Width = 120;
      this.dataGridViewTextBoxColumn2.HeaderText = "Provider";
      this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
      this.dataGridViewTextBoxColumn2.ReadOnly = true;
      this.dataGridViewTextBoxColumn2.Width = 60;
      this.dataGridViewTextBoxColumn3.HeaderText = "Symbol";
      this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
      this.dataGridViewTextBoxColumn3.ReadOnly = true;
      this.dataGridViewTextBoxColumn3.Width = 60;
      this.dataGridViewTextBoxColumn4.HeaderText = "Side";
      this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
      this.dataGridViewTextBoxColumn4.ReadOnly = true;
      this.dataGridViewTextBoxColumn4.Width = 60;
      this.dataGridViewTextBoxColumn5.HeaderText = "Type";
      this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
      this.dataGridViewTextBoxColumn5.ReadOnly = true;
      this.dataGridViewTextBoxColumn5.Width = 60;
      this.dataGridViewTextBoxColumn6.HeaderText = "Qty";
      this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
      this.dataGridViewTextBoxColumn6.ReadOnly = true;
      this.dataGridViewTextBoxColumn6.Width = 60;
      this.dataGridViewTextBoxColumn7.HeaderText = "Avg. Price";
      this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
      this.dataGridViewTextBoxColumn7.ReadOnly = true;
      this.dataGridViewTextBoxColumn7.Width = 60;
      this.dataGridViewTextBoxColumn8.HeaderText = "Price";
      this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
      this.dataGridViewTextBoxColumn8.ReadOnly = true;
      this.dataGridViewTextBoxColumn8.Width = 60;
      this.dataGridViewTextBoxColumn9.HeaderText = "Stop Price";
      this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
      this.dataGridViewTextBoxColumn9.ReadOnly = true;
      this.dataGridViewTextBoxColumn9.Width = 60;
      this.dataGridViewTextBoxColumn10.HeaderText = "Status";
      this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
      this.dataGridViewTextBoxColumn10.ReadOnly = true;
      this.dataGridViewTextBoxColumn10.Width = 60;
      this.dataGridViewTextBoxColumn11.HeaderText = "Text";
      this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
      this.dataGridViewTextBoxColumn11.ReadOnly = true;
      this.tabPage3.Controls.Add((Control) this.dgvFilledOrders);
      this.tabPage3.Location = new Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Padding = new Padding(3);
      this.tabPage3.Size = new Size(735, 343);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Filled";
      this.tabPage3.UseVisualStyleBackColor = true;
      this.dgvFilledOrders.AllowUserToAddRows = false;
      this.dgvFilledOrders.AllowUserToDeleteRows = false;
      this.dgvFilledOrders.AllowUserToOrderColumns = true;
      this.dgvFilledOrders.AllowUserToResizeRows = false;
      this.dgvFilledOrders.BackgroundColor = SystemColors.Window;
      this.dgvFilledOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
      this.dgvFilledOrders.ColumnHeadersHeight = 21;
      this.dgvFilledOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dgvFilledOrders.Columns.AddRange((DataGridViewColumn) this.dataGridViewTextBoxColumn12, (DataGridViewColumn) this.dataGridViewTextBoxColumn13, (DataGridViewColumn) this.dataGridViewTextBoxColumn14, (DataGridViewColumn) this.dataGridViewTextBoxColumn15, (DataGridViewColumn) this.dataGridViewTextBoxColumn16, (DataGridViewColumn) this.dataGridViewTextBoxColumn17, (DataGridViewColumn) this.dataGridViewTextBoxColumn18, (DataGridViewColumn) this.dataGridViewTextBoxColumn19, (DataGridViewColumn) this.dataGridViewTextBoxColumn20, (DataGridViewColumn) this.dataGridViewTextBoxColumn21, (DataGridViewColumn) this.dataGridViewTextBoxColumn22);
      this.dgvFilledOrders.ContextMenuStrip = this.ctxOrder;
      this.dgvFilledOrders.Dock = DockStyle.Fill;
      this.dgvFilledOrders.Location = new Point(3, 3);
      this.dgvFilledOrders.MultiSelect = false;
      this.dgvFilledOrders.Name = "dgvFilledOrders";
      this.dgvFilledOrders.ReadOnly = true;
      this.dgvFilledOrders.RowHeadersVisible = false;
      this.dgvFilledOrders.RowHeadersWidth = 30;
      this.dgvFilledOrders.RowTemplate.Height = 18;
      this.dgvFilledOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dgvFilledOrders.ShowCellErrors = false;
      this.dgvFilledOrders.ShowEditingIcon = false;
      this.dgvFilledOrders.ShowRowErrors = false;
      this.dgvFilledOrders.Size = new Size(729, 337);
      this.dgvFilledOrders.TabIndex = 2;
      this.dgvFilledOrders.VirtualMode = true;
      this.dgvFilledOrders.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvOrders_CellFormatting);
      this.dgvFilledOrders.CellValueNeeded += new DataGridViewCellValueEventHandler(this.dgvOrders_CellValueNeeded);
      this.dgvFilledOrders.SelectionChanged += new EventHandler(this.dgvOrders_SelectionChanged);
      this.dgvFilledOrders.MouseUp += new MouseEventHandler(this.dgvOrders_MouseUp);
      this.dataGridViewTextBoxColumn12.FillWeight = 120f;
      this.dataGridViewTextBoxColumn12.HeaderText = "DateTime";
      this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
      this.dataGridViewTextBoxColumn12.ReadOnly = true;
      this.dataGridViewTextBoxColumn12.Width = 120;
      this.dataGridViewTextBoxColumn13.HeaderText = "Provider";
      this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
      this.dataGridViewTextBoxColumn13.ReadOnly = true;
      this.dataGridViewTextBoxColumn13.Width = 60;
      this.dataGridViewTextBoxColumn14.HeaderText = "Symbol";
      this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
      this.dataGridViewTextBoxColumn14.ReadOnly = true;
      this.dataGridViewTextBoxColumn14.Width = 60;
      this.dataGridViewTextBoxColumn15.HeaderText = "Side";
      this.dataGridViewTextBoxColumn15.Name = "dataGridViewTextBoxColumn15";
      this.dataGridViewTextBoxColumn15.ReadOnly = true;
      this.dataGridViewTextBoxColumn15.Width = 60;
      this.dataGridViewTextBoxColumn16.HeaderText = "Type";
      this.dataGridViewTextBoxColumn16.Name = "dataGridViewTextBoxColumn16";
      this.dataGridViewTextBoxColumn16.ReadOnly = true;
      this.dataGridViewTextBoxColumn16.Width = 60;
      this.dataGridViewTextBoxColumn17.HeaderText = "Qty";
      this.dataGridViewTextBoxColumn17.Name = "dataGridViewTextBoxColumn17";
      this.dataGridViewTextBoxColumn17.ReadOnly = true;
      this.dataGridViewTextBoxColumn17.Width = 60;
      this.dataGridViewTextBoxColumn18.HeaderText = "Avg. Price";
      this.dataGridViewTextBoxColumn18.Name = "dataGridViewTextBoxColumn18";
      this.dataGridViewTextBoxColumn18.ReadOnly = true;
      this.dataGridViewTextBoxColumn18.Width = 60;
      this.dataGridViewTextBoxColumn19.HeaderText = "Price";
      this.dataGridViewTextBoxColumn19.Name = "dataGridViewTextBoxColumn19";
      this.dataGridViewTextBoxColumn19.ReadOnly = true;
      this.dataGridViewTextBoxColumn19.Width = 60;
      this.dataGridViewTextBoxColumn20.HeaderText = "Stop Price";
      this.dataGridViewTextBoxColumn20.Name = "dataGridViewTextBoxColumn20";
      this.dataGridViewTextBoxColumn20.ReadOnly = true;
      this.dataGridViewTextBoxColumn20.Width = 60;
      this.dataGridViewTextBoxColumn21.HeaderText = "Status";
      this.dataGridViewTextBoxColumn21.Name = "dataGridViewTextBoxColumn21";
      this.dataGridViewTextBoxColumn21.ReadOnly = true;
      this.dataGridViewTextBoxColumn21.Width = 60;
      this.dataGridViewTextBoxColumn22.HeaderText = "Text";
      this.dataGridViewTextBoxColumn22.Name = "dataGridViewTextBoxColumn22";
      this.dataGridViewTextBoxColumn22.ReadOnly = true;
      this.tabPage4.Controls.Add((Control) this.dgvCancelledOrders);
      this.tabPage4.Location = new Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Padding = new Padding(3);
      this.tabPage4.Size = new Size(735, 343);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Cancelled";
      this.tabPage4.UseVisualStyleBackColor = true;
      this.dgvCancelledOrders.AllowUserToAddRows = false;
      this.dgvCancelledOrders.AllowUserToDeleteRows = false;
      this.dgvCancelledOrders.AllowUserToOrderColumns = true;
      this.dgvCancelledOrders.AllowUserToResizeRows = false;
      this.dgvCancelledOrders.BackgroundColor = SystemColors.Window;
      this.dgvCancelledOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
      this.dgvCancelledOrders.ColumnHeadersHeight = 21;
      this.dgvCancelledOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dgvCancelledOrders.Columns.AddRange((DataGridViewColumn) this.dataGridViewTextBoxColumn23, (DataGridViewColumn) this.dataGridViewTextBoxColumn24, (DataGridViewColumn) this.dataGridViewTextBoxColumn25, (DataGridViewColumn) this.dataGridViewTextBoxColumn26, (DataGridViewColumn) this.dataGridViewTextBoxColumn27, (DataGridViewColumn) this.dataGridViewTextBoxColumn28, (DataGridViewColumn) this.dataGridViewTextBoxColumn29, (DataGridViewColumn) this.dataGridViewTextBoxColumn30, (DataGridViewColumn) this.dataGridViewTextBoxColumn31, (DataGridViewColumn) this.dataGridViewTextBoxColumn32, (DataGridViewColumn) this.dataGridViewTextBoxColumn33);
      this.dgvCancelledOrders.ContextMenuStrip = this.ctxOrder;
      this.dgvCancelledOrders.Dock = DockStyle.Fill;
      this.dgvCancelledOrders.Location = new Point(3, 3);
      this.dgvCancelledOrders.MultiSelect = false;
      this.dgvCancelledOrders.Name = "dgvCancelledOrders";
      this.dgvCancelledOrders.ReadOnly = true;
      this.dgvCancelledOrders.RowHeadersVisible = false;
      this.dgvCancelledOrders.RowHeadersWidth = 30;
      this.dgvCancelledOrders.RowTemplate.Height = 18;
      this.dgvCancelledOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dgvCancelledOrders.ShowCellErrors = false;
      this.dgvCancelledOrders.ShowEditingIcon = false;
      this.dgvCancelledOrders.ShowRowErrors = false;
      this.dgvCancelledOrders.Size = new Size(729, 337);
      this.dgvCancelledOrders.TabIndex = 2;
      this.dgvCancelledOrders.VirtualMode = true;
      this.dgvCancelledOrders.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvOrders_CellFormatting);
      this.dgvCancelledOrders.CellValueNeeded += new DataGridViewCellValueEventHandler(this.dgvOrders_CellValueNeeded);
      this.dgvCancelledOrders.SelectionChanged += new EventHandler(this.dgvOrders_SelectionChanged);
      this.dgvCancelledOrders.MouseUp += new MouseEventHandler(this.dgvOrders_MouseUp);
      this.dataGridViewTextBoxColumn23.FillWeight = 120f;
      this.dataGridViewTextBoxColumn23.HeaderText = "DateTime";
      this.dataGridViewTextBoxColumn23.Name = "dataGridViewTextBoxColumn23";
      this.dataGridViewTextBoxColumn23.ReadOnly = true;
      this.dataGridViewTextBoxColumn23.Width = 120;
      this.dataGridViewTextBoxColumn24.HeaderText = "Provider";
      this.dataGridViewTextBoxColumn24.Name = "dataGridViewTextBoxColumn24";
      this.dataGridViewTextBoxColumn24.ReadOnly = true;
      this.dataGridViewTextBoxColumn24.Width = 60;
      this.dataGridViewTextBoxColumn25.HeaderText = "Symbol";
      this.dataGridViewTextBoxColumn25.Name = "dataGridViewTextBoxColumn25";
      this.dataGridViewTextBoxColumn25.ReadOnly = true;
      this.dataGridViewTextBoxColumn25.Width = 60;
      this.dataGridViewTextBoxColumn26.HeaderText = "Side";
      this.dataGridViewTextBoxColumn26.Name = "dataGridViewTextBoxColumn26";
      this.dataGridViewTextBoxColumn26.ReadOnly = true;
      this.dataGridViewTextBoxColumn26.Width = 60;
      this.dataGridViewTextBoxColumn27.HeaderText = "Type";
      this.dataGridViewTextBoxColumn27.Name = "dataGridViewTextBoxColumn27";
      this.dataGridViewTextBoxColumn27.ReadOnly = true;
      this.dataGridViewTextBoxColumn27.Width = 60;
      this.dataGridViewTextBoxColumn28.HeaderText = "Qty";
      this.dataGridViewTextBoxColumn28.Name = "dataGridViewTextBoxColumn28";
      this.dataGridViewTextBoxColumn28.ReadOnly = true;
      this.dataGridViewTextBoxColumn28.Width = 60;
      this.dataGridViewTextBoxColumn29.HeaderText = "Avg. Price";
      this.dataGridViewTextBoxColumn29.Name = "dataGridViewTextBoxColumn29";
      this.dataGridViewTextBoxColumn29.ReadOnly = true;
      this.dataGridViewTextBoxColumn29.Width = 60;
      this.dataGridViewTextBoxColumn30.HeaderText = "Price";
      this.dataGridViewTextBoxColumn30.Name = "dataGridViewTextBoxColumn30";
      this.dataGridViewTextBoxColumn30.ReadOnly = true;
      this.dataGridViewTextBoxColumn30.Width = 60;
      this.dataGridViewTextBoxColumn31.HeaderText = "Stop Price";
      this.dataGridViewTextBoxColumn31.Name = "dataGridViewTextBoxColumn31";
      this.dataGridViewTextBoxColumn31.ReadOnly = true;
      this.dataGridViewTextBoxColumn31.Width = 60;
      this.dataGridViewTextBoxColumn32.HeaderText = "Status";
      this.dataGridViewTextBoxColumn32.Name = "dataGridViewTextBoxColumn32";
      this.dataGridViewTextBoxColumn32.ReadOnly = true;
      this.dataGridViewTextBoxColumn32.Width = 60;
      this.dataGridViewTextBoxColumn33.HeaderText = "Text";
      this.dataGridViewTextBoxColumn33.Name = "dataGridViewTextBoxColumn33";
      this.dataGridViewTextBoxColumn33.ReadOnly = true;
      this.tabPage5.Controls.Add((Control) this.dgvRejectedOrders);
      this.tabPage5.Location = new Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Padding = new Padding(3);
      this.tabPage5.Size = new Size(735, 343);
      this.tabPage5.TabIndex = 4;
      this.tabPage5.Text = "Rejected";
      this.tabPage5.UseVisualStyleBackColor = true;
      this.dgvRejectedOrders.AllowUserToAddRows = false;
      this.dgvRejectedOrders.AllowUserToDeleteRows = false;
      this.dgvRejectedOrders.AllowUserToOrderColumns = true;
      this.dgvRejectedOrders.AllowUserToResizeRows = false;
      this.dgvRejectedOrders.BackgroundColor = SystemColors.Window;
      this.dgvRejectedOrders.CellBorderStyle = DataGridViewCellBorderStyle.None;
      this.dgvRejectedOrders.ColumnHeadersHeight = 21;
      this.dgvRejectedOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dgvRejectedOrders.Columns.AddRange((DataGridViewColumn) this.dataGridViewTextBoxColumn34, (DataGridViewColumn) this.dataGridViewTextBoxColumn35, (DataGridViewColumn) this.dataGridViewTextBoxColumn36, (DataGridViewColumn) this.dataGridViewTextBoxColumn37, (DataGridViewColumn) this.dataGridViewTextBoxColumn38, (DataGridViewColumn) this.dataGridViewTextBoxColumn39, (DataGridViewColumn) this.dataGridViewTextBoxColumn40, (DataGridViewColumn) this.dataGridViewTextBoxColumn41, (DataGridViewColumn) this.dataGridViewTextBoxColumn42, (DataGridViewColumn) this.dataGridViewTextBoxColumn43, (DataGridViewColumn) this.dataGridViewTextBoxColumn44);
      this.dgvRejectedOrders.ContextMenuStrip = this.ctxOrder;
      this.dgvRejectedOrders.Dock = DockStyle.Fill;
      this.dgvRejectedOrders.Location = new Point(3, 3);
      this.dgvRejectedOrders.MultiSelect = false;
      this.dgvRejectedOrders.Name = "dgvRejectedOrders";
      this.dgvRejectedOrders.ReadOnly = true;
      this.dgvRejectedOrders.RowHeadersVisible = false;
      this.dgvRejectedOrders.RowHeadersWidth = 30;
      this.dgvRejectedOrders.RowTemplate.Height = 18;
      this.dgvRejectedOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      this.dgvRejectedOrders.ShowCellErrors = false;
      this.dgvRejectedOrders.ShowEditingIcon = false;
      this.dgvRejectedOrders.ShowRowErrors = false;
      this.dgvRejectedOrders.Size = new Size(729, 337);
      this.dgvRejectedOrders.TabIndex = 2;
      this.dgvRejectedOrders.VirtualMode = true;
      this.dgvRejectedOrders.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dgvOrders_CellFormatting);
      this.dgvRejectedOrders.CellValueNeeded += new DataGridViewCellValueEventHandler(this.dgvOrders_CellValueNeeded);
      this.dgvRejectedOrders.SelectionChanged += new EventHandler(this.dgvOrders_SelectionChanged);
      this.dgvRejectedOrders.MouseUp += new MouseEventHandler(this.dgvOrders_MouseUp);
      this.dataGridViewTextBoxColumn34.HeaderText = "DateTime";
      this.dataGridViewTextBoxColumn34.Name = "dataGridViewTextBoxColumn34";
      this.dataGridViewTextBoxColumn34.ReadOnly = true;
      this.dataGridViewTextBoxColumn34.Width = 140;
      this.dataGridViewTextBoxColumn35.HeaderText = "Provider";
      this.dataGridViewTextBoxColumn35.Name = "dataGridViewTextBoxColumn35";
      this.dataGridViewTextBoxColumn35.ReadOnly = true;
      this.dataGridViewTextBoxColumn35.Width = 60;
      this.dataGridViewTextBoxColumn36.HeaderText = "Symbol";
      this.dataGridViewTextBoxColumn36.Name = "dataGridViewTextBoxColumn36";
      this.dataGridViewTextBoxColumn36.ReadOnly = true;
      this.dataGridViewTextBoxColumn36.Width = 60;
      this.dataGridViewTextBoxColumn37.HeaderText = "Side";
      this.dataGridViewTextBoxColumn37.Name = "dataGridViewTextBoxColumn37";
      this.dataGridViewTextBoxColumn37.ReadOnly = true;
      this.dataGridViewTextBoxColumn37.Width = 60;
      this.dataGridViewTextBoxColumn38.HeaderText = "Type";
      this.dataGridViewTextBoxColumn38.Name = "dataGridViewTextBoxColumn38";
      this.dataGridViewTextBoxColumn38.ReadOnly = true;
      this.dataGridViewTextBoxColumn38.Width = 60;
      this.dataGridViewTextBoxColumn39.HeaderText = "Qty";
      this.dataGridViewTextBoxColumn39.Name = "dataGridViewTextBoxColumn39";
      this.dataGridViewTextBoxColumn39.ReadOnly = true;
      this.dataGridViewTextBoxColumn39.Width = 60;
      this.dataGridViewTextBoxColumn40.HeaderText = "Avg. Price";
      this.dataGridViewTextBoxColumn40.Name = "dataGridViewTextBoxColumn40";
      this.dataGridViewTextBoxColumn40.ReadOnly = true;
      this.dataGridViewTextBoxColumn40.Width = 60;
      this.dataGridViewTextBoxColumn41.HeaderText = "Price";
      this.dataGridViewTextBoxColumn41.Name = "dataGridViewTextBoxColumn41";
      this.dataGridViewTextBoxColumn41.ReadOnly = true;
      this.dataGridViewTextBoxColumn41.Width = 60;
      this.dataGridViewTextBoxColumn42.HeaderText = "Stop Price";
      this.dataGridViewTextBoxColumn42.Name = "dataGridViewTextBoxColumn42";
      this.dataGridViewTextBoxColumn42.ReadOnly = true;
      this.dataGridViewTextBoxColumn42.Width = 60;
      this.dataGridViewTextBoxColumn43.HeaderText = "Status";
      this.dataGridViewTextBoxColumn43.Name = "dataGridViewTextBoxColumn43";
      this.dataGridViewTextBoxColumn43.ReadOnly = true;
      this.dataGridViewTextBoxColumn43.Width = 60;
      this.dataGridViewTextBoxColumn44.HeaderText = "Text";
      this.dataGridViewTextBoxColumn44.Name = "dataGridViewTextBoxColumn44";
      this.dataGridViewTextBoxColumn44.ReadOnly = true;
      this.splitter1.Cursor = Cursors.HSplit;
      this.splitter1.Dock = DockStyle.Bottom;
      this.splitter1.Location = new Point(0, 366);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new Size(743, 3);
      this.splitter1.TabIndex = 7;
      this.splitter1.TabStop = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.splitter1);
      this.Controls.Add((Control) this.panel1);
      this.Controls.Add((Control) this.panel2);
      this.Name = "OrderManagerWindow";
      this.Size = new Size(743, 520);
      this.ctxOrder.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.ctxReportsSettings.ResumeLayout(false);
      this.tabReports.ResumeLayout(false);
      this.tabPage6.ResumeLayout(false);
      this.ctxOrdersSettings.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.tabOrders.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      ((ISupportInitialize) this.dgvOrders).EndInit();
      this.tabPage2.ResumeLayout(false);
      ((ISupportInitialize) this.dgvWorkingOrders).EndInit();
      this.tabPage3.ResumeLayout(false);
      ((ISupportInitialize) this.dgvFilledOrders).EndInit();
      this.tabPage4.ResumeLayout(false);
      ((ISupportInitialize) this.dgvCancelledOrders).EndInit();
      this.tabPage5.ResumeLayout(false);
      ((ISupportInitialize) this.dgvRejectedOrders).EndInit();
      this.ResumeLayout(false);
    }
  }
}
