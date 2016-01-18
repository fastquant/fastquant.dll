// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
    public class StrategyManager
    {
        private Framework framework;
        private int counter;
        private StrategyMode mode = StrategyMode.Backtest;
        private Dictionary<IDataProvider, InstrumentList> dictionary_0 = new Dictionary<IDataProvider, InstrumentList>();

        public Global Global { get; } = new Global();

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
            Clear();
        }

        public void Clear()
        {
            lock (this)
            {
                this.counter = 101;
                this.dictionary_0.Clear();
                Global.Clear();
            }
        }

        public int GetNextId()
        {
            lock (this)
                return this.counter++;
        }

        public void Stop()
        {
            lock (this)
            {
                if (Status != StrategyStatus.Stopped)
                {
                    Status = StrategyStatus.Stopped;
                    StopStrategy();
                }
            }
        }

        public void StartStrategy(Strategy strategy)
        {
            StartStrategy(strategy, Mode);
        }

        public void StartStrategy(Strategy strategy, StrategyMode mode)
        {
            lock (this)
            {
                this.dictionary_0.Clear();
                Strategy = strategy;
                Mode = mode;
                if (this.framework.Mode == FrameworkMode.Simulation)
                {
                    this.framework.Clock.DateTime = this.framework.ProviderManager.DataSimulator.DateTime1;
                    this.framework.ExchangeClock.DateTime = DateTime.MinValue;
                }
                if (this.framework.EventManager.Status != EventManagerStatus.Running)
                    this.framework.EventManager.Start();

                var info = new StrategyStatusInfo(this.framework.Clock.DateTime, StrategyStatusType.Started)
                {
                    Solution = strategy.Name ?? "Solution",
                    Mode = mode.ToString()
                };
                this.framework.EventServer.OnLog(new GroupEvent(info, null));

                if (Persistence == StrategyPersistence.Full || Persistence == StrategyPersistence.Save)
                {
                    this.framework.OrderServer.SeriesName = strategy.Name;
                    this.framework.OrderManager.IsPersistent = true;
                }

                if (Persistence == StrategyPersistence.Full || Persistence == StrategyPersistence.Load)
                {
                    this.framework.PortfolioManager.Load(strategy.Name);
                    this.framework.OrderManager.Load(strategy.Name, -1);
                }

                strategy.Init();

                if ((Persistence == StrategyPersistence.Full || Persistence == StrategyPersistence.Save) && !strategy.Portfolio.IsLoaded)
                    this.framework.PortfolioManager.Save(strategy.Portfolio);
              
                strategy.vmethod_0();

                if (!this.framework.IsExternalDataQueue)
                {
                    var dictionary = new Dictionary<IDataProvider, InstrumentList>();
                    while (this.dictionary_0.Count != 0)
                    {
                        var dictionary2 = new Dictionary<IDataProvider, InstrumentList>(this.dictionary_0);
                        this.dictionary_0.Clear();
                        foreach (var current in dictionary2)
                        {
                            InstrumentList instrumentList = null;
                            if (!dictionary.TryGetValue(current.Key, out instrumentList))
                            {
                                instrumentList = new InstrumentList();
                                dictionary[current.Key] = instrumentList;
                            }
                            InstrumentList instrumentList2 = new InstrumentList();
                            foreach (Instrument current2 in current.Value)
                            {
                                if (!instrumentList.Contains(current2))
                                {
                                    instrumentList.Add(current2);
                                    instrumentList2.Add(current2);
                                }
                            }
                            if (current.Key is SellSideStrategy)
                            {
                                this.framework.SubscriptionManager?.Subscribe(current.Key, instrumentList2);
                            }
                        }
                    }
                    this.method_1();
                    Status = StrategyStatus.Running;
                    this.dictionary_0 = dictionary;
                    if (this.dictionary_0.Count == 0 && mode == StrategyMode.Backtest)
                    {
                        Console.WriteLine($"{DateTime.Now } StrategyManager::StartStrategy {strategy.Name} has no data requests in backtest mode, stopping...");
                        StopStrategy();
                    }
                    else
                    {
                        foreach (var current3 in this.dictionary_0)
                        {
                            if (!(current3.Key is SellSideStrategy))
                                this.framework.SubscriptionManager?.Subscribe(current3.Key, current3.Value);
                        }
                        if (mode != StrategyMode.Backtest)
                            strategy.FundamentalProvider?.Connect();
                    }
                }
                else
                {
                    this.method_1();
                    Status = StrategyStatus.Running;
                }
            }
        }

        internal void method_1()
        {
            Group group = this.method_44("StrategyParameters");
            if (group == null)
            {
                group = new Group("StrategyParameters");
                this.framework.GroupManager.Add(group);
            }
            else
            {
                this.framework.EventServer.OnLog(group);
            }
            this.method_2(Strategy, "", group);
        }

        private void method_2(Strategy strategy_1, string string_0, Group group_0)
        {
            string_0 += ((string_0 == "") ? strategy_1.Name : ("\\" + strategy_1.Name));
            ParameterList parameters = strategy_1.GetParameters();
            parameters.Name = string_0;
            this.framework.EventServer.OnLog(new GroupEvent(parameters, group_0));
            foreach (Strategy current in strategy_1.Strategies)
            {
                this.method_2(current, string_0, group_0);
            }
        }

        private Group method_44(string string_0)
        {
            foreach (Group current in this.framework.GroupManager.GroupList)
            {
                if (current.Name == string_0)
                {
                    return current;
                }
            }
            return null;
        }


        private void StopStrategy()
        {
            lock (this)
            {
                Console.WriteLine($"{DateTime.Now} StrategyManager::StopStrategy {Strategy.Name}");
                var info = new StrategyStatusInfo(this.framework.Clock.DateTime, StrategyStatusType.Stopped)
                {
                    Solution = Strategy.Name ?? "Solution",
                    Mode = Mode.ToString()
                };
                this.framework.EventServer.OnLog(new GroupEvent(info, null));

                if (!this.framework.IsExternalDataQueue)
                    this.dictionary_0.ToList().ForEach(p => this.framework.SubscriptionManager?.Unsubscribe(p.Key, p.Value));

                if (Strategy.Status == StrategyStatus.Stopped)
                {
                    Console.WriteLine($"StrategyManager::StopStrategy Error: Can not stop stopped strategy ({Strategy.Name})");
                }
                else
                {
                    Strategy.vmethod_1();
                    if (this.framework.Mode == FrameworkMode.Simulation)
                    {
                        this.framework.ProviderManager.DataSimulator.Disconnect();
                        this.framework.ProviderManager.ExecutionSimulator.Disconnect();
                    }
                    Strategy.vmethod_2();
                    Status = StrategyStatus.Stopped;
                }
            }
        }

        public void RegisterMarketDataRequest(IDataProvider dataProvider, InstrumentList instrumentList)
        {
            InstrumentList instrumentList2 = null;
            if (!this.dictionary_0.TryGetValue(dataProvider, out instrumentList2))
            {
                instrumentList2 = new InstrumentList();
                this.dictionary_0[dataProvider] = instrumentList2;
            }
            foreach (var current in instrumentList)
            {
                if (!instrumentList2.Contains(current.Id))
                {
                    instrumentList2.Add(current);
                }
            }
            if (Status == StrategyStatus.Running)
            {
                this.framework.SubscriptionManager?.Subscribe(dataProvider, instrumentList);
            }
        }

        internal void OnAsk(Ask ask)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.UxFbinsqFw(ask);
        }

        internal void OnTrade(Trade trade)
        {
            throw new NotImplementedException();
        }

        internal void OnBid(Bid bid)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.vmethod_9(bid);
        }

        internal void OnBar(Bar bar_0)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.vmethod_13(bar_0);
        }

        internal void method_46(string source, Event e, Exception ex)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.vmethod_42(source, e, ex);
        }

        internal void method_0(IDataProvider provider, InstrumentList instruments)
        {
            if (Status == StrategyStatus.Running && instruments.Count > 0 && this.framework.SubscriptionManager != null)
            {
                this.framework.SubscriptionManager.Unsubscribe(provider, instruments);
            }
            InstrumentList instrumentList = this.dictionary_0[provider];
            if (instrumentList != null)
            {
                foreach (var current in instruments)
                {
                    if (!this.framework.SubscriptionManager.IsSubscribed(provider, current))
                    {
                        instrumentList.Remove(current);
                    }
                }
            }
        }

        internal void method_10(Level2Snapshot level2Snapshot_0)
        {
            if (Strategy?.Status == StrategyStatus.Running)
            {
                Strategy.vmethod_11(level2Snapshot_0);
            }
        }
        internal void method_11(Level2Update level2Update_0)
        {
            if (Strategy?.Status == StrategyStatus.Running)
            {
                Strategy.vmethod_12(level2Update_0);
            }
        }

        internal void method_30(OnFill e)
        {
            throw new NotImplementedException();
        }

        internal void method_31(OnTransaction e)
        {
            throw new NotImplementedException();
        }

        internal void method_29(AccountReport report)
        {
            throw new NotImplementedException();
        }

        internal void method_37(AccountData data)
        {
            throw new NotImplementedException();
        }

        internal void method_28(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_27(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_26(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_25(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_24(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_22(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_23(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_20(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_21(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_19(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_18(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_16(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_38(Command e)
        {
            throw new NotImplementedException();
        }

        internal void method_15(News news)
        {
            throw new NotImplementedException();
        }

        internal void gudLdqclqe(Fundamental fundamental)
        {
            throw new NotImplementedException();
        }

        internal void IbsLpdRkc3(ExecutionReport report)
        {
            throw new NotImplementedException();
        }

        internal void method_6(ProviderError e)
        {
            throw new NotImplementedException();
        }

        internal void method_34(Portfolio portfolio)
        {
            throw new NotImplementedException();
        }

        internal void method_36(Portfolio portfolio)
        {
            throw new NotImplementedException();
        }

        internal void method_35(Portfolio portfolio)
        {
            throw new NotImplementedException();
        }

        internal void method_40(OnPropertyChanged e)
        {
            throw new NotImplementedException();
        }

        internal void method_14(BarSlice barSlice)
        {
            throw new NotImplementedException();
        }

        internal void vMaLxjraoe(Portfolio portfolio, Position position)
        {
            throw new NotImplementedException();
        }

        internal void method_32(Portfolio portfolio, Position position)
        {
            throw new NotImplementedException();
        }

        internal void method_33(Portfolio portfolio, Position position)
        {
            throw new NotImplementedException();
        }

        internal void method_17(Order order)
        {
            throw new NotImplementedException();
        }

        internal void method_45(object data)
        {
            throw new NotImplementedException();
        }

        internal void method_4(Provider provider)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.vmethod_6(provider);
        }

        internal void method_5(Provider p)
        {
            throw new NotImplementedException();
        }
    }
}