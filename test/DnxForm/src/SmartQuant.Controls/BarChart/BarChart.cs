using System;
using System.Collections.Generic;
using SmartQuant;
using SmartQuant.Charting;
using SmartQuant.ChartViewers;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

#if GTK
using Compatibility.Gtk;
using Gtk;

#else
using System.Windows.Forms;
#endif

namespace SmartQuant.Controls.BarChart
{
    public class BarChart : FrameworkControl, IGroupListener
    {
        private IContainer components;
        private Dictionary<int, GroupItem> table = new Dictionary<int, GroupItem>();
        private Dictionary<object, List<GroupEvent>> eventsBySelectorKey = new Dictionary<object, List<GroupEvent>>();
        private Dictionary<int, List<Group>> orderedGroupTable = new Dictionary<int, List<Group>>();
        private Dictionary<object, Dictionary<int, List<Group>>> drawnGroupTable = new Dictionary<object, Dictionary<int, List<Group>>>();
        private long barSize = 60;
        private bool freezeUpdate;
        private DateTime firstDateTime;
        private Chart chart;
        private ComboBox cbxSelector;

        public PermanentQueue<Event> Queue { get; private set; }

        public BarChart()
        {
            InitComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.components?.Dispose();
            base.Dispose(disposing);
        }


        private void OnFrameworkCleared(object sender, FrameworkEventArgs args)
        {
            InvokeAction(delegate
            {
                #if GTK
                this.cbxSelector.ClearTexts();
                #else
                this.cbxSelector.Items.Clear();
                #endif
                Reset(false);
                this.chart.UpdatePads();
            });
            this.eventsBySelectorKey.Clear();
            this.eventsBySelectorKey[""] = new List<GroupEvent>();
        }

        protected override void OnInit()
        {
            Queue = new PermanentQueue<Event>();
            Queue.AddReader(this);
            Reset(true);
            this.framework.EventManager.Dispatcher.FrameworkCleared += OnFrameworkCleared;
            this.framework.GroupDispatcher.AddListener(this);
            this.eventsBySelectorKey[""] = new List<GroupEvent>();
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            Queue.RemoveReader(this);
            this.framework.EventManager.Dispatcher.FrameworkCleared -= OnFrameworkCleared;
            this.framework.GroupDispatcher.RemoveListener(this);
        }

        public bool OnNewGroup(Group group)
        {
            if (!group.Fields.ContainsKey("Pad"))
                return false;
            this.table[group.Id] = new GroupItem(group);
            List<Group> list = null;
            int key = (int)group.Fields["Pad"].Value;
            if (!this.orderedGroupTable.TryGetValue(key, out list))
            {
                list = new List<Group>();
                this.orderedGroupTable[key] = list;
            }
            list.Add(group);
            InvokeAction(delegate
            {
                if (!group.Fields.ContainsKey("SelectorKey"))
                    return;
                string str = (string)group.Fields["SelectorKey"].Value;
                #if GTK               
                if (this.cbxSelector.ContainsText(str))
                    return;
                this.cbxSelector.AppendText(str);
                this.eventsBySelectorKey[str] = new List<GroupEvent>();
                this.freezeUpdate = true;
                if (this.cbxSelector.Model.IterNChildren() == 1)
                    this.cbxSelector.Active = 0;
                #else
                if (this.cbxSelector.Items.Contains(str))
                    return;
                this.cbxSelector.Items.Add(str);
                this.eventsBySelectorKey[str] = new List<GroupEvent>();
                this.freezeUpdate = true;
                if (this.cbxSelector.Items.Count == 1)
                    this.cbxSelector.SelectedIndex = 0;
                #endif
                this.freezeUpdate = false;
            });
            return true;
        }

