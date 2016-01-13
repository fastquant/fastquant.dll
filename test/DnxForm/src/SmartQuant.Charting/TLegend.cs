// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;

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

        public TLegendItem(string text, Color color)
            : this(text, color, new Font("Arial", 8f))
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
                this.width = 0;
                foreach (TLegendItem item in Items)
                {
                    int num = (int)Pad.Graphics.MeasureString(item.Text, item.Font).Width;
                    if (num > this.width)
                        this.width = num;
                }
                this.width += 12;
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
                this.height = 0;
                foreach (TLegendItem item in Items)
                    this.height += (int)Pad.Graphics.MeasureString(item.Text, item.Font).Height + 2;
                this.height += 2;
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public bool BorderEnabled { get; set; }

        public Color BorderColor { get; set; }

        public Color BackColor { get; set; }

        public ArrayList Items { get; private set; }

        public TLegend(Pad pad)
        {
            Pad = pad;
            BorderEnabled = true;
            BorderColor = Color.Black;
            BackColor = Color.LightYellow;
            Items = new ArrayList();
        }

        public void Add(string text, Color color)
        {
            Items.Add(new TLegendItem(text, color));
        }

        public void Add(string text, Color color, Font font)
        {
            Items.Add(new TLegendItem(text, color, font));
        }

        public void Add(TLegendItem item)
        {
            Items.Add(item);
        }

        public virtual void Paint()
        {
            Pad.Graphics.FillRectangle(new SolidBrush(BackColor), X, Y, Width, Height);
            if (BorderEnabled)
                Pad.Graphics.DrawRectangle(new Pen(BorderColor), X, Y, Width, Height);
            int x1 = X + 5;
            int num1 = Y + 2;
            foreach (TLegendItem item in Items)
            {
                int num2 = (int)Pad.Graphics.MeasureString(item.Text, item.Font).Height;
                Pad.Graphics.DrawLine(new Pen(item.Color), x1, num1 + num2 / 2, x1 + 5, num1 + num2 / 2);
                Pad.Graphics.DrawString(item.Text, item.Font, new SolidBrush(Color.Black), x1 + 5 + 2, num1);
                num1 += 2 + num2;
            }
        }
    }
}
