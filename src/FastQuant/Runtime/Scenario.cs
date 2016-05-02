// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace FastQuant
{
    public class Scenario
    {
        protected internal string name;
        protected internal Framework framework;
        protected internal Strategy strategy;

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
            this.framework.GroupManager.Clear();
        }

        public virtual void Run()
        {
            // noop
        }

        public void RunWithLogger(string solutionName)
        {
            throw new NotImplementedException("Will never implement this!");
        }

        public void StartBacktest() => StartStrategy(StrategyMode.Backtest);

        public void StartLive() => StartStrategy(StrategyMode.Live);

        public void StartPaper() => StartStrategy(StrategyMode.Paper);

        public void StartStrategy(StrategyMode mode) => StartStrategy(Strategy, mode);

        public void StartStrategy() => StartStrategy(Strategy, this.framework.StrategyManager.Mode);

        public void StartStrategy(Strategy strategy) => StartStrategy(strategy, strategy.Mode);

        private void StartStrategy(Strategy strategy, StrategyMode mode)
        {
            Console.WriteLine($"{DateTime.Now} Scenario::StartStrategy {mode}");
            this.framework.StrategyManager.StartStrategy(strategy, mode);

            // Wait for completion
            while (strategy.Status != StrategyStatus.Stopped)
                Thread.Sleep(10);

            Console.WriteLine($"{DateTime.Now} Scenario::StartStrategy Done");
        }
    }

    public class PerformanceScenario : Scenario
    {
        public PerformanceScenario(Framework framework) : base(framework)
        {
        }

        public override void Run()
        {
            var provider = new PerformanceProvider(this.framework);
            this.strategy = new PerformanceStrategy(this.framework)
            {
                DataProvider = provider,
                ExecutionProvider = provider
            };
            StartStrategy(StrategyMode.Live);
        }
    }


    public class ScenarioManager
    {
        private Framework framework;

        public Scenario Scenario { get; set; }

        public ScenarioManager(Framework framework)
        {
            this.framework = framework;
        }

        public void Start()
        {
            if (Scenario != null)
            {
                this.framework.Clear();
                new Thread(() => Scenario.Run())
                {
                    Name = "Scenario Manager Thread",
                    IsBackground = true
                }.Start();
            }
        }

        public void Stop() => this.framework.StrategyManager.Stop();
    }
}