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
        private Dictionary<IDataProvider, InstrumentList> subscriptions = new Dictionary<IDataProvider, InstrumentList>();

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
                this.subscriptions.Clear();
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
                this.subscriptions.Clear();
                Strategy = strategy;
                Mode = mode;
                if (this.framework.Mode == FrameworkMode.Simulation)
                {
                    this.framework.Clock.DateTime = this.framework.ProviderManager.DataSimulator.DateTime1;
                    this.framework.ExchangeClock.DateTime = DateTime.MinValue;
                }
                if (this.framework.EventManager.Status != EventManagerStatus.Running)
                    this.framework.EventManager.Start();

                SetStatusType(StrategyStatusType.Started);
                if (Persistence == StrategyPersistence.Full || Persistence == StrategyPersistence.Load)
                {
                    this.framework.PortfolioManager.Load(strategy.Name);
                    this.framework.OrderManager.Load(strategy.Name, -1);
                    this.framework.OrderServer.SeriesName = strategy.Name;
                    this.framework.OrderManager.IsPersistent = true;
                }
                else
                {
                    this.framework.OrderManager.IsPersistent = false;
                }

                strategy.Init();

                if ((Persistence == StrategyPersistence.Full || Persistence == StrategyPersistence.Save) && !strategy.Portfolio.IsLoaded)
                    this.framework.PortfolioManager.Save(strategy.Portfolio);
              
                strategy.EmitStrategyStart();

                if (!this.framework.IsExternalDataQueue)
                {
                    var dictionary = new Dictionary<IDataProvider, InstrumentList>();
                    while (this.subscriptions.Count != 0)
                    {
                        var dictionary2 = new Dictionary<IDataProvider, InstrumentList>(this.subscriptions);
                        this.subscriptions.Clear();
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
                    SetParametersGroup();
                    Status = StrategyStatus.Running;
                    this.subscriptions = dictionary;
                    if (this.subscriptions.Count == 0 && mode == StrategyMode.Backtest)
                    {
                        Console.WriteLine($"{DateTime.Now } StrategyManager::StartStrategy {strategy.Name} has no data requests in backtest mode, stopping...");
                        StopStrategy();
                    }
                    else
                    {
                        foreach (var current3 in this.subscriptions)
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
                    SetParametersGroup();
                    Status = StrategyStatus.Running;
                }
            }
        }

        private void StopStrategy()
        {
            lock (this)
            {
                Console.WriteLine($"{DateTime.Now} StrategyManager::StopStrategy {Strategy.Name}");
                SetStatusType(StrategyStatusType.Stopped);
                if (!this.framework.IsExternalDataQueue)
                    this.subscriptions.ToList().ForEach(p => this.framework.SubscriptionManager?.Unsubscribe(p.Key, p.Value));

                if (Strategy.Status == StrategyStatus.Stopped)
                {
                    Console.WriteLine($"StrategyManager::StopStrategy Error: Can not stop stopped strategy ({Strategy.Name})");
                }
                else
                {
                    Strategy.EmitStrategyStop();
                    if (this.framework.Mode == FrameworkMode.Simulation)
                    {
                        this.framework.ProviderManager.DataSimulator.Disconnect();
                        this.framework.ProviderManager.ExecutionSimulator.Disconnect();
                    }
                    // TODO: figure out function name
                    Strategy.vmethod_2();
                    Status = StrategyStatus.Stopped;
                }
            }
        }

        public void RegisterMarketDataRequest(IDataProvider dataProvider, InstrumentList instrumentList)
        {
            InstrumentList alreadyRegistered = null;
            if (!this.subscriptions.TryGetValue(dataProvider, out alreadyRegistered))
            {
                alreadyRegistered = new InstrumentList();
                this.subscriptions[dataProvider] = alreadyRegistered;
            }

            foreach (var current in instrumentList)
                if (!alreadyRegistered.Contains(current.Id))
                    alreadyRegistered.Add(current);

            if (Status == StrategyStatus.Running)
                this.framework.SubscriptionManager?.Subscribe(dataProvider, instrumentList);
        }

        internal void UnregisterMarketDataRequest(IDataProvider dataProvider, InstrumentList instruments)
        {
            if (Status == StrategyStatus.Running && instruments.Count > 0)
                this.framework.SubscriptionManager?.Unsubscribe(dataProvider, instruments);

            var list = this.subscriptions[dataProvider];
            if (list != null)
            {
                foreach (var i in instruments)
                    if (!this.framework.SubscriptionManager.IsSubscribed(dataProvider, i))
                        list.Remove(i);
            }
        }

        internal void SetParametersGroup()
        {
            var group = FindOrCreateGroup("StrategyParameters");
            CorrectParametersName(Strategy, "", group);
        }

        private void CorrectParametersName(Strategy strategy, string name, Group group)
        {
            name += string.IsNullOrEmpty(name) ? strategy.Name : "\\" + strategy.Name;
            var parameters = strategy.GetParameters();
            parameters.Name = name;
            parameters.DateTime = DateTime.Now;
            this.framework.EventServer.OnLog(new GroupEvent(parameters, group));
            foreach (Strategy s in strategy.Strategies)
                CorrectParametersName(s, name, group);
        }

        private Group FindOrCreateGroup(string name)
        {
            var group = FindGroup(name);
            if (group == null)
            {
                group = new Group(name);
                group.dateTime = DateTime.Now;
                this.framework.GroupManager.Add(group);
            }
            else
            {
                this.framework.EventServer.OnLog(group);
            }
            return group;
        }

        private Group FindGroup(string name) => this.framework.GroupManager.GroupList.Find(g => g.Name == name);

        private void SetStatusType(StrategyStatusType type)
        {
            var group = FindOrCreateGroup("SolutionStatus");
            var info = new StrategyStatusInfo(this.framework.Clock.DateTime, type)
            {
                Solution = Strategy.Name ?? "Solution",
                Mode = Mode.ToString(),
                DateTime = DateTime.Now
            };
            this.framework.EventServer.OnLog(new GroupEvent(info, group));
        }

        #region EventHandlers
        internal void OnException(string source, Event e, Exception ex)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitException(source, e, ex);
        }

        internal void OnBid(Bid bid)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitBid(bid);
        }

        internal void OnAsk(Ask ask)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitAsk(ask);
        }

        internal void OnTrade(Trade trade)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitTrade(trade);
        }

        internal void OnBarOpen(Bar bar)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitBarOpen(bar);
        }

        internal void OnBar(Bar bar)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitBar(bar);
        }

        internal void OnBarSlice(BarSlice barSlice)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitBarSlice(barSlice);
        }

        internal void OnLevel2(Level2Snapshot l2s)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitLevel2(l2s);
        }

        internal void OnLevel2(Level2Update l2u)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitLevel2(l2u);
        }


        internal void OnNews(News news)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitNews(news);
        }

        internal void OnFundamental(Fundamental fundamental)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitFundamental(fundamental);
        }

        internal void OnFill(OnFill e)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitFill(e);
        }

        internal void OnTransaction(OnTransaction e)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitTransaction(e);
        }

        internal void OnAccountReport(AccountReport report)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitAccountReport(report);
        }

        internal void OnExecutionReport(ExecutionReport report)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitExecutionReport(report);
        }

        internal void OnCommand(Command command)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitCommand(command);
        }

        internal void OnAccountData(AccountData data)
        {
            if (Strategy != null && Strategy.Status == StrategyStatus.Running && Mode != StrategyMode.Backtest)
                Strategy.EmitAccountData(data);
        }

        internal void OnOrderDone(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderDone(order);
        }

        internal void OnOrderReplaceRejected(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderReplaceRejected(order);
        }

        internal void OnOrderCancelRejected(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderCancelRejected(order);
        }

        internal void OnOrderExpired(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderExpired(order);
        }

        internal void OnOrderRejected(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderRejected(order);
        }

        internal void OnOrderCancelled(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderCancelled(order);
        }

        internal void OnOrderReplaced(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderReplaced(order);
        }

        internal void OnOrderFilled(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderFilled(order);
        }

        internal void OnOrderPartiallyFilled(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderPartiallyFilled(order);
        }

        internal void OnOrderStatusChanged(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitOrderStatusChanged(order);
        }

        internal void OnNewOrder(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitNewOrder(order);
        }

        internal void OnSendOrder(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitSendOrder(order);
        }

        internal void OnPendingNewOrder(Order order)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitPendingNewOrder(order);
        }

        internal void OnProviderError(ProviderError error)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitProviderError(error);
        }

        internal void OnProviderConnected(Provider provider)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitProviderConnected(provider);
        }

        internal void OnProviderDisconnected(Provider provider)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitProviderDisconnected(provider);
        }

        internal void OnPortfolioAdded(Portfolio portfolio)
        {
            // noop
        }

        internal void OnPortfolioParentChanged(Portfolio portfolio)
        {
            // noop
        }

        internal void OnPortfolioRemoved(Portfolio portfolio)
        {
            // noop
        }

        internal void OnPropertyChanged(OnPropertyChanged e)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitPropertyChanged(e);
        }

        internal void OnPositionOpened(Portfolio portfolio, Position position)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitPositionOpened(position);
        }

        internal void OnPositionClosed(Portfolio portfolio, Position position)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitPositionClosed(position);
        }

        internal void OnPositionChanged(Portfolio portfolio, Position position)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitPositionChanged(position);
        }

        internal void OnStrategyEvent(object data)
        {
            if (Strategy?.Status == StrategyStatus.Running)
                Strategy.EmitStrategyEvent(data);
        }
        #endregion
    }
}