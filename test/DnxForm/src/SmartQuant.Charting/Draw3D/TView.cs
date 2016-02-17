using SmartQuant.Charting;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SmartQuant.Charting.Draw3D
{
    public class TView
    {
        private static TMat3x3 ReverseY = new TMat3x3Diagonal(1, -1, 1);
        private static TMat3x3 ExchangeYZ = new TExchangeYZ();
        private static TMat3x3 ToScreenCoords = ReverseY * ExchangeYZ;
        public TLight Light { get; set; } = new TLight();
        private double fScaleZ = 1;
        private TMat3x3 m;
        private TMat3x3 ms;

        public int Left { get; private set; }

        public int Top { get; private set; }

        public int H { get; private set; }

        public TVec3 O { get; private set; }

        public TVec3 Lx { get; private set; }

        public TVec3 Ly { get; private set; }

        public TVec3 Lz { get; private set; }

        public double ScaleZ
        {
            get
            {
                return this.fScaleZ;
            }
            set
            {
                this.fScaleZ = value < 0 ? 1 : value;
            }
        }

        public TView()
        {
            SetProjectionSpecial(-2, Math.PI / 6.0);
        }

        public static TView View(Pad pad)
        {
            pad.View3D = pad.View3D ?? new TView();
            return (TView)pad.View3D;
        }

        public void SetProjectionOrthogonal(double angleXY, double viewAngle)
        {
            this.m = new TRotZ(angleXY);
            this.m = new TRotX(viewAngle) * this.m;
            this.ms = ToScreenCoords * this.m;
        }

        public void SetProjectionSpecial(double angleXY, double viewAngle)
        {
            this.m = new TRotZ(angleXY);
            this.m = new TSpecialProjection(viewAngle) * this.m;
            this.ms = ToScreenCoords * this.m;
        }

        public void CalculateAxes(Pad pad, int left, int top, int h)
        {
            this.Left = left;
            this.Top = top;
            this.H = h;
            this.O = new TVec3((double)(left + h / 2), (double)(top + 3 * h / 4), 0.0);
            if (this.ScaleZ < 1.0)
                this.O.y -= (1.0 - this.ScaleZ) * 0.25 * (double)h;
            double num = 0.7 * (double)h;
            double Z = 0.5 * this.ScaleZ * (double)h;
            this.Lx = new TVec3(num, 0.0, 0.0);
            this.Ly = new TVec3(0.0, num, 0.0);
            this.Lz = new TVec3(0.0, 0.0, Z);
            this.Lx = this.ms * this.Lx;
            this.Ly = this.ms * this.Ly;
            this.Lz = this.ms * this.Lz;
        }

        private static void SetAxesPropertiesFor3D(Pad pad)
        {
            if (pad.Grid3D)
            {
                foreach (var axis in pad.Axes3D)
                    axis.GridEnabled = true;
            }
            else
            {
                foreach (var axis in pad.Axes3D)
                    axis.GridEnabled = false;
            }
        }

        public void PaintAxisGridAndTicks(Graphics g, Axis a, bool marks, TVec3 o, TVec3 o_, TVec3 L)
        {
            a.MinorGridEnabled = false;
            a.MinorTicksEnabled = false;
            int nTicks = (int)(L.NormInf / 10.0);
            if (nTicks < 3)
                nTicks = 3;
            if (nTicks > 10)
                nTicks = 10;
            TAxisCalc taxisCalc = new TAxisCalc(o, o + L, a.Min, a.Max, nTicks);
            TVec3 tvec3_1 = o_ - o;
            TVec3 tvec3_2 = TVec3.O;
            TVec3 tvec3_3 = -0.04 * tvec3_1;
            if (o_.y > o.y)
            {
                tvec3_2 = tvec3_1;
                tvec3_3 = -tvec3_3;
            }
            TVec3 tvec3_4 = tvec3_2 + 1.04 * tvec3_3;
            if (a.GridEnabled)
            {
                Pen pen = new Pen(a.GridColor, a.GridWidth);
                for (int i = 0; i < taxisCalc.nTicks; ++i)
                    g.DrawLine(pen, (Point)taxisCalc.TickPos(i), (Point)(taxisCalc.TickPos(i) + tvec3_1));
            }
            if (a.Position == EAxisPosition.Right && tvec3_3.x <= 0.0)
                marks = false;
            if (!marks)
                return;
            int num1 = a.Position == EAxisPosition.Bottom ? 0 : 1;
            int num2 = a.Position == EAxisPosition.Bottom ? taxisCalc.nTicks - 1 : taxisCalc.nTicks;
            float num3 = 0.0f;
            if (a.MajorTicksEnabled)
            {
                Pen pen = new Pen(a.GridColor, a.GridWidth);
                for (int i = num1; i < num2; ++i)
                    g.DrawLine(pen, (Point)(taxisCalc.TickPos(i) + tvec3_2), (Point)(taxisCalc.TickPos(i) + tvec3_2 + tvec3_3));
            }
            if (a.LabelEnabled)
            {
                Font labelFont = a.LabelFont;
                float height = labelFont.GetHeight(g);
                SolidBrush solidBrush = new SolidBrush(a.LabelColor);
                StringFormat format = new StringFormat();
                float num4;
                if (a.Position == EAxisPosition.Bottom)
                {
                    format.FormatFlags = StringFormatFlags.DirectionVertical;
                    num4 = (float)Math.Abs(taxisCalc.TickPos(1).x - taxisCalc.TickPos(0).x);
                }
                else
                {
                    tvec3_4.y -= 0.5 * (double)a.LabelFont.GetHeight();
                    num4 = (float)Math.Abs(taxisCalc.TickPos(1).y - taxisCalc.TickPos(0).y);
                }
                if (tvec3_3.x < 0.0)
                    tvec3_4.x -= (double)a.LabelFont.GetHeight();
                if ((double)num4 > 0.0)
                {
                    int num5 = (int)((double)height / (double)num4 + 1.0);
                    if (num1 + num5 < num2)
                    {
                        int i = num1;
                        while (i < num2)
                        {
                            if (i + num5 >= num2)
                                i = num2 - 1;
                            TVec3 tvec3_5 = taxisCalc.TickPos(i) + tvec3_4;
                            string str = taxisCalc.TickVal(i).ToString();
                            g.DrawString(str, labelFont, (Brush)solidBrush, (PointF)tvec3_5, format);
                            SizeF sizeF = g.MeasureString(str, labelFont);
                            if ((double)sizeF.Width > (double)num3)
                                num3 = sizeF.Width;
                            i += num5;
                        }
                    }
                    else if (num2 > 0)
                    {
                        int i = num2 - 1;
                        TVec3 tvec3_5 = taxisCalc.TickPos(i) + tvec3_4;
                        string str = taxisCalc.TickVal(i).ToString();
                        g.DrawString(str, labelFont, (Brush)solidBrush, (PointF)tvec3_5, format);
                        num3 = g.MeasureString(str, labelFont).Width;
                    }
                }
            }
            if (!a.TitleEnabled)
                return;
            SizeF sizeF1 = g.MeasureString(a.Title, a.TitleFont);
            PointF point1;
            PointF point2;
            float angle;
            if (a.Position == EAxisPosition.Bottom)
            {
                if (tvec3_3.x < 0.0)
                {
                    point1 = taxisCalc.TickPos(0).x > taxisCalc.TickPos(1).x ? (PointF)(taxisCalc.TickPos(taxisCalc.nTicks - 1) + tvec3_4) : (PointF)(taxisCalc.TickPos(0) + tvec3_4);
                    point1.Y += num3;
                    point2 = point1;
                    angle = (float)(Math.Atan2(Math.Abs(L.y), Math.Abs(L.x)) * 180.0 / Math.PI);
                }
                else
                {
                    point1 = taxisCalc.TickPos(0).x > taxisCalc.TickPos(1).x ? (PointF)(taxisCalc.TickPos(0) + tvec3_4) : (PointF)(taxisCalc.TickPos(taxisCalc.nTicks - 1) + tvec3_4);
                    point1.X += a.LabelFont.GetHeight(g);
                    point1.Y += num3;
                    point2 = point1;
                    point2.X -= sizeF1.Width;
                    angle = (float)(-Math.Atan2(Math.Abs(L.y), Math.Abs(L.x)) * 180.0 / Math.PI);
                }
            }
            else
            {
                point1 = taxisCalc.TickPos(0).z > taxisCalc.TickPos(1).z ? (PointF)(taxisCalc.TickPos(0) + tvec3_4) : (PointF)(taxisCalc.TickPos(taxisCalc.nTicks - 1) + tvec3_4);
                point1.X += num3;
                point2 = point1;
                point2.X -= sizeF1.Width;
                angle = -90f;
            }
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, point1, MatrixOrder.Append);
            g.Transform = matrix;
            g.DrawString(a.Title, a.LabelFont, (Brush)new SolidBrush(a.LabelColor), point2);
            matrix.Reset();
            g.Transform = matrix;
        }

        public void PaintAxes(Graphics g, Pad pad, int left, int top, int h)
        {
            Graphics graphics = pad.Graphics;
            int x1 = pad.X1;
            int x2 = pad.X2;
            int y1 = pad.Y1;
            int y2 = pad.Y2;
            pad.Graphics = g;
            pad.X1 = left;
            pad.X2 = left + h;
            pad.Y1 = top;
            pad.Y2 = top + h;
            PaintAxes(pad, left, top, h);
            pad.Graphics = graphics;
            pad.X1 = x1;
            pad.X2 = x2;
            pad.Y1 = y1;
            pad.Y2 = y2;
        }

        public void PaintAxes(Pad pad, int left, int top, int h)
        {
            CalculateAxes(pad, left, top, h);
            var v = new TVec3[]
            {
                this.O - 0.5 * Lx - 0.5 * Ly,
                this.O + 0.5 * Lx - 0.5 * Ly,
                this.O + 0.5 * Lx + 0.5 * Ly,
                this.O - 0.5 * Lx + 0.5 * Ly
            };
            double num1 = -1.0;
            int num2 = -1;
            for (int index = 0; index < v.Length; ++index)
            {
                if (v[index].y > num1)
                {
                    num1 = v[index].y;
                    num2 = index;
                }
            }
            int index1 = 0;
            int index2 = 0;
            int index3 = 0;
            switch (num2)
            {
                case 0:
                    index1 = 1;
                    index2 = 2;
                    index3 = 3;
                    break;
                case 1:
                    index1 = 2;
                    index2 = 3;
                    index3 = 0;
                    break;
                case 2:
                    index1 = 3;
                    index2 = 0;
                    index3 = 1;
                    break;
                case 3:
                    index1 = 0;
                    index2 = 1;
                    index3 = 2;
                    break;
            }
            var points1 = TVec3.PointArray(v);
            var points2 = new Point[]
            {
                v[index1],
                v[index2],
                v[index2] + this.Lz,
                v[index1] + this.Lz
            };
            var points3 = new Point[]
            {
                v[index2],
                v[index3],
                v[index3] + Lz,
                v[index2] + Lz
            };
            var graphics = pad.Graphics;
            graphics.Clip = new Region(new Rectangle(pad.X1, pad.Y1, pad.Width + 1, pad.Height + 1));
            var pen = new Pen(Color.Black, 1f);
            Brush brush = new SolidBrush(Color.White);
            graphics.FillPolygon(brush, points1);
            graphics.FillPolygon(brush, points2);
            graphics.FillPolygon(brush, points3);
            graphics.DrawPolygon(pen, points1);
            graphics.DrawPolygon(pen, points2);
            graphics.DrawPolygon(pen, points3);
            TView.SetAxesPropertiesFor3D(pad);
            pad.AxisX3D.Position = EAxisPosition.Bottom;
            pad.AxisY3D.Position = EAxisPosition.Bottom;
            pad.AxisZ3D.Position = EAxisPosition.Right;
            PaintAxisGridAndTicks(graphics, pad.AxisX3D, true, v[0], v[0] + Ly, Lx);
            PaintAxisGridAndTicks(graphics, pad.AxisY3D, true, v[0], v[0] + Lx, Ly);
            PaintAxisGridAndTicks(graphics, pad.AxisX3D, false, v[index2], v[index2] + Lz, Lx);
            PaintAxisGridAndTicks(graphics, pad.AxisY3D, false, v[index2], v[index2] + Lz, Ly);
            PaintAxisGridAndTicks(graphics, pad.AxisZ3D, true, v[index2], v[index2] + Lx, Lz);
            PaintAxisGridAndTicks(graphics, pad.AxisZ3D, true, v[index2], v[index2] + Ly, Lz);
        }
    }
}