        public void OnNewGroupEvent(GroupEvent groupEvent)
        {
            var item = this.table[groupEvent.Group.Id];
            Tuple<Viewer, object> tuple = null;
            item.Table.TryGetValue(groupEvent.Obj.TypeId, out tuple);
            switch (groupEvent.Obj.TypeId)
            {
                case DataObjectType.Bar:
                    object bs;
                    if (tuple == null)
                    {
                        bs = new BarSeries("", "", -1);
                        int padNumber = item.PadNumber;
                        this.EnsurePadExists(padNumber, item.Format);
                        int viewerIndex = this.GetViewerIndex(groupEvent.Group, padNumber);
                        Viewer viewer = this.chart.Pads[padNumber].Insert(viewerIndex, bs as BarSeries);
                        this.chart.Pads[padNumber].Legend.Add(groupEvent.Group.Name, Color.Black);
                        item.Table.Add(groupEvent.Obj.TypeId, new Tuple<Viewer, object>(viewer, bs));
                    }
                    else
                        bs = tuple.Item2 as BarSeries;
                    (bs as BarSeries).Add(groupEvent.Obj as Bar);
                    break;
                case DataObjectType.Fill:
                    object fs;
                    if (tuple == null)
                    {
                        fs = new FillSeries("");
                        int padNumber = item.PadNumber;
                        this.EnsurePadExists(padNumber, item.Format);
                        int viewerIndex = this.GetViewerIndex(groupEvent.Group, padNumber);
                        Viewer viewer = this.chart.Pads[padNumber].Insert(viewerIndex, fs);
                        item.Table.Add(groupEvent.Obj.TypeId, new Tuple<Viewer, object>(viewer, fs));
                    }
                    else
                        fs = tuple.Item2 as FillSeries;
                    (fs as FillSeries).Add(groupEvent.Obj as Fill);
                    break;
                case DataObjectType.TimeSeriesItem:
                    object ts;
                    if (tuple == null)
                    {
                        ts = new TimeSeries();
                        int padNumber = item.PadNumber;
                        EnsurePadExists(padNumber, item.Format);
                        int viewerIndex = this.GetViewerIndex(groupEvent.Group, padNumber);
                        Viewer viewer = this.chart.Pads[padNumber].Insert(viewerIndex, ts);
                        foreach (var kv in groupEvent.Group.Fields)
                            viewer.Set(ts, kv.Value.Name, kv.Value.Value);
                        if (groupEvent.Group.Fields.ContainsKey("Color"))
                            this.chart.Pads[padNumber].Legend.Add(groupEvent.Group.Name, (Color)groupEvent.Group.Fields["Color"].Value);
                        else
                            this.chart.Pads[padNumber].Legend.Add(groupEvent.Group.Name, Color.Black);
                        item.Table.Add(groupEvent.Obj.TypeId, new Tuple<Viewer, object>(viewer, ts));
                    }
                    else
                        ts = tuple.Item2 as TimeSeries;
                    (ts as TimeSeries).Add((groupEvent.Obj as TimeSeriesItem).DateTime, (groupEvent.Obj as TimeSeriesItem).Value);
                    break;
            }
        }

        private int GetViewerIndex(Group group, int padNumber)
        {
            var list1 = this.orderedGroupTable[padNumber];
            List<Group> list2;
            Dictionary<int, List<Group>> dictionary;
            var selected = GetComboBoxSelected();
            if (!this.drawnGroupTable.TryGetValue(selected, out dictionary))
            {
                dictionary = new Dictionary<int, List<Group>>();
                this.drawnGroupTable[selected] = dictionary;
            }
            if (!dictionary.TryGetValue(padNumber, out list2))
            {
                dictionary[padNumber] = new List<Group>() { group };
                return 0;
            }
            else
            {
                bool flag = false;
                for (int i = 0; i < list2.Count; ++i)
                {
                    Group group1 = list2[i];
                    if (group1 != group && list1.IndexOf(group) < list1.IndexOf(group1))
                    {
                        list2.Insert(i, group1);
                        return i;
                    }
                }
                if (flag)
                    return 0;
                list2.Add(group);
                return list2.Count - 1;
            }
        }

