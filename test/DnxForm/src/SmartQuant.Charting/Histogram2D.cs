using SmartQuant.Charting.Draw3D;
using System;
using System.Drawing;

namespace SmartQuant.Charting
{
    [Serializable]
    public class Histogram2D : IDrawable
    {
        private TDraw3DChart Draw3DChart = new TDraw3DChart();
        private TDraw3DChartSmoothedLinear Draw3DChartSmoothedLinear = new TDraw3DChartSmoothedLinear();
        private TDraw3DChartMulticolor Draw3DChartMulticolor = new TDraw3DChartMulticolor();
        private TDraw3DChartMulticolorSmoothedLinear Draw3DChartMulticolorSmoothedLinear = new TDraw3DChartMulticolorSmoothedLinear();
        public const double epsilon = 1E-09;
        protected string fName;
        protected string fTitle;
        protected int fNBinsX;
        protected int fNBinsY;
        protected double fXMin;
        protected double fXMax;
        protected double fYMin;
        protected double fYMax;
        protected double[,] fBins;
        protected double fBinSizeX;
        protected double fBinSizeY;
        protected double fBinMin;
        protected double fBinMax;
        protected double Kx;
        protected double Ky;
        protected double fShowMaxZ;
        protected int fNColors;
        protected Color[] fPalette;
        public ESmoothing Smoothing;
        [NonSerialized]
        private Brush[] fBrushes;
        private bool fToolTipEnabled;
        private string fToolTipFormat;
        public bool Multicolor3D;

        public EChartLook Look
        {
            get
            {
                return this.Draw3DChart.Look;
            }
            set
            {
                this.Draw3DChartMulticolorSmoothedLinear.Look = this.Draw3DChartMulticolor.Look = this.Draw3DChartSmoothedLinear.Look = this.Draw3DChart.Look = value;
            }
        }

        public TSurface Surface3D
        {
            get
            {
                return this.Draw3DChart.Surface;
            }
            set
            {
                this.Draw3DChartSmoothedLinear.Surface = this.Draw3DChart.Surface = value;
            }
        }

        public bool Grid3D
        {
            get
            {
                return this.Draw3DChart.Grid;
            }
            set
            {
                this.Draw3DChartMulticolorSmoothedLinear.Grid = this.Draw3DChartMulticolor.Grid = this.Draw3DChartSmoothedLinear.Grid = this.Draw3DChart.Grid = value;
            }
        }

        public bool LevelLines3D
        {
            get
            {
                return this.Draw3DChart.LevelLines;
            }
            set
            {
                this.Draw3DChartMulticolorSmoothedLinear.LevelLines = this.Draw3DChartMulticolor.LevelLines = this.Draw3DChartSmoothedLinear.LevelLines = this.Draw3DChart.LevelLines = value;
            }
        }

        public string Name
        {
            get
            {
                return this.fName;
            }
            set
            {
                this.fName = value;
            }
        }

        public string Title
        {
            get
            {
                return this.fTitle;
            }
            set
            {
                this.fTitle = value;
            }
        }

        public bool ToolTipEnabled
        {
            get
            {
                return this.fToolTipEnabled;
            }
            set
            {
                this.fToolTipEnabled = value;
            }
        }

        public string ToolTipFormat
        {
            get
            {
                return this.fToolTipFormat;
            }
            set
            {
                this.fToolTipFormat = value;
            }
        }

        public double dX => (this.fXMax - this.fXMin) / this.fNBinsX;

        public double dY => (this.fYMax - this.fYMin) / (double)this.fNBinsY;

        public Histogram2D(string name, string title, int nBinsX, double xMin, double xMax, int nBinsY, double yMin, double yMax)
        {
            this.fName = name;
            this.fTitle = title;
            this.Init(nBinsX, xMin, xMax, nBinsY, yMin, yMax);
        }

        public Histogram2D(string Name, int NBinsX, double XMin, double XMax, int NBinsY, double YMin, double YMax)
        {
            this.fName = Name;
            this.Init(NBinsX, XMin, XMax, NBinsY, YMin, YMax);
        }

        public Histogram2D(int NBinsX, double XMin, double XMax, int NBinsY, double YMin, double YMax)
        {
            this.Init(NBinsX, XMin, XMax, NBinsY, YMin, YMax);
        }

