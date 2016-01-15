using System;
using System.Collections;
using System.Drawing;

namespace SmartQuant.Charting
{
    [Serializable]
    class TPieItem
    {
        public double Weight { get; set; }

        public Color Color { get; set; }

        public string Text { get; set; }

        public TPieItem(double weight, string text, Color color)
        {
            Weight = weight;
            Text = text;
            Color = color;
        }
    }

    public class Pie : IDrawable
    {
        private Color[] palette;

        public string Name { get; set; }

        public string Title { get; set; }

        public bool ToolTipEnabled { get; set; }

        public string ToolTipFormat { get; set; }

        public ArrayList Pieces { get; private set; }

        public bool EnableContour { get; set; }

        public Color ContourColor { get; set; }

        public int Gap { get; set; }

        public string Format { get; set; }

        public Pie() : this(null, null)
        {
        }

        public Pie(string name) : this(name, null)
        {
        }

        public Pie(string name, string title)
        {
            Name = Name;
            Title = title;
            Pieces = new ArrayList();
            EnableContour = true;
            ContourColor = Color.Gray;
            Gap = 0;
            Format = "F1";
            palette = CreateRainbowPalette();
        }


        public void Add(double weight) => Pieces.Add(new TPieItem(weight, "", Color.Empty));

        public void Add(double weight, string text, Color color) => Pieces.Add(new TPieItem(weight, text, color));

        public void Add(double weight, string text) => Pieces.Add(new TPieItem(weight, text, Color.Empty));

        public virtual void Draw(string option)
        {
            if (Chart.Pad == null)
            {
                var canvas = new Canvas("Canvas", "Canvas");
            }
            Chart.Pad.AxisBottom.Enabled = false;
            Chart.Pad.AxisLeft.Enabled = false;
            Chart.Pad.AxisRight.Enabled = false;
            Chart.Pad.AxisTop.Enabled = false;
            Chart.Pad.ForeColor = Color.LightGray;
            Chart.Pad.Title.Text = Name;
            MakeLegend();
            Chart.Pad.Add(this);
        }

        private void MakeLegend()
        {
            int num1 = 0;
            foreach (TPieItem tpieItem in Pieces)
            {
                if (tpieItem.Color == Color.Empty)
                    tpieItem.Color = this.palette[num1 * 160 / Pieces.Count];
                ++num1;
            }
            double num2 = 0.0;
            foreach (TPieItem tpieItem in Pieces)
                num2 += tpieItem.Weight;
            foreach (TPieItem tpieItem in Pieces)
            {
                double num3 = tpieItem.Weight / num2;
                string Text = tpieItem.Text.Replace("&%", (num3 * 100.0).ToString(Format));
                Chart.Pad.Title.Add(Text, tpieItem.Color);
                Chart.Pad.Legend.Add(Text, tpieItem.Color);
            }
        }

        public virtual void Draw() => Draw("");

        private Color[] CreatePalette(Color LowColor, Color HighColor, int NColors)
        {
            Color[] colorArray = new Color[NColors];
            double num1 = (double)((int)HighColor.R - (int)LowColor.R) / (double)NColors;
            double num2 = (double)((int)HighColor.G - (int)LowColor.G) / (double)NColors;
            double num3 = (double)((int)HighColor.B - (int)LowColor.B) / (double)NColors;
            double num4 = (double)LowColor.R;
            double num5 = (double)LowColor.G;
            double num6 = (double)LowColor.B;
            colorArray[0] = LowColor;
            for (int index = 1; index < NColors; ++index)
            {
                num4 += num1;
                num5 += num2;
                num6 += num3;
                colorArray[index] = Color.FromArgb((int)num4, (int)num5, (int)num6);
            }
            return colorArray;
        }

        private Color[] CreateRainbowPalette()
        {
            Color[] colorArray = new Color[160];
            int num = 0;
            foreach (Color color in this.CreatePalette(Color.Purple, Color.Blue, 32))
                colorArray[num++] = color;
            foreach (Color color in this.CreatePalette(Color.Blue, Color.Green, 32))
                colorArray[num++] = color;
            foreach (Color color in this.CreatePalette(Color.Green, Color.Yellow, 32))
                colorArray[num++] = color;
            foreach (Color color in this.CreatePalette(Color.Yellow, Color.Orange, 32))
                colorArray[num++] = color;
            foreach (Color color in this.CreatePalette(Color.Orange, Color.Red, 32))
                colorArray[num++] = color;
            return colorArray;
        }

