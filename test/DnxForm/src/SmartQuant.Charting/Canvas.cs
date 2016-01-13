using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;

#if GTK
using Compatibility.Gtk;
#else
using System.Windows.Forms;
#endif

namespace SmartQuant.Charting
{
    [Serializable]
    public class Canvas : Form
    {
        private Chart chart;

        public static bool FileEnabled { get; set; }

        public static string FileDir { get; set; }

        public static string FileNamePrefix { get; set; }

        public static string FileNameSuffix { get; set; }

        public Pad Pad
        {
            get
            {
                return Chart.Pad;
            }
        }

        #if GTK
        public new string Title
        #else
        public string Title
        #endif
        {
            get
            {
                return Text;
            }
            set
            {
                Text = value;
            }
        }

        public bool GroupZoomEnabled
        {
            get
            {
                return this.chart.GroupZoomEnabled;
            }
            set
            {
                this.chart.GroupZoomEnabled = value;
            }
        }

        public bool GroupLeftMarginEnabled
        {
            get
            {
                return this.chart.GroupLeftMarginEnabled;
            }
            set
            {
                this.chart.GroupLeftMarginEnabled = value;
            }
        }

        public bool DoubleBufferingEnabled
        {
            get
            {
                return this.chart.DoubleBufferingEnabled;
            }
            set
            {
                this.chart.DoubleBufferingEnabled = value;
            }
        }

        public bool SmoothingEnabled
        {
            get
            {
                return this.chart.SmoothingEnabled;
            }
            set
            {
                this.chart.SmoothingEnabled = value;
            }
        }

        public bool AntiAliasingEnabled
        {
            get
            {
                return this.chart.AntiAliasingEnabled;
            }
            set
            {
                this.chart.AntiAliasingEnabled = value;
            }
        }

        public PrintDocument PrintDocument
        {
            get
            {
                return this.chart.PrintDocument;
            }
            set
            {
                this.chart.PrintDocument = value;
            }
        }

        public int PrintX
        {
            get
            {
                return this.chart.PrintX;
            }
            set
            {
                this.chart.PrintX = value;
            }
        }

        public int PrintY
        {
            get
            {
                return this.chart.PrintY;
            }
            set
            {
                this.chart.PrintY = value;
            }
        }

        public int PrintWidth
        {
            get
            {
                return this.chart.PrintWidth;
            }
            set
            {
                this.chart.PrintWidth = value;
            }
        }

        public int PrintHeight
        {
            get
            {
                return this.chart.PrintHeight;
            }
            set
            {
                this.chart.PrintHeight = value;
            }
        }

        public EPrintAlign PrintAlign
        {
            get
            {
                return this.chart.PrintAlign;
            }
            set
            {
                this.chart.PrintAlign = value;
            }
        }

        public EPrintLayout PrintLayout
        {
            get
            {
                return this.chart.PrintLayout;
            }
            set
            {
                this.chart.PrintLayout = value;
            }
        }

        public Chart Chart
        {
            get
            {
                return this.chart;
            }
        }

        static Canvas()
        {
            FileDir = FileNamePrefix = FileNameSuffix = string.Empty;
            FileEnabled = false;
        }

        public Canvas()
            : this("Canvas", "SmartQuant Canvas")
        {
        }

        public Canvas(string name, string title)
            : this(name, title, null)
        {
        }

        public Canvas(string name)
            : this(name, name)
        {
        }

        public Canvas(string name, string title, string fileName)
            : this(name, name, fileName, 0, 0)
        {
        }

        public Canvas(string Name, string Title, int Width, int Height)
            : this(Name, Title, null, Width, Height)
        {
        }

        public Canvas(string name, int width, int height)
            : this(name, name, width, height)
        {
        }

