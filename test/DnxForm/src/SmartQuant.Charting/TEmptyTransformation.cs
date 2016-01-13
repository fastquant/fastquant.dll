// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;

namespace SmartQuant.Charting
{
    public class TEmptyTransformation : IChartTransformation
    {
        public double CalculateNotInSessionTicks(double x, double y)
        {
            return 0;
        }

        public double CalculateRealQuantityOfTicks_Right(double x, double y)
        {
            return y - x;
        }

        public double CalculateRealQuantityOfTicks_Left(double x, double y)
        {
            return y - x;
        }

        public void GetFirstGridDivision(ref EGridSize gridSize, ref double min, ref double max, ref DateTime firstDateTime)
        {
            gridSize = Axis.CalculateSize(max - min);
        }

        public double GetNextGridDivision(double firstTick, double prevMajor, int majorCount, EGridSize gridSize)
        {
            return majorCount != 0 ? Axis.GetNextMajor((long)prevMajor, gridSize) : firstTick;
        }
    }
}
