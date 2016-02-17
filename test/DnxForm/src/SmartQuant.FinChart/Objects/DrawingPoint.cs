using System;

namespace SmartQuant.FinChart.Objects
{
    public class DrawingPoint : IUpdatable
    {
        private DateTime x;
        private double y;

        public DateTime X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
                EmitUpdated();
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
                EmitUpdated();
            }
        }

        public event EventHandler Updated;

        public DrawingPoint(DateTime x, double y)
        {
            this.x = x;
            this.y = y;
        }

        private void EmitUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    }
}
