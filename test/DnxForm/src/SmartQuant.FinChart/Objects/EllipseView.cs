// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using SmartQuant.FinChart;
using System;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.FinChart.Objects
{
    public class EllipseView : IChartDrawable, IZoomable
    {
        private DrawingEllipse rect;

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

        [Description("Tooltip format string. {1} - X coordinate, {2} - Y coordinte.")]
        [Category("ToolTip")]
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

        public EllipseView(DrawingEllipse rect, Pad pad)
        {
            this.rect = rect;
            Pad = pad;
            ToolTipEnabled = true;
            ToolTipFormat = "{0} {1} {2} - {3:F6}";
        }

        public void Paint()
        {
            int x1 = this.Pad.ClientX(this.rect.X1);
            int x2 = this.Pad.ClientX(this.rect.X2);
            int y1 = this.Pad.ClientY(this.rect.Y1);
            int y2 = this.Pad.ClientY(this.rect.Y2);
            Pad.Graphics.DrawEllipse(new Pen(this.rect.Color, this.rect.Width), Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }

        public void SetInterval(DateTime minDate, DateTime maxDate)
        {
            this.firstDate = minDate;
            this.lastDate = maxDate;
        }

        public Distance Distance(int x, double y)
        {
            return null;
        }

        public void Select()
        {
        }

        public void UnSelect()
        {
        }

        public PadRange GetPadRangeY(Pad pad)
        {
            return new PadRange(0, 0);
        }
    }
}