        public Canvas(string name, string title, string fileName, int width, int height)
        {
            InitializeComponent();
            Name = name;
            Text = title;
            Width = width;
            Height = height;
            CanvasManager.Add(this);
            FileEnabled = fileName != null ? true : FileEnabled;                                                                                                                                                                                   
            this.chart.FileName = fileName != null ? fileName : FileEnabled ? System.IO.Path.Combine(FileDir, string.Format("{0}{1}{2:MMddyyyhhmmss}{3}.gif", FileNamePrefix, Name, DateTime.Now, FileNameSuffix)) : this.chart.FileName;
            if (!FileEnabled)
                Show();
        }

        public Pad cd(int pad)
        {
            return this.chart.cd(pad);
        }

        public void Clear()
        {
            this.chart.Clear();
        }

        public void UpdateChart()
        {
            this.chart.UpdatePads();
        }

        public new void Update()
        {
            base.Update();
            this.UpdateChart();
        }

        public Pad AddPad(double x1, double y1, double x2, double y2)
        {
            return this.chart.AddPad(x1, y1, x2, y2);
        }

        public void Divide(int x, int y)
        {
            this.chart.Divide(x, y);
        }

        public void Divide(int x, int y, double[] widths, double[] heights)
        {
            this.chart.Divide(x, y, widths, heights);
        }

        protected override void Dispose(bool disposing)
        {
            CanvasManager.Remove(this);
            base.Dispose(disposing);
        }

        public virtual void Print()
        {
            this.chart.Print();
        }

        public virtual void PrintPreview()
        {
            this.chart.PrintPreview();
        }

        public virtual void PrintSetup()
        {
            this.chart.PrintSetup();
        }

        public virtual void PrintPageSetup()
        {
            this.chart.PrintPageSetup();
        }

        private void InitializeComponent()
        {
            this.chart = new Chart();
            #if GTK
            Add(chart);
            #else
            this.SuspendLayout();
            this.chart.AntiAliasingEnabled = false;
            this.chart.Dock = DockStyle.Fill;
            this.chart.DoubleBufferingEnabled = true;
            this.chart.FileName = null;
            this.chart.GroupLeftMarginEnabled = false;
            this.chart.GroupZoomEnabled = false;
            this.chart.ImeMode = ImeMode.Off;
            this.chart.Location = new Point(0, 0);
            this.chart.Name = "fChart";
            this.chart.PrintAlign = EPrintAlign.None;
            this.chart.PrintHeight = 400;
            this.chart.PrintLayout = EPrintLayout.Portrait;
            this.chart.PrintWidth = 600;
            this.chart.PrintX = 10;
            this.chart.PrintY = 10;
            this.chart.Size = new Size(488, 293);
            this.chart.SmoothingEnabled = false;
            this.chart.TabIndex = 0;
            this.AutoScaleBaseSize = new Size(5, 13);
            this.ClientSize = new Size(488, 293);
            this.Controls.Add(this.chart);
            this.Name = "TCanvas";
            this.Text = "TCanvas";
            this.ResumeLayout(false);
            #endif
        }
    }

    public class CanvasList : SortedList
    {
        public Canvas this[string name]
        {
            get
            {
                return base[name] as Canvas;
            }
        }

        public void Add(Canvas canvas)
        {
            Add(canvas.Name, canvas);
        }

        public void Remove(Canvas canvas)
        {
            Remove(canvas.Name);
        }

        public void Print()
        {
            foreach (Canvas canvas in this)
                canvas.Print();
        }
    }

    public class CanvasManager
    {
        public static CanvasList Canvases { get; private set; }

        static CanvasManager()
        {
            Canvases = new CanvasList();
        }

        public static void Add(Canvas canvas)
        {
            if (Canvases[canvas.Name] != null)
                Canvases.Remove(canvas.Name);
            Canvases.Add(canvas.Name, canvas);
        }

        public static void Remove(Canvas canvas)
        {
            Canvases.Remove(canvas.Name);
        }

        public static Canvas GetCanvas(string name)
        {
            return Canvases[name];
        }
    }
}
