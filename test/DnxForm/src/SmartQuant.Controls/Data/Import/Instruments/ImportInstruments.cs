// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.Import.Instruments.ImportInstruments
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
using System.Threading;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data.Import.Instruments
{
  public class ImportInstruments : FrameworkControl
  {
    private IInstrumentProvider provider;
    private string requestId;
    private List<Instrument> instruments;
    private IContainer components;
    private Panel panel1;
    private GroupBox gbxFilter;
    private StatusStrip statusStrip1;
    private ListView ltvInstruments;
    private Button btnRequest;
    private Panel panel3;
    private CheckBox chbSymbol;
    private CheckBox chbExchange;
    private CheckBox chbInstrumentType;
    private TextBox tbxSymbol;
    private TextBox tbxExchange;
    private ComboBox cbxInstrumentType;
    private Button btnCancel;
    private ToolStripStatusLabel tsiInfo;
    private ToolStripProgressBar tsiProgress;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    private ColumnHeader columnHeader3;
    private ColumnHeader columnHeader4;
    private ColumnHeader columnHeader5;
    private Button btnImport;
    private ContextMenuStrip ctxInstruments;
    private ToolStripMenuItem ctxInstruments_Properties;
    private CheckBox chbUpdate;
    private CheckBox chbUseContractSymbol;

    public override object PropertyObject
    {
      get
      {
        if (this.ltvInstruments.SelectedItems.Count == 1)
          return (object) ((InstrumentViewItem) this.ltvInstruments.SelectedItems[0]).Instrument;
        return (object) null;
      }
    }

    public ImportInstruments()
    {
      this.InitializeComponent();
      this.requestId = (string) null;
      this.instruments = new List<Instrument>();
      this.cbxInstrumentType.BeginUpdate();
      this.cbxInstrumentType.Items.Clear();
      foreach (InstrumentType instrumentType in Enum.GetValues(typeof (InstrumentType)))
        this.cbxInstrumentType.Items.Add((object) instrumentType);
      this.cbxInstrumentType.SelectedItem = (object) InstrumentType.Stock;
      this.cbxInstrumentType.EndUpdate();
    }

    protected override void OnInit()
    {
      this.provider = (IInstrumentProvider) this.args[0];
      this.framework.EventManager.Dispatcher.InstrumentDefinition += new InstrumentDefinitionEventHandler(this.Dispatcher_InstrumentDefinition);
      this.framework.EventManager.Dispatcher.InstrumentDefinitionEnd += new InstrumentDefinitionEndEventHandler(this.Dispatcher_InstrumentDefinitionEnd);
    }

    protected override void OnClosing(CancelEventArgs args)
    {
      this.framework.EventManager.Dispatcher.InstrumentDefinition -= new InstrumentDefinitionEventHandler(this.Dispatcher_InstrumentDefinition);
      this.framework.EventManager.Dispatcher.InstrumentDefinitionEnd -= new InstrumentDefinitionEndEventHandler(this.Dispatcher_InstrumentDefinitionEnd);
    }

    private void chbInstrumentType_CheckedChanged(object sender, EventArgs e)
    {
      this.cbxInstrumentType.Enabled = this.chbInstrumentType.Checked;
    }

    private void chbExchange_CheckedChanged(object sender, EventArgs e)
    {
      this.tbxExchange.Enabled = this.chbExchange.Checked;
    }

    private void chbSymbol_CheckedChanged(object sender, EventArgs e)
    {
      this.tbxSymbol.Enabled = this.chbSymbol.Checked;
    }

    private void btnRequest_Click(object sender, EventArgs e)
    {
      this.instruments.Clear();
      this.gbxFilter.Enabled = false;
      this.btnRequest.Visible = false;
      this.btnCancel.Visible = true;
      this.btnImport.Visible = false;
      this.chbUpdate.Visible = false;
      this.ltvInstruments.Enabled = false;
      this.tsiInfo.Text = "Requesting instruments...";
      this.tsiProgress.Value = 0;
      this.tsiProgress.Visible = true;
      this.requestId = Guid.NewGuid().ToString();
      InstrumentDefinitionRequest request = new InstrumentDefinitionRequest();
      request.Id = this.requestId;
      if (this.chbInstrumentType.Checked)
        request.FilterType = new InstrumentType?((InstrumentType) this.cbxInstrumentType.SelectedItem);
      if (this.chbExchange.Checked)
      {
        string str = this.tbxExchange.Text.Trim();
        if (str != string.Empty)
          request.FilterExchange = str;
      }
      if (this.chbSymbol.Checked)
      {
        string str = this.tbxSymbol.Text.Trim();
        if (str != string.Empty)
          request.FilterSymbol = str;
      }
      ThreadPool.QueueUserWorkItem((WaitCallback) (obj => this.provider.Send(request)));
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      this.provider.Cancel(this.requestId);
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
      List<Instrument> list = new List<Instrument>();
      foreach (InstrumentViewItem instrumentViewItem in this.ltvInstruments.CheckedItems)
      {
        if (instrumentViewItem.Text != instrumentViewItem.Instrument.Symbol)
        {
          Instrument instrument = instrumentViewItem.Instrument.Clone(instrumentViewItem.Text);
          list.Add(instrument);
        }
        else
          list.Add(instrumentViewItem.Instrument);
      }
      if (list.Count == 0)
      {
        int num1 = (int) MessageBox.Show((IWin32Window) this, "No instruments selected.", this.GetMessageBoxCaption(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else
      {
        int num2 = 0;
        int num3 = 0;
        foreach (Instrument instrument1 in list)
        {
          if (this.framework.InstrumentManager.Instruments.Contains(instrument1.Symbol))
          {
            ++num3;
            if (this.chbUpdate.Checked && instrument1.AltId.Get(this.provider.Id) != null)
            {
              Instrument instrument2 = this.framework.InstrumentManager.Instruments[instrument1.Symbol];
              if (instrument2.AltId.Get(this.provider.Id) != null)
                instrument2.AltId.Remove(instrument2.AltId.Get(this.provider.Id));
              instrument2.AltId.Add(instrument1.AltId.Get(this.provider.Id));
              this.framework.InstrumentManager.Save(instrument2);
            }
          }
          else
          {
            this.framework.InstrumentManager.Add(instrument1, true);
            ++num2;
          }
        }
        int num4 = (int) MessageBox.Show((IWin32Window) this, string.Format("Added: {0} Existing: {1}", (object) num2, (object) num3), this.GetMessageBoxCaption(), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
    }

    private void ltvInstruments_SelectedIndexChanged(object sender, EventArgs e)
    {
      this.OnShowProperties(false);
    }

    private void ctxInstruments_Opening(object sender, CancelEventArgs e)
    {
      if (this.ltvInstruments.SelectedItems.Count == 1)
        this.ctxInstruments_Properties.Enabled = true;
      else
        this.ctxInstruments_Properties.Enabled = false;
    }

    private void ctxInstruments_Properties_Click(object sender, EventArgs e)
    {
      this.OnShowProperties(true);
    }

    private void Dispatcher_InstrumentDefinition(object sender, InstrumentDefinitionEventArgs args)
    {
      InstrumentDefinition definition = args.Definition;
      if (definition.RequestId != this.requestId)
        return;
      this.instruments.AddRange((IEnumerable<Instrument>) definition.Instruments);
      this.InvokeAction((Action) (() =>
      {
        if (definition.TotalNum <= 0)
          return;
        double num = (double) this.instruments.Count / (double) definition.TotalNum * 100.0;
        if (num > 100.0)
          num = 100.0;
        this.tsiProgress.Value = (int) num;
      }));
    }

    private void Dispatcher_InstrumentDefinitionEnd(object sender, InstrumentDefinitionEndEventArgs args)
    {
      InstrumentDefinitionEnd end = args.End;
      if (end.RequestId != this.requestId)
        return;
      this.requestId = (string) null;
      this.InvokeAction((Action) (() =>
      {
        this.ltvInstruments.BeginUpdate();
        this.ltvInstruments.Items.Clear();
        foreach (Instrument instrument in this.instruments)
          this.ltvInstruments.Items.Add((ListViewItem) new InstrumentViewItem(instrument));
        this.ltvInstruments.EndUpdate();
        this.gbxFilter.Enabled = true;
        this.btnRequest.Visible = true;
        this.btnCancel.Visible = false;
        if (this.instruments.Count > 0)
        {
          this.btnImport.Visible = true;
          this.chbUpdate.Visible = true;
        }
        this.ltvInstruments.Enabled = true;
        this.tsiInfo.Text = string.Format("{0} - {1}", (object) end.Result, (object) end.Text);
        this.tsiProgress.Visible = false;
      }));
      if (!this.chbUseContractSymbol.Checked)
        return;
      this.chbUseContractSymbol_CheckedChanged((object) this, (EventArgs) null);
    }

    private void ltvInstruments_DoubleClick(object sender, EventArgs e)
    {
      if (this.ltvInstruments.SelectedItems.Count != 1)
        return;
      this.ltvInstruments.SelectedItems[0].BeginEdit();
    }

    private void ltvInstruments_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
      if (!string.IsNullOrEmpty(e.Label))
        return;
      e.CancelEdit = true;
    }

    private void chbUseContractSymbol_CheckedChanged(object sender, EventArgs e)
    {
      if (this.instruments.Count == 0)
        return;
      bool IsChecked = this.chbUseContractSymbol.Checked;
      this.InvokeAction((Action) (() =>
      {
        this.ltvInstruments.BeginUpdate();
        this.ltvInstruments.Items.Clear();
        foreach (Instrument instrument in this.instruments)
        {
          InstrumentViewItem instrumentViewItem1 = new InstrumentViewItem(instrument);
          if (IsChecked && instrument.Maturity != DateTime.MinValue)
          {
            if (instrument.Type == InstrumentType.Option || instrument.Type == InstrumentType.FutureOption)
            {
              InstrumentViewItem instrumentViewItem2 = instrumentViewItem1;
              string str = instrumentViewItem2.Text + (object) " " + instrument.Maturity.ToString("yyMMdd") + (string) (instrument.PutCall == PutCall.Call ? (object) "C" : (object) "P") + (string) (object) instrument.Strike;
              instrumentViewItem2.Text = str;
            }
            else
            {
              InstrumentViewItem instrumentViewItem2 = instrumentViewItem1;
              string str = instrumentViewItem2.Text + " " + this.ExpToString(instrument.Maturity);
              instrumentViewItem2.Text = str;
            }
          }
          this.ltvInstruments.Items.Add((ListViewItem) instrumentViewItem1);
        }
        this.ltvInstruments.EndUpdate();
      }));
    }

    private string ExpToString(DateTime date)
    {
      string str = "";
      switch (date.Month)
      {
        case 1:
          str = "F";
          break;
        case 2:
          str = "G";
          break;
        case 3:
          str = "H";
          break;
        case 4:
          str = "J";
          break;
        case 5:
          str = "K";
          break;
        case 6:
          str = "M";
          break;
        case 7:
          str = "N";
          break;
        case 8:
          str = "Q";
          break;
        case 9:
          str = "U";
          break;
        case 10:
          str = "V";
          break;
        case 11:
          str = "X";
          break;
        case 12:
          str = "Z";
          break;
      }
      return str + date.ToString("yy");
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
      this.panel1 = new Panel();
      this.chbUpdate = new CheckBox();
      this.btnImport = new Button();
      this.btnCancel = new Button();
      this.btnRequest = new Button();
      this.gbxFilter = new GroupBox();
      this.chbUseContractSymbol = new CheckBox();
      this.tbxSymbol = new TextBox();
      this.tbxExchange = new TextBox();
      this.cbxInstrumentType = new ComboBox();
      this.chbSymbol = new CheckBox();
      this.chbExchange = new CheckBox();
      this.chbInstrumentType = new CheckBox();
      this.statusStrip1 = new StatusStrip();
      this.tsiInfo = new ToolStripStatusLabel();
      this.tsiProgress = new ToolStripProgressBar();
      this.ltvInstruments = new ListView();
      this.columnHeader1 = new ColumnHeader();
      this.columnHeader2 = new ColumnHeader();
      this.columnHeader3 = new ColumnHeader();
      this.columnHeader4 = new ColumnHeader();
      this.columnHeader5 = new ColumnHeader();
      this.ctxInstruments = new ContextMenuStrip(this.components);
      this.ctxInstruments_Properties = new ToolStripMenuItem();
      this.panel3 = new Panel();
      this.panel1.SuspendLayout();
      this.gbxFilter.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.ctxInstruments.SuspendLayout();
      this.SuspendLayout();
      this.panel1.Controls.Add((Control) this.chbUpdate);
      this.panel1.Controls.Add((Control) this.btnImport);
      this.panel1.Controls.Add((Control) this.btnCancel);
      this.panel1.Controls.Add((Control) this.btnRequest);
      this.panel1.Dock = DockStyle.Right;
      this.panel1.Location = new Point(550, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(120, 436);
      this.panel1.TabIndex = 0;
      this.chbUpdate.Location = new Point(20, 118);
      this.chbUpdate.Name = "chbUpdate";
      this.chbUpdate.Size = new Size(110, 18);
      this.chbUpdate.TabIndex = 3;
      this.chbUpdate.Text = "Update";
      this.chbUpdate.UseVisualStyleBackColor = true;
      this.chbUpdate.Visible = false;
      this.btnImport.Location = new Point(20, 88);
      this.btnImport.Name = "btnImport";
      this.btnImport.Size = new Size(80, 24);
      this.btnImport.TabIndex = 2;
      this.btnImport.Text = "Import";
      this.btnImport.UseVisualStyleBackColor = true;
      this.btnImport.Visible = false;
      this.btnImport.Click += new EventHandler(this.btnImport_Click);
      this.btnCancel.Location = new Point(20, 40);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new Size(80, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Visible = false;
      this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
      this.btnRequest.Location = new Point(20, 8);
      this.btnRequest.Name = "btnRequest";
      this.btnRequest.Size = new Size(80, 24);
      this.btnRequest.TabIndex = 0;
      this.btnRequest.Text = "Request";
      this.btnRequest.UseVisualStyleBackColor = true;
      this.btnRequest.Click += new EventHandler(this.btnRequest_Click);
      this.gbxFilter.Controls.Add((Control) this.chbUseContractSymbol);
      this.gbxFilter.Controls.Add((Control) this.tbxSymbol);
      this.gbxFilter.Controls.Add((Control) this.tbxExchange);
      this.gbxFilter.Controls.Add((Control) this.cbxInstrumentType);
      this.gbxFilter.Controls.Add((Control) this.chbSymbol);
      this.gbxFilter.Controls.Add((Control) this.chbExchange);
      this.gbxFilter.Controls.Add((Control) this.chbInstrumentType);
      this.gbxFilter.Dock = DockStyle.Top;
      this.gbxFilter.Location = new Point(0, 4);
      this.gbxFilter.Name = "gbxFilter";
      this.gbxFilter.Size = new Size(550, 80);
      this.gbxFilter.TabIndex = 1;
      this.gbxFilter.TabStop = false;
      this.gbxFilter.Text = "Filter";
      this.chbUseContractSymbol.Location = new Point(400, 24);
      this.chbUseContractSymbol.Name = "chbUseContractSymbol";
      this.chbUseContractSymbol.Size = new Size(125, 18);
      this.chbUseContractSymbol.TabIndex = 6;
      this.chbUseContractSymbol.Text = "UseContractSymbol";
      this.chbUseContractSymbol.UseVisualStyleBackColor = true;
      this.chbUseContractSymbol.CheckedChanged += new EventHandler(this.chbUseContractSymbol_CheckedChanged);
      this.tbxSymbol.Enabled = false;
      this.tbxSymbol.Location = new Point(276, 48);
      this.tbxSymbol.Name = "tbxSymbol";
      this.tbxSymbol.Size = new Size(110, 20);
      this.tbxSymbol.TabIndex = 5;
      this.tbxExchange.Enabled = false;
      this.tbxExchange.Location = new Point(148, 48);
      this.tbxExchange.Name = "tbxExchange";
      this.tbxExchange.Size = new Size(110, 20);
      this.tbxExchange.TabIndex = 4;
      this.cbxInstrumentType.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbxInstrumentType.Enabled = false;
      this.cbxInstrumentType.FormattingEnabled = true;
      this.cbxInstrumentType.Location = new Point(20, 48);
      this.cbxInstrumentType.Name = "cbxInstrumentType";
      this.cbxInstrumentType.Size = new Size(110, 21);
      this.cbxInstrumentType.TabIndex = 3;
      this.chbSymbol.Location = new Point(276, 24);
      this.chbSymbol.Name = "chbSymbol";
      this.chbSymbol.Size = new Size(110, 18);
      this.chbSymbol.TabIndex = 2;
      this.chbSymbol.Text = "Symbol";
      this.chbSymbol.UseVisualStyleBackColor = true;
      this.chbSymbol.CheckedChanged += new EventHandler(this.chbSymbol_CheckedChanged);
      this.chbExchange.Location = new Point(148, 24);
      this.chbExchange.Name = "chbExchange";
      this.chbExchange.Size = new Size(110, 18);
      this.chbExchange.TabIndex = 1;
      this.chbExchange.Text = "Exchange";
      this.chbExchange.UseVisualStyleBackColor = true;
      this.chbExchange.CheckedChanged += new EventHandler(this.chbExchange_CheckedChanged);
      this.chbInstrumentType.Location = new Point(20, 24);
      this.chbInstrumentType.Name = "chbInstrumentType";
      this.chbInstrumentType.Size = new Size(110, 18);
      this.chbInstrumentType.TabIndex = 0;
      this.chbInstrumentType.Text = "InstrumentType";
      this.chbInstrumentType.UseVisualStyleBackColor = true;
      this.chbInstrumentType.CheckedChanged += new EventHandler(this.chbInstrumentType_CheckedChanged);
      this.statusStrip1.Items.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.tsiInfo,
        (ToolStripItem) this.tsiProgress
      });
      this.statusStrip1.Location = new Point(0, 440);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new Size(670, 22);
      this.statusStrip1.SizingGrip = false;
      this.statusStrip1.TabIndex = 2;
      this.statusStrip1.Text = "statusStrip1";
      this.tsiInfo.Name = "tsiInfo";
      this.tsiInfo.Size = new Size(655, 17);
      this.tsiInfo.Spring = true;
      this.tsiInfo.TextAlign = ContentAlignment.MiddleLeft;
      this.tsiProgress.Name = "tsiProgress";
      this.tsiProgress.Size = new Size(100, 16);
      this.tsiProgress.Visible = false;
      this.ltvInstruments.CheckBoxes = true;
      this.ltvInstruments.Columns.AddRange(new ColumnHeader[5]
      {
        this.columnHeader1,
        this.columnHeader2,
        this.columnHeader3,
        this.columnHeader4,
        this.columnHeader5
      });
      this.ltvInstruments.ContextMenuStrip = this.ctxInstruments;
      this.ltvInstruments.Dock = DockStyle.Fill;
      this.ltvInstruments.FullRowSelect = true;
      this.ltvInstruments.GridLines = true;
      this.ltvInstruments.HideSelection = false;
      this.ltvInstruments.LabelEdit = true;
      this.ltvInstruments.LabelWrap = false;
      this.ltvInstruments.Location = new Point(0, 92);
      this.ltvInstruments.Name = "ltvInstruments";
      this.ltvInstruments.ShowGroups = false;
      this.ltvInstruments.ShowItemToolTips = true;
      this.ltvInstruments.Size = new Size(550, 348);
      this.ltvInstruments.TabIndex = 3;
      this.ltvInstruments.UseCompatibleStateImageBehavior = false;
      this.ltvInstruments.View = View.Details;
      this.ltvInstruments.AfterLabelEdit += new LabelEditEventHandler(this.ltvInstruments_AfterLabelEdit);
      this.ltvInstruments.SelectedIndexChanged += new EventHandler(this.ltvInstruments_SelectedIndexChanged);
      this.ltvInstruments.DoubleClick += new EventHandler(this.ltvInstruments_DoubleClick);
      this.columnHeader1.Text = "Symbol";
      this.columnHeader1.Width = 93;
      this.columnHeader2.Text = "InstrumentType";
      this.columnHeader2.TextAlign = HorizontalAlignment.Right;
      this.columnHeader2.Width = 103;
      this.columnHeader3.Text = "Exchange";
      this.columnHeader3.TextAlign = HorizontalAlignment.Right;
      this.columnHeader3.Width = 80;
      this.columnHeader4.Text = "Currency";
      this.columnHeader4.TextAlign = HorizontalAlignment.Right;
      this.columnHeader4.Width = 80;
      this.columnHeader5.Text = "Maturity";
      this.columnHeader5.TextAlign = HorizontalAlignment.Right;
      this.columnHeader5.Width = 104;
      this.ctxInstruments.Items.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.ctxInstruments_Properties
      });
      this.ctxInstruments.Name = "ctxInstruments";
      this.ctxInstruments.Size = new Size(128, 26);
      this.ctxInstruments.Opening += new CancelEventHandler(this.ctxInstruments_Opening);
    //  this.ctxInstruments_Properties.Image = (Image) Resources.properties;
      this.ctxInstruments_Properties.Name = "ctxInstruments_Properties";
      this.ctxInstruments_Properties.Size = new Size((int) sbyte.MaxValue, 22);
      this.ctxInstruments_Properties.Text = "Properties";
      this.ctxInstruments_Properties.Click += new EventHandler(this.ctxInstruments_Properties_Click);
      this.panel3.Dock = DockStyle.Top;
      this.panel3.Location = new Point(0, 84);
      this.panel3.Name = "panel3";
      this.panel3.Size = new Size(550, 8);
      this.panel3.TabIndex = 5;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.ltvInstruments);
      this.Controls.Add((Control) this.panel3);
      this.Controls.Add((Control) this.gbxFilter);
      this.Controls.Add((Control) this.panel1);
      this.Controls.Add((Control) this.statusStrip1);
      this.Name = "ImportInstruments";
      this.Padding = new Padding(0, 4, 0, 0);
      this.Size = new Size(670, 462);
      this.panel1.ResumeLayout(false);
      this.gbxFilter.ResumeLayout(false);
      this.gbxFilter.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.ctxInstruments.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
