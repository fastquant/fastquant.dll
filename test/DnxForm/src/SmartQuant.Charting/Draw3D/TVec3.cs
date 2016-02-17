using System;
using System.Drawing;
using System.Linq;

namespace SmartQuant.Charting.Draw3D
{
    public class TVec3
    {
        public double x;
        public double y;
        public double z;

        public double this[int i]
        {
            get
            {
                return i == 0 ? this.x : (i != 1 ? this.z : this.y);
            }
            set
            {
                if (i == 0)
                    this.x = value;
                else if (i == 1)
                    this.y = value;
                else
                    this.z = value;
            }
        }

        public double Sqr => this * this;

        public double Norm1 => Math.Abs(this.x) + Math.Abs(this.y) + Math.Abs(this.z);

        public double Norm2 => Math.Sqrt(Sqr);

        public double NormInf => Math.Max(Math.Abs(this.x), Math.Max(Math.Abs(this.y), Math.Abs(this.z)));

        public static TVec3 O => new TVec3(0, 0, 0);

        public static TVec3 ex => new TVec3(1, 0, 0);

        public static TVec3 ey => new TVec3(0, 1, 0);

        public static TVec3 ez => new TVec3(0, 0, 1);

        public TVec3() : this(0, 0, 0)
        {
        }

        public TVec3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public TVec3(TVec3 v) : this(v.x, v.y, v.z)
        {
        }

        public static implicit operator Point(TVec3 v) => new Point((int)v.x, (int)v.y);

        public static implicit operator PointF(TVec3 v) => new PointF((float)v.x, (float)v.y);

        public static TVec3 operator +(TVec3 a) => a;

        public static TVec3 operator -(TVec3 a) => new TVec3(-a.x, -a.y, -a.z);

        public static TVec3 operator +(TVec3 a, TVec3 b) => new TVec3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static TVec3 operator -(TVec3 a, TVec3 b) => new TVec3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static TVec3 operator ^(TVec3 a, TVec3 b) => new TVec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

        public static TVec3 operator *(double k, TVec3 a) => new TVec3(k * a.x, k * a.y, k * a.z);

        public static TVec3 operator *(TVec3 a, double k) => k * a;

        public static TVec3 operator /(TVec3 a, double d) => new TVec3(a.x / d, a.y / d, a.z / d);

        public static double operator *(TVec3 a, TVec3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

        public static Point[] PointArray(TVec3[] v) => v.Select(i => (Point) i).ToArray();
    }
}
