using System;

namespace SmartQuant
{
    public class EventServer
    {
        private Framework framework;
        private EventBus bus;

        public EventServer(Framework framework, EventBus bus)
        {
            this.framework = framework;
            this.bus = bus;
        }

        internal void OnProviderStatusChanged(Provider provider)
        {
            throw new NotImplementedException();
        }

        internal void OnPortfolioParentChanged(Portfolio portfolio, bool v)
        {
            throw new NotImplementedException();
        }

        public void OnEvent(Event e)
        {
            throw new NotImplementedException();
        }

        internal void OnPositionChanged(Portfolio portfolio, Position position, bool queued)
        {
            throw new NotImplementedException();
        }

        internal void OnLog(Group group)
        {
            throw new NotImplementedException();
        }

        internal void OnPositionOpened(Portfolio portfolio, Position position, bool queued)
        {
            throw new NotImplementedException();
        }

        internal void OnTransaction(Portfolio portfolio, Transaction transaction, bool queued)
        {
            throw new NotImplementedException();
        }

        internal void OnFill(Portfolio portfolio, Fill fill, bool queued)
        {
            throw new NotImplementedException();
        }

        internal void OnPositionClosed(Portfolio portfolio, Position position, bool queued)
        {
            throw new NotImplementedException();
        }
    }
}