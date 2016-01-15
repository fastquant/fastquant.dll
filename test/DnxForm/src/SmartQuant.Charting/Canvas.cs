using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

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
       // private Chart chart;

        public static bool FileEnabled { get; set; } = false;

        public static string FileDir { get; set; } = "";

        public static string FileNamePrefix { get; set; } = "";

        public static string FileNameSuffix { get; set; } = "";

        public Pad Pad => Chart.Pad;

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
                return Chart.GroupZoomEnabled;
            }
            set
            {
                Chart.GroupZoomEnabled = value;
            }
        }

        public bool GroupLeftMarginEnabled
        {
            get
            {
                return Chart.GroupLeftMarginEnabled;
            }
            set
            {
                Chart.GroupLeftMarginEnabled = value;
            }
        }

        public bool DoubleBufferingEnabled
        {
            get
            {
                return Chart.DoubleBufferingEnabled;
            }
            set
            {
                Chart.DoubleBufferingEnabled = value;
            }
        }

        public bool SmoothingEnabled
        {
            get
            {
                return Chart.SmoothingEnabled;
            }
            set
            {
                Chart.SmoothingEnabled = value;
            }
        }

        public bool AntiAliasingEnabled
        {
            get
            {
                return Chart.AntiAliasingEnabled;
            }
            set
            {
                Chart.AntiAliasingEnabled = value;
            }
        }

        public PrintDocument PrintDocument
        {
            get
            {
                return Chart.PrintDocument;
            }
            set
            {
                Chart.PrintDocument = value;
            }
        }

        public int PrintX
        {
            get
            {
                return Chart.PrintX;
            }
            set
            {
                Chart.PrintX = value;
            }
        }

        public int PrintY
        {
            get
            {
                return Chart.PrintY;
            }
            set
            {
                Chart.PrintY = value;
            }
        }

        public int PrintWidth
        {
            get
            {
                return Chart.PrintWidth;
            }
            set
            {
                Chart.PrintWidth = value;
            }
        }

        public int PrintHeight
        {
            get
            {
                return Chart.PrintHeight;
            }
            set
            {
                Chart.PrintHeight = value;
            }
        }

        public EPrintAlign PrintAlign
        {
            get
            {
                return Chart.PrintAlign;
            }
            set
            {
                Chart.PrintAlign = value;
            }
        }

        public EPrintLayout PrintLayout
        {
            get
            {
                return Chart.PrintLayout;
            }
            set
            {
                Chart.PrintLayout = value;
            }
        }

        public Chart Chart { get; private set; }

        public Canvas() : this("Canvas", "SmartQuant Canvas")
        {
        }

        public Canvas(string name, string title) : this(name, title, null)
        {
        }

        public Canvas(string name) : this(name, name)
        {
        }

        public Canvas(string name, string title, string fileName) : this(name, name, fileName, 0, 0)
        {
        }

        public Canvas(string name, string title, int width, int height) : this(name, title, null, width, height)
        {
        }

        public Canvas(string name, int width, int height) : this(name, name, width, height)
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
            Chart.FileName = fileName != null ? fileName : FileEnabled ? Path.Combine(FileDir, $"{FileNamePrefix}{Name}{DateTime.Now:MMddyyyhhmmss}{FileNameSuffix}.gif") : Chart.FileName;
            if (!FileEnabled)
                Show();
        }

        public Pad cd(int pad) => Chart.cd(pad);

        public void Clear() => Chart.Clear();

        public void UpdateChart()=> Chart.UpdatePads();

        public new void Update()
        {
            base.Update();
            UpdateChart();
        }

        public Pad AddPad(double x1, double y1, double x2, double y2) => Chart.AddPad(x1, y1, x2, y2);

        public void Divide(int x, int y) => Chart.Divide(x, y);

        public void Divide(int x, int y, double[] widths, double[] heights)=> Chart.Divide(x, y, widths, heights);

        protected override void Dispose(bool disposing)
        {
            CanvasManager.Remove(this);
            base.Dispose(disposing);
        }

        public virtual void Print()=> Chart.Print();

        public virtual void PrintPreview() => Chart.PrintPreview();

        public virtual void PrintSetup() => Chart.PrintSetup();

        public virtual void PrintPageSetup()=> Chart.PrintPageSetup();

        private void InitializeComponent()
        {
            Chart = new Chart();
#if GTK
            Add(Chart);
#else
            SuspendLayout();
            Chart.AntiAliasingEnabled = false;
            Chart.Dock = DockStyle.Fill;
            Chart.DoubleBufferingEnabled = true;
            Chart.FileName = null;
            Chart.GroupLeftMarginEnabled = false;
            Chart.GroupZoomEnabled = false;
            Chart.ImeMode = ImeMode.Off;
            Chart.Location = new Point(0, 0);
            Chart.Name = "fChart";
            Chart.PrintAlign = EPrintAlign.None;
            Chart.PrintHeight = 400;
            Chart.PrintLayout = EPrintLayout.Portrait;
            Chart.PrintWidth = 600;
            Chart.PrintX = 10;
            Chart.PrintY = 10;
            Chart.Size = new Size(488, 293);
            Chart.SmoothingEnabled = false;
            Chart.TabIndex = 0;
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(488, 293);
            Controls.Add(Chart);
            Name = "TCanvas";
            Text = "TCanvas";
            ResumeLayout(false);
#endif
        }
    }

    public class CanvasList : SortedList
    {
        public Canvas this[string name] => base[name] as Canvas;

        public void Add(Canvas canvas) => Add(canvas.Name, canvas);

        public void Remove(Canvas canvas) => Remove(canvas.Name);

        public void Print()
        {
            foreach (Canvas canvas in this)
                canvas.Print();
        }
    }

    public class CanvasManager
    {
        public static CanvasList Canvases { get; } = new CanvasList();

        public static void Add(Canvas canvas)
        {
            if (Canvases[canvas.Name] != null)
                Canvases.Remove(canvas.Name);
            Canvases.Add(canvas.Name, canvas);
        }

        public static void Remove(Canvas canvas) => Canvases.Remove(canvas.Name);

        public static Canvas GetCanvas(string name) => Canvases[name];
    }
}
