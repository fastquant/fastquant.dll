// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

namespace SmartQuant.Charting
{
    public interface IZoomable
    {
        bool IsPadRangeX();

        bool IsPadRangeY();

        PadRange GetPadRangeX(Pad pad);

        PadRange GetPadRangeY(Pad pad);
    }
}