        private void Init(int NBinsX, double XMin, double XMax, int NBinsY, double YMin, double YMax)
        {
            this.fNBinsX = NBinsX;
            this.fNBinsY = NBinsY;
            this.fBins = new double[this.fNBinsX, this.fNBinsY];
            for (int index1 = 0; index1 < this.fNBinsX; ++index1)
            {
                for (int index2 = 0; index2 < this.fNBinsY; ++index2)
                    this.fBins[index1, index2] = 0.0;
            }
            this.fBinSizeX = Math.Abs(XMax - XMin) / (double)NBinsX;
            this.fBinSizeY = Math.Abs(YMax - YMin) / (double)NBinsY;
            if (XMin < XMax)
            {
                this.fXMin = XMin;
                this.fXMax = XMax;
            }
            else
            {
                this.fXMin = XMax;
                this.fXMax = XMin;
            }
            if (YMin < YMax)
            {
                this.fYMin = YMin;
                this.fYMax = YMax;
            }
            else
            {
                this.fYMin = YMax;
                this.fYMax = YMin;
            }
            this.Kx = (double)this.fNBinsX / (this.fXMax - this.fXMin);
            this.Ky = (double)this.fNBinsY / (this.fYMax - this.fYMin);
            this.fBinMin = double.MaxValue;
            this.fBinMax = double.MinValue;
            this.SetPalette(EPalette.Rainbow);
        }

        private int Index(double a, double min, double K)
        {
            return (int)(K * (a - min) + 1E-09);
        }

        private int IndexX(double X)
        {
            return this.Index(X, this.fXMin, this.Kx);
        }

        private int IndexY(double Y)
        {
            return this.Index(Y, this.fYMin, this.Ky);
        }

        public void Add(double X, double Y)
        {
            if (X < this.fXMin || X >= this.fXMax || (Y < this.fYMin || Y >= this.fYMax))
                return;
            int index1 = this.IndexX(X);
            int index2 = this.IndexY(Y);
            ++this.fBins[index1, index2];
            if (this.fBins[index1, index2] > this.fBinMax)
                this.fBinMax = this.fBins[index1, index2];
            if (this.fBins[index1, index2] >= this.fBinMin)
                return;
            this.fBinMin = this.fBins[index1, index2];
        }

        public void Set(double X, double Y, double Value)
        {
            if (X < this.fXMin || X >= this.fXMax || (Y < this.fYMin || Y >= this.fYMax))
                return;
            int index1 = this.IndexX(X);
            int index2 = this.IndexY(Y);
            this.fBins[index1, index2] = Value;
            if (this.fBins[index1, index2] > this.fBinMax)
                this.fBinMax = this.fBins[index1, index2];
            if (this.fBins[index1, index2] >= this.fBinMin)
                return;
            this.fBinMin = this.fBins[index1, index2];
        }

        public double Get(double X, double Y)
        {
            if (X < this.fXMin || X >= this.fXMax || (Y < this.fYMin || Y >= this.fYMax))
                return 0.0;
            else
                return this.fBins[this.IndexX(X), this.IndexY(Y)];
        }

        public double GetBinSizeX() => this.fBinSizeX;

        public double GetBinSizeY() => this.fBinSizeY;

        public double GetBinMinX(int index) => this.fXMin + this.fBinSizeX * (double)index;

        public double GetBinMinY(int index) => this.fYMin + this.fBinSizeY * (double)index;

        public double GetBinMaxX(int index)
        {
            return this.fXMin + this.fBinSizeX * (double)(index + 1);
        }

        public double GetBinMaxY(int index)
        {
            return this.fYMin + this.fBinSizeY * (double)(index + 1);
        }

        public double GetBinCentreX(int index)
        {
            return this.fXMin + this.fBinSizeX * ((double)index + 0.5);
        }

        public double GetBinCentreY(int index)
        {
            return this.fYMin + this.fBinSizeY * ((double)index + 0.5);
        }

        public double GetSum()
        {
            double num = 0.0;
            for (int index1 = 0; index1 < this.fNBinsX; ++index1)
            {
                for (int index2 = 0; index2 < this.fNBinsY; ++index2)
                    num += this.fBins[index1, index2];
            }
            return num;
        }

        public double GetSumSqr()
        {
            double num = 0.0;
            for (int index1 = 0; index1 < this.fNBinsX; ++index1)
            {
                for (int index2 = 0; index2 < this.fNBinsY; ++index2)
                    num += this.fBins[index1, index2] * this.fBins[index1, index2];
            }
            return num;
        }

        public double GetMin() => this.fBinMin;

        public double GetMax() => this.fBinMax;

        public void ShowMaxZ(double maxZ) => this.fShowMaxZ = maxZ;

