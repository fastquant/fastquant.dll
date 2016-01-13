// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;

namespace SmartQuant.FinChart
{
    public enum SimpleDSStyle
    {
        Line,
        Bar,
        Circle,
    }

    public enum SimpleBSStyle
    {
        Candle,
        Bar,
        Line,
    }

    public abstract class SeriesView : IChartDrawable, IAxesMarked, IZoomable
    {
        protected bool isMarkEnable = true;
        protected bool toolTipEnabled = true;
        protected string toolTipFormat = "";
        protected bool displayNameEnabled = true;
        protected Pad pad;
        protected DateTime firstDate;
        protected DateTime lastDate;
        protected bool selected;

        public virtual string DisplayName
        {
            get
            {
                return MainSeries.Name;
            }
        }

        public virtual bool DisplayNameEnabled
        {
            get
            {
                return this.displayNameEnabled;
            }
            set
            {
                this.displayNameEnabled = value;
            }
        }

        public bool IsMarkEnable
        {
            get
            {
                return this.isMarkEnable;
            }
            set
            {
                this.isMarkEnable = value;
            }
        }

        public virtual int LabelDigitsCount
        {
            get
            {
                return this.pad.Chart.LabelDigitsCount;
            }
        }

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

        public abstract Color Color { get; set; }

        [Browsable(false)]
        public abstract double LastValue { get; }

        [Browsable(false)]
        public abstract ISeries MainSeries { get; }

        public SeriesView(Pad pad)
        {
            this.pad = pad;
        }

        public void SetInterval(DateTime minDate, DateTime maxDate)
        {
            this.firstDate = minDate;
            this.lastDate = maxDate;
        }

        public abstract PadRange GetPadRangeY(Pad Pad);

        public abstract void Paint();

        public abstract Distance Distance(int x, double y);

        public void Select()
        {
            this.selected = true;
        }

        public void UnSelect()
        {
            this.selected = false;
        }
    }
}
