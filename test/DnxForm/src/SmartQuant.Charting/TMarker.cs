// Decompiled with JetBrains decompiler
// Type: SmartQuant.Charting.TMarker
// Assembly: SmartQuant.Charting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=23953e483e363d68
// MVID: F3B55EE9-4DBA-4875-B18A-7BD8DFCF4D88
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Charting.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TMarker : IDrawable, IMovable
    {
        protected Color fBuyColor = Color.Blue;
        protected Color fSellColor = Color.Red;
        protected Color fSellShortColor = Color.Yellow;
        protected Color fBuyShortColor = Color.Green;
        protected double fX;
        protected double fY;
        protected double fZ;
        protected double fHigh;
        protected double fLow;
        protected double fOpen;
        protected double fClose;
        protected EMarkerStyle fStyle;
        protected Color fColor;
        protected int fSize;
        protected bool fFilled;
        [NonSerialized]
        protected string fText;
        [NonSerialized]
        protected bool fTextEnabled;
        [NonSerialized]
        protected EMarkerTextPosition fTextPosition;
        [NonSerialized]
        protected int fTextOffset;
        [NonSerialized]
        protected Color fTextColor;
        [NonSerialized]
        protected Font fTextFont;
        protected bool fToolTipEnabled;
        protected string fToolTipFormat;

        public Color BuyColor
        {
            get
            {
                return this.fBuyColor;
            }
            set
            {
                this.fBuyColor = value;
            }
        }

        public Color SellColor
        {
            get
            {
                return this.fSellColor;
            }
            set
            {
                this.fSellColor = value;
            }
        }

        public Color BuyShortColor
        {
            get
            {
                return this.fBuyShortColor;
            }
            set
            {
                this.fBuyShortColor = value;
            }
        }

        public Color SellShortColor
        {
            get
            {
                return this.fSellShortColor;
            }
            set
            {
                this.fSellShortColor = value;
            }
        }

        [Description("Enable or disable tooltip appearance for this marker.")]
        [Category("ToolTip")]
        public bool ToolTipEnabled
        {
            get
            {
                return this.fToolTipEnabled;
            }
            set
            {
                this.fToolTipEnabled = value;
            }
        }

        [Category("ToolTip")]
        [Description("Tooltip format string. {1} - X coordinate, {2} - Y coordinte.")]
        public string ToolTipFormat
        {
            get
            {
                return this.fToolTipFormat;
            }
            set
            {
                this.fToolTipFormat = value;
            }
        }

        [Category("Position")]
        [Description("X position of this marker on the pad. (World coordinate system)")]
        public double X
        {
            get
            {
                return this.fX;
            }
            set
            {
                this.fX = value;
            }
        }

        [Category("Position")]
        [Description("Y position of this marker on the pad. (World coordinate system)")]
        public double Y
        {
            get
            {
                return this.fY;
            }
            set
            {
                this.fY = value;
            }
        }

        [Browsable(false)]
        public double Z
        {
            get
            {
                return this.fZ;
            }
            set
            {
                this.fZ = value;
            }
        }

        [Description("Marker style")]
        [Category("Marker")]
        public EMarkerStyle Style
        {
            get
            {
                return this.fStyle;
            }
            set
            {
                this.fStyle = value;
            }
        }

        [Description("Marker color")]
        [Category("Marker")]
        public Color Color
        {
            get
            {
                return this.fColor;
            }
            set
            {
                this.fColor = value;
            }
        }

        [Description("Marker size in pixels")]
        [Category("Marker")]
        public int Size
        {
            get
            {
                return this.fSize;
            }
            set
            {
                this.fSize = value;
            }
        }

        [Description("Marker interior will be filled if this propery is set to true, otherwise it will be transparent")]
        [Category("Marker")]
        public bool Filled
        {
            get
            {
                return this.fFilled;
            }
            set
            {
                this.fFilled = value;
            }
        }

        [Description("High of bar marker")]
        [Category("Value")]
        public double High
        {
            get
            {
                return this.fHigh;
            }
            set
            {
                this.fHigh = value;
            }
        }

        [Category("Value")]
        [Description("Low of bar marker")]
        public double Low
        {
            get
            {
                return this.fLow;
            }
            set
            {
                this.fLow = value;
            }
        }

        [Description("Open of bar marker")]
        [Category("Value")]
        public double Open
        {
            get
            {
                return this.fOpen;
            }
            set
            {
                this.fOpen = value;
            }
        }

        [Category("Value")]
        [Description("Close of bar marker")]
        public double Close
        {
            get
            {
                return this.fClose;
            }
            set
            {
                this.fClose = value;
            }
        }

        public string Text
        {
            get
            {
                return this.fText;
            }
            set
            {
                this.fText = value;
            }
        }

        public bool TextEnabled
        {
            get
            {
                return this.fTextEnabled;
            }
            set
            {
                this.fTextEnabled = value;
            }
        }

        public EMarkerTextPosition TextPosition
        {
            get
            {
                return this.fTextPosition;
            }
            set
            {
                this.fTextPosition = value;
            }
        }

        public int TextOffset
        {
            get
            {
                return this.fTextOffset;
            }
            set
            {
                this.fTextOffset = value;
            }
        }

        public Color TextColor
        {
            get
            {
                return this.fTextColor;
            }
            set
            {
                this.fTextColor = value;
            }
        }

        public Font TextFont
        {
            get
            {
                return this.fTextFont;
            }
            set
            {
                this.fTextFont = value;
            }
        }

        public TMarker(double x, double y)
            : this(x, y, 0, Color.Black)
        {
        }

        public TMarker(DateTime x, double y)
            : this(x.Ticks, y)
        {
        }

        public TMarker(string x, double y)
            : this(DateTime.Parse(x).Ticks, y)
        {
        }

        public TMarker(double x, double y, Color color)
            : this(x, y, 0, color)
        {
        }

        public TMarker(double x, double y, EMarkerStyle style)
            : this(x, y, 0, Color.Black)
        {
        }

        public TMarker(DateTime x, double y, EMarkerStyle style)
            : this(x.Ticks, y, 0, Color.Black)
        {
        }

        public TMarker(string x, double y, EMarkerStyle style)
            : this(DateTime.Parse(x).Ticks, y, 0, Color.Black)
        {

        }

        public TMarker(double x, double y, double z)
            : this(x, y, z, Color.Black)
        {
        }

        public TMarker(double x, double y, double z, Color color)
            : this(x, y, z, EMarkerStyle.Rectangle, 0, 0, 0, 0, Color.Black)
        {
        }

        public TMarker(double x, double high, double low, double open, double close)
            : this(x, high, low, open, close, Color.Black)
        {
        }

        public TMarker(double x, double high, double low, double open, double close, Color color)
            : this(x, 0, 0, EMarkerStyle.Rectangle, high, low, open, close, color)
        {
        }

        private TMarker(double x, double y, double z, EMarkerStyle style, double high, double low, double open, double close, Color color)
        {
            X = x;
            Y = y;
            Z = z;
            High = high;
            Low = low;
            Open = open;
            Close = close;
            Style = style;
            Color = color;
            Size = (Style == EMarkerStyle.Buy || Style == EMarkerStyle.Sell || Style == EMarkerStyle.SellShort || Style == EMarkerStyle.BuyShort) ? 10 : 5;
            Filled = true;
            TextEnabled = true;
            TextOffset = 2;
            TextPosition = EMarkerTextPosition.Bottom;
            TextFont = new Font("Arial", 8f);
            TextColor = Color.Black;
            ToolTipEnabled = true;
            ToolTipFormat = "X = {0:F2} Y = {1:F2}";
        }

        public virtual void Draw()
        {
            if (Chart.Pad == null)
            {
                var canvas = new Canvas("Canvas", "Canvas");
            }
            Chart.Pad.Add(this);
        }

        public virtual void Paint(Pad pad, double xMin, double xMax, double yMin, double yMax)
        {
            int num1 = pad.ClientX(this.fX);
            int y = pad.ClientY(this.fY);
            float num2 = (float)this.fSize;
            switch (this.fStyle)
            {
                case EMarkerStyle.Rectangle:
                    if (this.fFilled)
                    {
                        pad.Graphics.FillRectangle((Brush)new SolidBrush(this.fColor), (float)num1 - num2 / 2f, (float)y - num2 / 2f, num2, num2);
                        break;
                    }
                    else
                    {
                        Pen pen = new Pen(this.fColor);
                        pad.Graphics.DrawRectangle(pen, (float)num1 - num2 / 2f, (float)y - num2 / 2f, num2, num2);
                        break;
                    }
                case EMarkerStyle.Triangle:
                    float num3 = (float)((double)num2 / 2.0 * Math.Tan(Math.PI / 6.0));
                    float num4 = num2 * (float)Math.Cos(Math.PI / 6.0) - num3;
                    PointF pointF1 = new PointF((float)num1, (float)y - num4);
                    PointF pointF2 = new PointF((float)num1 - num2 / 2f, (float)y + num3);
                    PointF pointF3 = new PointF((float)num1 + num2 / 2f, (float)y + num3);
                    PointF[] points1 = new PointF[4]
                    {
                        pointF1,
                        pointF2,
                        pointF3,
                        pointF1
                    };
                    if (this.fFilled)
                    {
                        pad.Graphics.FillPolygon((Brush)new SolidBrush(this.fColor), points1);
                        break;
                    }
                    else
                    {
                        Pen pen = new Pen(this.fColor);
                        pad.Graphics.DrawLines(pen, points1);
                        break;
                    }
                case EMarkerStyle.Circle:
                    if (this.fFilled)
                    {
                        pad.Graphics.FillEllipse((Brush)new SolidBrush(this.fColor), (float)num1 - num2 / 2f, (float)y - num2 / 2f, num2, num2);
                        break;
                    }
                    else
                    {
                        Pen pen = new Pen(this.fColor);
                        pad.Graphics.DrawEllipse(pen, (float)num1 - num2 / 2f, (float)y - num2 / 2f, num2, num2);
                        break;
                    }
                case EMarkerStyle.Bar:
                    Pen pen1 = new Pen(this.fColor);
                    pad.Graphics.DrawLine(pen1, num1, pad.ClientY(this.fLow), pad.ClientX(this.fX), pad.ClientY(this.fHigh));
                    pad.Graphics.DrawLine(pen1, num1, pad.ClientY(this.fLow), pad.ClientX(this.fX) - 3, pad.ClientY(this.fLow));
                    pad.Graphics.DrawLine(pen1, num1, pad.ClientY(this.fHigh), pad.ClientX(this.fX) + 3, pad.ClientY(this.fHigh));
                    break;
                case EMarkerStyle.Buy:
                    PointF[] points2 = new PointF[3]
                    {
                        (PointF)new Point(num1, y),
                        (PointF)new Point((int)((double)num1 - (double)num2 / 2.0), (int)((double)y + (double)num2)),
                        (PointF)new Point((int)((double)num1 + (double)num2 / 2.0), (int)((double)y + (double)num2))
                    };
                    pad.Graphics.FillPolygon((Brush)new SolidBrush(this.fBuyColor), points2);
                    break;
                case EMarkerStyle.Sell:
                    Point[] points3 = new Point[3]
                    {
                        new Point(num1, y),
                        new Point((int)((double)num1 - (double)num2 / 2.0), (int)((double)y - (double)num2)),
                        new Point((int)((double)num1 + (double)num2 / 2.0), (int)((double)y - (double)num2))
                    };
                    pad.Graphics.FillPolygon((Brush)new SolidBrush(this.fSellColor), points3);
                    break;
                case EMarkerStyle.SellShort:
                    Point[] points4 = new Point[3]
                    {
                        new Point(num1, y),
                        new Point((int)((double)num1 - (double)num2 / 2.0), (int)((double)y - (double)num2)),
                        new Point((int)((double)num1 + (double)num2 / 2.0), (int)((double)y - (double)num2))
                    };
                    pad.Graphics.FillPolygon((Brush)new SolidBrush(this.fSellShortColor), points4);
                    break;
                case EMarkerStyle.BuyShort:
                    Point[] points5 = new Point[3]
                    {
                        new Point(num1, y),
                        new Point((int)((double)num1 - (double)num2 / 2.0), (int)((double)y + (double)num2)),
                        new Point((int)((double)num1 + (double)num2 / 2.0), (int)((double)y + (double)num2))
                    };
                    pad.Graphics.FillPolygon((Brush)new SolidBrush(this.fBuyShortColor), points5);
                    break;
                case EMarkerStyle.Plus:
                    Pen pen2 = new Pen(this.fColor);
                    pad.Graphics.DrawLine(pen2, (float)num1 - num2 / 2f, (float)y, (float)num1 + num2 / 2f, (float)y);
                    pad.Graphics.DrawLine(pen2, (float)num1, (float)y - num2 / 2f, (float)num1, (float)y + num2 / 2f);
                    break;
                case EMarkerStyle.Cross:
                    Pen pen3 = new Pen(this.fColor);
                    pad.Graphics.DrawLine(pen3, (float)num1 - num2 / 2f, (float)y - num2 / 2f, (float)num1 + num2 / 2f, (float)y + num2 / 2f);
                    pad.Graphics.DrawLine(pen3, (float)num1 - num2 / 2f, (float)y + num2 / 2f, (float)num1 + num2 / 2f, (float)y - num2 / 2f);
                    break;
            }
            if (!this.fTextEnabled || this.fText == null || !(this.fText != ""))
                return;
            int num5 = (int)pad.Graphics.MeasureString(this.fText, this.fTextFont).Width;
            int num6 = (int)pad.Graphics.MeasureString(this.fText, this.fTextFont).Height;
            switch (this.fStyle)
            {
                case EMarkerStyle.Buy:
                    pad.Graphics.DrawString(this.fText, this.fTextFont, (Brush)new SolidBrush(this.fTextColor), (float)(pad.ClientX(this.fX) - num5 / 2), (float)pad.ClientY(this.fY) + num2 + (float)this.fTextOffset);
                    break;
                case EMarkerStyle.Sell:
                    pad.Graphics.DrawString(this.fText, this.fTextFont, (Brush)new SolidBrush(this.fTextColor), (float)(pad.ClientX(this.fX) - num5 / 2), (float)pad.ClientY(this.fY) - num2 - (float)this.fTextOffset - (float)num6);
                    break;
                case EMarkerStyle.SellShort:
                    pad.Graphics.DrawString(this.fText, this.fTextFont, (Brush)new SolidBrush(this.fTextColor), (float)(pad.ClientX(this.fX) - num5 / 2), (float)pad.ClientY(this.fY) + num2 + (float)this.fTextOffset);
                    break;
                case EMarkerStyle.BuyShort:
                    pad.Graphics.DrawString(this.fText, this.fTextFont, (Brush)new SolidBrush(this.fTextColor), (float)(pad.ClientX(this.fX) - num5 / 2), (float)pad.ClientY(this.fY) + num2 + (float)this.fTextOffset);
                    break;
            }
        }

        public virtual TDistance Distance(double x, double y)
        {
            TDistance d = new TDistance();
            d.X = X;
            d.Y = Y;
            d.dX = Math.Abs(x - X);
            d.dY = Math.Abs(y - Y);
            d.ToolTipText = string.Format(ToolTipFormat, X, Y);
            return d;
        }

        public void Move(double x, double y, double dX, double dY)
        {
            X += dX;
            Y += dY;
        }
    }
}