        public void ShowUnnormalizedZ() => ShowMaxZ(GetMax());

        public bool IsNormalized() => this.fShowMaxZ != GetMax();

        public void ShowNormalizedByMax() => ShowMaxZ(1.0);

        public void ShowNormalizedBySum() => ShowMaxZ(GetMax() / GetSum());

        public void ShowDensityUnnormalized() => ShowMaxZ(GetMax() / (this.dX * this.dY));

        public bool IsDensityNormalized() => this.fShowMaxZ != GetMax() / (this.dX * this.dY);

        public void ShowDensityNormalizedByMax() => ShowMaxZ(1.0);

        public void ShowDensityNormalizedBySum() => ShowMaxZ(GetMax() / (GetSum() * this.dX * this.dY));

        public void Print()
        {
            for (int i = 0; i < this.fNBinsX; ++i)
            {
                for (int j = 0; j < this.fNBinsY; ++j)
                {
                    if (this.fBins[i, j] != 0.0)
                        Console.WriteLine((string)(object)i + (object)":" + (string)(object)j + " - [" + (string)(object)this.GetBinCentreX(i) + " " + (string)(object)this.GetBinCentreY(j) + "] : " + this.fBins[i, j].ToString("F2"));
                }
            }
        }

        public virtual void Draw() => Draw("");

        public virtual void Draw(string option)
        {
            if (Chart.Pad == null)
                new Canvas("Canvas", "Canvas");
            if (Chart.Pad.View3D == null)
                Chart.Pad.View3D = new TView();
            Chart.Pad.Add(this);
            if (option.ToLower().IndexOf("s") >= 0)
                return;
            if (Chart.Pad.For3D)
                new TText(this.fName, this.fXMin, this.fYMax).Draw();
            else
                Chart.Pad.Title.Text = this.fName;
            Chart.Pad.SetRange(this.fXMin, this.fXMax, this.fYMin, this.fYMax);
        }

        public Color[] CreatePalette(Color lowColor, Color highColor, int nColors)
        {
            Color[] colorArray = new Color[nColors];
            double num1 = (double)((int)highColor.R - (int)lowColor.R) / (double)nColors;
            double num2 = (double)((int)highColor.G - (int)lowColor.G) / (double)nColors;
            double num3 = (double)((int)highColor.B - (int)lowColor.B) / (double)nColors;
            double num4 = (double)lowColor.R;
            double num5 = (double)lowColor.G;
            double num6 = (double)lowColor.B;
            colorArray[0] = lowColor;
            for (int index = 1; index < nColors; ++index)
            {
                num4 += num1;
                num5 += num2;
                num6 += num3;
                colorArray[index] = Color.FromArgb((int)num4, (int)num5, (int)num6);
            }
            return colorArray;
        }

        public void SetPalette(Color lowColor, Color highColor, int nColors)
        {
            this.fNColors = nColors;
            this.fPalette = this.CreatePalette(lowColor, highColor, nColors);
        }

        public void SetPalette(Color[] colors, int nColors)
        {
            this.fNColors = nColors;
            this.fPalette = colors;
        }

        public void SetPalette(EPalette palette)
        {
            switch (palette)
            {
                case EPalette.Gray:
                    this.SetPalette(Color.White, Color.Black, (int)byte.MaxValue);
                    break;
                case EPalette.Rainbow:
                    Color[] Colors = new Color[160];
                    int num = 0;
                    foreach (Color color in this.CreatePalette(Color.Purple, Color.Blue, 32))
                        Colors[num++] = color;
                    foreach (Color color in this.CreatePalette(Color.Blue, Color.FromArgb(0, 128, (int) byte.MaxValue), 16))
                        Colors[num++] = color;
                    Color color1 = Color.FromArgb(0, 200, 0);
                    foreach (Color color2 in this.CreatePalette(Color.FromArgb(0, 128, (int) byte.MaxValue), color1, 16))
                        Colors[num++] = color2;
                    foreach (Color color2 in this.CreatePalette(color1, Color.Yellow, 32))
                        Colors[num++] = color2;
                    foreach (Color color2 in this.CreatePalette(Color.Yellow, Color.Orange, 32))
                        Colors[num++] = color2;
                    foreach (Color color2 in this.CreatePalette(Color.Orange, Color.Red, 32))
                        Colors[num++] = color2;
                    this.SetPalette(Colors, 160);
                    break;
            }
        }

        private void PrepareBrushes()
        {
            this.fBrushes = new Brush[this.fNColors];
            for (int index = 0; index < this.fNColors; ++index)
                this.fBrushes[index] = new SolidBrush(this.fPalette[index]);
        }

