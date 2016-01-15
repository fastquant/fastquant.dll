// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using SmartQuant;
using SmartQuant.FinChart;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace SmartQuant.FinChart.Objects
{
    public class RayView : IChartDrawable, IZoomable
    {
        private DrawingRay ray;

        protected bool toolTipEnabled = true;
        protected string toolTipFormat = "";
        protected DateTime firstDate;
        protected DateTime lastDate;
        protected bool selected;
        protected DateTime chartFirstDate;
        protected DateTime chartLastDate;

        public Pad Pad { get; set; }

        [Description("Enable or disable tooltip appearance for this marker.")]
        [Category("ToolTip")]
        public bool ToolTipEnabled
        {
            get
            {
                return this.toolTipEnabled;
            }
            set
            {
                this.toolTipEnabled = value;
            }
        }

        [Category("ToolTip")]
        [Description("Tooltip format string. {1} - X coordinate, {2} - Y coordinte.")]
        public string ToolTipFormat
        {
            get
            {
                return this.toolTipFormat;
            }
            set
            {
                this.toolTipFormat = value;
            }
        }

        public RayView(DrawingRay ray, Pad pad)
        {
            this.ray = ray;
            Pad = pad;
            this.toolTipEnabled = true;
            this.toolTipFormat = "{0} {1} {2} - {3:F6}";
            int index = pad.Series.GetIndex(ray.X, IndexOption.Prev);
            if (index == -1)
                return;
            this.chartFirstDate = pad.Series.GetDateTime(index);
            this.chartLastDate = DateTime.MaxValue;
        }

        public void Paint()
        {
            double y = this.ray.Y;
            int num1 = this.Pad.ClientX(this.chartFirstDate);
            int num2 = this.Pad.ClientY(y);
            if (num2 > this.Pad.Y2)
                return;
            Math.Max(2.0, this.Pad.IntervalWidth / 1.2);
            Pen pen = new Pen(this.ray.Color, (float)this.ray.Width);
            double val1_1 = (double)this.Pad.ClientX(this.firstDate);
            double val1_2 = (double)this.Pad.ClientX(this.lastDate);
            double val2 = val1_2;
            float x1 = (float)Math.Max(val1_1, (double)num1);
            float x2 = (float)Math.Min(val1_2, val2);
            if (x1 > x2)
                return;
            Pad.Graphics.DrawLine(pen, x1, num2, x2, num2);
        }

        public void SetInterval(DateTime minDate, DateTime maxDate)
        {
            this.firstDate = minDate;
            this.lastDate = maxDate;
        }

        public Distance Distance(int x, double y)
        { 
            var d = new Distance();
            var dateTime = this.Pad.GetDateTime(x);
            double y1 = this.ray.Y;
            d.X = (double)x;
            d.Y = y1;
            int num = this.Pad.ClientX(this.chartFirstDate);
            int x2 = this.Pad.X2;
            d.DX = num > x || x2 < x ? double.MaxValue : 0.0;
            d.DY = Math.Abs(y - this.ray.Y);
            if (d.DX == double.MaxValue || d.DY == double.MaxValue)
                return null;
            d.ToolTipText = string.Format(ToolTipFormat, "Ray", this.ray.Name, dateTime, this.ray.Y);
            return d;
        }

        public void Select()
        {
        }

        public void UnSelect()
        {
        }

        public PadRange GetPadRangeY(Pad pad) => new PadRange(this.ray.Y * 0.999, this.ray.Y * 1.001);
    }
}
