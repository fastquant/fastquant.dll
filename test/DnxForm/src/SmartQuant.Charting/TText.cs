using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TText : IDrawable
    {
        protected double fX;
        protected double fY;
        protected double fZ;
        protected string fText;
        protected ETextPosition fPosition;
        protected Font fFont;
        protected Color fColor;

        [Category("ToolTip")]
        [Description("")]
        public bool ToolTipEnabled { get; set; }

        [Category("ToolTip")]
        [Description("")]
        public string ToolTipFormat { get; set; }

        [Description("")]
        [Category("Position")]
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
        [Description("")]
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

        [Category("Text")]
        [Description("")]
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

        [Description("")]
        [Category("Text")]
        public ETextPosition Position
        {
            get
            {
                return this.fPosition;
            }
            set
            {
                this.fPosition = value;
            }
        }

        [Category("Text")]
        [Description("")]
        public Font Font
        {
            get
            {
                return this.fFont;
            }
            set
            {
                this.fFont = value;
            }
        }

        [Description("")]
        [Category("Text")]
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

        public TText(string text, double x, double y) : this(text, x, y, Color.Black)
        {
        }

        public TText(string text, double x, double y, Color color)
        {
            X = x;
            Y = y;
            Z = 0;
            Text = text;
            Position = ETextPosition.RightBottom;
            Font = SystemFonts.DefaultFont;
            Color = color;
        }

        public TText(string text, DateTime x, double y) : this(text, x.Ticks, y, Color.Black)
        {
        }

        public TText(string text, DateTime x, double y, Color color) : this(text, x.Ticks, y, color)
        {
        }

        public virtual void Draw()
        {
            if (Chart.Pad == null)
                new Canvas("Canvas", "Canvas");
            Chart.Pad.Add((object)this);
        }

        public void Paint(Pad pad, double minX, double maxX, double minY, double maxY)
        {
            if (Text == null)
                return;
            var w = (int) pad.Graphics.MeasureString(Text, Font).Width;
            var h =  pad.Graphics.MeasureString(Text, Font).Height;
            switch (Position)
            {
                case ETextPosition.RightBottom:
                    pad.Graphics.DrawString(Text, Font, new SolidBrush(Color), pad.ClientX(X), pad.ClientY(Y));
                    break;
                case ETextPosition.LeftBottom:
                    pad.Graphics.DrawString(Text, Font, new SolidBrush(Color), pad.ClientX(X) - w, pad.ClientY(Y));
                    break;
                case ETextPosition.CentreBottom:
                    pad.Graphics.DrawString(Text, Font, new SolidBrush(Color), pad.ClientX(X) - w/2, pad.ClientY(Y));
                    break;
            }
        }

        public TDistance Distance(double x, double y) => null;
    }
}
