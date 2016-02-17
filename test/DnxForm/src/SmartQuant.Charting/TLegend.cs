using System;
using System.Collections;
using System.Drawing;
using System.Linq;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TLegendItem
    {
        public string Text { get; set; }

        public Color Color { get; set; }

        public Font Font { get; set; }

        public TLegendItem(string text, Color color, Font font)
        {
            Text = text;
            Color = color;
            Font = font;
        }

        public TLegendItem(string text, Color color) : this(text, color, new Font("Arial", 8))
        {
        }
    }

    public class TLegend
    {
        private int width;
        private int height;

        public Pad Pad { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width
        {
            get
            {
                this.width = Items.Count > 0
                    ? (int) Items.Cast<TLegendItem>().Max(item => Pad.Graphics.MeasureString(item.Text, item.Font).Width) + 12
                    : 12;
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }

        public int Height
        {
            get
            {
                this.height = Items.Count > 0
                    ? (int)Items.Cast<TLegendItem>().Sum(item => Pad.Graphics.MeasureString(item.Text, item.Font).Height + 2) + 2
                    : 2;
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public bool BorderEnabled { get; set; } = true;

        public Color BorderColor { get; set; } = Color.Black;

        public Color BackColor { get; set; } = Color.LightYellow;

        public ArrayList Items { get; } = new ArrayList();

        public TLegend(Pad pad)
        {
            Pad = pad;
        }

        public void Add(string text, Color color) => Items.Add(new TLegendItem(text, color));

        public void Add(string text, Color color, Font font) => Items.Add(new TLegendItem(text, color, font));

        public void Add(TLegendItem item) => Items.Add(item);

        public virtual void Paint()
        {
            Pad.Graphics.FillRectangle(new SolidBrush(BackColor), X, Y, Width, Height);
            if (BorderEnabled)
                Pad.Graphics.DrawRectangle(new Pen(BorderColor), X, Y, Width, Height);
            var x = X + 5;
            var y = Y + 2;
            foreach (TLegendItem item in Items)
            {
                var h = (int)Pad.Graphics.MeasureString(item.Text, item.Font).Height;
                Pad.Graphics.DrawLine(new Pen(item.Color), x, y + h / 2, x + 5, y + h / 2);
                Pad.Graphics.DrawString(item.Text, item.Font, new SolidBrush(Color.Black), x + 5 + 2, y);
                y += 2 + h;
            }
        }
    }
}