        public virtual void Paint(Pad Pad, double XMin, double XMax, double YMin, double YMax)
        {
            if (Pad.For3D)
            {
                int millisecond = DateTime.Now.Millisecond;
                int num1 = Pad.ClientX(XMin);
                int num2 = (Pad.ClientY(YMax) + Pad.Y1) / 2;
                int num3 = Math.Abs(Pad.ClientX(XMax) - Pad.ClientX(XMin));
                int num4 = Math.Abs(Pad.ClientY(YMax) - Pad.ClientY(YMin));
                int H = num3 < num4 ? num3 : num4;
                int Left = num1 + num3 / 2 - H / 2;
                int Top = num2;
                if (this.fShowMaxZ == 0.0)
                    this.ShowUnnormalizedZ();
                Pad.AxisZ3D.Min = 0.0;
                Pad.AxisZ3D.Max = this.fShowMaxZ;
                TView.View(Pad).PaintAxes(Pad, Left, Top, H);
                if (!this.Multicolor3D)
                {
                    switch (this.Smoothing)
                    {
                        case ESmoothing.Disabled:
                            this.Draw3DChart.Set(Pad, this.fBins, this.fNBinsX, this.fNBinsY, this.fXMin, this.fXMax, this.fYMin, this.fYMax, this.fBinMax);
                            this.Draw3DChart.Paint(Pad);
                            break;
                        case ESmoothing.Linear:
                            this.Draw3DChartSmoothedLinear.Set(Pad, this.fBins, this.fNBinsX, this.fNBinsY, this.fXMin, this.fXMax, this.fYMin, this.fYMax, this.fBinMax);
                            this.Draw3DChartSmoothedLinear.Paint(Pad);
                            break;
                    }
                }
                else
                {
                    switch (this.Smoothing)
                    {
                        case ESmoothing.Disabled:
                            this.Draw3DChartMulticolor.Set(Pad, this.fBins, this.fNBinsX, this.fNBinsY, this.fXMin, this.fXMax, this.fYMin, this.fYMax, this.fBinMin, this.fBinMax, this.fPalette);
                            this.Draw3DChartMulticolor.Paint(Pad);
                            break;
                        case ESmoothing.Linear:
                            this.Draw3DChartMulticolorSmoothedLinear.Set(Pad, this.fBins, this.fNBinsX, this.fNBinsY, this.fXMin, this.fXMax, this.fYMin, this.fYMax, this.fBinMin, this.fBinMax, this.fPalette);
                            this.Draw3DChartMulticolorSmoothedLinear.Paint(Pad);
                            break;
                    }
                }
                int num5 = DateTime.Now.Millisecond - millisecond;
            }
            else
            {
                int millisecond1 = DateTime.Now.Millisecond;
                int millisecond2 = DateTime.Now.Millisecond;
                int x = Pad.ClientX(this.fXMin);
                int y = Pad.ClientY(this.fYMax);
                int W = Pad.ClientX(this.fXMax) - x;
                int H = Pad.ClientY(this.fYMin) - y;
                int length = this.fPalette.Length;
                int[] numArray = new int[length];
                for (int index = 0; index < length; ++index)
                    numArray[index] = this.fPalette[index].ToArgb();
                TPaintingBitmap tpaintingBitmap = new TPaintingBitmap(W, H);
                tpaintingBitmap.Fill(Pad.ForeColor);
                double num1 = (double)W / (double)this.fNBinsX;
                double num2 = (double)H / (double)this.fNBinsY;
                int w = (int)(num1 + 1.0);
                int h = (int)(num2 + 1.0);
                double num3 = (double)(this.fNColors - 1) / (this.fBinMax - this.fBinMin);
                int index1 = 0;
                double num4 = 0.0;
                while (index1 < this.fNBinsX)
                {
                    int index2 = 0;
                    double num5 = 0.0;
                    while (index2 < this.fNBinsY)
                    {
                        if (this.fBins[index1, index2] != 0.0)
                        {
                            int index3 = (int)(num3 * (this.fBins[index1, index2] - this.fBinMin));
                            tpaintingBitmap.FillRectangle(numArray[index3], (int)num4, H - (int)num5 - h, w, h);
                        }
                        ++index2;
                        num5 += num2;
                    }
                    ++index1;
                    num4 += num1;
                }
                Bitmap bitmap = tpaintingBitmap.Get();
                Pad.Graphics.DrawImage((Image)bitmap, x, y);
                int millisecond3 = DateTime.Now.Millisecond;
                int num6 = millisecond2 - millisecond1;
                int num7 = millisecond3 - millisecond1;
            }
        }

