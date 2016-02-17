using System;
using System.Collections;
using System.Drawing;

#if GTK
using Gtk;
using Compatibility.Gtk;

#else
using System.Windows.Forms;
#endif

namespace SmartQuant.FinChart
{
    public enum PadScaleStyle
    {
        Arith,
        Log
    }

    public interface IChartDrawable
    {
        bool ToolTipEnabled { get; set; }
        string ToolTipFormat { get; set; }
        void Paint();
        void SetInterval(DateTime minDate, DateTime maxDate);
        Distance Distance(int x, double y);
        void Select();
        void UnSelect();
    }

    public interface IDateDrawable
    {
        DateTime DateTime { get; }
    }

    public interface IZoomable
    {
        PadRange GetPadRangeY(Pad pad);
    }

    public class Pad
    {
        private Chart chart;
        private ArrayList simplePrimitives;
        private SortedRangeList rangeList;
        private SortedRangeList intervalLeftList;
        private SortedRangeList intervalRightList;
        private object selectedObject;
        private int width;
        private int height;
        private int marginLeft;
        private int marginRight;
        private bool onPrimitive;
        private bool outlineEnabled;
        private Rectangle outlineRectangle;
        private string axisLabelFormat;

        public AxisRight Axis { get; private set; }

        internal int AxisGap { get; private set; }

        internal Chart Chart => this.chart;

        public bool DrawItems { get; set; }

        public string AxisLabelFormat
        {
            get
            {
                return this.axisLabelFormat != null ? this.axisLabelFormat : string.Format("F{0}", chart.LabelDigitsCount);
            }
            set
            {
                this.axisLabelFormat = value;
            }
        }

        internal IChartDrawable SelectedPrimitive { get; set; }

        public bool DrawGrid { get; set; }

        public PadScaleStyle ScaleStyle { get; set; }

        public int X1 { get; private set; }

        public int X2 { get; private set; }

        public int Y1 { get; private set; }

        public int Y2 { get; private set; }

        public double MaxValue { get; private set; }

        public double MinValue { get; private set; }