        private void OnNewGroupUpdate_(GroupUpdate groupUpdate)
        {
            var item = this.table[groupUpdate.GroupId];
            if (groupUpdate.FieldName == "Pad")
            {
                int padNumber = item.PadNumber;
                string format = item.Format;
                int newPad = (int)groupUpdate.Value;
                string labelFormat = (string)groupUpdate.Value;
                foreach (var kv in item.Table)
                {
                    this.chart.Pads[padNumber].Remove(kv.Value.Item2);
                    this.EnsurePadExists(newPad, labelFormat);
                    this.chart.Pads[newPad].Add(kv.Value.Item2);
                }
                item.PadNumber = newPad;
                item.Format = labelFormat;
            }
            if (groupUpdate.FieldName == "Color")
            {
                Color color = (Color)groupUpdate.Value;
                foreach (var kv in item.Table)
                {
                    if (kv.Value.Item1 is TimeSeriesViewer)
                        (kv.Value.Item1 as TimeSeriesViewer).Color = color;
                }
                this.chart.UpdatePads();
            }
        }

        public void OnNewGroupUpdate(GroupUpdate groupUpdate)
        {
            #if GTK
            Gtk.Application.Invoke((sender, e) => OnNewGroupUpdate_(groupUpdate));
            #else
            if (InvokeRequired)
                Invoke((Action)delegate
                {
                    OnNewGroupUpdate(groupUpdate);
                });
            else
                OnNewGroupUpdate_(groupUpdate);
            #endif
        }

        private void EnsurePadExists(int newPad, string labelFormat)
        {
            for (int count = this.chart.Pads.Count; count <= newPad; ++count)
            {
                var pad = AddPad();
                pad.RegisterViewer(new BarSeriesViewer());
                pad.RegisterViewer(new TimeSeriesViewer());
                pad.RegisterViewer(new FillSeriesViewer());
                pad.RegisterViewer(new TickSeriesViewer());
                pad.AxisBottom.Type = EAxisType.DateTime;
            }
            this.chart.Pads[newPad].MarginBottom = 0;
            this.chart.Pads[newPad].AxisBottom.Type = EAxisType.DateTime;
            this.chart.Pads[newPad].AxisBottom.LabelEnabled = true;
            this.chart.Pads[newPad].AxisLeft.LabelFormat = labelFormat;
            this.chart.Pads[newPad].AxisRight.LabelFormat = labelFormat;
        }

        private Pad AddPad()
        {
            double num1 = 0.15;
            double Y1 = 0.0;
            foreach (Pad pad in this.chart.Pads)
            {
                double canvasHeight = pad.CanvasHeight;
                double h = canvasHeight - num1 * canvasHeight / (1.0 - num1);
                pad.CanvasY1 = Y1;
                pad.CanvasY2 = Y1 + h;
                Y1 = pad.CanvasY2;
            }
            Pad pad1 = this.chart.Pads[0];
            Pad pad2 = this.chart.Pads[this.chart.Pads.Count - 1];
            Pad pad3 = this.chart.AddPad(0.0, Y1, 1.0, 1.0);
            pad3.TitleEnabled = pad2.TitleEnabled;
            pad3.BackColor = pad1.BackColor;
            pad3.BorderColor = pad1.BorderColor;
            pad3.BorderEnabled = pad1.BorderEnabled;
            pad3.BorderWidth = pad1.BorderWidth;
            pad3.ForeColor = pad1.ForeColor;
            pad2.AxisBottom.LabelEnabled = false;
            pad2.AxisBottom.Height = 0;
            pad3.AxisBottom.LabelEnabled = true;
            pad3.AxisBottom.LabelFormat = "MMMM yyyy";
            pad3.AxisBottom.Type = EAxisType.DateTime;
            pad2.MarginBottom = 0;
            pad3.MarginBottom = 10;
            pad3.AxisBottom.TitleEnabled = pad2.AxisBottom.TitleEnabled;
            pad3.MarginTop = 0;
            pad3.MarginLeft = pad1.MarginLeft;
            pad3.MarginRight = pad1.MarginRight;
            pad3.AxisLeft.LabelEnabled = pad1.AxisLeft.LabelEnabled;
            pad3.AxisLeft.TitleEnabled = pad1.AxisLeft.TitleEnabled;
            pad3.AxisLeft.Width = 50;
            pad3.Width = pad2.Width;
            pad3.AxisRight.LabelEnabled = pad2.AxisRight.LabelEnabled;
            pad3.AxisBottom.Type = pad2.AxisBottom.Type;
            pad3.YAxisLabelFormat = "F5";
            pad3.LegendEnabled = pad1.LegendEnabled;
            pad3.LegendPosition = pad1.LegendPosition;
            pad3.LegendBackColor = pad1.LegendBackColor;
            pad3.AxisBottom.LabelColor = pad1.AxisBottom.LabelColor;
            pad3.AxisRight.LabelColor = pad1.AxisRight.LabelColor;
            pad3.XGridColor = pad1.XGridColor;
            pad3.YGridColor = pad1.YGridColor;
            pad3.XGridDashStyle = pad1.XGridDashStyle;
            pad3.YGridDashStyle = pad1.YGridDashStyle;
            pad3.SetRangeX(pad1.XRangeMin, pad1.XRangeMax);
            pad3.AxisBottom.SetRange(pad1.AxisBottom.Min, pad1.AxisBottom.Max);
            pad3.AxisTop.SetRange(pad1.AxisTop.Min, pad1.AxisTop.Max);
            pad3.AxisBottom.Zoomed = pad1.AxisBottom.Zoomed;
            pad3.AxisTop.Zoomed = pad1.AxisTop.Zoomed;
            pad3.AxisBottom.Enabled = true;
            pad3.AxisBottom.Height = 20;
            pad3.AxisBottom.Type = EAxisType.DateTime;
            pad3.AxisBottom.LabelFormat = "d";
            pad3.AxisBottom.LabelEnabled = true;
            return pad3;
        }

