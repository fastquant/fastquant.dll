using System;

namespace SmartQuant.Charting
{
    [Serializable]
    public class TIntradayTransformation : IChartTransformation
    {
        private long firstSessionTick;
        private long lastSessionTick;
        private long fGap;

        public long FirstSessionTick
        {
            get
            {
                return this.firstSessionTick;
            }
            set
            {
                this.firstSessionTick = value;
                Session = this.lastSessionTick - this.firstSessionTick;
            }
        }

        public long LastSessionTick
        {
            get
            {
                return this.lastSessionTick;
            }
            set
            {
                this.lastSessionTick = value;
                Session = this.lastSessionTick - this.firstSessionTick;
            }
        }

        public long Session { get; private set; }

        public bool SessionGridEnabled { get; set; }

        public TIntradayTransformation()
            : this(0, TimeSpan.TicksPerDay)
        {
        }

        public TIntradayTransformation(long firstSessionTick, long lastSessionTick)
        {
            SessionGridEnabled = true;
            SetSessionBounds(firstSessionTick, lastSessionTick);
        }

        public void SetSessionBounds(long firstSessionTick, long lastSessionTick)
        {
            this.firstSessionTick = firstSessionTick;
            this.lastSessionTick = lastSessionTick;
            Session = this.lastSessionTick - this.firstSessionTick;
        }

        public void GetFirstGridDivision(ref EGridSize gridSize, ref double min, ref double max, ref DateTime firstDateTime)
        {
            if ((max - min) / Session <= 10.0)
            {
                gridSize = Axis.CalculateSize(max - min);
                max = min + CalculateRealQuantityOfTicks_Right(min, max);
            }
            else
            {
                max = min + this.CalculateRealQuantityOfTicks_Right(min, max);
                gridSize = Axis.CalculateSize(max - min);
            }
            this.fGap = this.CalculateGap((long)min, (long)gridSize);
            if ((long)min / 864000000000L - (long)((long)min + this.fGap + gridSize) / 864000000000L < 0L && gridSize < (EGridSize)576000000000)
            {
                min += (double)this.CalculateJumpGap((long)min, (long)gridSize);
                this.fGap = this.CalculateGap((long)min, (long)gridSize);
            }
            if (gridSize < (EGridSize)576000000000)
                firstDateTime = new DateTime((long)min + this.fGap);
            else
                this.fGap = -this.firstSessionTick;
        }

        public double GetNextGridDivision(double firstTick, double prevMajor, int majorCount, EGridSize gridSize)
        {
            return majorCount != 0 ? (gridSize >= (EGridSize)576000000000 ? (double)Axis.GetNextMajor((long)prevMajor, gridSize) : this.GetNextMajor(prevMajor, (long)gridSize)) : (double)this.GetFirstMajor((long)firstTick - this.fGap);
        }

        private double GetNextMajor(double x, long gridSize)
        {
            double num1 = 0.0;
            if (x > 10000.0)
            {
                if (x % 864000000000.0 < (double)this.firstSessionTick)
                    x = (double)((long)x / 864000000000L * 864000000000L + this.firstSessionTick);
                else if (x % 864000000000.0 > (double)this.lastSessionTick)
                    x = (double)(((long)x / 864000000000L + 1L) * 864000000000L + this.firstSessionTick);
                double num2 = (double)((long)x / 864000000000L * 864000000000L + this.lastSessionTick);
                double num3 = num2 - x;
                num1 = num3 >= (double)gridSize ? x + (double)gridSize : num2 + 864000000000.0 - (double)Session - num3 + (double)gridSize;
            }
            return num1;
        }

        private long GetFirstMajor(long x)
        {
            long num = 0L;
            if (x > 10000L)
                num = x % 864000000000L < this.firstSessionTick || x % 864000000000L > this.lastSessionTick ? this.GetFirstMajor(x + 864000000000L -Session) : x;
            return num;
        }

        public double CalculateRealQuantityOfTicks_Right(double x, double y)
        {
            long num1 = (long)(y - x) / Session * 864000000000L;
            long num2 = (long)(y - x) % Session;
            return (double)((long)(x + (double)num1) / 864000000000L * 864000000000L + this.lastSessionTick) - (x + (double)num1) < (double)num2 ? (double)(num1 + num2 + 864000000000L - Session) : (double)(num1 + num2);
        }

        public double CalculateRealQuantityOfTicks_Left(double x, double y)
        {
            long num1 = (long)(x - y) /Session * 864000000000L;
            long num2 = (long)(x - y) % Session;
            long num3 = (long)(x - (double)num1) / 864000000000L * 864000000000L + this.firstSessionTick;
            return (double)-(x - (double)num1 - (double)num3 < (double)num2 ? num1 + num2 + 864000000000L - Session : num1 + num2);
        }

        private long CalculateGap(long x, long gridSize)
        {
            x = x / 864000000000L * 864000000000L + this.firstSessionTick;
            return -((x - this.firstSessionTick) / 864000000000L) * ((864000000000L - Session) % gridSize) % gridSize;
        }

        private long CalculateJumpGap(long x, long gridSize)
        {
            if ((x + gridSize) / 864000000000L - x / 864000000000L > 0L && (x + gridSize) % 864000000000L > this.firstSessionTick && gridSize < 576000000000L)
                return (x / 864000000000L + 1L) * 864000000000L + this.firstSessionTick - x;
            else
                return 0L;
        }

        public double CalculateNotInSessionTicks(double x, double y)
        {
            if (y <= 10000.0)
                return 0.0;
            long num1 = (long)x % 864000000000L;
            long num2 = (long)y % 864000000000L;
            double num3 = (double)((long)x / 864000000000L * 864000000000L);
            double num4 = (double)((long)y / 864000000000L * 864000000000L);
            double num5 = 0.0;
            double num6 = 0.0;
            double num7 = num3 + (double)this.lastSessionTick;
            if (num1 < this.firstSessionTick)
                num5 = (double)(this.firstSessionTick - num1);
            if (num1 > this.lastSessionTick)
                num5 = (double)(this.firstSessionTick + 864000000000L - num1);
            double num8 = num4 + (double)this.lastSessionTick;
            if (num2 < this.firstSessionTick)
                num6 = (double)(-this.firstSessionTick + num2);
            if (num2 > this.lastSessionTick)
                num6 = (double)(num2 - this.lastSessionTick);
            return num5 + num6 + (double)(864000000000L - Session) * ((num8 - num7) / 864000000000.0);
        }
    }
}
