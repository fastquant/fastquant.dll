using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    using SubMap = Dictionary<int, Dictionary<Instrument, int>>;

    public class Subscription
    {
        [Parameter]
        public string Symbol { get; set; } = string.Empty;

        [Parameter]
        public Instrument Instrument { get; set; }

        [Parameter]
        public int InstrumentId { get; set; } = -1;

        [Parameter]
        public IDataProvider Provider { get; set; }

        [Parameter]
        public int ProviderId { get; set; } = -1;

        [Parameter]
        public int RequestId { get; set; } = -1;

        [Parameter]
        public int SourceId { get; set; } = -1;

        [Parameter]
        public int RouteId { get; set; } = -1;

        public Subscription(Instrument instrument, IDataProvider provider, int sourceId = -1)
        {
            SourceId = sourceId;
            Instrument = instrument;
            InstrumentId = instrument != null ? instrument.Id : InstrumentId;
            Provider = provider;
            ProviderId = provider != null ? provider.Id : ProviderId;
        }
    }

    public class SubscriptionList : IEnumerable<Subscription>
    {
        private List<Subscription> subscriptions = new List<Subscription>();

        public int Count => this.subscriptions.Count;

        public void Clear()
        {
            this.subscriptions.Clear();
        }

        public void Add(Instrument instrument, IDataProvider provider)
        {
            Add(new Subscription(instrument, provider, -1));
        }


        public void Add(Subscription subscription)
        {
            throw new NotImplementedException();
        }

        public void Remove(Subscription subscription)
        {
            Remove(subscription.Instrument, subscription.Provider);
        }

        public void Remove(Instrument instrument, IDataProvider provider)
        {
            Remove(instrument.Id, provider.Id);
        }

        public void Remove(int instrumentId, int providerId)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Instrument instrument) => Contains(instrument.Id);

        public bool Contains(int instrumentId) => Contains(instrumentId, 0);

        public bool Contains(int instrumentId, int providerId)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Instrument instrument, IDataProvider provider)=> Contains(instrument.Id, provider != null ? provider.Id : 0);

        public int GetCount(Instrument instrument, IDataProvider provider) => GetCount(instrument.Id, provider.Id);

        public int GetCount(int instrumentId, int providerId)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Subscription> GetEnumerator() => this.subscriptions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.subscriptions.GetEnumerator();
    }

    public class SubscriptionManager
    {
        private Framework framework;
        public bool ConnectOnSubscribe { get; private set; } = true;
        private SubMap submap = new SubMap();

        public SubscriptionManager(Framework framework)
        {
            this.framework = framework;
        }

        public void Clear()
        {
            this.submap.Clear();
        }

        public void Subscribe(int providerId, Instrument instrument)
        {
            var provider = this.framework.ProviderManager.GetProvider(providerId) as IDataProvider;
            Subscribe(provider, instrument);
        }

        public void Subscribe(int providerId, int instrumentId)
        {
            var instrument = this.framework.InstrumentManager.GetById(instrumentId);
            Subscribe(providerId, instrument);
        }

        public void Subscribe(string provider, Instrument instrument)
        {
            var p = this.framework.ProviderManager.GetProvider(provider) as IDataProvider;
            this.Subscribe(p, instrument);
        }

        public void Subscribe(string provider, string symbol)
        {
            var p = this.framework.ProviderManager.GetProvider(provider) as IDataProvider;
            var i = this.framework.InstrumentManager.Get(symbol);
            Subscribe(p, i);
        }

        public void Subscribe(IDataProvider provider, Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(int providerId, Instrument instrument)
        {
            var provider = this.framework.ProviderManager.GetProvider(providerId) as IDataProvider;
            Unsubscribe(provider, instrument);
        }

        public void Unsubscribe(int providerId, int instrumentId)
        {
            var instrument = this.framework.InstrumentManager.GetById(instrumentId);
            Unsubscribe(providerId, instrument);
        }

        public void Unsubscribe(string provider, Instrument instrument)
        {
            var p = this.framework.ProviderManager.GetProvider(provider) as IDataProvider;
            Unsubscribe(p, instrument);
        }

        public void Unsubscribe(IDataProvider provider, Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IDataProvider provider, InstrumentList instruments)
        {
            throw new NotImplementedException();
        }
    }
}