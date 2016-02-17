using System;

namespace SmartQuant.Charting.Draw3D
{
    public class TMat3x3
    {
        private double[,] m;

        public double this [int i, int j]
        {
            get
            {
                return this.m[i, j];
            }
            set
            {
                this.m[i, j] = value;
            }
        }

        public double xx
        {
            get
            {
                return this.m[0, 0];
            }
            set
            {
                this.m[0, 0] = value;
            }
        }

        public double xy
        {
            get
            {
                return this.m[0, 1];
            }
            set
            {
                this.m[0, 0] = value;
            }
        }

        public double xz
        {
            get
            {
                return this.m[0, 2];
            }
            set
            {
                this.m[0, 0] = value;
            }
        }

        public double yx
        {
            get
            {
                return this.m[1, 0];
            }
            set
            {
                this.m[1, 0] = value;
            }
        }

        public double yy
        {
            get
            {
                return this.m[1, 1];
            }
            set
            {
                this.m[1, 1] = value;
            }
        }

        public double yz
        {
            get
            {
                return this.m[1, 2];
            }
            set
            {
                this.m[1, 2] = value;
            }
        }

        public double zx
        {
            get
            {
                return this.m[2, 0];
            }
            set
            {
                this.m[2, 0] = value;
            }
        }

        public double zy
        {
            get
            {
                return this.m[2, 1];
            }
            set
            {
                this.m[2, 1] = value;
            }
        }

        public double zz
        {
            get
            {
                return this.m[2, 2];
            }
            set
            {
                this.m[2, 2] = value;
            }
        }

        public TMat3x3()
        {
            this.m = new double[3, 3];
        }

        public TMat3x3(double x)
            : this()
        {
            this.m[0, 0] = this.m[1, 1] = this.m[2, 2] = x;
        }

        public TMat3x3(double a, double b, double c, double d, double e, double f, double g, double h, double i)
            : this()
        {
            this.m[0, 0] = a;
            this.m[0, 1] = b;
            this.m[0, 2] = c;
            this.m[1, 0] = d;
            this.m[1, 1] = e;
            this.m[1, 2] = f;
            this.m[2, 0] = g;
            this.m[2, 1] = h;
            this.m[2, 2] = i;
        }

        public override bool Equals(object obj)
        {
            var that = (TMat3x3)obj;
            return that.xx == this.xx && that.xy == this.xy && that.xz == this.xz && that.yx == this.yx && that.yy == this.yy && that.yz == this.xz && that.zx == this.zx && that.zy == this.zy && that.zz == this.xz;
        }

        public override int GetHashCode()
        {
            return this.xx.GetHashCode() ^ this.xy.GetHashCode() ^ this.xz.GetHashCode() ^ this.yx.GetHashCode() ^ this.yy.GetHashCode() ^ this.yz.GetHashCode() ^ this.zx.GetHashCode() ^ this.zy.GetHashCode() ^ this.zz.GetHashCode();
        }

        public static bool operator ==(TMat3x3 b, TMat3x3 a) => a.Equals(b);

        public static bool operator !=(TMat3x3 b, TMat3x3 a) => !(b == a);

        public static TMat3x3 operator -(TMat3x3 m) => new TMat3x3(-m.xx, -m.xy, -m.xz, -m.yx, -m.yy, -m.yz, -m.zx, -m.zy, -m.zz);

        public static TMat3x3 operator *(TMat3x3 b, TMat3x3 a)
        {
            var r = new TMat3x3();
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    for (var k = 0; k < 3; ++k)
                        r[i, j] += b[i, k] * a[k, j];
            return r;
        }

        public static TMat3x3 operator *(double k, TMat3x3 m)
        {
            var r = new TMat3x3();
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    r[i, j] = k * m[i, j];
            return r;
        }

        public static TMat3x3 operator *(TMat3x3 m, double k) => k * m;

        public static TVec3 operator *(TMat3x3 m, TVec3 v)
        {
            var r = new TVec3();
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    r[i] += m[i, j] * v[j];
            return r;
        }

        public static TVec3 operator *(TVec3 v, TMat3x3 m)
        {
            var r = new TVec3();
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    r[j] += v[i] * m[i, j];
            return r;
        }

        public void SetZero()
        {
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    this.m[i, j] = 0;
        }

        public void SetNumber(double k)
        {
            SetZero();
            for (var i = 0; i < 3; ++i)
                this.m[i, i] = k;
        }

        public void SetUnit()
        {
            SetNumber(1);
        }

        public void SetDiagonal(double lx, double ly, double lz)
        {
            SetZero();
            this.xx = lx;
            this.yy = ly;
            this.zz = lz;
        }

        public void SetRot(int i1, int i2, double angle)
        {
            SetUnit();
            this.m[i1, i1] = this.m[i2, i2] = Math.Cos(angle);
            this.m[i1, i2] = -(this.m[i2, i1] = Math.Sin(angle));
        }

        public void SetRotYZ(double angle) => SetRot(1, 2, angle);

        public void SetRotZX(double angle) => SetRot(2, 0, angle);

        public void SetRotXY(double angle) => SetRot(0, 1, angle);

        public void SetRotX(double angle) => SetRotYZ(angle);

        public void SetRotY(double angle) => SetRotZX(angle);

        public void SetRotZ(double angle) => SetRotXY(angle);

        public void SetExchangeAxes(int i, int j)
        {
            SetUnit();
            this.m[i, i] = this.m[j, j] = 0;
            this.m[i, j] = this.m[j, i] = 1;
        }

        public void SetExchangeYZ() => SetExchangeAxes(1, 2);

        public void SetSpecialProjection(double angle)
        {
            SetUnit();
            this.zy = Math.Sin(angle);
        }
    }

    public class TMat3x3Diagonal : TMat3x3
    {
        public TMat3x3Diagonal(double lx, double ly, double lz)
        {
            SetDiagonal(lx, ly, lz);
        }
    }

    public class TSpecialProjection : TMat3x3
    {
        public TSpecialProjection(double angle)
        {
            SetSpecialProjection(angle);
        }
    }

    public class TRotX : TMat3x3
    {
        public TRotX(double angle)
        {
            SetRotX(angle);
        }
    }

    public class TRotY : TMat3x3
    {
        public TRotY(double angle)
        {
            SetRotY(angle);
        }
    }

    public class TRotZ : TMat3x3
    {
        public TRotZ(double angle)
        {
            SetRotZ(angle);
        }
    }

    public class TExchangeYZ : TMat3x3
    {
        public TExchangeYZ()
        {
            SetExchangeYZ();
        }
    }
}
