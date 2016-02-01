using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartQuant
{
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

        public Subscription()
        {
        }

        public Subscription(Instrument instrument, IDataProvider provider, int sourceId = -1)
        {
            SourceId = sourceId;
            Instrument = instrument;
            InstrumentId = instrument?.Id ?? InstrumentId;
            Provider = provider;
            ProviderId = provider?.Id ?? ProviderId;
        }
    }

    /// <summary>
    /// Track instruments subscription with a DataProvider.
    /// No connection action involved. mainly used in Strategy or its subclasses
    /// </summary>
    public class SubscriptionList : IEnumerable<Subscription>
    {
        private List<Subscription> subscriptions = new List<Subscription>();
        private IdArray<IdArray<int>> subscriptionsByIIdAndPId = new IdArray<IdArray<int>>();
        private IdArray<int> subscriptionsByIId = new IdArray<int>();

        public int Count => this.subscriptions.Count;

        public void Clear()
        {
            this.subscriptions.Clear();
            this.subscriptionsByIIdAndPId.Clear();
        }

        public void Add(Instrument instrument, IDataProvider provider) => Add(new Subscription(instrument, provider, -1));

        public void Add(Subscription subscription)
        {
            var iId = subscription.Instrument.Id;
            this.subscriptionsByIIdAndPId[iId] = this.subscriptionsByIIdAndPId[iId] ?? new IdArray<int>();

            if (this.subscriptionsByIIdAndPId[iId][subscription.ProviderId] == 0)
                this.subscriptions.Add(subscription);

            this.subscriptionsByIId[iId] += 1;
            this.subscriptionsByIIdAndPId[iId][subscription.ProviderId] += 1;
        }

        public void Remove(Subscription subscription) => Remove(subscription.Instrument, subscription.Provider);

        public void Remove(Instrument instrument, IDataProvider provider) => Remove(instrument.Id, provider.Id);

        public void Remove(int instrumentId, int providerId)
        {
            if (this.subscriptionsByIIdAndPId[instrumentId] == null)
                return;
            if (this.subscriptionsByIIdAndPId[instrumentId][providerId] == 0)
                return;

            this.subscriptionsByIIdAndPId[instrumentId][providerId] -= 1;
            this.subscriptionsByIId[instrumentId] -= 1;

            if (this.subscriptionsByIIdAndPId[instrumentId][providerId] == 0)
            {
                var i = this.subscriptions.FindIndex(s => s.InstrumentId == instrumentId && s.ProviderId == providerId);
                if (i != -1)
                    this.subscriptions.RemoveAt(i);
            }
        }

        public bool Contains(Instrument instrument) => Contains(instrument.Id);

        public bool Contains(int instrumentId) => Contains(instrumentId, 0);

        public bool Contains(int instrumentId, int providerId) => this.subscriptionsByIIdAndPId[instrumentId] != null && this.subscriptionsByIIdAndPId[instrumentId][providerId] != 0;

        public bool Contains(Instrument instrument, IDataProvider provider) => Contains(instrument.Id, provider?.Id ?? 0);

        public int GetCount(Instrument instrument, IDataProvider provider) => GetCount(instrument.Id, provider.Id);

        public int GetCount(int instrumentId, int providerId) => this.subscriptionsByIIdAndPId[instrumentId]?[providerId] ?? 0;

        public IEnumerator<Subscription> GetEnumerator() => this.subscriptions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    /// <summary>
    /// Manage the Subscription with DataProvider in Framework.
    /// Track each instrument with a ref counter
    /// </summary>
    public class SubscriptionManager
    {
        private Framework framework;

        public bool ConnectOnSubscribe { get; } = true;

        private Dictionary<int, Dictionary<Instrument, int>> submap = new Dictionary<int, Dictionary<Instrument, int>>();

        public SubscriptionManager(Framework framework)
        {
            this.framework = framework;
        }

        public void Clear()
        {
            this.submap.Clear();
        }

        public bool IsSubscribed(IDataProvider provider, Instrument instrument)
        {
            return this.submap.ContainsKey(provider.Id) && this.submap[provider.Id].ContainsKey(instrument) && this.submap[provider.Id][instrument] > 0;
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
            Subscribe(p, instrument);
        }

        public void Subscribe(string provider, string symbol)
        {
            var p = this.framework.ProviderManager.GetProvider(provider) as IDataProvider;
            var i = this.framework.InstrumentManager.Get(symbol);
            Subscribe(p, i);
        }

        public void Subscribe(IDataProvider provider, Instrument instrument)
        {
            Console.WriteLine($"SubscriptionManager::Subscribe {provider} {instrument}");
            if (ConnectOnSubscribe && provider.Status != ProviderStatus.Connected)
                provider.Connect();

            Dictionary<Instrument, int> dictionary = null;
            if (!this.submap.TryGetValue(provider.Id, out dictionary))
            {
                dictionary = new Dictionary<Instrument, int>();
                this.submap[provider.Id] = dictionary;
            }
            int count = 0;
            bool needSubcribe = false;
            if (!dictionary.TryGetValue(instrument, out count))
            {
                needSubcribe = true;
                count = 1;
            }
            else
            {
                if (count == 0)
                    needSubcribe = true;
                count++;
            }
            dictionary[instrument] = count;
            if (needSubcribe)
                provider.Subscribe(instrument);
        }

        public void Subscribe(IDataProvider provider, InstrumentList instruments)
        {
            if (provider.Status != ProviderStatus.Connected)
                provider.Connect();

            var newInstruments = new InstrumentList();
            for (int i = 0; i < instruments.Count; i++)
            {
                var instrument = instruments.GetByIndex(i);
                if (!this.submap.ContainsKey(provider.Id))
                    this.submap[provider.Id] = new Dictionary<Instrument, int>();
                if (!this.submap[provider.Id].ContainsKey(instrument) || this.submap[provider.Id][instrument] == 0)
                {
                    this.submap[provider.Id][instrument] = 0;
                    newInstruments.Add(instrument);
                }
                this.submap[provider.Id][instrument] += 1;
            }
            if (newInstruments.Count > 0)
                provider.Subscribe(newInstruments);
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
            if (!this.submap.ContainsKey(provider.Id))
                return;
            if (this.submap[provider.Id][instrument] == 0)
            {
                Console.WriteLine($"SubscriptionManager::Unsubscribe Error. Instrument has no subscriptions {instrument.Symbol} on data provider {provider.Name}");
                return;
            }
            this.submap[provider.Id][instrument] -= 1;
            if (this.submap[provider.Id][instrument] == 0)
                provider.Unsubscribe(instrument);
        }

        public void Unsubscribe(IDataProvider provider, InstrumentList instruments)
        {
            var list = new InstrumentList();
            for (int i = 0; i < instruments.Count; i++)
            {
                var instrument = instruments.GetByIndex(i);
                if (this.submap.ContainsKey(provider.Id) && this.submap[provider.Id][instrument] != 0)
                {
                    this.submap[provider.Id][instrument] -= 1;
                    if (this.submap[provider.Id][instrument] == 0)
                        list.Add(instrument);
                }
                else
                    Console.WriteLine($"SubscriptionManager::Unsubscribe Error. Instrument has no subscriptions {instrument.Symbol} on data provider {provider.Name}");
            }
            provider.Unsubscribe(list);
        }

        internal void OnProviderConnected(IDataProvider dataProvider)
        {
            if (this.submap.ContainsKey(dataProvider.Id))
            {
                foreach (var i in this.submap[dataProvider.Id].Keys.Where(k => this.submap[dataProvider.Id][k] != 0))
                {
                    Console.WriteLine($"SubscriptionManager::OnProviderConnected {dataProvider.Name} resubscribing {i.Symbol}");
                    dataProvider.Subscribe(i);
                }
            }
        }

        internal void OnProviderDisconnected(IDataProvider provider)
        {
            // noop
        }
    }
}