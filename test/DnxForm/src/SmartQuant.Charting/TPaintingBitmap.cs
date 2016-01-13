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
        private int[] M;
        private int width;
        private int height;
        private int Sz;

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
        }

        public bool Valid
        {
            get
            {
                return this.M != null;
            }
        }

        public TPaintingBitmap()
        {
        }

        public TPaintingBitmap(int W, int H)
        {
            this.BeginDrawing(W, H);
        }

        public bool BeginDrawing(int W, int H)
        {
            this.width = W;
            this.height = H;
            this.Sz = W * H;
            this.M = new int[this.Sz];
            return true;
        }

        public Bitmap Get()
        {
            Bitmap bitmap = new Bitmap(this.width, this.height, PixelFormat.Format32bppRgb);
            Rectangle rect = new Rectangle(0, 0, this.width, this.height);
            BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(this.M, 0, bitmapdata.Scan0, this.Sz);
            bitmap.UnlockBits(bitmapdata);
            return bitmap;
        }

        public int intGetPixel(int x, int y)
        {
            return this.M[this.width * y + x];
        }

        public Color ColorGetPixel(int x, int y)
        {
            return Color.FromArgb(this.M[this.width * y + x]);
        }

        public void SetPixel(int x, int y, int c)
        {
            this.M[this.width * y + x] = c;
        }

        public void SetPixel(int x, int y, Color c)
        {
            this.M[this.width * y + x] = c.ToArgb();
        }

        public void Fill(int c)
        {
            for (int i = 0; i < this.Sz; ++i)
                this.M[i] = c;
        }

        public void Fill(Color c)
        {
            Fill(c.ToArgb());
        }

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
            fixed (int* numPtr1 = this.M)
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

        public void FillRectangle(Color c, int x, int y, int w, int h)
        {
            FillRectangle(c.ToArgb(), x, y, w, h);
        }
    }
}
