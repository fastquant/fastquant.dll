// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

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

        public TText(string text, double x, double y)
            : this(text, x, y, Color.Black)
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

        public TText(string text, DateTime x, double y)
            : this(text, x.Ticks, y, Color.Black)
        {
        }

        public TText(string text, DateTime x, double y, Color color)
            : this(text, x.Ticks, y, color)
        {
        }

        public virtual void Draw()
        {
            if (Chart.Pad == null)
            {
                Canvas canvas = new Canvas("Canvas", "Canvas");
            }
            Chart.Pad.Add((object)this);
        }

        public void Paint(Pad pad, double minX, double maxX, double minY, double maxY)
        {
            if (this.fText == null)
                return;
            int num1 = (int)pad.Graphics.MeasureString(this.fText, this.fFont).Width;
            double num2 = (double)pad.Graphics.MeasureString(this.fText, this.fFont).Height;
            switch (this.fPosition)
            {
                case ETextPosition.RightBottom:
                    pad.Graphics.DrawString(this.fText, this.fFont, (Brush)new SolidBrush(this.fColor), (float)pad.ClientX(this.fX), (float)pad.ClientY(this.fY));
                    break;
                case ETextPosition.LeftBottom:
                    pad.Graphics.DrawString(this.fText, this.fFont, (Brush)new SolidBrush(this.fColor), (float)(pad.ClientX(this.fX) - num1), (float)pad.ClientY(this.fY));
                    break;
                case ETextPosition.CentreBottom:
                    pad.Graphics.DrawString(this.fText, this.fFont, (Brush)new SolidBrush(this.fColor), (float)(pad.ClientX(this.fX) - num1 / 2), (float)pad.ClientY(this.fY));
                    break;
            }
        }

        public TDistance Distance(double X, double Y)
        {
            return null;
        }
    }
}
