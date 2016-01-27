using System;

namespace SmartQuant.Shared
{
    public class DataLoader
    {
        public DataLoader(Framework framework)
        {
            OrderManagerQueue = new PermanentQueue<Event>();
            PortfolioEventQueue = new PermanentQueue<Event>();
            PortfolioManagerEventQueue = new PermanentQueue<Event>();
            framework.EventManager.Dispatcher.FrameworkCleared += DispatcherFrameworkCleared;
            framework.EventManager.Dispatcher.ExecutionCommand += DispatcherExecutionCommand;
            framework.EventManager.Dispatcher.ExecutionReport += DispatcherExecutionReport;
            framework.EventManager.Dispatcher.OrderManagerCleared += DispatcherOrderManagerCleared;
            framework.EventManager.Dispatcher.PositionOpened += DispatcherPositionOpened;
            framework.EventManager.Dispatcher.PositionChanged += DispatcherPositionChanged;
            framework.EventManager.Dispatcher.PositionClosed += DispatcherPositionClosed;
            framework.EventManager.Dispatcher.Fill += DispatcherNewFill;
            framework.EventManager.Dispatcher.Transaction += DispatcherTransaction;
            framework.EventManager.Dispatcher.PortfolioAdded += DispatcherPortfolioAdded;
            framework.EventManager.Dispatcher.PortfolioRemoved += DispatcherPortfolioRemoved;
            framework.EventManager.Dispatcher.PortfolioParentChanged += DispatcherParentChanged;
        }

        private void DispatcherExecutionCommand(object sender, ExecutionCommand command)
        {
            OrderManagerQueue.Enqueue(command);
        }

        private void DispatcherExecutionReport(object sender, ExecutionReport report)
        {
            OrderManagerQueue.Enqueue(report);
        }

        private void DispatcherFrameworkCleared(object sender, FrameworkEventArgs args)
        {
            PortfolioEventQueue.Clear();
            PortfolioManagerEventQueue.Clear();
            PortfolioEventQueue.Enqueue(new OnFrameworkCleared(args.Framework));
            PortfolioManagerEventQueue.Enqueue(new OnFrameworkCleared(args.Framework));
        }

        private void DispatcherNewFill(object sender, OnFill fill)
        {
            PortfolioEventQueue.Enqueue(fill);
        }

        private void DispatcherOrderManagerCleared(object sender, OnOrderManagerCleared data)
        {
            OrderManagerQueue.Clear();
            OrderManagerQueue.Enqueue(data);
        }

        private void DispatcherParentChanged(object sender, PortfolioEventArgs args)
        {
            PortfolioManagerEventQueue.Enqueue(new OnPortfolioParentChanged(args.Portfolio));
        }

        private void DispatcherPortfolioAdded(object sender, PortfolioEventArgs args)
        {
            PortfolioManagerEventQueue.Enqueue(new OnPortfolioAdded(args.Portfolio));
        }

        private void DispatcherPortfolioRemoved(object sender, PortfolioEventArgs args)
        {
            PortfolioManagerEventQueue.Enqueue(new OnPortfolioRemoved(args.Portfolio));
        }

        private void DispatcherPositionChanged(object sender, PositionEventArgs args)
        {
            PortfolioEventQueue.Enqueue(new OnPositionChanged(args.Portfolio, args.Position));
        }

        private void DispatcherPositionClosed(object sender, PositionEventArgs args)
        {
            PortfolioEventQueue.Enqueue(new OnPositionClosed(args.Portfolio, args.Position));
        }

        private void DispatcherPositionOpened(object sender, PositionEventArgs args)
        {
            PortfolioEventQueue.Enqueue(new OnPositionOpened(args.Portfolio, args.Position));
        }

        private void DispatcherTransaction(object sender, OnTransaction transaction)
        {
            PortfolioEventQueue.Enqueue(transaction);
        }

        public PermanentQueue<Event> OrderManagerQueue { get; }
        public PermanentQueue<Event> PortfolioEventQueue { get; }
        public PermanentQueue<Event> PortfolioManagerEventQueue { get; }
    }
}
