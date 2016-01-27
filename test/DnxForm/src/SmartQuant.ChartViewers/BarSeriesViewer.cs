using SmartQuant;
using SmartQuant.Charting;
using System;
using System.Drawing;

namespace SmartQuant.ChartViewers
{
    public class BarSeriesViewer : Viewer
    {
        public Pad Pad { get; set; }

        public ChartStyle ChartStyle { get; set; }

        public Color BarColor { get; set; }

        public Color CandleColor { get; set; }

        public Color CandleBorderColor { get; set; }

        public Color CandleWhiteColor { get; set; }

        public Color CandleBlackColor { get; set; }

        public EWidthStyle CandleWidthStyle { get; set; }

        public EWidthStyle BarWidthStyle { get; set; }

        public int BarWidth { get; set; }

        public int CandleWidth { get; set; }

        public Color Color { get; set; }

        public int DrawWidth { get; set; }

        public override bool IsZoomable => true;

        public BarSeriesViewer()
        {
            Type = typeof(BarSeries);
            Color = Color.Black;
            DrawWidth = 1;
            ChartStyle = ChartStyle.Candle;
            BarWidthStyle = EWidthStyle.Auto;
            CandleWidthStyle = EWidthStyle.Auto;
            CandleBlackColor = Color.Black;
            CandleWhiteColor = Color.White;
            CandleBorderColor = Color.Black;
            CandleColor = Color.Black;
            BarColor = Color.Black;
        }

        public override PadRange GetPadRangeX(object obj, Pad pad) => null;

        public override PadRange GetPadRangeY(object obj, Pad pad)
        {
            var bs = obj as BarSeries;
            if (bs == null || bs.Count == 0)
                return null;
            var dt1 = new DateTime((long)pad.XMin);
            var dt2 = new DateTime((long)pad.XMax);
            var lowest = bs.LowestLowBar(dt1, dt2);
            var min = lowest?.Low ?? 0;
            var highest = bs.HighestHighBar(dt1, dt2);
            var max = highest?.High ?? 0;
            return new PadRange(min, max);
        }

