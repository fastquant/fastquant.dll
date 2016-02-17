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
        public new ETextPosition TextPosition { get; set; } = ETextPosition.RightBottom;

        [Category("Text")]
        [Description("Text font of this label")]
        public new Font TextFont { get; set; } = new Font("Arial", 8f);

        [Category("Text")]
        [Description("Text color of this label")]
        public new Color TextColor { get; set; } = Color.Black;

        [Category("Text")]
        [Description("Text offset in pixels alone X coordinate")]
        public int TextOffsetX { get; set; } = 0;

        [Category("Text")]
        [Description("Text offset in pixels alone Y coordinate")]
        public int TextOffsetY { get; set; } = 2;

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
            TextColor = textColor;
        }

        public override void Paint(Pad pad, double minX, double maxX, double minY, double maxY)
        {
            base.Paint(pad, minX, maxX, minY, maxY);
            if (string.IsNullOrWhiteSpace(Text))
                return;
            var size = pad.Graphics.MeasureString(Text, TextFont);
            var w = size.Width;
            var h = size.Height;
            var clientX = pad.ClientX(X);
            var clientY = pad.ClientY(Y);
            var point = PointF.Empty;
            switch (TextPosition)
            {
                case ETextPosition.RightTop:
                    point = new PointF(clientX + TextOffsetX, clientY - h - TextOffsetY);
                    break;
                case ETextPosition.LeftTop:
                    point = new PointF(clientX - w - TextOffsetX, clientY - h - TextOffsetY);
                    break;
                case ETextPosition.CentreTop:
                    point = new PointF(clientX - w/2 - TextOffsetX, clientY - h - TextOffsetY);
                    break;
                case ETextPosition.RightBottom:
                    point = new PointF(clientX + TextOffsetX, clientY + Size/2 + TextOffsetY);
                    break;
                case ETextPosition.LeftBottom:
                    point = new PointF(clientX - w - TextOffsetX, clientY + Size/2 + TextOffsetY);
                    break;
                case ETextPosition.CentreBottom:
                    point = new PointF(clientX - w/2 - TextOffsetX, clientY + Size/2 + TextOffsetY);
                    break;
            }
            pad.Graphics.DrawString(Text, TextFont, new SolidBrush(TextColor), point.X, point.Y);
        }
    }
}
