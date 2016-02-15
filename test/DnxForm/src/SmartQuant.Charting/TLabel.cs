// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TLabel : TMarker
    {
        [Category("Text")]
        [Description("Text that this label displays")]
        public new string Text { get; set; }

        [Description("Text position of this label")]
        [Category("Text")]
        public new ETextPosition TextPosition { get; set; }

        [Category("Text")]
        [Description("Text font of this label")]
        public new Font TextFont { get; set; }

        [Category("Text")]
        [Description("Text color of this label")]
        public new Color TextColor { get; set; }

        [Category("Text")]
        [Description("Text offset in pixels alone X coordinate")]
        public int TextOffsetX { get; set; }

        [Category("Text")]
        [Description("Text offset in pixels alone Y coordinate")]
        public int TextOffsetY { get; set; }

        public TLabel(string text, double x, double y)
            : this(text, x, y, default(Color), Color.Black)
        {
        }

        public TLabel(string text, double x, double y, Color markerColor)
            : this(text, x, y, markerColor, Color.Black)
        {
        }

        public TLabel(string text, double x, double y, Color markerColor, Color textColor)
            : base(x, y, markerColor)
        {
            Text = text;
            TextFont = new Font("Arial", 8f);
            TextPosition = ETextPosition.RightBottom;
            TextColor = Color.Black;
            TextOffsetX = 0;
            TextOffsetY = 2;
            TextColor = textColor;
        }

        public override void Paint(Pad pad, double minX, double maxX, double minY, double maxY)
        {
            base.Paint(pad, minX, maxX, minY, maxY);
            if (string.IsNullOrWhiteSpace(Text))
                return;
            var size = pad.Graphics.MeasureString(Text, TextFont);
            float w = size.Width;
            float h = size.Height;
            float clientX = pad.ClientX(X);
            float clientY = pad.ClientY(Y);
            PointF point = PointF.Empty;
            switch (TextPosition)
            {
                case ETextPosition.RightTop:
                    point = new PointF(clientX + TextOffsetX, clientY - h - TextOffsetY);
                    break;
                case ETextPosition.LeftTop:
                    point = new PointF(clientX - w - TextOffsetX, clientY - h - TextOffsetY);
                    break;
                case ETextPosition.CentreTop:
                    point = new PointF(clientX - w / 2 - TextOffsetX, clientY - h - TextOffsetY);
                    break;
                case ETextPosition.RightBottom:
                    point = new PointF(clientX + TextOffsetX, clientY + Size / 2 + TextOffsetY);
                    break;
                case ETextPosition.LeftBottom:
                    point = new PointF(clientX - w - TextOffsetX, clientY + Size / 2 + TextOffsetY);
                    break;
                case ETextPosition.CentreBottom:
                    point = new PointF(clientX - w / 2 - TextOffsetX, clientY + Size / 2 + TextOffsetY);
                    break;
            }
            pad.Graphics.DrawString(Text, TextFont, new SolidBrush(TextColor), point.X, point.Y);
        }
    }
}
