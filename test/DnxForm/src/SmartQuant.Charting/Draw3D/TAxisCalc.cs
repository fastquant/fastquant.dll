using System;
using System.Threading.Tasks;
using static System.Math;

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

        public int nTicks => n;

        public TAxisCalc(TVec3 origin, TVec3 end, double valO, double valEnd, int nTicks)
        {
            this.origin = origin;
            this.end = end;
            this.valO = valO;
            this.valEnd = valEnd;
            n = nTicks;
            SetTicks();

            // Set tick positions
            Parallel.ForEach(ticks, t => t.Position =  this.origin + (this.end - this.origin) * (t.Value - this.valO) / (this.valEnd - this.valO));
        }

        public double TickVal(int i) => ticks[i].Value;

        public TVec3 TickPos(int i) => new TVec3(ticks[i].Position);

        public bool TickPassed(ref TTick tick, double val)
        {
            foreach (var t in ticks)
            {
                if (val == t.Value || (val - t.Value) * (lastVal - t.Value) < 0.0)
                {
                    tick = t;
                    lastVal = val;
                    return true;
                }
            }
            lastVal = val;
            return false;
        }

        public bool TickPassed(double val)
        {
            var tick = new TTick();
            return TickPassed(ref tick, val);
        }

        public static double Round(double x)
        {
            return Pow(10, Math.Round(Log10(x)));
        }

        public static double Ceiling(double x, double d)
        {
            return x < 0 ? d * Floor(x / d) : d * Math.Ceiling(x / d);
        }

        private void SetTicks()
        {
            double d = Round(Abs(valEnd - valO) / n);
            double num1 = Ceiling(valO, d);
            n = (int) (Abs(valEnd - num1) / d) + 1;
            if (n < 3)
                n = 3;
            ticks = new TTick[n];
            if (n == 3)
            {
                ticks[0].Value = valO;
                ticks[1].Value = 0.5 * (valO + valEnd);
                ticks[2].Value = valEnd;
            }
            else
            {
                double num2 = num1;
                if (valEnd < valO)
                    d = -d;
                int index = 0;
                while (index < n)
                {
                    ticks[index].Value = num2;
                    ++index;
                    num2 += d;
                }
            }
        }

        public struct TTick
        {
            public double Value { get; set; }
            public TVec3 Position { get; set; }
        }
    }
}
