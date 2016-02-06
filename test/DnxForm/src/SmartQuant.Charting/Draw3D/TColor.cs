// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Drawing;
using System;

namespace SmartQuant.Charting.Draw3D
{
    public struct TColor
    {
        private double r;
        private double g;
        private double b;

        public Color Color
        {
            get
            {
                return Color.FromArgb((int)(byte.MaxValue * this.r), (int)(byte.MaxValue * this.g), (int)(byte.MaxValue * this.b));
            }
            set
            {
                this = new TColor(value);
            }
        }

        public TColor(double r, double g, double b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public TColor(double gray)
        {
            this.r = this.g = this.b = gray;
        }

        public TColor(Color c)
        {
            this.r = 1.0 / byte.MaxValue * c.R;
            this.g = 1.0 / byte.MaxValue * c.G;
            this.b = 1.0 / byte.MaxValue * c.B;
        }

        public static implicit operator TColor(Color c)
        {
            return new TColor(c);
        }

        public static TColor operator +(TColor a, TColor b)
        {
            return new TColor(a.r + b.r, a.g + b.g, a.b + b.b);
        }

        public static TColor operator *(TColor a, TColor b)
        {
            return new TColor(a.r * b.r, a.g * b.g, a.b * b.b);
        }

        public static TColor operator *(double k, TColor c)
        {
            return new TColor(k * c.r, k * c.g, k * c.b);
        }

        public static TColor operator *(TColor c, double k)
        {
            return k * c;
        }

        public int Get888()
        {
            return ((int)(byte.MaxValue * this.r) << 16) + ((int)(byte.MaxValue * this.g) << 8) + (int)(byte.MaxValue * this.b);
        }

        private void Clip(ref double x)
        {
            x = Math.Max(x, 1.0 / 254);
            x = Math.Min(x, 1.0);
        }

        public void Clip()
        {
            Clip(ref this.r);
            Clip(ref this.g);
            Clip(ref this.b);
        }

        public static TColor Clip(TColor c)
        {
            c.Clip();
            return c;
        }
    }
}
