// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FastQuant.Optimization;

namespace FastQuant
{
    public class EventServer
    {
        private Framework framework;
        private EventBus bus;
        private EventQueue queue = new EventQueue();

        public EventServer(Framework framework, EventBus bus)
        {
            this.framework = framework;
            this.bus = bus;
        }

        public void Clear()
        {
            this.queue?.Clear();
        }

        public void EmitQueued()
        {
            while (!this.queue.IsEmpty())
                OnEvent(this.queue.Read());
        }

        public void OnEvent(Event e) => this.framework.EventManager.OnEvent(e);

        public void OnData(DataObject data)=> OnEvent(data);

        internal void OnAccountReport(AccountReport report) => OnEvent(report);

        internal void OnExecutionReport(ExecutionReport report) => OnEvent(report);

        public void OnProviderAdded(IProvider provider) => OnEvent(new OnProviderAdded(provider));

        public void OnProviderRemoved(Provider provider) => OnEvent(new OnProviderRemoved(provider));

        public void OnProviderConnected(Provider provider) => OnEvent(new OnProviderConnected(this.framework.Clock.DateTime, provider));

        public void OnProviderDisconnected(Provider provider) => OnEvent(new OnProviderDisconnected(this.framework.Clock.DateTime, provider));

        public void OnProviderError(ProviderError error) => OnEvent(error);

        public void OnProviderStatusChanged(Provider provider)
        {
            switch (provider.Status)
            {
                case ProviderStatus.Connected:
                    OnProviderConnected(provider);
                    break;
                case ProviderStatus.Disconnected:
                    OnProviderDisconnected(provider);
                    break;
            }
            OnEvent(new OnProviderStatusChanged(provider));
        }

        internal void OnPortfolioParentChanged(Portfolio portfolio, bool queued)
        {
            if (queued)
                OnEvent(new OnPortfolioParentChanged(portfolio));
        }

        public void OnPortfolioAdded(Portfolio portfolio) => OnEvent(new OnPortfolioAdded(portfolio));

        public void OnPortfolioRemoved(Portfolio portfolio) => OnEvent(new OnPortfolioRemoved(portfolio));

        public void OnInstrumentAdded(Instrument instrument) => OnEvent(new OnInstrumentAdded(instrument));

        public void OnInstrumentDefinition(InstrumentDefinition definition) => OnEvent(new OnInstrumentDefinition(definition));

        public void OnInstrumentDefintionEnd(InstrumentDefinitionEnd end) => OnEvent(new OnInstrumentDefinitionEnd(end));

        public void OnInstrumentDeleted(Instrument instrument) => OnEvent(new OnInstrumentDeleted(instrument));

        public void OnStrategyAdded(Strategy strategy) => OnEvent(new OnStrategyAdded(strategy));

        internal void OnOrderManagerCleared() => OnEvent(new OnOrderManagerCleared());

        public void OnPositionOpened(Portfolio portfolio, Position position, bool queued)
        {
            var e = new OnPositionOpened(portfolio, position);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnPositionClosed(Portfolio portfolio, Position position, bool queued)
        {
            var e = new OnPositionClosed(portfolio, position);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnPositionChanged(Portfolio portfolio, Position position, bool queued)
        {
            var e = new OnPositionChanged(portfolio, position);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        public void OnLog(Event e) => OnEvent(e);

        internal void OnTransaction(Portfolio portfolio, Transaction transaction, bool queued)
        {
            var e = new OnTransaction(portfolio, transaction);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnFill(Portfolio portfolio, Fill fill, bool queued)
        {
            var e = new OnFill(portfolio, fill);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        public void OnFrameworkCleared(Framework framework) => OnEvent(new OnFrameworkCleared(framework));

        internal void OnSendOrder(Order order) => OnEvent(new OnSendOrder(order));

        internal void OnExecutionCommand(ExecutionCommand command) => OnEvent(command);

        internal void OnPendingNewOrder(Order order, bool queued = true)
        {
            var e = new OnPendingNewOrder(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderStatusChanged(Order order, bool queued = true)
        {
            var e = new OnOrderStatusChanged(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnNewOrder(Order order, bool queued = true)
        {
            var e = new OnNewOrder(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderRejected(Order order, bool queued = true)
        {
            var e = new OnOrderRejected(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderDone(Order order, bool queued = true)
        {
            var e = new OnOrderDone(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderExpired(Order order, bool queued = true)
        {
            var e = new OnOrderExpired(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderPartiallyFilled(Order order, bool queued = true)
        {
            var e = new OnOrderPartiallyFilled(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderFilled(Order order, bool queued = true)
        {
            var e = new OnOrderFilled(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderCancelled(Order order, bool queued = true)
        {
            var e = new OnOrderCancelled(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderCancelRejected(Order order, bool queued = true)
        {
            var e = new OnOrderCancelRejected(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderReplaceRejected(Order order, bool queued = true)
        {
            var e = new OnOrderReplaceRejected(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOrderReplaced(Order order, bool queued = true)
        {
            var e = new OnOrderReplaced(order);
            if (queued)
                this.queue.Enqueue(e);
            else
                OnEvent(e);
        }

        internal void OnOptimizationProgress(OptimizationProgress progress)
        {
            OnEvent(new OnOptimizationProgress(progress));
        }

        internal void OnOptimizationStart()
        {
            OnEvent(new OnOptimizationStart());
        }

        internal void OnOptimizationStop()
        {
            OnEvent(new OnOptimizationStop());
        }
    }
}