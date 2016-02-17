using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace SmartQuant.Charting
{
    public class TTextBoxItem
    {
        public string Text { get; set; }

        public Color Color { get; set; }

        public Font Font { get; set; }

        public TTextBoxItem(string text, Color color, Font font)
        {
            Text = text;
            Color = color;
            Font = font;
        }

        public TTextBoxItem(string text, Color color)
            : this(text, color, new Font("Arial", 8f))
        {
        }
    }

    [Serializable]
    public class TTextBox : IDrawable
    {
        [Category("ToolTip")]
        [Description("")]
        public bool ToolTipEnabled { get; set; }

        [Category("ToolTip")]
        [Description("")]
        public string ToolTipFormat { get; set; }

        public ETextBoxPosition Position { get; set; } = ETextBoxPosition.TopRight;

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; private set; } = -1;

        public int Height { get; private set; } = -1;

        public bool BorderEnabled { get; set; } = true;

        public Color BorderColor { get; set; } = Color.Black;

        public Color BackColor { get; set; } = Color.LightYellow;

        public ArrayList Items { get; } = new ArrayList();

        public TTextBox() : this(10, 10)
        {
        }

        public TTextBox(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Add(string text, Color color) => Items.Add(new TTextBoxItem(text, color));

        public void Add(string text, Color color, Font font) => Items.Add(new TTextBoxItem(text, color, font));

        public void Add(TTextBoxItem item) => Items.Add(item);

        public void Clear() => Items.Clear();

        public virtual void Draw()
        {
            if (Chart.Pad == null)
                new Canvas("Canvas", "Canvas");
            Chart.Pad.Add(this);
        }

        private float GetWidth(Pad pad)
        {
            Width = Items.Cast<TTextBoxItem>().Max(i => (int)pad.Graphics.MeasureString(i.Text, i.Font).Width);
            Width += 12;
            return Width;
        }

        private float GetHeight(Pad pad)
        {
            Height = Items.Cast<TTextBoxItem>().Sum(i => (int) pad.Graphics.MeasureString(i.Text, i.Font).Height + 2);
            //foreach (TTextBoxItem ttextBoxItem in Items)
            //    Height += (int)pad.Graphics.MeasureString(ttextBoxItem.Text, ttextBoxItem.Font).Height + 2;
            Height += 2;
            return Height;
        }

        public virtual void Paint(Pad pad, double minX, double maxX, double minY, double maxY)
        {
            float height = GetHeight(pad);
            float width = GetWidth(pad);
            float x = 0;
            float y = 0;
            switch (Position)
            {
                case ETextBoxPosition.TopRight:
                    x = pad.ClientX() + pad.ClientWidth() - X - width;
                    y = pad.ClientY() + Y;
                    break;
                case ETextBoxPosition.TopLeft:
                    x = pad.ClientX() + X;
                    y = pad.ClientY() + Y;
                    break;
                case ETextBoxPosition.BottomRight:
                    x = pad.ClientX() + pad.ClientWidth() - X - width;
                    y = pad.ClientY() + pad.ClientHeight() - Y - height;
                    break;
                case ETextBoxPosition.BottomLeft:
                    x = pad.ClientX() + X;
                    y = pad.ClientY() + pad.ClientHeight() - Y - height;
                    break;
            }
            pad.Graphics.FillRectangle(new SolidBrush(BackColor), x, y, width, height);
            if (BorderEnabled)
                pad.Graphics.DrawRectangle(new Pen(BorderColor), x, y, width, height);
            foreach (TTextBoxItem item in Items)
            {
                var h = pad.Graphics.MeasureString(item.Text, item.Font).Height;
                pad.Graphics.DrawString(item.Text, item.Font, new SolidBrush(item.Color), x + 5, y);
                y += 2 + h;
            }
        }

        public TDistance Distance(double x, double y) => null;
    }
}
