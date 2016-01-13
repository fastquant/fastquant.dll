using System;
using System.Collections.Generic;
using System.ComponentModel;
using SmartQuant.FinChart;
using System.Drawing;
using System.Drawing.Drawing2D;

#if GTK
using Gtk;
using Compatibility.Gtk;
#else
using System.Windows.Forms;
#endif

namespace SmartQuant.Controls.BarChart
{
    public class BarChart2 : FrameworkControl, IGroupListener
    {
        private bool freezeUpdate;
        private Dictionary<int, GroupItem2> table;
        private Dictionary<string, List<int>> groupIdsBySelectorKey;
        private Dictionary<object, List<GroupEvent>> eventsBySelectorKey;
        private Chart chart;
        private ComboBox cbxSelector;

        public PermanentQueue<Event> Queue { get; private set; }

        public BarChart2()
        {
            InitComponent();
            this.table = new Dictionary<int, GroupItem2>();
            this.groupIdsBySelectorKey = new Dictionary<string, List<int>>();
            this.eventsBySelectorKey = new Dictionary<object, List<GroupEvent>>();
        }

        protected override void OnInit()
        {
            Queue = new PermanentQueue<Event>();
            Queue.AddReader(this);
            Reset(true);
            this.framework.EventManager.Dispatcher.FrameworkCleared += new FrameworkEventHandler(OnFrameworkCleared);
            this.framework.GroupDispatcher.AddListener(this);
            this.eventsBySelectorKey[""] = new List<GroupEvent>();
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            Queue.RemoveReader(this);
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
            });
            this.eventsBySelectorKey.Clear();
            this.eventsBySelectorKey[""] = new List<GroupEvent>();
        }

        public bool OnNewGroup(Group group)
        {
            if (!group.Fields.ContainsKey("Pad"))
                return false;
            var gi = new GroupItem2(group);
            this.table[group.Id] = gi;
            if (this.groupIdsBySelectorKey.ContainsKey(gi.SelectorKey))
            {
                if (!this.groupIdsBySelectorKey[gi.SelectorKey].Contains(group.Id))
                    this.groupIdsBySelectorKey[gi.SelectorKey].Add(group.Id);
            }
            else
                this.groupIdsBySelectorKey[gi.SelectorKey] = new List<int>() { group.Id };
            InvokeAction(delegate 
            {
                #if GTK
                if (cbxSelector.ContainsText(gi.SelectorKey))
                    return;
                this.cbxSelector.AppendText(gi.SelectorKey);
                this.eventsBySelectorKey[gi.SelectorKey] = new List<GroupEvent>();
                this.freezeUpdate = true;
                if (this.cbxSelector.Model.IterNChildren() == 1)
                    this.cbxSelector.Active = 0;
                #else
                if (this.cbxSelector.Items.Contains(gi.SelectorKey))
                    return;
                this.cbxSelector.Items.Add(gi.SelectorKey);
                this.eventsBySelectorKey[gi.SelectorKey] = new List<GroupEvent>();
                this.freezeUpdate = true;
                if (this.cbxSelector.Items.Count == 1)
                    this.cbxSelector.SelectedIndex = 0;
                #endif
                this.freezeUpdate = false;
            });
            return true;
        }

        public void OnNewGroupUpdate(GroupUpdate groupUpdate)
        {
            // no-op
        }

        public void OnNewGroupEvent(GroupEvent groupEvent)
        {
            var groupItem2 = this.table[groupEvent.Group.Id];
            object obj;
            groupItem2.Table.TryGetValue((int)groupEvent.Obj.TypeId, out obj);
            switch (groupEvent.Obj.TypeId)
            {
                case DataObjectType.Bar:
                    if (obj == null)
                    {
                        obj = new BarSeries(groupItem2.Name, "", -1);
                        groupItem2.Table.Add((int)groupEvent.Obj.TypeId, obj);
                    }
                    (obj as BarSeries).Add(groupEvent.Obj as Bar);
                    break;
                case DataObjectType.Fill:
                    if (obj == null)
                    {
                        obj = new FillSeries(groupItem2.Name);
                        groupItem2.Table.Add((int)groupEvent.Obj.TypeId, obj);
                    }
                    (obj as FillSeries).Add(groupEvent.Obj as Fill);
                    break;
                case DataObjectType.TimeSeriesItem:
                    if (obj == null)
                    {
                        obj = new TimeSeries(groupItem2.Name, "");
                        groupItem2.Table.Add((int)groupEvent.Obj.TypeId, obj);
                    }
                    (obj as TimeSeries).Add((groupEvent.Obj as TimeSeriesItem).DateTime, (groupEvent.Obj as TimeSeriesItem).Value);
                    break;
            }
        }

        public void UpdateGUI()
        {
            if (FrameworkControl.UpdatedSuspened && this.framework.Mode == FrameworkMode.Simulation)
                return;
            var evnts = Queue.DequeueAll(this);
            if (evnts == null)
                return;
            var list1 = new List<GroupEvent>();
            for (int i = 0; i < evnts.Length; ++i)
            {
                Event e = evnts[i];
                if (e.TypeId == EventType.GroupEvent)
                {
                    GroupEvent groupEvent = e as GroupEvent;
                    GroupItem2 groupItem2 = this.table[groupEvent.Group.Id];
                    string selected = GetComboBoxSelected();
                    if (selected == null && string.IsNullOrWhiteSpace(groupItem2.SelectorKey) || selected != null && selected == groupItem2.SelectorKey)
                        list1.Add(groupEvent);
                    List<GroupEvent> list2;
                    if (this.eventsBySelectorKey.TryGetValue(groupItem2.SelectorKey, out list2))
                        list2.Add(groupEvent);
                }
                else if (e.TypeId == EventType.OnFrameworkCleared)
                {
                    list1.Clear();
                    Reset(false);
                }
            }
            for (int i = 0; i < list1.Count; ++i)
                ProcessEvent(list1[i], i == list1.Count - 1);
            SetSeries();
        }

        public void Crosshair(bool isChecked)
        {
            this.chart.ActionType = isChecked ? ChartActionType.None : ChartActionType.Cross;
        }

        public void ZoomIn()
        {
            this.chart.ZoomIn();
        }

        public void ZoomOut()
        {
            this.chart.ZoomOut();
        }

        private void ProcessEvent(GroupEvent groupEvent, bool lastEvent)
        {
            OnNewGroupEvent(groupEvent);
        }

        private void SetSeries()
        {
            List<int> list;
            var selected = GetComboBoxSelected();
            if (selected == null || !this.groupIdsBySelectorKey.TryGetValue(selected, out list))
                return;
            this.chart.Reset();
            for (int i = 0; i < list.Count; ++i)
            {
                var item = this.table[list[i]];
                foreach (int key in item.Table.Keys)
                {
                    EnsurePadExists(item.PadNumber, item.Format);
                    int padNumber = this.chart.VolumePadVisible || item.PadNumber <= 1 ? item.PadNumber : item.PadNumber + 1;
                    if (key == DataObjectType.Bar)
                    {
                        if (item.IsColor)
                            this.chart.SetMainSeries(item.Table[key] as BarSeries, true, item.Color);
                        else
                            this.chart.SetMainSeries(item.Table[key] as BarSeries);
                    }
                    if (key == DataObjectType.TimeSeriesItem)
                    {
                        var color = item.IsColor ? item.Color : Color.White;
                        if (item.IsStyle)
                            this.chart.DrawSeries(item.Table[key] as TimeSeries, padNumber, color, item.Style);
                        else
                            this.chart.DrawSeries(item.Table[key] as TimeSeries, padNumber, color);
                    }
                    if (item.Table[key] is FillSeries)
                    {
                        foreach (Fill fill in item.Table[key] as FillSeries)
                            this.chart.DrawFill(fill, padNumber);
                    }
                }
            }
        }

        private void EnsurePadExists(int newPad, string labelFormat)
        {
            while (this.chart.PadCount < newPad + 1)
                this.chart.AddPad();
            this.chart.Pads[newPad].AxisLabelFormat = labelFormat;
        }

        private void Reset(bool clearTable)
        {
            if (clearTable)
            {
                this.table.Clear();
                this.groupIdsBySelectorKey.Clear();
                this.eventsBySelectorKey.Clear();
            }
            else
            {
                foreach (var item in this.table.Values)
                    item.Table.Clear();
            }
            this.chart.Reset();
        }

        private void OnSelectorValueChanged(object sender, EventArgs e)
        {
            if (this.freezeUpdate)
                return;
            Reset(false);
            var selected = GetComboBoxSelected();
            var list = this.eventsBySelectorKey[selected];
            if (list.Count <= 0)
                return;
            for (int i = 0; i < list.Count; ++i)
                ProcessEvent(list[i], i == list.Count - 1);
            SetSeries();
        }

        private void InitComponent()
        {
            this.chart = new Chart();
            #if GTK
            this.cbxSelector = ComboBox.NewText();
            this.cbxSelector.Changed += OnSelectorValueChanged;
            InitChartCommon();
            VBox vb = new VBox();
            vb.PackStart(this.cbxSelector, false, true, 0);
            vb.PackEnd(this.chart, true, true, 0);
            Add(vb);
            ShowAll();
            #else
            this.cbxSelector = new ComboBox();
            this.SuspendLayout();
            this.cbxSelector.Dock = DockStyle.Top;
            this.cbxSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxSelector.FormattingEnabled = true;
            this.cbxSelector.TabIndex = 1;
            this.cbxSelector.SelectedIndexChanged += new EventHandler(OnSelectorValueChanged);
            this.chart.Dock = DockStyle.Fill;
            this.chart.AutoScroll = true;
            this.chart.TabIndex = 0;
            InitChartCommon();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.chart);
            this.Controls.Add(this.cbxSelector);
            this.ResumeLayout(false);
            #endif
        }

        private void InitChartCommon()
        {
            this.chart.ActionType = ChartActionType.None;
            this.chart.BarSeriesStyle = BSStyle.Candle;
            this.chart.BorderColor = Color.Gray;
            this.chart.BottomAxisGridColor = Color.LightGray;
            this.chart.BottomAxisLabelColor = Color.LightGray;
            this.chart.CanvasColor = Color.MidnightBlue;
            this.chart.ChartBackColor = Color.MidnightBlue;
            this.chart.ContextMenuEnabled = true;
            this.chart.CrossColor = Color.DarkGray;
            this.chart.DateTipRectangleColor = Color.LightGray;
            this.chart.DateTipTextColor = Color.Black;
            this.chart.DrawItems = false;
            this.chart.Font = new Font("Microsoft Sans Serif", 7f, FontStyle.Regular, GraphicsUnit.Point, (byte)204);
            this.chart.ItemTextColor = Color.LightGray;
            this.chart.LabelDigitsCount = 2;
            this.chart.MinNumberOfBars = 125;
            this.chart.PrimitiveDeleteImage = null;
            this.chart.PrimitivePropertiesImage = null;
            this.chart.RightAxesFontSize = 7;
            this.chart.RightAxisGridColor = Color.DimGray;
            this.chart.RightAxisMajorTicksColor = Color.LightGray;
            this.chart.RightAxisMinorTicksColor = Color.LightGray;
            this.chart.RightAxisTextColor = Color.LightGray;
            this.chart.ScaleStyle = PadScaleStyle.Arith;
            this.chart.SelectedFillHighlightColor = Color.FromArgb(100, 173, 216, 230);
            this.chart.SelectedItemTextColor = Color.Yellow;
            this.chart.SessionEnd = TimeSpan.Parse("00:00:00");
            this.chart.SessionGridColor = Color.Empty;
            this.chart.SessionGridEnabled = false;
            this.chart.SessionStart = TimeSpan.Parse("00:00:00");
            this.chart.SmoothingMode = SmoothingMode.Default;
            this.chart.SplitterColor = Color.LightGray;
            this.chart.UpdateStyle = ChartUpdateStyle.Trailing;
            this.chart.ValTipRectangleColor = Color.LightGray;
            this.chart.ValTipTextColor = Color.Black;
            this.chart.VolumePadVisible = false;
        }

        private string GetComboBoxSelected()
        {
            #if GTK
            return this.cbxSelector.ActiveText;
            #else
            return this.cbxSelector.SelectedItem.ToString();
            #endif        
        }
    }
}
