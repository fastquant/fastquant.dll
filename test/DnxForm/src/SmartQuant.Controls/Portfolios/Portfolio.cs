using SmartQuant;
using SmartQuant.Controls;
//using SmartQuant.Controls.Properties;
using SmartQuant.ExcelLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SmartQuant.Controls.Portfolios
{
  public class Portfolio : FrameworkControl
  {
    private string portfolioName;
    private object propertyObject;
    private PermanentQueue<Event> messageQueue;
    private IContainer components;
    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private TabPage tabPage3;
    private PerformanceWindow performance;
    private Statistics statistics;
    private ChildrenStatistics childrenStatistics;
    private CorrelationMatrix correlationMatrix;
    private ImageList imgStates;
    private TabPage tabPage4;
    private TabPage tabPage5;
    private Composition composition;
    private ContextMenuStrip ctxSettings;
    private ToolStripMenuItem ctxSettings_ExportToCSV;
    private ToolStripMenuItem ctxSettings_ExportToXLS;
    private Label lblSettings;

    public override object PropertyObject
    {
      get
      {
        return this.propertyObject;
      }
    }

    public Portfolio()
    {
      this.InitializeComponent();
    }

    public void Init(PermanentQueue<Event> messages)
    {
      this.messageQueue = messages;
      messages.AddReader((object) this);
      this.portfolioName = this.args[0] as string;
      this.Reset();
    }

    private void Reset()
    {
      this.composition.OnInit(this.portfolioName, this);
      this.performance.OnInit(this.portfolioName);
      this.statistics.OnInit(this.portfolioName);
      this.childrenStatistics.OnInit(this.portfolioName);
      this.correlationMatrix.OnInit(this.portfolioName);
    }

    protected override void OnClosing(CancelEventArgs args)
    {
      this.messageQueue.RemoveReader((object) this);
    }

    public void UpdateGUI()
    {
      if (FrameworkControl.UpdatedSuspened && this.framework.Mode != FrameworkMode.Realtime)
        return;
      Event[] eventArray = this.messageQueue.DequeueAll((object) this);
      if (eventArray != null)
      {
        Dictionary<Instrument, Event> dictionary = new Dictionary<Instrument, Event>();
        foreach (Event @event in eventArray)
        {
          switch (@event.TypeId)
          {
            case 99:
              this.Reset();
              break;
            case 110:
              OnPositionOpened onPositionOpened = @event as OnPositionOpened;
              if (onPositionOpened.Portfolio.Name == this.portfolioName)
              {
                dictionary[onPositionOpened.Position.Instrument] = @event;
                break;
              }
              break;
            case 111:
              OnPositionClosed onPositionClosed = @event as OnPositionClosed;
              if (onPositionClosed.Portfolio.Name == this.portfolioName)
              {
                dictionary[onPositionClosed.Position.Instrument] = @event;
                break;
              }
              break;
            case 112:
              OnPositionChanged onPositionChanged = @event as OnPositionChanged;
              if (onPositionChanged.Portfolio.Name == this.portfolioName)
              {
                dictionary[onPositionChanged.Position.Instrument] = @event;
                break;
              }
              break;
            case 114:
              OnTransaction onTransaction = @event as OnTransaction;
              if (onTransaction.Portfolio.Name == this.portfolioName)
              {
                this.composition.TransactionsViewItems.Add((ListViewItem) new TransactionViewItem(onTransaction.Transaction));
                break;
              }
              break;
          }
        }
        PositionViewItem positionViewItem = (PositionViewItem) null;
        foreach (Event @event in dictionary.Values)
        {
          switch (@event.TypeId)
          {
            case 99:
              this.Reset();
              continue;
            case 110:
              OnPositionOpened onPositionOpened = @event as OnPositionOpened;
              if (this.composition.PositionViewItems.TryGetValue(onPositionOpened.Position, out positionViewItem))
              {
                this.composition.UpdatePosition(onPositionOpened.Position);
                continue;
              }
              this.composition.AddPosition(onPositionOpened.Position);
              continue;
            case 111:
              OnPositionClosed onPositionClosed = @event as OnPositionClosed;
              if (this.composition.PositionViewItems.TryGetValue(onPositionClosed.Position, out positionViewItem))
              {
                this.composition.RemovePosition(onPositionClosed.Position);
                continue;
              }
              continue;
            case 112:
              OnPositionChanged onPositionChanged = @event as OnPositionChanged;
              if (this.composition.PositionViewItems.TryGetValue(onPositionChanged.Position, out positionViewItem))
              {
                this.composition.UpdatePosition(onPositionChanged.Position);
                continue;
              }
              this.composition.AddPosition(onPositionChanged.Position);
              continue;
            default:
              continue;
          }
        }
      }
      this.composition.UpdateGUI();
      this.performance.UpdateGUI();
      this.statistics.UpdateGUI();
      this.childrenStatistics.UpdateGUI();
      this.correlationMatrix.UpdateGUI();
    }

    public void UpdatePropertyObject(object obj, bool focus)
    {
      this.propertyObject = obj;
      this.OnShowProperties(focus);
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
          switch (tabPage.Text)
          {
            case "Composition":
              IEnumerator enumerator = tabPage.Controls[0].Controls.GetEnumerator();
              try
              {
                while (enumerator.MoveNext())
                {
                  Control control = (Control) enumerator.Current;
                  worksheets.AddLast();
                  Worksheet sheet = worksheets[worksheets.Count - 1];
                  sheet.Name = control.Text;
                  this.CopyDataToWorksheet(sheet, new ListView[1]
                  {
                    control.Controls[0] as ListView
                  });
                }
                break;
              }
              finally
              {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                  disposable.Dispose();
              }
            case "Performance":
              PortfolioPerformance performance = ((PerformanceWindow) tabPage.Controls[0]).Portfolio.Performance;
              worksheets.AddLast();
              Worksheet sheet1 = worksheets[worksheets.Count - 1];
              sheet1.Name = "Performance";
              this.CopyDataToWorksheet(sheet1, performance);
              break;
            case "Correlation Matrix":
            case "Children Statistics":
            case "Statistics":
              Control control1 = tabPage.Controls[0];
              worksheets.AddLast();
              Worksheet sheet2 = worksheets[worksheets.Count - 1];
              sheet2.Name = tabPage.Text;
              this.CopyDataToWorksheet(sheet2, new ListView[1]
              {
                control1.Controls[0] as ListView
              });
              break;
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

    private void CopyDataToWorksheet(Worksheet sheet, ListView[] listViewList)
    {
      int num = 2;
      int count = listViewList[0].Columns.Count;
      for (int index = 0; index < count; ++index)
      {
        ColumnHeader columnHeader = listViewList[0].Columns[index];
        Range range = sheet.GetRange(1, index + 1);
        range.Bold = true;
        range.Value = (object) columnHeader.Text;
      }
      foreach (ListView listView in listViewList)
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

    private void CopyDataToWorksheet(Worksheet sheet, PortfolioPerformance portfolioPerformance)
    {
      int num = 2;
      Range range1 = sheet.GetRange(1, 1);
      range1.Bold = true;
      range1.Value = (object) "DateTime";
      Range range2 = sheet.GetRange(1, 2);
      range2.Bold = true;
      range2.Value = (object) "Equity";
      Range range3 = sheet.GetRange(1, 3);
      range3.Bold = true;
      range3.Value = (object) "Drowdawn";
      for (int index = 0; index < portfolioPerformance.EquitySeries.Count; ++index)
      {
        sheet.GetRange(num + index, 1).Value = (object) portfolioPerformance.EquitySeries.GetDateTime(index).ToString();
        sheet.GetRange(num + index, 2).Value = (object) portfolioPerformance.EquitySeries[index];
        sheet.GetRange(num + index, 3).Value = (object) portfolioPerformance.DrawdownSeries[index];
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
          switch (tabPage.Text)
          {
            case "Composition":
              IEnumerator enumerator = tabPage.Controls[0].Controls.GetEnumerator();
              try
              {
                while (enumerator.MoveNext())
                {
                  Control control = (Control) enumerator.Current;
                  sw.WriteLine("Tab[" + tabPage.Text + "]");
                  this.WriteDataToCsv(sw, new ListView[1]
                  {
                    control.Controls[0] as ListView
                  });
                }
                break;
              }
              finally
              {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                  disposable.Dispose();
              }
            case "Performance":
              PortfolioPerformance performance = ((PerformanceWindow) tabPage.Controls[0]).Portfolio.Performance;
              sw.WriteLine("Tab[" + tabPage.Text + "]");
              this.WriteDataToCsv(sw, performance);
              break;
            case "Correlation Matrix":
            case "Children Statistics":
            case "Statistics":
              Control control1 = tabPage.Controls[0];
              sw.WriteLine("Tab[" + tabPage.Text + "]");
              this.WriteDataToCsv(sw, new ListView[1]
              {
                control1.Controls[0] as ListView
              });
              break;
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

    private void WriteDataToCsv(StreamWriter sw, PortfolioPerformance portfolioPerformance)
    {
      string listSeparator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
      sw.WriteLine(string.Format("DateTime{0}Equity{0}Drowdawn", (object) listSeparator));
      for (int index = 0; index < portfolioPerformance.EquitySeries.Count; ++index)
      {
        string str = string.Format("{1}{0}{2}{0}{3}", (object) listSeparator, (object) portfolioPerformance.EquitySeries.GetDateTime(index), (object) portfolioPerformance.EquitySeries[index], (object) portfolioPerformance.DrawdownSeries[index]);
        sw.WriteLine(str);
      }
    }

    private void ctxReportsSettings_ExportToXLS_Click(object sender, EventArgs e)
    {
      this.ExportXls(this.tabControl1);
    }

    private void ctxSettings_ExportToCSV_Click(object sender, EventArgs e)
    {
      this.ExportCsv(this.tabControl1);
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Portfolio));
      this.tabControl1 = new TabControl();
      this.tabPage1 = new TabPage();
      this.composition = new Composition();
      this.tabPage2 = new TabPage();
      this.performance = new PerformanceWindow();
      this.tabPage3 = new TabPage();
      this.statistics = new Statistics();
      this.tabPage4 = new TabPage();
      this.childrenStatistics = new ChildrenStatistics();
      this.tabPage5 = new TabPage();
      this.correlationMatrix = new CorrelationMatrix();
      this.imgStates = new ImageList(this.components);
      this.ctxSettings = new ContextMenuStrip(this.components);
      this.ctxSettings_ExportToCSV = new ToolStripMenuItem();
      this.ctxSettings_ExportToXLS = new ToolStripMenuItem();
      this.lblSettings = new Label();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.tabPage3.SuspendLayout();
      this.tabPage4.SuspendLayout();
      this.tabPage5.SuspendLayout();
      this.ctxSettings.SuspendLayout();
      this.SuspendLayout();
      this.tabControl1.Controls.Add((Control) this.tabPage1);
      this.tabControl1.Controls.Add((Control) this.tabPage2);
      this.tabControl1.Controls.Add((Control) this.tabPage3);
      this.tabControl1.Controls.Add((Control) this.tabPage4);
      this.tabControl1.Controls.Add((Control) this.tabPage5);
      this.tabControl1.Dock = DockStyle.Fill;
      this.tabControl1.Location = new Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new Size(621, 447);
      this.tabControl1.TabIndex = 0;
      this.tabPage1.Controls.Add((Control) this.composition);
      this.tabPage1.Location = new Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(613, 421);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Composition";
      this.tabPage1.UseVisualStyleBackColor = true;
      this.composition.Dock = DockStyle.Fill;
      this.composition.Location = new Point(3, 3);
      this.composition.Name = "composition";
      this.composition.Size = new Size(607, 415);
      this.composition.TabIndex = 0;
      this.tabPage2.Controls.Add((Control) this.performance);
      this.tabPage2.Location = new Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new Size(613, 421);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Performance";
      this.tabPage2.UseVisualStyleBackColor = true;
      this.performance.Dock = DockStyle.Fill;
      this.performance.Location = new Point(0, 0);
      this.performance.Name = "performance";
      this.performance.Size = new Size(613, 421);
      this.performance.TabIndex = 0;
      this.tabPage3.Controls.Add((Control) this.statistics);
      this.tabPage3.Location = new Point(4, 22);
      this.tabPage3.Name = "tabPage3";
      this.tabPage3.Size = new Size(613, 421);
      this.tabPage3.TabIndex = 2;
      this.tabPage3.Text = "Statistics";
      this.tabPage3.UseVisualStyleBackColor = true;
      this.statistics.Dock = DockStyle.Fill;
      this.statistics.Location = new Point(0, 0);
      this.statistics.Name = "statistics";
      this.statistics.Size = new Size(613, 421);
      this.statistics.TabIndex = 0;
      this.tabPage4.Controls.Add((Control) this.childrenStatistics);
      this.tabPage4.Location = new Point(4, 22);
      this.tabPage4.Name = "tabPage4";
      this.tabPage4.Size = new Size(613, 421);
      this.tabPage4.TabIndex = 3;
      this.tabPage4.Text = "Children Statistics";
      this.tabPage4.UseVisualStyleBackColor = true;
      this.childrenStatistics.Dock = DockStyle.Fill;
      this.childrenStatistics.Location = new Point(0, 0);
      this.childrenStatistics.Name = "childrenStatistics";
      this.childrenStatistics.Size = new Size(613, 421);
      this.childrenStatistics.TabIndex = 0;
      this.tabPage5.Controls.Add((Control) this.correlationMatrix);
      this.tabPage5.Location = new Point(4, 22);
      this.tabPage5.Name = "tabPage5";
      this.tabPage5.Size = new Size(613, 421);
      this.tabPage5.TabIndex = 4;
      this.tabPage5.Text = "Correlation Matrix";
      this.tabPage5.UseVisualStyleBackColor = true;
      this.correlationMatrix.Dock = DockStyle.Fill;
      this.correlationMatrix.Location = new Point(0, 0);
      this.correlationMatrix.Name = "correlationMatrix";
      this.correlationMatrix.Size = new Size(613, 421);
      this.correlationMatrix.TabIndex = 0;
      this.imgStates.ImageStream = (ImageListStreamer) componentResourceManager.GetObject("imgStates.ImageStream");
      this.imgStates.TransparentColor = Color.Transparent;
      this.imgStates.Images.SetKeyName(0, "collapsed.png");
      this.imgStates.Images.SetKeyName(1, "expanded.png");
      this.imgStates.Images.SetKeyName(2, "empty.png");
      this.ctxSettings.Items.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.ctxSettings_ExportToCSV,
        (ToolStripItem) this.ctxSettings_ExportToXLS
      });
      this.ctxSettings.Name = "ctxReportsSettings";
      this.ctxSettings.Size = new Size(153, 70);
      this.ctxSettings_ExportToCSV.Name = "ctxSettings_ExportToCSV";
      this.ctxSettings_ExportToCSV.Size = new Size(152, 22);
      this.ctxSettings_ExportToCSV.Text = "ExportToCSV";
      this.ctxSettings_ExportToCSV.Click += new EventHandler(this.ctxSettings_ExportToCSV_Click);
      this.ctxSettings_ExportToXLS.Name = "ctxSettings_ExportToXLS";
      this.ctxSettings_ExportToXLS.Size = new Size(152, 22);
      this.ctxSettings_ExportToXLS.Text = "ExportToXLS";
      this.ctxSettings_ExportToXLS.Click += new EventHandler(this.ctxReportsSettings_ExportToXLS_Click);
      this.lblSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.lblSettings.ContextMenuStrip = this.ctxSettings;
 //     this.lblSettings.Image = (Image) Resources.settings;
      this.lblSettings.Location = new Point(601, 0);
      this.lblSettings.Name = "lblSettings";
      this.lblSettings.Size = new Size(16, 16);
      this.lblSettings.TabIndex = 4;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.lblSettings);
      this.Controls.Add((Control) this.tabControl1);
      this.Name = "Portfolio";
      this.Size = new Size(621, 447);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage3.ResumeLayout(false);
      this.tabPage4.ResumeLayout(false);
      this.tabPage5.ResumeLayout(false);
      this.ctxSettings.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
