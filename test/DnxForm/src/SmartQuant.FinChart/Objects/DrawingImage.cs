// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Drawing;

namespace SmartQuant.FinChart.Objects
{
    public class DrawingImage : IUpdatable
    {
        private DateTime x;
        private double y;
        private Image image;

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

        public Image Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
                EmitUpdated();
            }
        }

        public event EventHandler Updated;

        public DrawingImage(DateTime x, double y, Image image, string name)
        {
            Name = name;
            this.x = x;
            this.y = y;
            this.image = image;
        }

        private void EmitUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    }
}