        private void MoveWindow(DateTime dateTime)
        {
            if (this.firstDateTime == dateTime)
            {
                this.chart.SetRangeX(dateTime.Ticks - this.barSize * TimeSpan.TicksPerSecond * 30, dateTime.Ticks);
                this.firstDateTime = dateTime;
            }
            else
                this.chart.SetRangeX(this.firstDateTime.Ticks, dateTime.Ticks);
            this.chart.UpdatePads();
        }

        public void UpdateGUI()
        { 
            if (UpdatedSuspened && this.framework.Mode == FrameworkMode.Simulation)
                return;
            var events = Queue.DequeueAll(this);
            if (events == null)
                return;
         
            var evts = new List<GroupEvent>();
            for (int i = 0; i < events.Length; ++i)
            {
                var e = events[i];
                if (e.TypeId == EventType.GroupEvent)
                {
                    var gevent = e as GroupEvent;
                    object key = "";
                    GroupField groupField = null;
                    var selected = GetComboBoxSelected();
                    if (gevent.Group.Fields.TryGetValue("SelectorKey", out groupField))
                        key = groupField.Value;
                    if (selected == null && string.IsNullOrEmpty(key.ToString()) || (selected != null && selected.Equals(key)))
                        evts.Add(gevent);
                    List<GroupEvent> list;
                    if (this.eventsBySelectorKey.TryGetValue(key, out list))
                        list.Add(gevent);
                }
                else if (e.TypeId == EventType.OnFrameworkCleared)
                    evts.Clear();
            }
            for (int i = 0; i < evts.Count; ++i)
                ProcessEvent(evts[i], i == evts.Count - 1);
        }

        private void ProcessEvent(GroupEvent groupEvent, bool lastEvent)
        {
            OnNewGroupEvent(groupEvent);
            if (this.firstDateTime == DateTime.MinValue)
                this.firstDateTime = groupEvent.Obj.DateTime;
            if (lastEvent)
                MoveWindow(groupEvent.Obj.DateTime);
        }

