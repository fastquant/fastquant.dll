using System.Drawing;
using System.Drawing.Imaging;

namespace SmartQuant.Charting.Draw3D
{
    public class TSurface
    {
        public TColor Diffuse { get; set; } = new TColor(Color.White);
        public TColor GridDiffuse { get; set; } = new TColor(Color.Orange);
        public TColor Specular { get; set; } = new TColor(Color.White);
    }

    public abstract class TFunction
    {
        protected double MaxX = 1.0;
        protected double MaxY = 1.0;
        public TSurface Surface = new TSurface();
        private bool BitmapWriteOnly = true;
        protected double MinX;
        protected double MinY;
        protected int nx;
        protected int ny;
        public EChartLook Look;
        public bool Grid;
        public bool LevelLines;
        private TLight Light;
        private int Top;
        private int Left;
        private int W;
        private int H;
        private TVec3 o;
        private TVec3 Lx;
        private TVec3 Ly;
        private TVec3 Lz;

        public TFunction()
        {
            this.Surface.Diffuse = 0.59 * new TColor(0.5, 0.7, 1.0);
        }

        public abstract double f(double x, double y);

        public virtual TColor color0(double x, double y)
        {
            return this.Surface.Diffuse;
        }

        public unsafe void Paint(Pad Pad)
        {
            TView tview = TView.View(Pad);
            this.Left = tview.Left;
            this.Top = tview.Top;
            this.W = tview.H;
            this.H = tview.H;
            this.o = tview.O - new TVec3((double) this.Left, (double) this.Top, 0.0);
            this.Lx = tview.Lx;
            this.Ly = tview.Ly;
            this.Lz = tview.Lz;
            this.Light = tview.Light;
            if (this.Look == EChartLook.SurfaceOnly)
                this.BitmapWriteOnly = false;
            Bitmap bm = new Bitmap(this.W, this.H, PixelFormat.Format32bppRgb);
            this.BackgroundIntoBitmapIfNeeded(Pad, bm);
            Rectangle rect = new Rectangle(0, 0, this.W, this.H);
            BitmapData bitmapdata = bm.LockBits(rect, this.BitmapWriteOnly ? ImageLockMode.WriteOnly : ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            this.PaintBuffer((int*) bitmapdata.Scan0.ToPointer());
            bm.UnlockBits(bitmapdata);
            Color transparentColor = Color.FromArgb((int) byte.MaxValue, 0, 0, 0);
            bm.MakeTransparent(transparentColor);
            Pad.Graphics.DrawImage((Image) bm, this.Left, this.Top);
        }

        private TColor color(double x, double y, double dx, double dy, TColor Diffuse)
        {
            TVec3 n = new TVec3(dx, 0.0, this.f(x + dx, y) - this.f(x, y)) ^ new TVec3(0.0, dy, this.f(x, y + dy) - this.f(x, y));
            return this.Light.Result(new TVec3(x, y, this.f(x, y)), n, Diffuse);
        }

        private void BackgroundIntoBitmapIfNeeded(Pad pad, Bitmap bm)
        {
            if (this.Look != EChartLook.SurfaceOnly)
                return;
            var g = Graphics.FromImage(bm);
            TView.View(pad).PaintAxes(g, pad, 0, 0, this.H);
        }

        private unsafe void PaintBuffer(int* b)
        {
            int[,] numArray = new int[this.W, this.H];
            if (!this.BitmapWriteOnly)
            {
                int* numPtr = b;
                for (int index1 = 0; index1 < this.H; ++index1)
                {
                    for (int index2 = 0; index2 < this.W; ++index2)
                        numArray[index2, index1] = *numPtr++;
                }
            }
            double normInf1 = this.Lx.NormInf;
            double normInf2 = this.Ly.NormInf;
            TVec3 Origin = this.o - 0.5 * this.Lx - 0.5 * this.Ly;
            TVec3 tvec3_1 = this.Lx / normInf1;
            TVec3 tvec3_2 = this.Ly / normInf2;
            double num1 = (this.MaxX - this.MinX) / normInf1;
            double num2 = (this.MaxY - this.MinY) / normInf2;
            double dx = num1;
            double dy = num2;
            if ((double) (2 * this.nx) >= normInf1 && (double) (2 * this.ny) >= normInf2)
            {
                dx = (this.MaxX - this.MinX) / (double) this.nx;
                dy = (this.MaxY - this.MinY) / (double) this.ny;
            }
            double ValO1 = this.MinX + 0.01 * dx;
            double ValO2 = this.MinY + 0.01 * dy;
            TVec3 tvec3_3 = this.Lx;
            TVec3 tvec3_4 = this.Ly;
            if (this.Lx.z > 0.0)
            {
                ValO1 = this.MaxX - 0.99 * dx;
                num1 = -num1;
                dx = -dx;
                Origin += this.Lx;
                tvec3_3 = -this.Lx;
                tvec3_1 = -tvec3_1;
            }
            if (this.Ly.z > 0.0)
            {
                ValO2 = this.MaxY - 0.99 * dy;
                num2 = -num2;
                dy = -dy;
                Origin += this.Ly;
                tvec3_4 = -this.Ly;
                tvec3_2 = -tvec3_2;
            }
            int num3 = (int) normInf1;
            int num4 = (int) normInf2;
            int num5 = (int) this.Lz.NormInf;
            bool[] flagArray1 = new bool[num3 + 2];
            bool[] flagArray2 = new bool[num4 + 2];
            bool[] flagArray3 = new bool[num5 + 2];
            if (this.Grid)
            {
                TAxisCalc taxisCalc1 = new TAxisCalc(Origin, Origin + tvec3_3, ValO1, ValO1 + normInf1 * num1, 10);
                TAxisCalc taxisCalc2 = new TAxisCalc(Origin, Origin + tvec3_4, ValO2, ValO2 + normInf2 * num2, 10);
                double Val1 = ValO1;
                int index1 = 0;
                while (index1 <= num3)
                {
                    if (taxisCalc1.TickPassed(Val1))
                        flagArray1[index1] = true;
                    ++index1;
                    Val1 += num1;
                }
                double Val2 = ValO2;
                int index2 = 0;
                while (index2 <= num4)
                {
                    if (taxisCalc2.TickPassed(Val2))
                        flagArray2[index2] = true;
                    ++index2;
                    Val2 += num2;
                }
            }
            if (this.LevelLines)
            {
                for (int index = 0; index < num5; ++index)
                    flagArray3[index] = (index & 4) != 0;
            }
            switch (this.Look)
            {
                case EChartLook.FromZeroToSurface:
                    double x1 = ValO1;
                    int index3 = 0;
                    while (index3 <= num3)
                    {
                        bool flag = flagArray1[index3];
                        TVec3 tvec3_5 = Origin;
                        double y = ValO2;
                        int index1 = 0;
                        while (index1 <= num4)
                        {
                            double num6 = this.f(x1, y);
                            if (num6 > 0.0)
                            {
                                int num7 = (int) tvec3_5.y;
                                int num8 = (int) (tvec3_5.y - num6);
                                if (num8 < 0)
                                    num8 = 0;
                                TColor Diffuse = this.color0(x1, y);
                                if (this.Grid && (flag || flagArray2[index1]))
                                    Diffuse = this.Surface.GridDiffuse;
                                Diffuse = this.color(x1, y, dx, dy, Diffuse);
                                Diffuse.Clip();
                                int num9 = Diffuse.Get888();
                                int index2 = (int) tvec3_5.x;
                                if (this.LevelLines)
                                {
                                    TColor tcolor = 0.81 * Diffuse;
                                    tcolor.Clip();
                                    int num10 = tcolor.Get888();
                                    for (int index4 = num7; index4 >= num8; --index4)
                                        numArray[index2, index4] = flagArray3[num7 - index4] ? num10 : num9;
                                }
                                else
                                {
                                    for (int index4 = num7; index4 >= num8; --index4)
                                        numArray[index2, index4] = num9;
                                }
                            }
                            ++index1;
                            y += num2;
                            tvec3_5 += tvec3_2;
                        }
                        ++index3;
                        x1 += num1;
                        Origin += tvec3_1;
                    }
                    break;
                case EChartLook.SurfaceOnly:
                    double x2 = ValO1;
                    int index5 = 0;
                    while (index5 < num3)
                    {
                        bool flag = flagArray1[index5];
                        TVec3 tvec3_5 = Origin;
                        double y = ValO2;
                        int index1 = 0;
                        while (index1 < num4)
                        {
                            double num6 = this.f(x2, y);
                            double num7 = this.f(x2 + num1, y);
                            double num8 = this.f(x2, y + num2);
                            double num9 = this.f(x2 + num1, y + num2);
                            double num10 = num6;
                            double num11 = num6;
                            if (num7 < num10)
                                num10 = num7;
                            if (num8 < num10)
                                num10 = num8;
                            if (num9 < num10)
                                num10 = num9;
                            if (num7 > num11)
                                num11 = num7;
                            if (num8 > num11)
                                num11 = num8;
                            if (num9 > num11)
                                num11 = num9;
                            int num12 = (int) tvec3_5.y;
                            int num13 = (int) (tvec3_5.y - num10 + 1.0);
                            int num14 = (int) (tvec3_5.y - num11);
                            if (num13 < this.H && num13 >= 0 && (num14 < this.H && num14 >= 0))
                            {
                                TColor Diffuse = this.color0(x2, y);
                                if (this.Grid && (flag || flagArray2[index1]))
                                    Diffuse = this.Surface.GridDiffuse;
                                Diffuse = this.color(x2, y, dx, dy, Diffuse);
                                Diffuse.Clip();
                                int num15 = Diffuse.Get888();
                                int index2 = (int) tvec3_5.x;
                                if (this.LevelLines)
                                {
                                    TColor tcolor = 0.81 * Diffuse;
                                    tcolor.Clip();
                                    int num16 = tcolor.Get888();
                                    for (int index4 = num13; index4 >= num14; --index4)
                                    {
                                        if (num10 > 0.0 || numArray[index2, index4] >= 0 || numArray[index2, index4] == -1)
                                            numArray[index2, index4] = (num12 - index4 & 4) != 0 ? num16 : num15;
                                    }
                                }
                                else
                                {
                                    for (int index4 = num13; index4 >= num14; --index4)
                                    {
                                        if (num10 > 0.0 || numArray[index2, index4] >= 0 || numArray[index2, index4] == -1)
                                            numArray[index2, index4] = num15;
                                    }
                                }
                            }
                            ++index1;
                            y += num2;
                            tvec3_5 += tvec3_2;
                        }
                        ++index5;
                        x2 += num1;
                        Origin += tvec3_1;
                    }
                    break;
            }
            for (int index1 = 0; index1 < this.H; ++index1)
            {
                for (int index2 = 0; index2 < this.W; ++index2)
                    *b++ = numArray[index2, index1];
            }
        }
    }
}
