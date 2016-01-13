// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;

namespace SmartQuant.Charting
{
    public interface IChartTransformation
    {
        double CalculateNotInSessionTicks(double x, double y);

        double CalculateRealQuantityOfTicks_Right(double x, double y);

        double CalculateRealQuantityOfTicks_Left(double x, double y);

        void GetFirstGridDivision(ref EGridSize gridSize, ref double min, ref double max, ref DateTime firstDateTime);

        double GetNextGridDivision(double firstTick, double prevMajor, int majorCount, EGridSize gridSize);
    }
}
