using System;
using System.Threading.Tasks;

namespace SmartQuant.Charting.Draw3D
{
    public enum EChartLook
    {
        FromZeroToSurface,
        SurfaceOnly
    }

    public enum ESmoothing
    {
        Disabled,
        Linear
    }
    public class TAxisCalc
    {
        private TTick[] ticks = new TTick[0];
        private TVec3 origin;
        private TVec3 end;
        private double valO;
        private double valEnd;
        private int n;
        private double lastVal;

        public int nTicks
        {
            get
            {
                return this.n;
            }
        }

        public TAxisCalc(TVec3 origin, TVec3 end, double valO, double valEnd, int nTicks)
        {
            this.origin = origin;
            this.end = end;
            this.valO = valO;
            this.valEnd = valEnd;
            this.n = nTicks;
            SetTicks();

            // Set tick positions
            Parallel.ForEach(this.ticks, t => t.Position =  this.origin + (this.end - this.origin) * (t.Value - this.valO) / (this.valEnd - this.valO));

        }

        public double TickVal(int i)
        {
            return this.ticks[i].Value;
        }

        public TVec3 TickPos(int i)
        {
            return new TVec3(this.ticks[i].Position);
        }

        public bool TickPassed(ref TTick tick, double val)
        {
            foreach (var t in this.ticks)
            {
                if (val == t.Value || (val - t.Value) * (this.lastVal - t.Value) < 0.0)
                {
                    tick = t;
                    this.lastVal = val;
                    return true;
                }
            }
            this.lastVal = val;
            return false;
        }

        public bool TickPassed(double val)
        {
            var tick = new TTick();
            return TickPassed(ref tick, val);
        }

        public static double Round(double x)
        {
            return Math.Pow(10, Math.Round(Math.Log10(x)));
        }

        public static double Ceiling(double x, double d)
        {
            return x < 0 ? d * Math.Floor(x / d) : d * Math.Ceiling(x / d);
        }

        private void SetTicks()
        {
            double d = Round(Math.Abs(this.valEnd - this.valO) / this.n);
            double num1 = Ceiling(this.valO, d);
            this.n = (int) (Math.Abs(this.valEnd - num1) / d) + 1;
            if (this.n < 3)
                this.n = 3;
            this.ticks = new TTick[this.n];
            if (this.n == 3)
            {
                this.ticks[0].Value = this.valO;
                this.ticks[1].Value = 0.5 * (this.valO + this.valEnd);
                this.ticks[2].Value = this.valEnd;
            }
            else
            {
                double num2 = num1;
                if (this.valEnd < this.valO)
                    d = -d;
                int index = 0;
                while (index < this.n)
                {
                    this.ticks[index].Value = num2;
                    ++index;
                    num2 += d;
                }
            }
        }

        public struct TTick
        {
            public double Value;
            public TVec3 Position;
        }
    }
}
