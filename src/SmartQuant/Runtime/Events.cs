using System;
using System.ComponentModel;

namespace SmartQuant
{
    public class OnException : Event
    {
        public override byte TypeId => EventType.OnException;

        internal Event Event { get; }
        internal Exception Exception { get; }
        internal string Source { get; }

        public OnException(string source, Event ev, Exception exception)
        {
            Source = source;
            Event = ev;
            Exception = exception;
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

        public string Symbol { get; }

        public Subscription Subscription { get; internal set; }

        public Instrument Instrument { get; }

        internal InstrumentList Instruments { get; }

        internal DateTime DateTime1 { get; } = DateTime.MinValue;

        internal DateTime DateTime2 { get; } = DateTime.MaxValue;

        public OnSubscribe(Subscription subscription)
        {
            Subscription = subscription;
            Symbol = subscription.Symbol;
            Instrument = subscription.Instrument;
        }

        public OnSubscribe(string symbol)
        {
            Symbol = symbol;
        }

        public OnSubscribe(InstrumentList instruments)
        {
            Instruments = instruments;
        }

    }

    public class OnUnsubscribe : Event
    {
        public override byte TypeId => EventType.OnUnsubscribe;
        public string Symbol { get; internal set; }

        public Subscription Subscription { get; internal set; }

        public Instrument Instrument { get; private set; }

        internal InstrumentList Instruments { get; }

        public OnUnsubscribe()
        {
        }

        public OnUnsubscribe(Subscription subscription)
        {
            Subscription = subscription;
            Symbol = subscription.Symbol;
            Instrument = subscription.Instrument;
        }

        public OnUnsubscribe(string symbol)
        {
            Symbol = symbol;
        }

        public OnUnsubscribe(InstrumentList instruments)
        {
            Instruments = instruments;
        }

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
        public Portfolio Portfolio { get; }
        public Position Position { get; }

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
        public Portfolio Portfolio { get; }
        public Position Position { get; }

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
        public Portfolio Portfolio { get; }
        public Position Position { get; }

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

        public override byte TypeId => EventType.OnFrameworkCleared;

        public OnFrameworkCleared(Framework framework)
        {
            Framework = framework;
        }
    }

    public class OnStrategyAdded : Event
    {
        public override byte TypeId => EventType.OnStrategyAdded;

        public Strategy Strategy { get; }

        public string StrategyName { get; }

        public OnStrategyAdded(Strategy strategy)
        {
            Strategy = strategy;
            StrategyName = strategy.Name;
        }
    }

    public class OnStrategyEvent : Event
    {
        public override byte TypeId => EventType.OnStrategyEvent;

        internal object Data { get; }

        public OnStrategyEvent(object data)
        {
            Data = data;
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

        public override byte TypeId => EventType.OnPortfolioRemoved;

        public OnPortfolioRemoved(Portfolio portfolio)
        {
            Portfolio = portfolio;
        }
    }

    public class OnPortfolioParentChanged : Event
    {
        public OnPortfolioParentChanged(Portfolio portfolio)
        {
            Portfolio = portfolio;
        }

        public Portfolio Portfolio { get; }

        public override byte TypeId => EventType.OnPortfolioParentChanged;

    }


    public class OnProviderAdded : Event
    {
        public byte ProviderId { get; }

        public IProvider Provider { get; }

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

        public override byte TypeId => EventType.OnProviderConnected;

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

        public override byte TypeId => EventType.OnProviderDisconnected;

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

    public class OnLogin : Event
    {
        public OnLogin()
        {
        }

        public OnLogin(DateTime dateTime) : base(dateTime)
        {
        }

        public override byte TypeId => EventType.OnLogin;

        [Parameter]
        public string GUID { get; set; } = "";

        [Parameter]
        public int Id { get; set; } = -1;

        [Parameter, PasswordPropertyText(true)]
        public string Password { get; set; } = "";

        [Parameter]
        public string ProductName { get; set; } = "";

        [Parameter]
        public string UserName { get; set; } = "";
    }

    public class OnLogout : Event
    {
        public OnLogout()
        {
        }

        public OnLogout(DateTime dateTime) : base(dateTime)
        {
        }

        public override byte TypeId => EventType.OnLogout;

        public int Id { get; set; } = -1;
        public string ProductName { get; set; } = "";
        public string Reason { get; set; } = "";
        public string UserName { get; set; } = "";
    }

    public class OnLoggedIn : Event
    {
        public OnLoggedIn()
        {
        }

        public OnLoggedIn(DateTime dateTime) : base(dateTime)
        {
        }

        public int DefaultAlgoId { get; set; } = -1;

        public ObjectTable Fields { get; internal set; } = new ObjectTable();

        public object this[int index]
        {
            get
            {
                return Fields[index];
            }
            set
            {
                Fields[index] = value;
            }
        }

        public override byte TypeId => EventType.OnLoggedIn;

        public int UserId { get; set; } = -1;

        public string UserName { get; set; } = string.Empty;
    }

    public class OnLoggedOut : Event
    {
        public OnLoggedOut()
        {
        }

        public OnLoggedOut(DateTime dateTime) : base(dateTime)
        {
        }

        public override byte TypeId => EventType.OnLoggedOut;
    }

    public class OnHeartbeat : Event
    {
        public OnHeartbeat()
        {
        }

        public OnHeartbeat(DateTime dateTime) : base(dateTime)
        {
        }

        public override byte TypeId => EventType.OnHeartbeat;
    }
}