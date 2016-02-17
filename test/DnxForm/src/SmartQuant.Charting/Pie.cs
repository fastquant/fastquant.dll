using System;
using System.Collections;
using System.Drawing;
using System.Linq;

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
        private readonly Color[] palette;

        public string Name { get; set; }

        public string Title { get; set; }

        public bool ToolTipEnabled { get; set; }

        public string ToolTipFormat { get; set; }

        public ArrayList Pieces { get; } = new ArrayList();

        public bool EnableContour { get; set; } = true;

        public Color ContourColor { get; set; } = Color.Gray;

        public int Gap { get; set; } = 0;

        public string Format { get; set; } = "F1";

        public Pie() : this(null, null)
        {
        }

        public Pie(string name) : this(name, null)
        {
        }

        public Pie(string name, string title)
        {
            Name = name;
            Title = title;
            this.palette = CreateRainbowPalette();
        }


        public void Add(double weight) => Pieces.Add(new TPieItem(weight, "", Color.Empty));

        public void Add(double weight, string text, Color color) => Pieces.Add(new TPieItem(weight, text, color));

        public void Add(double weight, string text) => Pieces.Add(new TPieItem(weight, text, Color.Empty));

        public virtual void Draw(string option)
        {
            if (Chart.Pad == null)
                new Canvas("Canvas", "Canvas");
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
            foreach (TPieItem item in Pieces)
            {
                if (item.Color == Color.Empty)
                    item.Color = this.palette[num1 * 160 / Pieces.Count];
                ++num1;
            }
            double num2 = Pieces.Cast<TPieItem>().Sum(item => item.Weight);
            foreach (TPieItem item in Pieces)
            {
                double num3 = item.Weight / num2;
                var text = item.Text.Replace("&%", (num3 * 100.0).ToString(Format));
                Chart.Pad.Title.Add(text, item.Color);
                Chart.Pad.Legend.Add(text, item.Color);
            }
        }

        public virtual void Draw() => Draw("");

        private Color[] CreatePalette(Color lowColor, Color highColor, int nColors)
        {
            var colors = new Color[nColors];
            var num1 = ((double)highColor.R - lowColor.R) / nColors;
            var num2 = ((double)highColor.G - lowColor.G) / nColors;
            var num3 = ((double)highColor.B - lowColor.B) / nColors;
            double r = lowColor.R;
            double g = lowColor.G;
            double b = lowColor.B;
            colors[0] = lowColor;
            for (var index = 1; index < nColors; ++index)
            {
                r += num1;
                g += num2;
                b += num3;
                colors[index] = Color.FromArgb((int)r, (int)g, (int)b);
            }
            return colors;
        }

        private Color[] CreateRainbowPalette()
        {
            var colors = new Color[160];
            int num = 0;
            foreach (var color in CreatePalette(Color.Purple, Color.Blue, 32))
                colors[num++] = color;
            foreach (var color in CreatePalette(Color.Blue, Color.Green, 32))
                colors[num++] = color;
            foreach (var color in CreatePalette(Color.Green, Color.Yellow, 32))
                colors[num++] = color;
            foreach (var color in CreatePalette(Color.Yellow, Color.Orange, 32))
                colors[num++] = color;
            foreach (var color in CreatePalette(Color.Orange, Color.Red, 32))
                colors[num++] = color;
            return colors;
        }

        // TODO: rewrite it
        public virtual void Paint(Pad pad, double xMin, double xMax, double yMin, double yMax)
        {
            var num1 = Pieces.Count > 0 ? Pieces.Cast<TPieItem>().Sum(item => item.Weight) : 0;
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

