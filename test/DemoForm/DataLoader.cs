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
            framework.EventManager.Dispatcher.FrameworkCleared += new FrameworkEventHandler(this.Dispatcher_FrameworkCleared);
            framework.EventManager.Dispatcher.ExecutionCommand += new ExecutionCommandEventHandler(this.Dispatcher_ExecutionCommand);
            framework.EventManager.Dispatcher.ExecutionReport += new ExecutionReportEventHandler(this.Dispatcher_ExecutionReport);
            framework.EventManager.Dispatcher.OrderManagerCleared += new OrderManagerClearedEventHandler(this.Dispatcher_OrderManagerCleared);
            framework.EventManager.Dispatcher.PositionOpened += new PositionEventHandler(this.Dispatcher_PositionOpened);
            framework.EventManager.Dispatcher.PositionChanged += new PositionEventHandler(this.Dispatcher_PositionChanged);
            framework.EventManager.Dispatcher.PositionClosed += new PositionEventHandler(this.Dispatcher_PositionClosed);
            framework.EventManager.Dispatcher.Fill += new FillEventHandler(this.Dispatcher_NewFill);
            framework.EventManager.Dispatcher.Transaction += new TransactionEventHandler(this.Dispatcher_Transaction);
            framework.EventManager.Dispatcher.PortfolioAdded += new PortfolioEventHandler(this.Dispatcher_PortfolioAdded);
            framework.EventManager.Dispatcher.PortfolioRemoved += new PortfolioEventHandler(this.Dispatcher_PortfolioRemoved);
            framework.EventManager.Dispatcher.PortfolioParentChanged += new PortfolioEventHandler(this.Dispatcher_ParentChanged);
        }

        private void Dispatcher_ExecutionCommand(object sender, ExecutionCommand command)
        {
            this.OrderManagerQueue.Enqueue(command);
        }

        private void Dispatcher_ExecutionReport(object sender, ExecutionReport report)
        {
            this.OrderManagerQueue.Enqueue(report);
        }

        private void Dispatcher_FrameworkCleared(object sender, FrameworkEventArgs args)
        {
            this.PortfolioEventQueue.Clear();
            this.PortfolioManagerEventQueue.Clear();
            this.PortfolioEventQueue.Enqueue(new OnFrameworkCleared(args.Framework));
            this.PortfolioManagerEventQueue.Enqueue(new OnFrameworkCleared(args.Framework));
        }

        private void Dispatcher_NewFill(object sender, OnFill fill)
        {
            this.PortfolioEventQueue.Enqueue(fill);
        }

        private void Dispatcher_OrderManagerCleared(object sender, OnOrderManagerCleared data)
        {
            this.OrderManagerQueue.Clear();
            this.OrderManagerQueue.Enqueue(data);
        }

        private void Dispatcher_ParentChanged(object sender, PortfolioEventArgs args)
        {
            this.PortfolioManagerEventQueue.Enqueue(new OnPortfolioParentChanged(args.Portfolio));
        }

        private void Dispatcher_PortfolioAdded(object sender, PortfolioEventArgs args)
        {
            this.PortfolioManagerEventQueue.Enqueue(new OnPortfolioAdded(args.Portfolio));
        }

        private void Dispatcher_PortfolioRemoved(object sender, PortfolioEventArgs args)
        {
            this.PortfolioManagerEventQueue.Enqueue(new OnPortfolioRemoved(args.Portfolio));
        }

        private void Dispatcher_PositionChanged(object sender, PositionEventArgs args)
        {
            this.PortfolioEventQueue.Enqueue(new OnPositionChanged(args.Portfolio, args.Position));
        }

        private void Dispatcher_PositionClosed(object sender, PositionEventArgs args)
        {
            this.PortfolioEventQueue.Enqueue(new OnPositionClosed(args.Portfolio, args.Position));
        }

        private void Dispatcher_PositionOpened(object sender, PositionEventArgs args)
        {
            PortfolioEventQueue.Enqueue(new OnPositionOpened(args.Portfolio, args.Position));
        }

        private void Dispatcher_Transaction(object sender, OnTransaction transaction)
        {
            PortfolioEventQueue.Enqueue(transaction);
        }

        public PermanentQueue<Event> OrderManagerQueue { get; }
        public PermanentQueue<Event> PortfolioEventQueue { get; }
        public PermanentQueue<Event> PortfolioManagerEventQueue { get; }
    }
}
