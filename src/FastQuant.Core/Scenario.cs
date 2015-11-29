using System;
using System.Threading;

namespace SmartQuant
{
    public class Scenario
    {
        protected string name;
        protected Framework framework;
        protected Strategy strategy;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public Strategy Strategy => this.strategy;

        public Clock Clock => this.framework.Clock;

        public AccountDataManager AccountDataManager => this.framework.AccountDataManager;

        public BarFactory BarFactory => this.framework.EventManager.BarFactory;

        public DataFileManager DataFileManager => this.framework.DataFileManager;

        public DataManager DataManager => this.framework.DataManager;

        public EventManager EventManager => this.framework.EventManager;

        public IDataSimulator DataSimulator => this.framework.ProviderManager.DataSimulator;

        public IExecutionSimulator ExecutionSimulator => this.framework.ProviderManager.ExecutionSimulator;

        public GroupManager GroupManager => this.framework.GroupManager;

        public InstrumentManager InstrumentManager => this.framework.InstrumentManager;

        public OrderManager OrderManager => this.framework.OrderManager;
        public ProviderManager ProviderManager => this.framework.ProviderManager;
        public StatisticsManager StatisticsManager => this.framework.StatisticsManager;
        public StrategyManager StrategyManager => this.framework.StrategyManager;


        public Scenario(Framework framework)
        {
            this.framework = framework;
            framework.GroupManager.Clear();
        }

        public virtual void Run()
        {
        }

        public void RunWithLogger(string solutionName)
        {
            throw new NotImplementedException("We don't have QuantControllerLogger");   
        }

        public void StartBacktest()
        {
            StartStrategy(StrategyMode.Backtest);
        }

        public void StartLive()
        {
            StartStrategy(StrategyMode.Live);
        }

        public void StartPaper()
        {
            StartStrategy(StrategyMode.Paper);
        }

        public void StartStrategy(StrategyMode mode)
        {
            StartStrategy(Strategy, mode);
        }

        public void StartStrategy()
        {
            StartStrategy(Strategy, this.framework.StrategyManager.Mode);
        }

        public void StartStrategy(Strategy strategy)
        {
            StartStrategy(strategy, strategy.Mode);
        }

        private void StartStrategy(Strategy strategy, StrategyMode mode)
        {
            Console.WriteLine($"{DateTime.Now} Scenario::StartStrategy {mode}");
            this.framework.StrategyManager.StartStrategy(strategy, mode);
            while (strategy.Status != StrategyStatus.Stopped)
            {
                Thread.Sleep(10);
            }
            Console.WriteLine($"{DateTime.Now} Scenario::StartStrategy Done");
        }
    }

    public class ScenarioManager
    {
        private Framework framework;

        private Thread thread;

        public Scenario Scenario { get; set; }

        public ScenarioManager(Framework framework)
        {
            this.framework = framework;
        }

        private void Run()
        {
            Scenario.Run();
        }

        public void Start()
        {
            if (Scenario != null)
            {
                this.framework.Clear();
                this.thread = new Thread(new ThreadStart(Run));
                this.thread.Name = "Scenario Manager Thread";
                this.thread.IsBackground = true;
                this.thread.Start();
            }
        }

        public void Stop()
        {
            this.framework.StrategyManager.Stop();
        }
    }
}