// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections.Generic;
using System.Drawing;

namespace SmartQuant.FinChart.Objects
{
    public class DrawingPath : IUpdatable
    {
        private int wigth = 1;
        private Color color;
        private bool rangeY;

        public string Name { get; }

        public List<DrawingPoint> Points { get; } = new List<DrawingPoint>();

        public bool RangeY
        {
            get
            {
                return this.rangeY;
            }
            set
            {
                this.rangeY = value;
                EmitUpdated();
            }
        }

        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
                EmitUpdated();
            }
        }

        public int Width
        {
            get
            {
                return this.wigth;
            }
            set
            {
                this.wigth = value;
                EmitUpdated();
            }
        }



        public event EventHandler Updated;

        public DrawingPath(string name)
        {
            Name = name;
        }

        public void Add(DateTime x, double y)
        {
            Points.Add(new DrawingPoint(x, y));
            EmitUpdated();
        }

        public void RemoveAt(int index)
        {
            Points.RemoveAt(index);
            EmitUpdated();
        }

        public void Insert(int index, DateTime x, double y)
        {
            Points.Insert(index, new DrawingPoint(x, y));
            EmitUpdated();
        }

        private void EmitUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    }
}