        private void Reset(bool clearTable)
        {
            if (clearTable)
            {
                this.orderedGroupTable.Clear();
                this.table.Clear();
            }
            else
            {
                foreach (var item in this.table.Values)
                    item.Table.Clear();
            }
            this.drawnGroupTable.Clear();
            this.firstDateTime = DateTime.MinValue;
            this.chart.Clear();
            this.chart.Divide(1, 1);
            Pad pad = this.chart.Pads[0];
            pad.RegisterViewer(new BarSeriesViewer());
            pad.RegisterViewer(new TimeSeriesViewer());
            pad.RegisterViewer(new FillSeriesViewer());
            pad.RegisterViewer(new TickSeriesViewer());
            this.chart.Pads[0].AxisBottom.LabelFormat = "a";
            this.chart.GroupRightMarginEnabled = true;
            this.chart.GroupLeftMarginEnabled = true;
            this.chart.GroupZoomEnabled = true;
            this.chart.Pads[0].MarginBottom = 0;
            this.chart.Pads[this.chart.Pads.Count - 1].AxisBottom.Type = EAxisType.DateTime;
            for (int i = 0; i < this.chart.Pads.Count; ++i)
            {
                this.chart.Pads[i].MarginRight = 10;
                this.chart.Pads[i].XAxisLabelEnabled = i == this.chart.Pads.Count - 1;
                this.chart.Pads[i].XAxisTitleEnabled = false;
                this.chart.Pads[i].TitleEnabled = false;
                this.chart.Pads[i].BorderEnabled = false;
                this.chart.Pads[i].BackColor = Color.FromKnownColor(KnownColor.Control);
                this.chart.Pads[i].AxisLeft.Width = 50;
                this.chart.Pads[i].AxisBottom.GridDashStyle = DashStyle.Dot;
                this.chart.Pads[i].AxisLeft.GridDashStyle = DashStyle.Dot;
                this.chart.Pads[i].LegendEnabled = true;
                this.chart.Pads[i].LegendPosition = ELegendPosition.TopLeft;
                this.chart.Pads[i].LegendBackColor = Color.White;
                this.chart.Pads[i].AxisBottom.Type = EAxisType.DateTime;
            }
        }

        private void OnSelectorValueChanged(object sender, EventArgs e)
        {
            var selected = GetComboBoxSelected();
            if (this.freezeUpdate)
                return;
            Reset(false);
            var list = this.eventsBySelectorKey[selected];
            for (int i = 0; i < list.Count; ++i)
                ProcessEvent(list[i], i == list.Count - 1);
            this.chart.UpdatePads();
        }

        private string GetComboBoxSelected()
        {
            #if GTK
            return this.cbxSelector.ActiveText;
            #else
            return this.cbxSelector.SelectedItem.ToString();
            #endif
        }

        #if GTK
        private void InitComponent()
        {
            this.chart = new Chart();
            this.cbxSelector = ComboBox.NewText();
            this.cbxSelector.Changed += OnSelectorValueChanged;
            InitChartCommon();
            VBox vb = new VBox();
            vb.PackStart(this.cbxSelector, false, true, 0);
            vb.PackEnd(this.chart, true, true, 0);
            Add(vb);
            ShowAll();
        }
        #else
        private void InitComponent()
        {
            this.chart = new Chart();
            this.cbxSelector = new ComboBox();
            SuspendLayout();
            this.cbxSelector.Dock = DockStyle.Top;
            this.cbxSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxSelector.FormattingEnabled = true;
            this.cbxSelector.TabIndex = 1;
            this.cbxSelector.SelectedIndexChanged += OnSelectorValueChanged;
            this.chart.Dock = DockStyle.Fill;
            this.chart.TabIndex = 0;
            InitChartCommon();
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(this.chart);
            Controls.Add(this.cbxSelector);
            ResumeLayout(false);
        }
        #endif

        private void InitChartCommon()
        {
            this.chart.AntiAliasingEnabled = false;
            this.chart.DoubleBufferingEnabled = true;
            this.chart.FileName = null;
            this.chart.GroupLeftMarginEnabled = false;
            this.chart.GroupRightMarginEnabled = false;
            this.chart.GroupZoomEnabled = false;
            this.chart.PadsForeColor = Color.White;
            this.chart.PrintAlign = EPrintAlign.None;
            this.chart.PrintHeight = 400;
            this.chart.PrintLayout = EPrintLayout.Portrait;
            this.chart.PrintWidth = 600;
            this.chart.PrintX = 10;
            this.chart.PrintY = 10;
            this.chart.SessionStart = TimeSpan.Parse("0.00:00:00");
            this.chart.SessionEnd = TimeSpan.Parse("1.00:00:00");
            this.chart.SessionGridColor = Color.Blue;
            this.chart.SessionGridEnabled = false;
            this.chart.SmoothingEnabled = false;
            this.chart.TransformationType = ETransformationType.Empty;
        }
    }
}
