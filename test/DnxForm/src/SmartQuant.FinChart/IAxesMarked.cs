// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System.Drawing;

namespace SmartQuant.FinChart
{
    public interface IAxesMarked
    {
        Color Color { get; }
        double LastValue { get; }
        bool IsMarkEnable { get; }
        int LabelDigitsCount { get; }
    }
}
