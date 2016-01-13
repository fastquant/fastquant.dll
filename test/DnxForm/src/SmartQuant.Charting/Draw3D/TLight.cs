using System.Drawing;

namespace SmartQuant.Charting.Draw3D
{
    public class TLight
    {
        public TColor Ambient = new TColor(Color.PaleTurquoise);
        public TSource[] ParallelBeams = new TSource[] { new TSource(new TVec3(3.0, -2.0, 2.0), (TColor)Color.LightYellow) };
        public TLight.TSource[] NearSources = new TLight.TSource[0];

        public TLight()
        {
            SetSfumatoDay();
            SetShadowSources(0.25);
        }

        public void SetSfumatoDay()
        {
            this.Ambient = new TColor(0.55, 0.55, 0.7);
            this.ParallelBeams[0].c = 2.05 * new TColor(1.0, 1.0, 0.55);
        }

        public void SetNormalDay()
        {
            SetSfumatoDay();
            this.Ambient *= 1.1;
            this.ParallelBeams[0].c *= 1.1;
        }

        public void SetVeryBrightDay()
        {
            SetSfumatoDay();
            this.Ambient *= 1.2;
            this.ParallelBeams[0].c *= 1.2;
        }

        public void SetShadowSources(double K)
        {
            TLight.TSource[] tsourceArray = new TLight.TSource[2 * this.ParallelBeams.Length];
            for (int index1 = 0; index1 < this.ParallelBeams.Length; ++index1)
            {
                int index2 = 2 * index1;
                tsourceArray[index2] = this.ParallelBeams[index1];
                tsourceArray[index2 + 1].o = -tsourceArray[index2].o;
                tsourceArray[index2 + 1].c = -K * tsourceArray[index2].c;
            }
            this.ParallelBeams = tsourceArray;
        }

        public virtual TColor Result(TVec3 r, TVec3 n, TColor diffuse)
        {
            TColor tcolor = this.Ambient;
            foreach (TLight.TSource tsource in this.ParallelBeams)
            {
                double num1 = n * tsource.o;
                if (num1 >= 0.0)
                {
                    double num2 = num1 * num1 / (n * n * tsource.o * tsource.o);
                    tcolor += num2 * tsource.c;
                }
            }
            foreach (TLight.TSource tsource in this.NearSources)
            {
                TVec3 tvec3 = tsource.o - r;
                double num1 = n * tvec3;
                double num2 = tvec3 * tvec3;
                if (num1 >= 0.0)
                {
                    double num3 = num1 * num1 / (n * n * num2 * num2);
                    tcolor += num3 * tsource.c;
                }
            }
            return diffuse * tcolor;
        }

        public struct TSource
        {
            public TVec3 o;
            public TColor c;

            public TSource(TVec3 o, TColor c)
            {
                this.o = o;
                this.c = c;
            }
        }
    }
}
