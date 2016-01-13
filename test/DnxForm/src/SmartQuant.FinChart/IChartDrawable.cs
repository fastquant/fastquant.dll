// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;

namespace SmartQuant.FinChart
{
    public interface IChartDrawable
    {
        bool ToolTipEnabled { get; set; }
        string ToolTipFormat { get; set; }
        void Paint();
        void SetInterval(DateTime minDate, DateTime maxDate);
        Distance Distance(int x, double y);
        void Select();
        void UnSelect();
    }
}
