// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace SmartQuant
{
    public class ProviderId
    {
        public const byte DataSimulator = 1;
        public const byte ExecutionSimulator = 2;
        public const byte QuickFIX42 = 3;
        public const byte IB = 4;
        public const byte ESignal = 5;
        public const byte MBTrading = 6;
        public const byte Opentick = 7;
        public const byte QuoteTracker = 8;
        public const byte TAL = 9;
        public const byte TTFIX = 10;
        public const byte TTAPI = 11;
        public const byte Genesis = 12;
        public const byte MyTrack = 13;
        public const byte Photon = 14;
        public const byte Bloomberg = 15;
        public const byte Reuters = 16;
        public const byte Yahoo = 17;
        public const byte DC = 18;
        public const byte CSI = 19;
        public const byte QuantHouse = 20;
        public const byte PATSAPI = 21;
        public const byte OpenECry = 22;
        public const byte OpenTick = 23;
        public const byte FIX = 24;
        public const byte Google = 25;
        public const byte Hotspot = 26;
        public const byte AlfaDirect = 27;
        public const byte Currenex = 28;
        public const byte SmartCOM = 29;
        public const byte GenericEOD = 30;
        public const byte QUIKFIX = 31;
        public const byte OSLFIX = 32;
        public const byte Nordnet = 33;
        public const byte Integral = 35;
        public const byte QuantBase = 36;
        public const byte QuantRouter = 38;
        public const byte IQFeed = 39;
        public const byte QuantRouter2014 = 40;
        public const byte MNI = 45;
        public const byte MatchingEngine = 99;

        private static Dictionary<string, byte> mapping;

        static ProviderId()
        {
            mapping = new Dictionary<string, byte>();

            foreach (var info in typeof(ProviderId).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (info.FieldType == typeof(byte))
                {
                    mapping.Add(info.Name, (byte)info.GetValue(null));
                }
            }
        }

        public static void Add(string name, byte id)
        {
            mapping.Add(name, id);
        }

        public static void Remove(string name)
        {
            mapping.Remove(name);
        }

        public static byte Get(string name)
        {
            byte id;
            mapping.TryGetValue(name, out id);
            return id;
        }
    }

    public enum ProviderStatus
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected
    }

    public enum ProviderType
    {
        DataProvider = 1,
        NewsProvider = 2,
        ExecutionProvider = 4,
        FundamentalProvider = 8,
        InstrumentProvider = 16,
        HistoricalDataProvider = 32
    }

    public enum ProviderErrorType : byte
    {
        Error,
        Warning,
        Message
    }

    public interface IProvider
    {
        ProviderStatus Status { get; }

        bool IsConnected { get; }

        bool IsDisconnected { get; }

        bool IsConnecting { get; }

        bool IsDisconnecting { get; }

        byte Id { get; }

        string Name { get; }

        void Connect();

        bool Connect(int timeout);

        void Disconnect();
    }

    public interface IDataProvider : IProvider
    {
        void Subscribe(Instrument instrument);

        void Subscribe(InstrumentList instrument);

        void Unsubscribe(Instrument instrument);

        void Unsubscribe(InstrumentList instrument);
    }

    public interface INewsProvider
    {
    }

    public interface IFundamentalProvider : IProvider
    {
    }

    public interface IInstrumentProvider : IProvider
    {
        void Send(InstrumentDefinitionRequest request);

        void Cancel(string requestId);
    }

    public interface IHistoricalDataProvider : IProvider
    {
        void Send(HistoricalDataRequest request);

        void Cancel(string requestId);
    }

    public interface IExecutionProvider : IProvider
    {
        void Send(ExecutionCommand command);
    }

    public interface IDataSimulator : IDataProvider
    {
        DateTime DateTime1 { get; set; }

        DateTime DateTime2 { get; set; }

        bool SubscribeBid { get; set; }

        bool SubscribeAsk { get; set; }

        bool SubscribeTrade { get; set; }

        bool SubscribeBar { get; set; }

        bool SubscribeLevelII { get; set; }

        bool SubscribeNews { get; set; }

        bool SubscribeFundamental { get; set; }

        bool SubscribeAll { set; }

        BarFilter BarFilter { get; }

        DataProcessor Processor { get; set; }

        List<IDataSeries> Series { get; set; }

        void Clear();
    }

    public interface IExecutionSimulator : IExecutionProvider
    {
        bool FillOnQuote { get; set; }

        bool FillOnTrade { get; set; }

        bool FillOnBar { get; set; }

        bool FillOnBarOpen { get; set; }

        bool FillOnLevel2 { get; set; }

        bool FillMarketOnNext { get; set; }

        bool FillLimitOnNext { get; set; }

        bool FillStopOnNext { get; set; }

        bool FillAtStopPrice { get; set; }

        bool FillStopLimitOnNext { get; set; }

        bool FillAtLimitPrice { get; set; }

        bool PartialFills { get; set; }

        bool Queued { get; set; }

        BarFilter BarFilter { get; }

        ICommissionProvider CommissionProvider { get; set; }

        ISlippageProvider SlippageProvider { get; set; }

        void OnBid(Bid bid);

        void OnAsk(Ask ask);

        void OnLevel2(Level2Snapshot snapshot);

        void OnLevel2(Level2Update update);

        void OnTrade(Trade trade);

        void OnBarOpen(Bar bar);

        void OnBar(Bar bar);

        void Clear();
    }

    public class Provider : IProvider
    {
        protected const string CATEGORY_INFO = "Information";
        protected const string CATEGORY_STATUS = "Status";
        protected const string CATEGORY_MARKET_DATA_DEFAULTS = "Market Data (defaults)";
        protected const string CATEGORY_EXECUTION_DEFAULTS = "Execution (defaults)";

        protected internal byte id;
        protected internal string name;
        protected internal string description;
        protected internal string url;

        protected internal Framework framework;
        private ProviderStatus status;

        protected EventQueue dataQueue;
        protected EventQueue executionQueue;
        protected EventQueue historicalQueue;
        protected EventQueue instrumentQueue;

        [Category("Information")]
        public byte Id => this.id;

        [Category("Information")]
        public string Name => this.name;

        [Category("Information")]
        public string Description => this.description;

        [Category("Information")]
        public string Url => this.url;

        [Category("Status")]
        public bool IsConnected => Status == ProviderStatus.Connected;

        [Category("Status")]
        public bool IsDisconnected => Status == ProviderStatus.Disconnected;

        [Category("Status")]
        public bool IsConnecting => Status == ProviderStatus.Connecting;

        [Category("Status")]
        public bool IsDisconnecting => Status == ProviderStatus.Disconnecting;

        public ProviderStatus Status
        {
            get
            {
                return this.status;
            }
            protected set
            {
                if (this.status != value)
                {
                    WriteDebugInfo($"Status: {this.status} -> {value}");
                    this.status = value;

                    if (this.status == ProviderStatus.Connected)
                        this.OnConnected();

                    if (this.status == ProviderStatus.Disconnected)
                        this.OnDisconnected();

                    this.framework.EventServer.OnProviderStatusChanged(this);
                }
            }

        }

        public Provider(Framework framework)
        {
            this.framework = framework;
            this.status = ProviderStatus.Disconnected;
        }

        public virtual void Clear()
        {
        }

        public virtual void Connect()
        {
            Status = ProviderStatus.Connecting;
            Status = ProviderStatus.Connected;
        }

        public virtual bool Connect(int timeout)
        {
            long ticks = DateTime.Now.Ticks;
            Connect();
            while (!IsConnected)
            {
                Thread.Sleep(1);
                if (TimeSpan.FromTicks(DateTime.Now.Ticks - ticks).TotalMilliseconds >= timeout)
                {
                    Console.WriteLine($"Provider::Connect timed out : {Name}");
                    return false;
                }
            }
            return true;
        }

        public virtual void Disconnect()
        {
            Status = ProviderStatus.Disconnecting;
            Status = ProviderStatus.Disconnected;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public override string ToString() => $"provider id = {Id} {Name} ({Description}) {Url}";

        public virtual void Subscribe(Instrument instrument)
        {
        }

        public virtual void Subscribe(InstrumentList instruments)
        {
            foreach (Instrument instrument in instruments)
                Subscribe(instrument);
        }

        public virtual void Unsubscribe(Instrument instrument)
        {
        }

        public virtual void Unsubscribe(InstrumentList instruments)
        {
            foreach (Instrument instrument in instruments)
                Unsubscribe(instrument);
        }

        public virtual void Process(Event e)
        {
            switch (e.TypeId)
            {
                case EventType.OnConnect:
                    Connect();
                    break;
                case EventType.OnDisconnect:
                    Disconnect();
                    break;
                case EventType.OnSubscribe:
                    Subscribe((e as OnSubscribe).Instrument);
                    break;
                case EventType.OnUnsubscribe:
                    Unsubscribe((e as OnUnsubscribe).Instrument);
                    break;
            }
        }

        public virtual void Send(ExecutionCommand command)
        {
            Console.WriteLine("Provider::Send is not implemented in the base class");
        }

        protected void WriteDebugInfo(string format, params object[] args)
        {
        }

        protected internal virtual void SetProperties(ProviderPropertyList properties)
        {
            throw new NotImplementedException();
        }

        protected internal virtual ProviderPropertyList GetProperties()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnConnected()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnDisconnected()
        {
            throw new NotImplementedException();
        }
    }

        public class ProviderInfo
    {
        public byte Id { get; internal set; }

        public string Name { get; internal set; }

        public string Description { get; internal set; }

        public string Url { get; internal set; }

        public ProviderType ProviderType { get; set; }

        public ProviderStatus Status { get; set; }

        public ParameterList Parameters { get; set; }

        public bool IsDataProvider => ProviderType.HasFlag(ProviderType.DataProvider);

        public bool IsNewsProvider => ProviderType.HasFlag(ProviderType.NewsProvider);

        public bool IsExecutionProvider => ProviderType.HasFlag(ProviderType.ExecutionProvider);

        public bool IsFundamentalProvider => ProviderType.HasFlag(ProviderType.FundamentalProvider);

        public bool IsInstrumentProvider => ProviderType.HasFlag(ProviderType.InstrumentProvider);

        public bool IsHistoricalDataProvider => ProviderType.HasFlag(ProviderType.HistoricalDataProvider);

        public ProviderInfo(Provider provider)
        {
            Id = provider.Id;
            Name = provider.Name;
            Description = provider.Description;
            Url = provider.Url;
            if (provider is IDataProvider)
                ProviderType |= ProviderType.DataProvider;
            if (provider is INewsProvider)
                ProviderType |= ProviderType.NewsProvider;
            if (provider is IExecutionProvider)
                ProviderType |= ProviderType.ExecutionProvider;
            if (provider is IFundamentalProvider)
                ProviderType |= ProviderType.FundamentalProvider;
            if (provider is IInstrumentProvider)
                ProviderType |= ProviderType.InstrumentProvider;
            if (provider is IHistoricalDataProvider)
                ProviderType |= ProviderType.HistoricalDataProvider;
            Status = provider.Status;
        }

        public override string ToString()
        {
            return $"provider id = {Id} {Name} ({Description}) {Url}";
        }
    }

    public class ProviderError : DataObject
    {
        public override byte TypeId => DataObjectType.ProviderError;

        public ProviderErrorType Type { get; set; }

        public byte ProviderId { get; set; }

        public int Id { get; set; }

        public int Code { get; set; }

        public string Text { get; set; }

        public ProviderError(DateTime dateTime, ProviderErrorType type, byte providerId, int id, int code, string text)
            : base(dateTime)
        {
            Type = type;
            ProviderId = providerId;
            Id = id;
            Code = code;
            Text = text;
        }

        public ProviderError(DateTime dateTime, ProviderErrorType type, byte providerId, string text)
            : this(dateTime, type, providerId, -1, -1, text)
        {
        }

        public ProviderError()
        {
        }

        public override string ToString() => $"id={Id} type={Type} code={Code} text={Text}";
    }

    public class ProviderEventArgs : EventArgs
    {
        public IProvider Provider { get; private set; }

        public ProviderEventArgs(IProvider provider)
        {
            Provider = provider;
        }
    }
    public class ProviderErrorEventArgs : EventArgs
    {
        public ProviderError Error { get; private set; }

        public ProviderErrorEventArgs(ProviderError error)
        {
            Error = error;
        }
    }

    public delegate void ProviderEventHandler(object sender, ProviderEventArgs args);

    public delegate void ProviderErrorEventHandler(object sender, ProviderErrorEventArgs args);

}
