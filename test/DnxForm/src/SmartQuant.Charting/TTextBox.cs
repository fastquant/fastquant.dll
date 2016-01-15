// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

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

        public ETextBoxPosition Position { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool BorderEnabled { get; set; }

        public Color BorderColor { get; set; }

        public Color BackColor { get; set; }

        public ArrayList Items { get; private set; }

        public TTextBox() : this(10, 10)
        {
        }

        public TTextBox(int x, int y)
        {
            X = x;
            Y = y;
            Width = -1;
            Height = -1;
            Position = ETextBoxPosition.TopRight;
            BorderEnabled = true;
            BorderColor = Color.Black;
            BackColor = Color.LightYellow;
            Items = new ArrayList();
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
            Width = 0;
            foreach (TTextBoxItem item in Items)
            {
                int num = (int)pad.Graphics.MeasureString(item.Text, item.Font).Width;
                if (num > Width)
                    Width = num;
            }
            Width += 12;
            return (float)Width;
        }

        private float GetHeight(Pad pad)
        {
            Height = 0;
            foreach (TTextBoxItem ttextBoxItem in Items)
                Height += (int)pad.Graphics.MeasureString(ttextBoxItem.Text, ttextBoxItem.Font).Height + 2;
            Height += 2;
            return (float)Height;
        }

        public virtual void Paint(Pad pad, double minX, double maxX, double minY, double maxY)
        {
            float height = GetHeight(pad);
            float width = GetWidth(pad);
            float x = 0f;
            float y = 0f;
            switch (Position)
            {
                case ETextBoxPosition.TopRight:
                    x = (float)(pad.ClientX() + pad.ClientWidth() - X) - width;
                    y = (float)(pad.ClientY() + Y);
                    break;
                case ETextBoxPosition.TopLeft:
                    x = (float)(pad.ClientX() + X);
                    y = (float)(pad.ClientY() + Y);
                    break;
                case ETextBoxPosition.BottomRight:
                    x = (float)(pad.ClientX() + pad.ClientWidth() - X) - width;
                    y = (float)(pad.ClientY() + pad.ClientHeight() - Y) - height;
                    break;
                case ETextBoxPosition.BottomLeft:
                    x = (float)(pad.ClientX() + X);
                    y = (float)(pad.ClientY() + pad.ClientHeight() - Y) - height;
                    break;
            }
            pad.Graphics.FillRectangle((Brush)new SolidBrush(BackColor), x, y, width, height);
            if (BorderEnabled)
                pad.Graphics.DrawRectangle(new Pen(BorderColor), x, y, width, height);
            foreach (TTextBoxItem ttextBoxItem in Items)
            {
                int num = (int)pad.Graphics.MeasureString(ttextBoxItem.Text, ttextBoxItem.Font).Height;
                pad.Graphics.DrawString(ttextBoxItem.Text, ttextBoxItem.Font, (Brush)new SolidBrush(ttextBoxItem.Color), x + 5f, y);
                y += (float)(2 + num);
            }
        }

        public TDistance Distance(double x, double y) => null;
    }
}
