// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

namespace SmartQuant.Charting
{
    public interface IDrawable
    {
        bool ToolTipEnabled { get; set; }

        string ToolTipFormat { get; set; }

        void Draw();

        void Paint(Pad pad, double minX, double maxX, double minY, double maxY);

        TDistance Distance(double x, double y);
    }
}
