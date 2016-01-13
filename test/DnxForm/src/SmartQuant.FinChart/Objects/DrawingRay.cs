// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Drawing;

namespace SmartQuant.FinChart.Objects
{
    public class DrawingRay : IUpdatable
    {
        private int wigth = 1;
        private DateTime x;
        private double y;
        private Color color;
        private bool rangeY;

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

        public string Name { get; private set; }

        public DateTime X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
                EmitUpdated();
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
                EmitUpdated();
            }
        }

        public event EventHandler Updated;

        public DrawingRay(DateTime x, double y, string name)
        {
            Name = name;
            this.x = x;
            this.y = y;
        }

        private void EmitUpdated()
        {
            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }
    }
}
