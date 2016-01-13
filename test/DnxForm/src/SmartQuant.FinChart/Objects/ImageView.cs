// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using SmartQuant.FinChart;
using System;
using System.ComponentModel;

namespace SmartQuant.FinChart.Objects
{
    public class ImageView : IChartDrawable, IZoomable
    {
        private DrawingImage image;

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

        public ImageView(DrawingImage image, Pad pad)
        {
            this.image = image;
            Pad = pad;
            ToolTipEnabled = true;
            ToolTipFormat = "{0} {1} {2} - {3:F6}";
        }

        public void Paint()
        {
            Pad.Graphics.DrawImage(this.image.Image, this.Pad.ClientX(this.image.X), this.Pad.ClientY(this.image.Y));
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
