using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TPaintingBitmap
    {
        private const PixelFormat pixel_format = PixelFormat.Format32bppRgb;
        private int[] m;
        private int sz;

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool Valid => this.m != null;

        public TPaintingBitmap()
        {
        }

        public TPaintingBitmap(int w, int h)
        {
            BeginDrawing(w, h);
        }

        public bool BeginDrawing(int w, int h)
        {
            Width = w;
            Height = h;
            this.sz = w * h;
            this.m = new int[this.sz];
            return true;
        }

        public Bitmap Get()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppRgb);
            var rect = new Rectangle(0, 0, Width, Height);
            var bitmapdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(this.m, 0, bitmapdata.Scan0, this.sz);
            bitmap.UnlockBits(bitmapdata);
            return bitmap;
        }

        public int intGetPixel(int x, int y) => this.m[Width * y + x];

        public Color ColorGetPixel(int x, int y) => Color.FromArgb(this.m[Width * y + x]);

        public void SetPixel(int x, int y, int c) => this.m[Width * y + x] = c;

        public void SetPixel(int x, int y, Color c) => this.m[Width * y + x] = c.ToArgb();

        public void Fill(int c)
        {
            for (var i = 0; i < this.sz; ++i)
                this.m[i] = c;
        }

        public void Fill(Color c) => Fill(c.ToArgb());

        // TODO: review
        public unsafe void FillRectangle(int c, int x, int y, int w, int h)
        {
            if (x + w > Width)
                w -= x + w - Width;
            if (y + h > Height)
                h -= y + h - Height;
            if (x < 0)
            {
                w += x;
                x = 0;
            }
            if (y < 0)
            {
                h += y;
                y = 0;
            }
            fixed (int* numPtr1 = this.m)
            {
                int* numPtr2 = numPtr1 + Width * y + x;
                int num = y + h;
                while (y < num)
                {
                    int* numPtr3 = numPtr2;
                    for (int* numPtr4 = numPtr2 + w; numPtr3 < numPtr4; ++numPtr3)
                        *numPtr3 = c;
                    ++y;
                    numPtr2 += Width;
                }
            }
        }

        public void FillRectangle(Color c, int x, int y, int w, int h) => FillRectangle(c.ToArgb(), x, y, w, h);
    }
}
