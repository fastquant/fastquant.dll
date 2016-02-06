using System;

namespace SmartQuant.FinChart.Objects
{
    public interface IUpdatable
    {
        event EventHandler Updated;
    }
}
