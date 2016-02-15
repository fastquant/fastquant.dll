using System;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.Charting
{
    public class TImage : IDrawable, IMovable
    {
        private Image Image { get; }

        [Category("Position")]
        [Description("X position of this image on the pad. (World coordinate system)")]
        public double X { get; set; }

        [Category("Position")]
        [Description("Y position of this image on the pad. (World coordinate system)")]
        public double Y { get; set; }

        [Description("Enable or disable tooltip appearance for this image.")]
        [Category("ToolTip")]
        public bool ToolTipEnabled { get; set; }

        [Category("ToolTip")]
        [Description("Tooltip format string. {1} - X coordinate, {2} - Y coordinte.")]
        public string ToolTipFormat { get; set; }

        public TImage(Image image, double x, double y)
        {
            Image = image;
            X = x;
            Y = y;
            ToolTipEnabled = true;
            ToolTipFormat = "X = {0:F2} Y = {1:F2}";
        }

        public TImage(Image image, DateTime x, double y)
            : this(image, x.Ticks, y)
        {
        }

        public TImage(string fileName, double x, double y)
            : this(Image.FromFile(fileName), x, y)
        {
        }

        public TImage(string fileName, DateTime x, double y)
            : this(Image.FromFile(fileName), x.Ticks, y)
        {
        }

        public virtual void Draw()
        {   
            throw new NotImplementedException();
        }

        public virtual void Paint(Pad pad, double xMin, double xMax, double yMin, double yMax)
        {
            pad.Graphics.DrawImage(Image, pad.ClientX(X), pad.ClientY(Y));
        }

        public TDistance Distance(double x, double y)
        {
            var d = new TDistance
            {
                X = X,
                Y = Y,
                dX = Math.Abs(x - X),
                dY = Math.Abs(y - Y),
                ToolTipText = string.Format(ToolTipFormat, X, Y)
            };
            return d;
        }

        public void Move(double x, double y, double dx, double dy)
        {
            X += dx;
            Y += dy;
        }
    }
}
