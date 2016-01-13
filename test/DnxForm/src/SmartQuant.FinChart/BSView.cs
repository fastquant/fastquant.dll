// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;

namespace SmartQuant.FinChart
{
    public abstract class BSView : SeriesView
    {
        public string ToolTipDateTimeFormat { get; set; }

        public BSView(Pad pad)
            : base(pad)
        {
        }

        public override PadRange GetPadRangeY(Pad pad)
        {
            double max = MainSeries.GetMax(this.firstDate, this.lastDate);
            double min = MainSeries.GetMin(this.firstDate, this.lastDate);
            if (max >= min)
            {
                double num = max / 10.0;
                max -= num;
                min += num;
            }
            return new PadRange(max, min);
        }

        public override Distance Distance(int x, double y)
        {
            var d = new Distance();
            var bar = (MainSeries as BarSeries)[this.pad.GetDateTime(x), IndexOption.Null];
            d.DX = 0;
            d.DY = bar.Low <= y && y <= bar.High ? 0 : d.DY;
            if (d.DX == double.MaxValue || d.DY == double.MaxValue)
                return null;
            d.ToolTipText = string.Format(ToolTipFormat, MainSeries.Name, MainSeries.Description, ToolTipDateTimeFormat, bar.High, bar.Low, bar.Open, bar.Close, bar.Volume);
            return d;
        }
    }
}

