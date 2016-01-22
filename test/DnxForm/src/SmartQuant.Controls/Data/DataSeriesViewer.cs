using SmartQuant;
using SmartQuant.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal class DataSeriesViewer : UserControl
  {
    private Instrument instrument;
    private DataSeries dataSeries;
    private byte? dataObjectType;
    private DataObjectViewer viewer;
    private IContainer components;
    private ListViewNB listView;
    private ContextMenuStrip ctxObjects;
    private ToolStripMenuItem ctxObjects_Add;
    private ToolStripMenuItem ctxObjects_Edit;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem ctxObjects_Delete;
    private ToolStripSeparator toolStripMenuItem1;
    private ToolStripMenuItem ctxObjects_GoTo;
    private ToolStripMenuItem ctxObjects_DeleteRange;

    public DataSeriesViewer()
    {
      this.InitializeComponent();
      this.instrument = (Instrument) null;
      this.dataSeries = (DataSeries) null;
    }

    public void SetInstrument(Instrument instrument)
    {
      this.instrument = instrument;
    }

    public void SetDataSeries(DataSeries dataSeries, byte? dataObjectType)
    {
      this.dataSeries = dataSeries;
      this.dataObjectType = dataObjectType;
      this.Init();
    }

    private void Init()
    {
      this.listView.Clear();
      this.listView.VirtualListSize = 0;
      this.viewer = DataObjectViewer.GetViewer(this.dataObjectType);
      if (this.viewer == null)
        return;
      this.viewer.SetDataSeries(this.dataSeries);
      this.viewer.SetPriceFormat(this.instrument.PriceFormat);
      this.listView.Columns.AddRange(this.viewer.GetColumnHeaders());
      this.listView.VirtualListSize = (int) this.dataSeries.Count;
    }

    private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
      e.Item = this.viewer.GetListViewItem(e.ItemIndex);
    }

    private void listView_DoubleClick(object sender, EventArgs e)
    {
      this.EditObject();
    }

    private void listView_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Delete || this.listView.SelectedIndices.Count <= 0)
        return;
      this.DeleteObjects();
    }

    private void ctxObjects_Opening(object sender, CancelEventArgs e)
    {
      if (this.viewer == null)
      {
        e.Cancel = true;
      }
      else
      {
        switch (this.listView.SelectedIndices.Count)
        {
          case 0:
            this.ctxObjects_Add.Enabled = true;
            this.ctxObjects_Edit.Enabled = false;
            this.ctxObjects_Delete.Enabled = false;
            break;
          case 1:
            this.ctxObjects_Add.Enabled = true;
            this.ctxObjects_Edit.Enabled = true;
            this.ctxObjects_Delete.Enabled = true;
            break;
          default:
            this.ctxObjects_Add.Enabled = true;
            this.ctxObjects_Edit.Enabled = false;
            this.ctxObjects_Delete.Enabled = true;
            break;
        }
      }
    }

    private void ctxObjects_Add_Click(object sender, EventArgs e)
    {
      this.AddObject();
    }

    private void ctxObjects_Edit_Click(object sender, EventArgs e)
    {
      this.EditObject();
    }

    private void ctxObjects_GoTo_Click(object sender, EventArgs e)
    {
      this.GotoObject();
    }

    private void ctxObjects_DeleteRange_Click(object sender, EventArgs e)
    {
      this.DeleteRange();
    }

    private void ctxObjects_Delete_Click(object sender, EventArgs e)
    {
      this.DeleteObjects();
    }

    private void AddObject()
    {
      DataObjectEditor editor = this.viewer.GetEditor();
      if (editor != null)
      {
        DateTime dateTime = this.listView.SelectedIndices.Count > 0 ? this.dataSeries[(long) this.listView.SelectedIndices[0]].DateTime : DateTime.Now;
        editor.Init((SmartQuant.DataObject) null, dateTime, this.instrument.PriceFormat, this.instrument.Id);
        if (editor.ShowDialog((IWin32Window) this) == DialogResult.OK)
        {
          this.dataSeries.Add(editor.GetDataObject());
          this.viewer.ResetLastItem();
          this.listView.VirtualListSize = (int) this.dataSeries.Count;
        }
        editor.Dispose();
      }
      else
      {
        int num = (int) MessageBox.Show((IWin32Window) this, "Operation is not supported for this type of objects", "Add New", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
    }

    private void EditObject()
    {
      DataObjectEditor editor = this.viewer.GetEditor();
      if (editor != null)
      {
        int num = this.listView.SelectedIndices[0];
        editor.Init(this.dataSeries[(long) num], this.dataSeries[(long) num].DateTime, this.instrument.PriceFormat, this.instrument.Id);
        if (editor.ShowDialog((IWin32Window) this) == DialogResult.OK)
        {
          this.dataSeries.Update((long) num, editor.GetDataObject());
          this.viewer.ResetLastItem();
        }
        editor.Dispose();
      }
      else
      {
        int num1 = (int) MessageBox.Show((IWin32Window) this, "Operation is not supported for this type of objects", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
      }
    }

    private void GotoObject()
    {
      if (this.dataSeries.Count == 0L)
      {
        int num1 = (int) MessageBox.Show((IWin32Window) this, "The series is empty.", "", MessageBoxButtons.OK, MessageBoxIcon.None);
      }
      else
      {
        GotoForm gotoForm = new GotoForm();
        gotoForm.SetRange(this.dataSeries.DateTime1, this.dataSeries.DateTime2);
        if (this.viewer.GetEditor() != null)
        {
          int num2 = this.listView.SelectedIndices[0];
          if (num2 > 0 && (long) num2 < this.dataSeries.Count)
            gotoForm.SetInitialValue(this.dataSeries[(long) num2].DateTime);
        }
        if (gotoForm.ShowDialog((IWin32Window) this) == DialogResult.OK)
        {
          try
          {
            this.Cursor = Cursors.WaitCursor;
            int index = !(gotoForm.Result > this.dataSeries.DateTime2) ? (int) this.dataSeries.GetIndex(gotoForm.Result, SearchOption.Next) : (int) this.dataSeries.Count - 1;
            if (index != -1)
            {
              this.listView.EnsureVisible(index);
              this.listView.Items[index].Selected = true;
            }
            this.Cursor = Cursors.Default;
          }
          catch
          {
            this.Cursor = Cursors.Default;
            int num2 = (int) MessageBox.Show((IWin32Window) this, "An error occuired while searching position. See log file for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
          }
        }
        gotoForm.Dispose();
      }
    }

    private void DeleteRange()
    {
      if (this.dataSeries.Count == 0L)
      {
        int num1 = (int) MessageBox.Show((IWin32Window) this, "The series is empty.", "", MessageBoxButtons.OK, MessageBoxIcon.None);
      }
      else
      {
        DeleteRangeForm deleteRangeForm = new DeleteRangeForm();
        deleteRangeForm.SetRange(this.dataSeries.DateTime1, this.dataSeries.DateTime2);
        if (deleteRangeForm.ShowDialog((IWin32Window) this) == DialogResult.OK)
        {
          try
          {
            this.Cursor = Cursors.WaitCursor;
            if (deleteRangeForm.From > this.dataSeries.DateTime2 || deleteRangeForm.To < this.dataSeries.DateTime1)
            {
              int num2 = (int) MessageBox.Show((IWin32Window) this, "No items found with in the specified range.", "", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            int num3 = (int) this.dataSeries.GetIndex(deleteRangeForm.From.AddTicks(-1L), SearchOption.Next);
            int num4 = (int) this.dataSeries.GetIndex(deleteRangeForm.To.AddTicks(1L), SearchOption.Prev);
            if (num3 != -1 && num4 != -1 && MessageBox.Show((IWin32Window) this, "Are you sure you want to delete " + (num4 - num3 + 1).ToString() + " items?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
              for (int index = num3; index <= num4; ++index)
                this.dataSeries.Remove((long) num3);
              this.Init();
              int num5 = (int) MessageBox.Show((IWin32Window) this, (num4 - num3 + 1).ToString() + " items removed successfully", "", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            this.Cursor = Cursors.Default;
          }
          catch
          {
            this.Cursor = Cursors.Default;
            int num2 = (int) MessageBox.Show((IWin32Window) this, "An error occuired while searching position. See log file for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
          }
        }
        deleteRangeForm.Dispose();
      }
    }

    private void DeleteObjects()
    {
      if (MessageBox.Show((IWin32Window) this, "Do you really want to delete selected object(s)?", "Delete Object(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        return;
      List<int> list = new List<int>();
      foreach (int num in this.listView.SelectedIndices)
        list.Add(num);
      list.Sort();
      for (int index = 0; index < list.Count; ++index)
        this.dataSeries.Remove((long) (list[index] - index));
      this.viewer.ResetLastItem();
      this.listView.VirtualListSize = (int) this.dataSeries.Count;
      this.listView.SelectedIndices.Clear();
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
      this.listView = new ListViewNB();
      this.ctxObjects = new ContextMenuStrip(this.components);
      this.ctxObjects_Add = new ToolStripMenuItem();
      this.ctxObjects_Edit = new ToolStripMenuItem();
      this.toolStripMenuItem1 = new ToolStripSeparator();
      this.ctxObjects_GoTo = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.ctxObjects_DeleteRange = new ToolStripMenuItem();
      this.ctxObjects_Delete = new ToolStripMenuItem();
      this.ctxObjects.SuspendLayout();
      this.SuspendLayout();
      this.listView.ContextMenuStrip = this.ctxObjects;
      this.listView.Dock = DockStyle.Fill;
      this.listView.FullRowSelect = true;
      this.listView.GridLines = true;
      this.listView.HideSelection = false;
      this.listView.Location = new Point(0, 0);
      this.listView.Name = "listView";
      this.listView.ShowGroups = false;
      this.listView.Size = new Size(430, 315);
      this.listView.TabIndex = 0;
      this.listView.UseCompatibleStateImageBehavior = false;
      this.listView.View = View.Details;
      this.listView.VirtualMode = true;
      this.listView.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this.listView_RetrieveVirtualItem);
      this.listView.DoubleClick += new EventHandler(this.listView_DoubleClick);
      this.listView.KeyDown += new KeyEventHandler(this.listView_KeyDown);
      this.ctxObjects.Items.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.ctxObjects_Add,
        (ToolStripItem) this.ctxObjects_Edit,
        (ToolStripItem) this.toolStripMenuItem1,
        (ToolStripItem) this.ctxObjects_GoTo,
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.ctxObjects_DeleteRange,
        (ToolStripItem) this.ctxObjects_Delete
      });
      this.ctxObjects.Name = "ctxObjects";
      this.ctxObjects.Size = new Size(153, 148);
      this.ctxObjects.Opening += new CancelEventHandler(this.ctxObjects_Opening);
      this.ctxObjects_Add.Name = "ctxObjects_Add";
      this.ctxObjects_Add.Size = new Size(152, 22);
      this.ctxObjects_Add.Text = "Add New...";
      this.ctxObjects_Add.Click += new EventHandler(this.ctxObjects_Add_Click);
      this.ctxObjects_Edit.Name = "ctxObjects_Edit";
      this.ctxObjects_Edit.Size = new Size(152, 22);
      this.ctxObjects_Edit.Text = "Edit...";
      this.ctxObjects_Edit.Click += new EventHandler(this.ctxObjects_Edit_Click);
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new Size(149, 6);
      this.ctxObjects_GoTo.Name = "ctxObjects_GoTo";
      this.ctxObjects_GoTo.Size = new Size(152, 22);
      this.ctxObjects_GoTo.Text = "Go to...";
      this.ctxObjects_GoTo.Click += new EventHandler(this.ctxObjects_GoTo_Click);
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(149, 6);
      this.ctxObjects_DeleteRange.Name = "ctxObjects_DeleteRange";
      this.ctxObjects_DeleteRange.Size = new Size(152, 22);
      this.ctxObjects_DeleteRange.Text = "Delete Range...";
      this.ctxObjects_DeleteRange.Click += new EventHandler(this.ctxObjects_DeleteRange_Click);
      this.ctxObjects_Delete.Name = "ctxObjects_Delete";
      this.ctxObjects_Delete.Size = new Size(152, 22);
      this.ctxObjects_Delete.Text = "Delete";
      this.ctxObjects_Delete.Click += new EventHandler(this.ctxObjects_Delete_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.listView);
      this.Name = "DataSeriesViewer";
      this.Size = new Size(430, 315);
      this.ctxObjects.ResumeLayout(false);
      this.ResumeLayout(false);
    }
  }
}
