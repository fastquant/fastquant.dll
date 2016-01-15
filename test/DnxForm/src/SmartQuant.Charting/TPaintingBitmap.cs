using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TPaintingBitmap
    {
        public const PixelFormat pixel_format = PixelFormat.Format32bppRgb;
        private int[] m;
        private int width;
        private int height;
        private int sz;

        public int Width => this.width;

        public int Height => this.height;

        public bool Valid => this.m != null;

        public TPaintingBitmap()
        {
        }

        public TPaintingBitmap(int W, int H)
        {
            BeginDrawing(W, H);
        }

        public bool BeginDrawing(int w, int h)
        {
            this.width = w;
            this.height = h;
            this.sz = w * h;
            this.m = new int[this.sz];
            return true;
        }

        public Bitmap Get()
        {
            Bitmap bitmap = new Bitmap(this.width, this.height, PixelFormat.Format32bppRgb);
            Rectangle rect = new Rectangle(0, 0, this.width, this.height);
            BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(this.m, 0, bitmapdata.Scan0, this.sz);
            bitmap.UnlockBits(bitmapdata);
            return bitmap;
        }

        public int intGetPixel(int x, int y) => this.m[this.width * y + x];

        public Color ColorGetPixel(int x, int y) => Color.FromArgb(this.m[this.width * y + x]);

        public void SetPixel(int x, int y, int c) => this.m[this.width * y + x] = c;

        public void SetPixel(int x, int y, Color c) => this.m[this.width * y + x] = c.ToArgb();

        public void Fill(int c)
        {
            for (int i = 0; i < this.sz; ++i)
                this.m[i] = c;
        }

        public void Fill(Color c) => Fill(c.ToArgb());

        public unsafe void FillRectangle(int c, int x, int y, int w, int h)
        {
            if (x + w > this.width)
                w -= x + w - this.width;
            if (y + h > this.height)
                h -= y + h - this.height;
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
                int* numPtr2 = numPtr1 + this.width * y + x;
                int num = y + h;
                while (y < num)
                {
                    int* numPtr3 = numPtr2;
                    for (int* numPtr4 = numPtr2 + w; numPtr3 < numPtr4; ++numPtr3)
                        *numPtr3 = c;
                    ++y;
                    numPtr2 += this.width;
                }
            }
        }

        public void FillRectangle(Color c, int x, int y, int w, int h) => FillRectangle(c.ToArgb(), x, y, w, h);
    }
}