        internal int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
                X2 = X1 + this.width;
                Axis.SetBounds(X2, Y1, Y2);
            }
        }

        public ISeries Series => this.chart.Series;

        public ISeries MainSeries => this.chart.MainSeries;

        public double IntervalWidth => this.chart.IntervalWidth;

        public int FirstIndex => this.chart.FirstIndex;

        public int LastIndex => this.chart.LastIndex;

        public Graphics Graphics { get; private set; }

        public ArrayList Primitives { get; private set; }

        public Pad(Chart chart, int x1, int x2, int y1, int y2)
        {
            this.chart = chart;
            this.marginLeft = this.marginRight = 0;
            this.onPrimitive = false;
            this.outlineEnabled = false;
            this.outlineRectangle = Rectangle.Empty;
            DrawGrid = true;
            SetCanvas(x1, x2, y1, y2);
            Primitives = ArrayList.Synchronized(new ArrayList());
            this.simplePrimitives = new ArrayList();
            this.rangeList = new SortedRangeList();
            this.intervalLeftList = new SortedRangeList();
            this.intervalRightList = new SortedRangeList(true);
        }

        public void SetCanvas(int x1, int x2, int y1, int y2)
        {
            X1 = x1 + this.marginLeft;
            X2 = x2 - this.marginRight;
            Y1 = y1;
            Y2 = y2;
            this.width = X2 - X1;
            this.height = Y2 - Y1;
            if (Axis == null)
                Axis = new AxisRight(this.chart, this, x2, y1, y2);
            else
                Axis.SetBounds(x2, y1, y2);
        }

        public void AddPrimitive(IChartDrawable primitive)
        {
            Primitives.Add(primitive);
            if (primitive is IDateDrawable)
                this.rangeList.Add(primitive as IDateDrawable);
            else
                this.simplePrimitives.Add(primitive);
        }

        public void RemovePrimitive(IChartDrawable primitive)
        {
            Primitives.Remove(primitive);
            if (primitive is IDateDrawable)
                this.rangeList[(primitive as IDateDrawable).DateTime].Remove(primitive);
            else
                this.simplePrimitives.Remove(primitive);
        }

        public void ClearPrimitives()
        {
            Primitives.Clear();
            this.rangeList.Clear();
            this.simplePrimitives.Clear();
        }

        public void SetSelectedObject(object obj)
        {
            this.selectedObject = obj;
        }

        internal void Reset()
        {
            ClearPrimitives();
        }

        public int ClientX(DateTime dateTime)
        {
            double w = (double)Width / (LastIndex - FirstIndex + 1);
            return X1 + (int)((MainSeries.GetIndex(dateTime, IndexOption.Null) - FirstIndex) * w + w / 2);
        }

        public int ClientY(double worldY)
        {
            if (ScaleStyle == PadScaleStyle.Log)
                return Y1 + (int)((1.0 - (Math.Log10(worldY) - Math.Log10(MinValue)) / (Math.Log10(MaxValue) - Math.Log10(MinValue))) * this.height);
            else
                return Y1 + (int)((1.0 - (worldY - MinValue) / (MaxValue - MinValue)) * this.height);
        }

        public void SetInterval(DateTime minDate, DateTime maxDate)
        {
            foreach (IChartDrawable drawable in Primitives)
                drawable.SetInterval(minDate, maxDate);
        }

        public void DrawHorizontalGrid(Pen pen, double y)
        {
            if (DrawGrid)
            {
                var cY = ClientY(y);
                Graphics.DrawLine(pen, X1, cY, X2, cY);
            }
        }

        public void DrawHorizontalTick(Pen pen, double x, double y, int length)
        {
            var cY = ClientY(y);
            Graphics.DrawLine(pen, (int)x, cY, (int)x + length, cY);
        }

        internal void PrepareForUpdate()
        {
            if (this.chart.MainSeries == null || this.chart.MainSeries.Count == 0)
                return;
            MaxValue = double.MinValue;
            MinValue = double.MaxValue;
            ArrayList primitives;
            lock (Primitives.SyncRoot)
                primitives = new ArrayList(Primitives);
            foreach (IChartDrawable drawable in primitives)
            {
                if ((DrawItems || drawable is SeriesView) && drawable is IZoomable)
                {
                    var range = (drawable as IZoomable).GetPadRangeY(this);
                    if (range.IsValid)
                    {
                        MaxValue = Math.Max(MaxValue, range.Max);
                        MinValue = Math.Min(MinValue, range.Min);
                    }
                }
            }
            if (MinValue != double.MaxValue && MaxValue != double.MinValue)
            {
                var offset = (MaxValue - MinValue) / 10;
                MinValue -= offset;
                MaxValue += offset;
                AxisGap = Axis.GetAxisGap();
            }
            else
                AxisGap = -1;
        }

        internal void Update(Graphics g)
        {
            if (this.chart.MainSeries == null || this.chart.MainSeries.Count == 0)
                return;
            Graphics = g;
            if (MinValue != double.MaxValue && MaxValue != double.MinValue)
            {
                Axis.Paint();
                g.SetClip(new Rectangle(X1, Y1, this.width, this.height));
                foreach (IChartDrawable drawable in this.simplePrimitives)
                    if (DrawItems || drawable is SeriesView)
                        drawable.Paint();
                if (DrawItems)
                {
                    int nextIndex = this.rangeList.GetNextIndex(this.chart.MainSeries.GetDateTime(this.chart.FirstIndex));
                    int prevIndex = this.rangeList.GetPrevIndex(this.chart.MainSeries.GetDateTime(this.chart.LastIndex));
                    if (nextIndex != -1 && prevIndex != -1)
                        for (int i = nextIndex; i <= prevIndex; ++i)
                            foreach (IChartDrawable drawable in this.rangeList[i])
                                drawable.Paint();
                }
                g.ResetClip();
            }
            float num = X1 + 2f;
            string legend = null;
            foreach (IChartDrawable drawable in this.simplePrimitives)
            {
                if (drawable is SeriesView)
                {
                    var sView = drawable as SeriesView;
                    if (sView.DisplayNameEnabled)
                    {
                        legend = legend == null ? sView.DisplayName : " " + sView.DisplayName;
                        var size = g.MeasureString(legend, this.chart.Font);
                        g.FillRectangle(new SolidBrush(this.chart.CanvasColor), num + 2f, Y1 + 2f, size.Width, size.Height);
                        g.DrawString(legend, this.chart.Font, new SolidBrush(sView.Color), num + 2f, Y1 + 2f);
                        num += size.Width;
                    }
                }
            }
            if (this.outlineEnabled)
                g.DrawRectangle(new Pen(Color.Green), this.outlineRectangle);
        }

        public DateTime GetDateTime(int x) => this.chart.GetDateTime(x);

        public double WorldY(int y)
        {
            if (ScaleStyle == PadScaleStyle.Log)
                return Math.Pow(10, (double)(Y2 - y) / (double)(Y2 - Y1) * (Math.Log10(MaxValue) - Math.Log10(MinValue))) * MinValue;
            else
                return MinValue + (MaxValue - MinValue) * (double)(Y2 - y) / (double)(Y2 - Y1);
        }

        public virtual void MouseDown(MouseEventArgs evnt)
        {
            if (this.chart.MainSeries == null || this.chart.MainSeries.Count == 0 || evnt.X <= X1 || evnt.X >= X2)
                return;
            double num1 = 10.0;
            double num2 = (MaxValue - MinValue) / 20.0;
            foreach (IChartDrawable primitive in this.simplePrimitives)
            {
                if (primitive is DSView)
                {
                    var d = primitive.Distance(evnt.X, WorldY(evnt.Y));
                    if (d != null)
                    {
                        this.chart.UnSelectAll();
                        if (d.DX < num1 && d.DY < num2)
                        {
                            primitive.Select();
                            this.chart.ContentUpdated = true;
                            this.chart.Invalidate();
                            this.chart.ShowProperties(primitive as DSView, this, false);
                            SelectedPrimitive = primitive;
                            if (this.chart.ContextMenuEnabled && evnt.Button == System.Windows.Forms.MouseButtons.Right)
                                ShowContextMenu(primitive, this.chart, evnt);
                            break;
                        }
                    }
                }
            }
        }

        public virtual void MouseUp(MouseEventArgs Event)
        {
            this.chart.ContentUpdated = true;
            this.chart.Invalidate();
        }

        public virtual void MouseMove(MouseEventArgs evnt)
        {
            if (this.chart.MainSeries == null || this.chart.MainSeries.Count == 0 || evnt.X <= X1 || evnt.X >= X2)
                return;
            double num1 = 10.0;
            double num2 = (MaxValue - MinValue) / 20.0;
            int x = evnt.X;
            double y = this.WorldY(evnt.Y);
            bool flag = false;
            string caption = "";
            this.onPrimitive = false;
            foreach (IChartDrawable drawable in this.simplePrimitives)
            {
                if (DrawItems || drawable is SeriesView)
                {
                    Distance distance = drawable.Distance(x, y);
                    if (distance != null && distance.DX < num1 && distance.DY < num2)
                    {
                        if (drawable.ToolTipEnabled)
                        {
                            if (caption != "")
                                caption = caption + "\n\n";
                            caption = caption + distance.ToolTipText;
                            flag = true;
                        }
                        this.onPrimitive = true;
                        this.chart.SetCursor(ChartCursorType.Hand);
//                        #if GTK
//                        this.chart.GdkWindow.Cursor = UserControl.HandCursor;
//                        #else
//                        Cursor.Current = Cursors.Hand;
//                        #endif
                    }
                }
            }
            if (DrawItems)
            {
                int num3 = 0;
                int index1 = this.chart.MainSeries.GetIndex(GetDateTime(evnt.X), IndexOption.Null);
                DateTime dateTime1 = this.chart.MainSeries.GetDateTime(index1);
                if (index1 != 0)
                {
                    DateTime dateTime2 = this.chart.MainSeries.GetDateTime(index1 - 1);
                    num3 = this.rangeList.GetNextIndex(dateTime2);
                    if (this.rangeList.Contains(dateTime2))
                        ++num3;
                }
                int prevIndex = this.rangeList.GetPrevIndex(dateTime1);
                if (num3 != -1 && prevIndex != -1)
                {
                    for (int index2 = num3; index2 <= prevIndex; ++index2)
                    {
                        foreach (IChartDrawable drawable in this.rangeList[index2])
                        {
                            var distance = drawable.Distance(x, y);
                            if (distance != null && distance.DX < num1 && distance.DY < num2)
                            {
                                if (drawable.ToolTipEnabled)
                                {
                                    if (caption != "")
                                        caption = caption + "\n\n";
                                    caption = caption + distance.ToolTipText;
                                    flag = true;
                                }
                                this.onPrimitive = true;
                                this.chart.SetCursor(ChartCursorType.Hand);
//                                #if GTK
//                                #else
//                                Cursor.Current = Cursors.Hand;
//                                #endif
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                this.chart.ToolTip.SetToolTip(this.chart, caption);
                this.chart.ToolTip.Active = true;
            }
            else
                this.chart.ToolTip.Active = false;
        }

        private void ShowContextMenu(IChartDrawable primitive, UserControl control, MouseEventArgs e)
        {
            #if GTK
            GtkWorkarounds.ShowContextMenu(InitContextMenu(primitive), control, e.GdkEventButton, Gdk.Rectangle.Zero);
            #else
            InitContextMenu(primitive).Show(control, e.Location);
            #endif
        }

        #if GTK
        private Menu InitContextMenu(IChartDrawable primitive)
        {
            string displayName = (primitive as DSView).DisplayName;
            var menu = new Menu();
            ImageMenuItem mi;
            mi = new ImageMenuItem(string.Format("Delete {0}", displayName));
            mi.Activated += OnDeleteMenuItemClick;
            menu.Append(mi);
            menu.Append(new SeparatorMenuItem());
            mi = new ImageMenuItem(string.Format("Properties {0}", displayName));
            mi.Activated += OnPropertiesMenuItemClick;
            menu.Append(mi);
            menu.ShowAll();
            return menu;
        }
        #else
        private ContextMenuStrip InitContextMenu(IChartDrawable primitive)
        {
            string displayName = (primitive as DSView).DisplayName;
            var menu = new ContextMenuStrip();
            ToolStripMenuItem mi;
            mi = new ToolStripMenuItem();
            mi.Text = string.Format("Delete {0}", displayName);
            mi.Click += OnDeleteMenuItemClick;
            mi.Image = this.chart.PrimitiveDeleteImage;
            menu.Items.Add(mi);
            menu.Items.Add(new ToolStripSeparator());
            mi = new ToolStripMenuItem();
            mi.Text = string.Format("Properties {0}", displayName); 
            mi.Click += OnPropertiesMenuItemClick;
            mi.Image = this.chart.PrimitivePropertiesImage;
            menu.Items.Add(mi);
            return menu;
        }
        #endif

        private void OnPropertiesMenuItemClick(object sender, EventArgs e)
        {
            this.chart.ShowProperties(SelectedPrimitive as DSView, this, true);
        }

        private void OnDeleteMenuItemClick(object sender, EventArgs e)
        {
            Primitives.Remove(SelectedPrimitive);
            this.simplePrimitives.Remove(SelectedPrimitive);
            this.chart.ContentUpdated = true;
            this.chart.Invalidate();
        }

        public bool IsInRange(double x, double y) => X1 <= x && x <= X2 && Y1 <= y && y <= Y2;

        #region Helper Methods

        private Distance GetPrimitiveDistance(IChartDrawable primitive, Point point)
        {
            var x = point.X;
            var y = WorldY(point.Y);
            return primitive.Distance(x, y);
        }

        #endregion
    }

    [Serializable]
    public class PadList : IList
    {
        private ArrayList list = new ArrayList();

        public bool IsReadOnly => this.list.IsReadOnly;

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool IsFixedSize => this.list.IsFixedSize;

        public bool IsSynchronized => this.list.IsSynchronized;

        public int Count => this.list.Count;

        public object SyncRoot => this.list.SyncRoot;

        public Pad this[int index] => this.list[index] as Pad;

        public void RemoveAt(int index) => this.list.RemoveAt(index);

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value) => Remove(value as Pad);

        bool IList.Contains(object value) => this.list.Contains(value);

        public void Clear() => this.list.Clear();

        int IList.IndexOf(object value) => IndexOf(value as Pad);

        int IList.Add(object value) => Add(value as Pad);

        public void CopyTo(Array array, int index) => this.list.CopyTo(array, index);

        public IEnumerator GetEnumerator() => this.list.GetEnumerator();

        public int Add(Pad pad) => this.list.Add(pad);

        public void Remove(Pad pad) => this.list.Remove(pad);

        public void Insert(int index, Pad pad) => this.list.Insert(index, pad);

        public int IndexOf(Pad pad) => this.list.IndexOf(pad);
    }

    [Serializable]
    public class PadRange
    {
        public double Min { get; set; }

        public double Max { get; set; }

        protected bool isValid;

        public bool IsValid => this.isValid;

        public PadRange(double min, double max)
        {
            this.Min = min;
            this.Max = max;
            this.isValid = max - min > double.Epsilon;
        }
    }
}