        public TDistance Distance(double X, double Y)
        {
            return (TDistance)null;
        }

        private class TDraw3DChart : TFunction
        {
            protected double[,] h;
            protected double M;
            protected double Kx;
            protected double Ky;
            protected double Kz;

            public virtual void Set(Pad Pad, double[,] h, int nx, int ny, double MinX, double MaxX, double MinY, double MaxY, double MaxZ)
            {
                this.h = h;
                this.nx = nx;
                this.ny = ny;
                this.MinX = MinX;
                this.MaxX = MaxX;
                this.MinY = MinY;
                this.MaxY = MaxY;
                this.M = MaxZ;
                this.Kx = (double)nx / (MaxX - MinX);
                this.Ky = (double)ny / (MaxY - MinY);
                this.Kz = TView.View(Pad).Lz.NormInf / this.M;
            }

            public override double f(double x, double y)
            {
                if (x < this.MinX || x >= this.MaxX || (y < this.MinY || y >= this.MaxY))
                    return 0.0;
                else
                    return this.Kz * this.h[(int)(this.Kx * (x - this.MinX)), (int)(this.Ky * (y - this.MinY))];
            }
        }

        private class TDraw3DChartSmoothedLinear : TDraw3DChart
        {
            public override double f(double x, double y)
            {
                double num1 = this.Kx * (x - this.MinX) - 0.5;
                double num2 = this.Ky * (y - this.MinY) - 0.5;
                int index1 = (int)num1;
                int index2 = (int)num2;
                int index3 = index1 + 1;
                int index4 = index2 + 1;
                if (index3 >= this.h.GetLength(0))
                    index3 = index1;
                if (index4 >= this.h.GetLength(1))
                    index4 = index2;
                double num3 = num1 - (double)index1;
                double num4 = num2 - (double)index2;
                if (num3 < 0.0)
                    num3 = 0.0;
                if (num4 < 0.0)
                    num4 = 0.0;
                double num5 = this.h[index1, index2];
                double num6 = this.h[index3, index2];
                double num7 = this.h[index1, index4];
                double num8 = this.h[index3, index4];
                return this.Kz * (num5 + (num6 - num5) * num3 + (num7 - num5) * num4 + (num5 - num6 - num7 + num8) * num3 * num4);
            }
        }

        private class TDraw3DChartMulticolor : TDraw3DChart
        {
            private TColor[] c0;
            private double MinZ;
            private double K;

            public override TColor color0(double x, double y)
            {
                return this.c0[(int)(this.K * (this.f(x, y) - this.MinZ))];
            }

            public void Set(Pad Pad, double[,] h, int nx, int ny, double MinX, double MaxX, double MinY, double MaxY, double MinZ, double MaxZ, Color[] palette)
            {
                base.Set(Pad, h, nx, ny, MinX, MaxX, MinY, MaxY, MaxZ);
                this.c0 = new TColor[palette.Length];
                for (int index = 0; index < this.c0.Length; ++index)
                    this.c0[index] = (TColor)palette[index];
                this.K = (double)(this.c0.Length - 1) / (MaxZ - MinZ) / this.Kz;
                this.MinZ = MinZ * this.Kz;
            }
        }

        private class TDraw3DChartMulticolorSmoothedLinear : TDraw3DChartMulticolor
        {
            public override double f(double x, double y)
            {
                double num1 = this.Kx * (x - this.MinX) - 0.5;
                double num2 = this.Ky * (y - this.MinY) - 0.5;
                int index1 = (int)num1;
                int index2 = (int)num2;
                int index3 = index1 + 1;
                int index4 = index2 + 1;
                if (index3 >= this.h.GetLength(0))
                    index3 = index1;
                if (index4 >= this.h.GetLength(1))
                    index4 = index2;
                double num3 = num1 - (double)index1;
                double num4 = num2 - (double)index2;
                if (num3 < 0.0)
                    num3 = 0.0;
                if (num4 < 0.0)
                    num4 = 0.0;
                double num5 = this.h[index1, index2];
                double num6 = this.h[index3, index2];
                double num7 = this.h[index1, index4];
                double num8 = this.h[index3, index4];
                return this.Kz * (num5 + (num6 - num5) * num3 + (num7 - num5) * num4 + (num5 - num6 - num7 + num8) * num3 * num4);
            }
        }
    }
}