        public virtual void Paint(Pad pad, double xMin, double xMax, double yMin, double yMax)
        {
            double num1 = 0.0;
            for (int index = 0; index < Pieces.Count; ++index)
                num1 += ((TPieItem)Pieces[index]).Weight;
            int num2 = pad.ClientX(0.0);
            int num3 = pad.ClientY(100.0);
            int num4 = Math.Abs(pad.ClientX(100.0) - pad.ClientX(0.0));
            int num5 = Math.Abs(pad.ClientY(100.0) - pad.ClientY(0.0));
            int num6 = 0;
            int num7 = 0;
            if (num4 > num5)
            {
                num6 = (num4 - num5) / 2;
                num4 = num5;
            }
            else
            {
                num7 = (num5 - num4) / 2;
                num5 = num4;
            }
            int num8 = num5 / 10;
        //     double num9 ;
            double num10 = 0.0;
            for (int index = 0; index < Pieces.Count; ++index)
            {
                double num11 = ((TPieItem)Pieces[index]).Weight / num1;
                var brush = new SolidBrush(((TPieItem)Pieces[index]).Color);
                double num12 = num10 + (double)Gap;
                num10 += 360.0 * num11;
                double num13 = num10 - num12 + 1.0;
                pad.Graphics.FillPie(brush, num2 + num6 + num8, num3 + num7 + num8, num4 - 2 * num8, num5 - 2 * num8, (int)num12, (int)num13);
            }
            if (EnableContour)
            {
           //     num9 = 0.0;
                double num11 = 0.0;
                for (int index = 0; index < Pieces.Count; ++index)
                {
                    double num12 = ((TPieItem)Pieces[index]).Weight / num1;
                    Pen pen = new Pen(ContourColor);
                    double num13 = num11 + Gap;
                    num11 += 360.0 * num12;
                    double num14 = num11 - num13 + 1.0;
                    pad.Graphics.DrawPie(pen, (float)(num2 + num6 + num8), (float)(num3 + num7 + num8), (float)(num4 - 2 * num8), (float)(num5 - 2 * num8), (float)num13, (float)num14);
                }
            }
     //       num9 = 0.0;
            double num15 = 0.0;
            for (int index = 0; index < Pieces.Count; ++index)
            {
                double num11 = ((TPieItem)Pieces[index]).Weight / num1;
                Pen pen = new Pen(ContourColor);
                double num12 = num15 + Gap;
                num15 += 360.0 * num11;
                double num13 = num12 + (num15 - num12) / 2.0 + 90.0;
                int num14 = (num4 - 2 * num8) / 4;
                int num16 = (num4 - 2 * num8) / 2;
                Math.Sin(Math.PI / 180.0 * num13);
                int num17 = (num4 - 2 * num8) / 2;
                Math.Cos(Math.PI / 180.0 * num13);
                int num18 = (num4 - 2 * num8) / 2;
                int num19 = (num4 - 2 * num8) / 2 + 10;
                int x1 = (int)((double)(num2 + num6 + num8 + (num4 - 2 * num8) / 2) + (double)num18 * Math.Sin(Math.PI / 180.0 * num13));
                int y1 = (int)((double)(num3 + num7 + num8 + (num4 - 2 * num8) / 2) - (double)num18 * Math.Cos(Math.PI / 180.0 * num13));
                int num20 = (int)((double)(num2 + num6 + num8 + (num4 - 2 * num8) / 2) + (double)num19 * Math.Sin(Math.PI / 180.0 * num13));
                int num21 = (int)((double)(num3 + num7 + num8 + (num4 - 2 * num8) / 2) - (double)num19 * Math.Cos(Math.PI / 180.0 * num13));
                Font font = new Font("Arial", 8f);
                pad.Graphics.DrawLine(new Pen(Color.Gray), x1, y1, num20, num21);
                string str = ((TPieItem)Pieces[index]).Text.Replace("&%", (num11 * 100.0).ToString(Format));
                if (num20 > num2 + num6 + num4 / 2)
                {
                    pad.Graphics.DrawLine(new Pen(Color.Gray), num20, num21, num20 + 5, num21);
                    SizeF sizeF = pad.Graphics.MeasureString(str, font);
                    pad.Graphics.DrawString(str, font, (Brush)new SolidBrush(Color.Black), (float)(num20 + 5), (float)num21 - sizeF.Height / 2f);
                }
                else
                {
                    pad.Graphics.DrawLine(new Pen(Color.Gray), num20, num21, num20 - 5, num21);
                    SizeF sizeF = pad.Graphics.MeasureString(str, font);
                    pad.Graphics.DrawString(str, font, (Brush)new SolidBrush(Color.Black), (float)(num20 - 5) - sizeF.Width, (float)num21 - sizeF.Height / 2f);
                }
            }
        }

        public TDistance Distance(double x, double y) => null;
    }
}