        public override void Paint(object obj, Pad pad)
        {
            var bs = obj as BarSeries;
            if (bs == null || bs.Count == 0)
                return;
            double xmin = pad.XMin;
            double xmax = pad.XMax;
            double ymin = pad.YMin;
            double ymax = pad.YMax;
            Pen pen1 = new Pen(Color);
            Pen pen2 = new Pen(BarColor);
            Pen pen3 = new Pen(CandleColor);
            Pen pen4 = new Pen(CandleBorderColor);
            Brush brush1 = new SolidBrush(CandleWhiteColor);
            Brush brush2 = new SolidBrush(CandleBlackColor);
            int num1 = 0;
            double num2 = 0.0;
            double num3 = 0.0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int x2 = 0;
            long num7 = 0L;
            long num8 = 0L;
            int num9 = 0;
            int num10 = 0;
            DateTime datetime1 = new DateTime((long)xmin);
            DateTime datetime2 = new DateTime((long)xmax);
            int num11 = !(datetime1 < bs.FirstDateTime) ? bs.GetIndex(datetime1, IndexOption.Prev) : 0;
            int num12 = !(datetime2 > bs.LastDateTime) ? bs.GetIndex(datetime2, IndexOption.Next) : bs.Count - 1;
            if (num11 == -1 || num12 == -1)
                return;
            for (int index = num11; index <= num12; ++index)
            {
                Bar bar = bs[index];
                long num13 = bar.OpenDateTime.Ticks;
                long num14 = bar.CloseDateTime.Ticks;
                double num15 = (double)(num13 + (num14 - num13) / 2L);
                int num16 = pad.ClientX(num15);
                double high = bar.High;
                double low = bar.Low;
                double open = bar.Open;
                double close = bar.Close;
                Pen pen5 = pen2;
                switch (this.ChartStyle)
                {
                    case ChartStyle.Bar:
                        switch (this.BarWidthStyle)
                        {
                            case EWidthStyle.Pixel:
                                num6 = pad.ClientX(num15) - this.BarWidth / 2;
                                x2 = pad.ClientX(num15) + this.BarWidth / 2;
                                break;
                            case EWidthStyle.DateTime:
                                num6 = pad.ClientX(num15 - (double)((long)this.BarWidth * 10000000L / 2L));
                                x2 = pad.ClientX(num15 + (double)((long)this.BarWidth * 10000000L / 2L));
                                break;
                            case EWidthStyle.Auto:
                                num6 = pad.ClientX((double)num13);
                                x2 = pad.ClientX((double)num14);
                                break;
                        }
                        pad.Graphics.DrawLine(pen5, num16, pad.ClientY(low), num16, pad.ClientY(high));
                        if (open != 0.0)
                            pad.Graphics.DrawLine(pen5, num16, pad.ClientY(open), num6, pad.ClientY(open));
                        if (close != 0.0)
                        {
                            pad.Graphics.DrawLine(pen5, num16, pad.ClientY(close), x2, pad.ClientY(close));
                            break;
                        }
                        else
                            break;
                    case ChartStyle.Line:
                        double num17 = close;
                        if (num1 != 0)
                        {
                            num13 = (long)pad.ClientX(num2);
                            num4 = pad.ClientY(num3);
                            num14 = (long)pad.ClientX(num15);
                            num5 = pad.ClientY(num17);
                            if ((pad.IsInRange(num15, num17) || pad.IsInRange(num2, num3)) && (num13 != num7 || num14 != num8 || (num4 != num9 || num5 != num10)))
                                pad.Graphics.DrawLine(pen1, (float)num13, (float)num4, (float)num14, (float)num5);
                        }
                        num7 = num13;
                        num9 = num4;
                        num8 = num14;
                        num10 = num5;
                        num2 = num15;
                        num3 = num17;
                        ++num1;
                        break;
                    case ChartStyle.Candle:
                        switch (this.CandleWidthStyle)
                        {
                            case EWidthStyle.Pixel:
                                num6 = pad.ClientX(num15) - this.CandleWidth / 2;
                                x2 = pad.ClientX(num15) + this.CandleWidth / 2;
                                break;
                            case EWidthStyle.DateTime:
                                num6 = pad.ClientX(num15 - (double)((long)this.CandleWidth * 10000000L / 2L));
                                x2 = pad.ClientX(num15 + (double)((long)this.CandleWidth * 10000000L / 2L));
                                break;
                            case EWidthStyle.Auto:
                                num6 = pad.ClientX((double)num13);
                                x2 = pad.ClientX((double)num14);
                                break;
                        }
                        pad.Graphics.DrawLine(pen3, num16, pad.ClientY(low), num16, pad.ClientY(high));
                        if (open != 0.0 && close != 0.0)
                        {
                            if (open > close)
                            {
                                int width = x2 - num6;
                                int height = pad.ClientY(close) - pad.ClientY(open);
                                if (height == 0)
                                    height = 1;
                                pad.Graphics.FillRectangle(brush2, num6, pad.ClientY(open), width, height);
                                break;
                            }
                            else
                            {
                                int width = x2 - num6;
                                int height = pad.ClientY(open) - pad.ClientY(close);
                                if (height == 0)
                                    height = 1;
                                if (pad.ForeColor == this.CandleWhiteColor)
                                {
                                    pad.Graphics.DrawRectangle(pen4, num6, pad.ClientY(close), width, height);
                                    pad.Graphics.FillRectangle(brush1, num6 + 1, pad.ClientY(close) + 1, width - 2, height - 1);
                                    break;
                                }
                                else
                                {
                                    pad.Graphics.FillRectangle(brush1, num6, pad.ClientY(close), width, height);
                                    break;
                                }
                            }
                        }
                        else
                            break;
                }
            }
        }
    }
}
