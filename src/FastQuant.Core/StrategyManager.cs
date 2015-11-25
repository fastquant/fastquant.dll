using System;

namespace SmartQuant
{
    public class StrategyManager
    {
        private Framework framework;
        private int counter;
        public Global Global { get; private set; }
        public StrategyMode Mode { get; private set; }
        public StrategyStatus Status { get; private set; }
        public Strategy Strategy { get; private set; }

        public StrategyPersistence Persistence { get; set; }

        public StrategyManager(Framework framework)
        {
            this.framework = framework;
        }

        public int GetNextId()
        {
            lock (this)
                return this.counter++;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}