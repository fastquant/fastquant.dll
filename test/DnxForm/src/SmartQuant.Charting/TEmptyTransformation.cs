using System;

namespace SmartQuant.Charting
{
    public class TEmptyTransformation : IChartTransformation
    {
        public double CalculateNotInSessionTicks(double x, double y) => 0;

        public double CalculateRealQuantityOfTicks_Right(double x, double y) => y - x;

        public double CalculateRealQuantityOfTicks_Left(double x, double y) => y - x;

        public void GetFirstGridDivision(ref EGridSize gridSize, ref double min, ref double max, ref DateTime firstDateTime) => gridSize = Axis.CalculateSize(max - min);

        public double GetNextGridDivision(double firstTick, double prevMajor, int majorCount, EGridSize gridSize) => majorCount != 0 ? Axis.GetNextMajor((long)prevMajor, gridSize) : firstTick;
    }
}
