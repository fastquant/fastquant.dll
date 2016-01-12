using System;

namespace SmartQuant
{
    public class OnException : Event
    {
        private Event ev;
        private Exception exception;
        private string source;

        public override byte TypeId => EventType.OnException;

        public OnException(string source, Event ev, Exception exception)
        {
            this.source = source;
            this.ev = ev;
            this.exception = exception;
        }
    }

    public class OnConnect : Event
    {
        public override byte TypeId => EventType.OnConnect;
    }

    public class OnDisconnect : Event
    {
        public override byte TypeId => EventType.OnDisconnect;
    }

    public class OnSubscribe : Event
    {
        public override byte TypeId => EventType.OnSubscribe;

        internal Instrument Instrument { get; }

        internal DateTime DateTime1 { get; } = DateTime.MinValue;

        internal DateTime DateTime2 { get; } = DateTime.MaxValue;

        public OnSubscribe(InstrumentList instruments)
        {
        }

        public OnSubscribe(Instrument instrument)
        {
            Instrument = instrument;
        }
    }

    public class OnUnsubscribe : Event
    {
        public override byte TypeId => EventType.OnUnsubscribe;

        internal Instrument Instrument { get; private set; }

        public OnUnsubscribe(Instrument instrument)
        {
            Instrument = instrument;
        }
    }

    public class OnQueueOpened : Event
    {
        private EventQueue queue;

        public override byte TypeId => EventType.OnQueueOpened;

        public OnQueueOpened(EventQueue queue)
        {
            this.queue = queue;
            DateTime = DateTime.MinValue;
        }

        public override string ToString() => $"{nameof(OnQueueOpened)} : {this.queue.Name}";
    }

    public class OnQueueClosed : Event
    {
        private EventQueue queue;

        public override byte TypeId => EventType.OnQueueClosed;

        public OnQueueClosed(EventQueue queue)
        {
            this.queue = queue;
            DateTime = DateTime.MinValue;
        }

        public override string ToString() => $"{nameof(OnQueueClosed)} : {this.queue.Name}";

    }

    public class OnPositionOpened : Event
    {
        internal Portfolio Portfolio { get; }
        internal Position Position { get; }

        public override byte TypeId => EventType.OnPositionOpened;

        public OnPositionOpened(Portfolio portfolio, Position position)
        {
            Portfolio = portfolio;
            Position = position;
        }

        public override string ToString() => $"{nameof(OnPositionOpened)} {Position}";
    }

    public class OnPositionClosed : Event
    {
        internal Portfolio Portfolio { get; }
        internal Position Position { get; }

        public override byte TypeId => EventType.OnPositionClosed;

        public OnPositionClosed(Portfolio portfolio, Position position)
        {
            Portfolio = portfolio;
            Position = position;
        }

        public override string ToString() => $"{nameof(OnPositionClosed)} {Position}";
    }

    public class OnPositionChanged : Event
    {
        internal Portfolio Portfolio { get; }
        internal Position Position { get; }

        public override byte TypeId => EventType.OnPositionChanged;

        public OnPositionChanged(Portfolio portfolio, Position position)
        {
            Portfolio = portfolio;
            Position = position;
        }

        public override string ToString() => $"{nameof(OnPositionChanged)} {Position}";
    }

    public class OnTransaction : Event
    {
        public Transaction Transaction { get; }

        public Portfolio Portfolio { get; }

        public override byte TypeId => EventType.OnTransaction;

        public OnTransaction(Portfolio portfolio, Transaction transaction)
        {
            Portfolio = portfolio;
            Transaction = transaction;
        }

        public override string ToString() => $"{nameof(OnTransaction)} {Transaction}";
    }

    public class OnEventManagerStarted : Event
    {
        public override byte TypeId => EventType.OnEventManagerStarted;
    }

    public class OnEventManagerStopped : Event
    {
        public override byte TypeId => EventType.OnEventManagerStopped;
    }

    public class OnEventManagerPaused : Event
    {
        public override byte TypeId => EventType.OnEventManagerPaused;
    }

