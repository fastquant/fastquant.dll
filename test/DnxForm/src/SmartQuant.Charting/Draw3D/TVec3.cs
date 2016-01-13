// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Drawing;

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

        public double Sqr
        {
            get
            {
                return this * this;
            }
        }

        public double Norm1
        {
            get
            {
                return Math.Abs(this.x) + Math.Abs(this.y) + Math.Abs(this.z);
            }
        }

        public double Norm2
        {
            get
            {
                return Math.Sqrt(Sqr);
            }
        }

        public double NormInf
        {
            get
            {
                return Math.Max(Math.Abs(this.x), Math.Max(Math.Abs(this.y), Math.Abs(this.z)));
            }
        }

        public static TVec3 O
        {
            get
            {
                return new TVec3(0, 0, 0);
            }
        }

        public static TVec3 ex
        {
            get
            {
                return new TVec3(1, 0, 0);
            }
        }

        public static TVec3 ey
        {
            get
            {
                return new TVec3(0, 1, 0);
            }
        }

        public static TVec3 ez
        {
            get
            {
                return new TVec3(0, 0, 1);
            }
        }

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

        public static implicit operator Point(TVec3 v)
        {
            return new Point((int)v.x, (int)v.y);
        }

        public static implicit operator PointF(TVec3 v)
        {
            return new PointF((float)v.x, (float)v.y);
        }

        public static TVec3 operator +(TVec3 a)
        {
            return a;
        }

        public static TVec3 operator -(TVec3 a)
        {
            return new TVec3(-a.x, -a.y, -a.z);
        }

        public static TVec3 operator +(TVec3 a, TVec3 b)
        {
            return new TVec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static TVec3 operator -(TVec3 a, TVec3 b)
        {
            return new TVec3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static TVec3 operator ^(TVec3 a, TVec3 b)
        {
            return new TVec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public static TVec3 operator *(double k, TVec3 a)
        {
            return new TVec3(k * a.x, k * a.y, k * a.z);
        }

        public static TVec3 operator *(TVec3 a, double k)
        {
            return k * a;
        }

        public static TVec3 operator /(TVec3 a, double d)
        {
            return new TVec3(a.x / d, a.y / d, a.z / d);
        }

        public static double operator *(TVec3 a, TVec3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Point[] PointArray(TVec3[] v)
        {
            var points = new Point[v.Length];
            Parallel.For(0, v.Length, i => points[i] = v[i]);
            return points;
        }
    }
}
