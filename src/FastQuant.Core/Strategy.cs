// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class StrategyEventArgs : EventArgs
    {
    }

    public enum StrategyMode
    {
        Backtest = 1,
        Paper,
        Live
    }

    public enum StrategyPersistence
    {
        None,
        Full,
        Save,
        Load
    }

    public enum StrategyStatus
    {
        Running,
        Stopped
    }

    public enum StrategyStatusType : byte
    {
        Started,
        Stopped
    }

    public class StrategyMethodAttribute : Attribute
    {
    }

    public class Strategy
    {
        private Framework framework;

        public int Id { get; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Strategy Parent { get; private set;  }
        public StrategyStatus Status { get; private set; }
        public LinkedList<Strategy> Strategies { get; private set; }
        public Portfolio Portfolio { get; private set; }
        public TickSeries Bids { get; private set; }
        public TickSeries Asks { get; private set; }
        public BarSeries Bars { get; private set; }
        public TimeSeries Equity { get; private set; }
        public InstrumentList Instruments { get; private set; }
        public Clock Clock => this.framework.Clock;
        public StrategyMode Mode => this.framework.StrategyManager.Mode;
        public Global Global => this.framework.StrategyManager.Global;
        public OrderManager OrderManager => this.framework.OrderManager;
        public InstrumentManager InstrumentManager => this.framework.InstrumentManager;
        public GroupManager GroupManager => this.framework.GroupManager;
        public DataManager DataManager => this.framework.DataManager;
        public AccountDataManager AccountDataManager => this.framework.AccountDataManager;
        public EventManager EventManager => this.framework.EventManager;
        public ProviderManager ProviderManager => this.framework.ProviderManager;
        public StrategyManager StrategyManager => this.framework.StrategyManager;
    }

    public class MetaStrategy : Strategy
    {
        public MetaStrategy(Framework framework, string name)
            : base(framework, name)
        {
        }

        public void Add(Strategy strategy)
        {
        }
    }
}