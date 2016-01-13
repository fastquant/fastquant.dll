using System;
using System.Drawing;
using System.Linq;

namespace SmartQuant.Charting
{
    [Serializable]
    public class Histogram : IDrawable
    {
        [NonSerialized]
        private Brush fFillBrush;

        protected int fNBins;
        protected double[] fBins;
        protected double fBinSize;
        protected double fXMin;
        protected double fXMax;
        protected double fYMin;
        protected double fYMax;
        protected double[] fIntegral;
        protected bool fIntegralChanged;

        public string Name { get; set; }

        public string Title { get; set; }

        public bool ToolTipEnabled { get; set; }

        public string ToolTipFormat { get; set; }

        public Color LineColor { get; set; }

        public Color FillColor { get; set; }

        public Histogram(string name, string title, int nBins, double xMin, double xMax)
        {
            Name = name;
            Title = title;
            this.fNBins = nBins;
            this.fBins = new double[this.fNBins];
            this.fBinSize = Math.Abs(xMax - xMin) / (double)nBins;
            this.fXMin = Math.Min(xMin, xMax);
            this.fXMax = Math.Max(xMin, xMax);
            this.fYMin = this.fYMax = 0;
            LineColor = Color.Black;
            FillColor = Color.Blue;
            this.fFillBrush = null;
            this.fIntegral = new double[this.fNBins];
            this.fIntegralChanged = false;
        }

        public Histogram(string name, int nBins, double xMin, double xMax)
            : this(name, null, nBins, xMin, xMax)
        {
        }

        public Histogram(int nBins, double xMin, double xMax)
            : this(null, nBins, xMin, xMax)
        {
        }

        public void Add(double x)
        {
            if (x < this.fXMin || x >= this.fXMax)
                return;
            int index = (int)((double)this.fNBins * (x - this.fXMin) / (this.fXMax - this.fXMin));
            ++this.fBins[index];
            if (this.fBins[index] > this.fYMax)
                this.fYMax = this.fBins[index];
            this.fIntegralChanged = true;
        }

        public void Add(double x, double value)
        {
            if (x < this.fXMin || x >= this.fXMax)
                return;
            int index = (int)((double)this.fNBins * (x - this.fXMin) / (this.fXMax - this.fXMin));
            this.fBins[index] = value;
            if (this.fBins[index] > this.fYMax)
                this.fYMax = this.fBins[index];
            this.fIntegralChanged = true;
        }

        public double GetBinSize()
        {
            return this.fBinSize;
        }

        public double GetBinMin(int index)
        {
            return this.fXMin + this.fBinSize * index;
        }

        public double GetBinMax(int index)
        {
            return this.fXMin + this.fBinSize * (index + 1);
        }

        public double GetBinCentre(int index)
        {
            return this.fXMin + this.fBinSize * (index + 0.5);
        }

        public double[] GetIntegral()
        {
            if (this.fIntegralChanged)
            {
                for (int index = 0; index < this.fNBins; ++index)
                    this.fIntegral[index] = index != 0 ? this.fIntegral[index - 1] + this.fBins[index] : this.fBins[index];
                if (this.fIntegral[this.fNBins - 1] == 0.0)
                {
                    Console.WriteLine("Error in THistogram::GetIntegral, Integral = 0");
                    return (double[])null;
                }
                else
                {
                    for (int index = 0; index < this.fNBins; ++index)
                        this.fIntegral[index] /= this.fIntegral[this.fNBins - 1];
                }
            }
            return this.fIntegral;
        }

        public double GetSum()
        {
            return this.fBins.Sum();
        }

        public double GetMean()
        {
            double num1 = 0.0;
            double num2 = 0.0;
            for (int i = 0; i < this.fNBins; ++i)
            {
                num1 += this.fBins[i];
                num2 += this.GetBinCentre(i) * this.fBins[i];
            }
            if (num1 != 0.0)
                return num2 / num1;
            else
                return 0.0;
        }

        public void Print()
        {
            for (int i = 0; i < this.fNBins; ++i)
                Console.WriteLine("{0} - [{1} {2} {3}] : {4:F2}", i, GetBinMin(i), GetBinCentre(i), GetBinMax(i), this.fBins[i]);
        }

        public virtual void Draw()
        {
            Draw("");
        }

        public virtual void Draw(string option)
        {
            if (Chart.Pad == null)
                new Canvas("Canvas", "Canvas");
            Chart.Pad.Add(this);
            Chart.Pad.Title.Add(Name, FillColor);
            Chart.Pad.Legend.Add(Name, FillColor);
            if (option.ToLower().IndexOf("s") >= 0)
                return;
            Chart.Pad.SetRange(this.fXMin, this.fXMax, this.fYMin - (this.fYMax - this.fYMin) / 10.0, this.fYMax + (this.fYMax - this.fYMin) / 10.0);
        }

        public virtual void Paint(Pad pad, double xMin, double xMax, double yMin, double yMax)
        {
            var pen = new Pen(LineColor);
            Brush brush = this.fFillBrush != null ? this.fFillBrush : new SolidBrush(FillColor);
            for (int i = 0; i < this.fNBins; ++i)
            {
                pad.Graphics.FillRectangle(brush, pad.ClientX(this.GetBinMin(i)), pad.ClientY(this.fBins[i]), Math.Abs(pad.ClientX(this.GetBinMax(i)) - pad.ClientX(this.GetBinMin(i))), Math.Abs(pad.ClientY(this.fBins[i]) - pad.ClientY(0.0)));
                pad.DrawLine(pen, this.GetBinMin(i), 0.0, this.GetBinMin(i), this.fBins[i]);
                pad.DrawLine(pen, this.GetBinMin(i), this.fBins[i], this.GetBinMax(i), this.fBins[i]);
                pad.DrawLine(pen, this.GetBinMax(i), this.fBins[i], this.GetBinMax(i), 0.0);
            }
        }

        public TDistance Distance(double x, double y)
        {
            return null;
        }
    }
}
