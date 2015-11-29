using System;

namespace SmartQuant
{
    public class StrategyManager
    {
        private Framework framework;
        private int counter;
        private StrategyMode mode;

        public Global Global { get; private set; }
        public StrategyMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                if (this.mode != value)
                {
                    this.mode = value;
                    switch (this.mode)
                    {
                        case StrategyMode.Backtest:
                            this.framework.Mode = FrameworkMode.Simulation;
                            return;
                        case StrategyMode.Paper:
                            this.framework.Mode = FrameworkMode.Realtime;
                            return;
                        case StrategyMode.Live:
                            this.framework.Mode = FrameworkMode.Realtime;
                            break;
                        default:
                            return;
                    }
                }
            }
        }
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


        public void StartStrategy(Strategy strategy)
        {
            StartStrategy(strategy, Mode);
        }

        public void StartStrategy(Strategy strategy, StrategyMode mode)
        {
            throw new NotImplementedException();
        }
    }
}