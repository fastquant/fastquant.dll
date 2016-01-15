using SmartQuant;
using SmartQuant.Charting;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmartQuant.ChartViewers
{
    public class FillSeriesViewer : Viewer
    {
        public Color BuyColor { get; set; }

        public Color SellColor { get; set; }

        public bool TextEnabled { get; set; }

        public override bool IsZoomable => true;

        public FillSeriesViewer()
        {
            Type = typeof(FillSeries);
            BuyColor = Color.Blue;
            SellColor = Color.Red;
            TextEnabled = true;
        }

        public override PadRange GetPadRangeX(object obj, Pad pad) => null;

        public override PadRange GetPadRangeY(object obj, Pad pad) => null;

        public override void Paint(object obj, Pad pad)
        {
            var fs = obj as FillSeries;
            if (fs == null || fs.Count == 0)
                return;
            double xmin = pad.XMin;
            double xmax = pad.XMax;
            double ymin = pad.YMin;
            double ymax = pad.YMax;
            List<Viewer.Property> list =  null;
            if (this.metadata.TryGetValue(obj, out list))
            {
                foreach (var property in list)
                {
                    if (property.Name == "BuyColor")
                        this.BuyColor = (Color)property.Value;
                    if (property.Name == "SellColor")
                        this.SellColor = (Color)property.Value;
                    if (property.Name == "TextEnabled")
                        this.TextEnabled = (bool)property.Value;
                }
            }
            DateTime datetime1 = new DateTime((long)xmin);
            DateTime datetime2 = new DateTime((long)xmax);
            int num1 = !(datetime1 < fs[0].DateTime) ? fs.GetIndex(datetime1, IndexOption.Prev) : 0;
            int num2 = !(datetime2 > fs[fs.Count - 1].DateTime) ? fs.GetIndex(datetime2, IndexOption.Next) : fs.Count - 1;
            if (num1 == -1 || num2 == -1)
                return;
            for (int index = num1; index <= num2; ++index)
            {
                Fill fill = fs[index];
                int x = pad.ClientX((double)fill.DateTime.Ticks);
                int y = pad.ClientY(fill.Price);
                float num3 = 12f;
                string str = string.Format("{0} {1} @ {2} {3}", fill.Side, fill.Qty, fill.Price.ToString(fill.Instrument.PriceFormat), fill.Text);
                Font font = new Font("Arial", 8f);
                switch (fill.Side)
                {
                    case OrderSide.Buy:
                        Color buyColor = this.BuyColor;
                        PointF[] points1 = new PointF[]
                        {
                            new Point(x, y),
                            new Point((int)((double)x - (double)num3 / 2.0), (int)((double)y + (double)num3 / 2.0)),
                            new Point((int)((double)x + (double)num3 / 2.0), (int)((double)y + (double)num3 / 2.0))
                        };
                        pad.Graphics.DrawPolygon(new Pen(Color.Black), points1);
                        pad.Graphics.DrawRectangle(new Pen(Color.Black), (float)x - num3 / 4f, (float)y + num3 / 2f, num3 / 2f, num3 / 2f);
                        pad.Graphics.FillPolygon((Brush)new SolidBrush(buyColor), points1);
                        pad.Graphics.FillRectangle((Brush)new SolidBrush(buyColor), (float)x - num3 / 4f, (float)((double)y + (double)num3 / 2.0 - 1.0), num3 / 2f, (float)((double)num3 / 2.0 + 1.0));
                        break;
                    case OrderSide.Sell:
                        Color sellColor = SellColor;
                        var points2 = new Point[]
                        {
                            new Point(x, y),
                            new Point((int)((double)x - (double)num3 / 2.0), (int)((double)y - (double)num3 / 2.0)),
                            new Point((int)((double)x + (double)num3 / 2.0), (int)((double)y - (double)num3 / 2.0))
                        };
                        pad.Graphics.DrawPolygon(new Pen(Color.Black), points2);
                        pad.Graphics.DrawRectangle(new Pen(Color.Black), (float)x - num3 / 4f, (float)y - num3, num3 / 2f, (float)((double)num3 / 2.0 + 1.0));
                        pad.Graphics.FillPolygon(new SolidBrush(sellColor), points2);
                        pad.Graphics.FillRectangle(new SolidBrush(sellColor), (float)x - num3 / 4f, (float)y - num3, num3 / 2f, (float)((double)num3 / 2.0 + 1.0));
                        break;
                }
                if (TextEnabled)
                {
                    int num4 = (int)pad.Graphics.MeasureString(str, font).Width;
                    int num5 = (int)pad.Graphics.MeasureString(str, font).Height;
                    switch (fill.Side)
                    {
                        case OrderSide.Buy:
                            pad.Graphics.DrawString(str, font, (Brush)new SolidBrush(Color.Black), (float)(x - num4 / 2), (float)((double)y + (double)num3 + 2.0));
                            continue;
                        case OrderSide.Sell:
                            pad.Graphics.DrawString(str, font, (Brush)new SolidBrush(Color.Black), (float)(x - num4 / 2), (float)((double)y - (double)num3 - 2.0) - (float)num5);
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }
    }
}
