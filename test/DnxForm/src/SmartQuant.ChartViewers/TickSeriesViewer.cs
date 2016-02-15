using SmartQuant.Charting;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmartQuant.ChartViewers
{
    public class TickSeriesViewer : Viewer
    {
        public Pad Pad { get; set; }

        public Color Color { get; set; }

        public int DrawWidth { get; set; }

        public override bool IsZoomable => true;

        public TickSeriesViewer()
        {
            Type = typeof(TickSeries);
            Color = Color.Black;
            DrawWidth = 1;
        }

        public override PadRange GetPadRangeX(object obj, Pad pad) => null;

        public override PadRange GetPadRangeY(object obj, Pad pad)
        {
            var ts = obj as TickSeries;
            if (ts == null || ts.Count == 0)
                return null;
            var dt1 = new DateTime((long) pad.XMin);
            var dt2 = new DateTime((long) pad.XMax);
            var min = ts.GetMin(dt1, dt2);
            var minPx = min?.Price ?? 0;
            var max = ts.GetMax(dt1, dt2);
            var maxPx = max?.Price ?? 0;
            return new PadRange(minPx, maxPx);
        }

        // TODO: review it
        public override void Paint(object obj, Pad pad)
        {
            var tickSeries = obj as TickSeries;
            if (tickSeries.Count == 0)
                return;
            double xmin = pad.XMin;
            double xmax = pad.XMax;
            double ymin = pad.YMin;
            double ymax = pad.YMax;
            List<Property> list = null;
            if (this.metadata.TryGetValue(obj, out list))
            {
                foreach (var property in list)
                {
                    if (property.Name == "Color")
                        this.Color = (Color)property.Value;
                    if (property.Name == "Width")
                        this.DrawWidth = (int)property.Value;
                }
            }
            var pen = new Pen(Color, DrawWidth);
            int num1 = 0;
            double num2 = 0.0;
            double num3 = 0.0;
            long num4 = 0L;
            long num5 = 0L;
            int num6 = 0;
            int num7 = 0;
            long num8 = 0L;
            long num9 = 0L;
            int num10 = 0;
            int num11 = 0;
            DateTime datetime1 = new DateTime((long)xmin);
            DateTime datetime2 = new DateTime((long)xmax);
            int num12 = !(datetime1 < tickSeries.FirstDateTime) ? tickSeries.GetIndex(datetime1, IndexOption.Prev) : 0;
            int num13 = !(datetime2 > tickSeries.LastDateTime) ? tickSeries.GetIndex(datetime2, IndexOption.Next) : tickSeries.Count - 1;
            if (num12 == -1 || num13 == -1)
                return;
            for (int index = num12; index <= num13; ++index)
            {
                Tick tick = tickSeries[index];
                double num14 = (double)tick.DateTime.Ticks;
                pad.ClientX(num14);
                double price = tick.Price;
                if (num1 != 0)
                {
                    num4 = (long)pad.ClientX(num2);
                    num6 = pad.ClientY(num3);
                    num5 = (long)pad.ClientX(num14);
                    num7 = pad.ClientY(price);
                    if ((pad.IsInRange(num14, price) || pad.IsInRange(num2, num3)) && (num4 != num8 || num5 != num9 || (num6 != num10 || num7 != num11)))
                        pad.Graphics.DrawLine(pen, (float)num4, (float)num6, (float)num5, (float)num7);
                }
                num8 = num4;
                num10 = num6;
                num9 = num5;
                num11 = num7;
                num2 = num14;
                num3 = price;
                ++num1;
            }
        }
    }
}