    public class OnEventManagerStep : Event
    {
        public override byte TypeId => EventType.OnEventManagerStep;
    }

    public class OnEventManagerResumed : Event
    {
        public override byte TypeId => EventType.OnEventManagerResumed;
    }

    public class OnFrameworkCleared : Event
    {
        internal Framework Framework { get; }

        public override byte TypeId=> EventType.OnFrameworkCleared;
 
        public OnFrameworkCleared(Framework framework)
        {
            Framework = framework;
        }
    }

    public class OnInstrumentAdded : Event
    {
        public Instrument Instrument { get; }

        public override byte TypeId => EventType.OnInstrumentAdded;

        public OnInstrumentAdded(Instrument instrument)
        {
            Instrument = instrument;
        }
    }

    public class OnInstrumentDeleted : Event
    {
        public Instrument Instrument { get; }

        public override byte TypeId => EventType.OnInstrumentDeleted;

        public OnInstrumentDeleted(Instrument instrument)
        {
            Instrument = instrument;
        }
    }

    public class OnInstrumentDefinition : Event
    {
        public override byte TypeId => EventType.OnInstrumentDefinition;

        public InstrumentDefinition InstrumentDefinition { get; }

        public OnInstrumentDefinition(InstrumentDefinition definition)
        {
            InstrumentDefinition = definition;
        }
    }

    public class OnInstrumentDefinitionEnd : Event
    {
        public override byte TypeId => EventType.OnInstrumentDefintionEnd;

        public InstrumentDefinitionEnd InstrumentDefinitionEnd { get; }

        public OnInstrumentDefinitionEnd(InstrumentDefinitionEnd end)
        {
            InstrumentDefinitionEnd = end;
        }
    }

    public class OnPortfolioAdded : Event
    {
        public Portfolio Portfolio { get; }

        public override byte TypeId => EventType.OnPortfolioAdded;

        public OnPortfolioAdded(Portfolio portfolio)
        {
            Portfolio = portfolio;
        }
    }


    public class OnPortfolioRemoved : Event
    {
        public Portfolio Portfolio { get; private set; }

        public override byte TypeId=> EventType.OnPortfolioRemoved;

        public OnPortfolioRemoved(Portfolio portfolio)
        {
            Portfolio = portfolio;
        }
    }

    public class OnProviderAdded : Event
    {
        public byte ProviderId { get; }

        public IProvider Provider { get;  }

        public override byte TypeId => EventType.OnProviderAdded;

        public OnProviderAdded(IProvider provider)
        {
            Provider = provider;
            ProviderId = Provider.Id;
        }
    }

    public class OnProviderRemoved : Event
    {
        public byte ProviderId { get; }

        public Provider Provider { get; }

        public override byte TypeId => EventType.OnProviderRemoved;

        public OnProviderRemoved(Provider provider)
        {
            Provider = provider;
            ProviderId = provider.Id;
        }
    }

    public class OnProviderConnected : Event
    {
        public Provider Provider { get; }

        public byte ProviderId { get; }

        public override byte TypeId=> EventType.OnProviderConnected;

        public OnProviderConnected(DateTime dateTime, byte providerId)
            : base(dateTime)
        {
            ProviderId = providerId;
        }

        public OnProviderConnected(DateTime dateTime, Provider provider)
            : base(dateTime)
        {
            Provider = provider;
        }
    }

    public class OnProviderDisconnected : Event
    {
        public Provider Provider { get; }

        public byte ProviderId { get; }

        public override byte TypeId=> EventType.OnProviderDisconnected;

        public OnProviderDisconnected(DateTime dateTime, byte providerId)
            : base(dateTime)
        {
            ProviderId = providerId;
        }

        public OnProviderDisconnected(DateTime dateTime, Provider provider)
            : base(dateTime)
        {
            Provider = provider;
        }
    }

    public class OnProviderStatusChanged : Event
    {
        public byte ProviderId { get; }

        public Provider Provider { get; }

        public override byte TypeId => EventType.OnProviderStatusChanged;

        public OnProviderStatusChanged(Provider provider)
        {
            Provider = provider;
            ProviderId = provider.Id;
        }
    }
}