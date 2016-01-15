using System.Drawing;

namespace SmartQuant.Charting.Draw3D
{
    public class TLight
    {
        public TColor Ambient { get; set; } = new TColor(Color.PaleTurquoise);
        public TSource[] ParallelBeams { get; set; } = new TSource[] { new TSource(new TVec3(3.0, -2.0, 2.0), (TColor)Color.LightYellow) };
        public TSource[] NearSources { get; set; } = new TSource[0];

        public TLight()
        {
            SetSfumatoDay();
            SetShadowSources(0.25);
        }

        public void SetSfumatoDay()
        {
            Ambient = new TColor(0.55, 0.55, 0.7);
            ParallelBeams[0].C = 2.05 * new TColor(1.0, 1.0, 0.55);
        }

        public void SetNormalDay()
        {
            SetSfumatoDay();
            Ambient *= 1.1;
            ParallelBeams[0].C *= 1.1;
        }

        public void SetVeryBrightDay()
        {
            SetSfumatoDay();
            Ambient *= 1.2;
            ParallelBeams[0].C *= 1.2;
        }

        public void SetShadowSources(double k)
        {
            var sArray = new TSource[2 * ParallelBeams.Length];
            for (int i1 = 0; i1 < ParallelBeams.Length; ++i1)
            {
                int i = 2 * i1;
                sArray[i] = ParallelBeams[i1];
                sArray[i + 1].O = -sArray[i].O;
                sArray[i + 1].C = -k * sArray[i].C;
            }
            ParallelBeams = sArray;
        }

        public virtual TColor Result(TVec3 r, TVec3 n, TColor diffuse)
        {
            var c = Ambient;
            foreach (var source in ParallelBeams)
            {
                double num1 = n * source.O;
                if (num1 >= 0.0)
                {
                    double num2 = num1 * num1 / (n * n * source.O * source.O);
                    c += num2 * source.C;
                }
            }
            foreach (var source in NearSources)
            {
                var tvec3 = source.O - r;
                double num1 = n * tvec3;
                double num2 = tvec3 * tvec3;
                if (num1 >= 0.0)
                {
                    double num3 = num1 * num1 / (n * n * num2 * num2);
                    c += num3 * source.C;
                }
            }
            return diffuse * c;
        }

        public struct TSource
        {
            public TVec3 O { get; set; }
            public TColor C { get; set; }

            public TSource(TVec3 o, TColor c)
            {
                O = o;
                C = c;
            }
        }
    }
}
