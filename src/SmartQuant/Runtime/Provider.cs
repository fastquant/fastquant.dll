// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Diagnostics;

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

        private static Dictionary<string, byte> mapping = new Dictionary<string, byte>();

        static ProviderId()
        {
            var fields = typeof(ProviderId).GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.FieldType == typeof(byte)).ToList();
            fields.ForEach(f => mapping.Add(f.Name, (byte)f.GetValue(null)));
        }

        public static void Add(string name, byte id) => mapping.Add(name, id);

        public static void Remove(string name) => mapping.Remove(name);

        public static byte Get(string name)
        {
            byte id; mapping.TryGetValue(name, out id);
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

    [Flags]
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

        byte Id { get; set; }

        string Name { get; set; }

        bool Enabled { get; set; }

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
        protected internal bool enabled;

        protected EventQueue dataQueue;
        protected EventQueue executionQueue;
        protected EventQueue historicalQueue;
        protected EventQueue instrumentQueue;

        [Category("Information")]
        public byte Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        [Category("Information")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

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

        [Category("Status")]
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

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
                        OnConnected();

                    if (this.status == ProviderStatus.Disconnected)
                        OnDisconnected();

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
            // noop
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
            // noop
        }

        public override string ToString() => $"provider id = {Id} {Name} ({Description}) {Url}";

        public virtual void Subscribe(Instrument instrument)
        {
            // noop
        }

        public virtual void Subscribe(InstrumentList instruments)
        {
            foreach (Instrument instrument in instruments)
                Subscribe(instrument);
        }

        public virtual void Unsubscribe(Instrument instrument)
        {
            // noop
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

        public virtual void RequestHistoricalData(HistoricalDataRequest request)
        {
            // noop
        }

        public virtual void RequestInstrumentDefinitions(InstrumentDefinitionRequest request)
        {
            // noop
        }

        public virtual void Send(ExecutionCommand command)
        {
            Console.WriteLine("Provider::Send is not implemented in the base class");
        }

        public virtual void Send(HistoricalDataRequest request)
        {
            // noop
        }

        public virtual void Send(InstrumentDefinitionRequest request)
        {
            // noop
        }

        protected void Reject(ExecutionCommand command, string format, params object[] args)
        {
            var report = new ExecutionReport();
            report.DateTime = this.framework.Clock.DateTime;
            report.Order = command.Order;
            report.OrderId = command.OrderId;
            report.Instrument = command.Instrument;
            report.InstrumentId = command.InstrumentId;
            report.CurrencyId = command.Instrument.CurrencyId;
            report.OrdType = command.Order.Type;
            report.Side = command.Order.Side;
            report.OrdQty = command.Order.Qty;
            report.Price = command.Order.Price;
            report.StopPx = command.Order.StopPx;
            report.TimeInForce = command.Order.TimeInForce;
            report.AvgPx = command.Order.AvgPx;
            report.CumQty = command.Order.CumQty;
            report.LeavesQty = command.Order.LeavesQty;
            switch (command.Type)
            {
                case ExecutionCommandType.Send:
                    report.ExecType = ExecType.ExecRejected;
                    report.OrdStatus = OrderStatus.Rejected;
                    break;
                case ExecutionCommandType.Cancel:
                    report.ExecType = ExecType.ExecCancelReject;
                    report.OrdStatus = command.Order.Status;
                    break;
                case ExecutionCommandType.Replace:
                    report.ExecType = ExecType.ExecReplaceReject;
                    report.OrdStatus = command.Order.Status;
                    break;
            }
            report.Text = format != null ? string.Format(format, args) : report.Text;
            EmitExecutionReport(report, true);
        }

        protected void WriteDebugInfo(string format, params object[] args)
        {
            // noop
        }

        protected internal virtual void SetProperties(ProviderPropertyList properties)
        {
            foreach(var p in GetType().GetProperties().Where(p => p.CanRead && p.CanWrite))
            {
                var converter = TypeDescriptor.GetConverter(p.PropertyType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    string value = properties.GetStringValue(p.Name, null);
                    //if (value != null && converter.IsValid(value))
                    if (value != null)
                        p.SetValue(this, converter.ConvertFromInvariantString(value), null);
                }
            }
         }

        protected internal virtual ProviderPropertyList GetProperties()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnConnected()
        {
            if ((this is IDataProvider || this is IFundamentalProvider) && this.dataQueue == null)
            {
                this.dataQueue = new EventQueue(EventQueueId.Data, EventQueueType.Master, EventQueuePriority.Normal, 25600, this.framework.EventBus);
                this.dataQueue.Enqueue(new OnQueueOpened(this.dataQueue));
                this.dataQueue.Name = $"{this.name} data queue";
                this.framework.EventBus.DataPipe.Add(this.dataQueue);
            }
            if (this is IExecutionProvider && this.executionQueue == null)
            {
                this.executionQueue = new EventQueue(EventQueueId.Execution, EventQueueType.Master, EventQueuePriority.Normal, 25600, this.framework.EventBus);
                this.executionQueue.Enqueue(new OnQueueOpened(this.executionQueue));
                this.executionQueue.Name = $"{this.name}  execution queue";
                this.framework.EventBus.ExecutionPipe.Add(this.executionQueue);
            }
        }

        protected virtual void OnDisconnected()
        {
            if (this is IDataProvider && this.dataQueue != null)
            {
                this.dataQueue.Enqueue(new OnQueueClosed(this.dataQueue));
                this.dataQueue = null;
            }
            if (this is IExecutionProvider && this.executionQueue != null)
            {
                this.executionQueue.Enqueue(new OnQueueClosed(this.executionQueue));
                this.executionQueue = null;
            }
        }

        #region Event Emitters

        protected internal void EmitAccountData(AccountData data)
        {
            if (this.executionQueue != null)
                this.executionQueue.Enqueue(data);
            else
                this.framework.EventServer.OnEvent(data);
        }

        protected internal void EmitAccountReport(AccountReport report, bool queued = true)
        {
            if (queued && this.executionQueue != null)
                this.executionQueue.Enqueue(report);
            else
                this.framework.EventServer.OnAccountReport(report);
        }

        protected internal void EmitExecutionReport(ExecutionReport report, bool queued = true)
        {
            if (queued && this.executionQueue != null)
                this.executionQueue.Enqueue(report);
            else
                this.framework.EventServer.OnExecutionReport(report);
        }

        protected internal void EmitData(DataObject data, bool queued = true)
        {
            if (queued && this.dataQueue != null)
                this.dataQueue.Enqueue(data);
            else
                this.framework.EventServer.OnData(data);
        }

        protected internal void EmitHistoricalData(HistoricalData data)
        {
            OpenHistricalQueue();
            this.historicalQueue.Enqueue(data);
        }

        protected internal void EmitHistoricalDataEnd(HistoricalDataEnd end)
        {
            OpenHistricalQueue();
            this.historicalQueue.Enqueue(end);
            CloseHistricalQueue();
        }

        protected internal void EmitHistoricalDataEnd(string requestId, RequestResult result, string text)
        {
            EmitHistoricalDataEnd(new HistoricalDataEnd { RequestId = requestId, Result = result, Text = text });
        }

        protected internal void EmitInstrumentDefinition(InstrumentDefinition definition)
        {
            OpenInstrumentQueue();
            this.instrumentQueue.Enqueue(new OnInstrumentDefinition(definition));
        }

        protected internal void EmitInstrumentDefinitionEnd(InstrumentDefinitionEnd end)
        {
            OpenInstrumentQueue();
            this.instrumentQueue.Enqueue(new OnInstrumentDefinitionEnd(end));
            CloseInstrumentQueue();
        }

        protected internal void EmitInstrumentDefinitionEnd(string requestId, RequestResult result, string text)
        {
            EmitInstrumentDefinitionEnd(new InstrumentDefinitionEnd { RequestId = requestId, Result = result, Text = text });
        }

        protected internal void EmitProviderError(ProviderError error) => this.framework.EventServer.OnProviderError(error);

        protected internal void EmitMessage(string text) => EmitMessage(-1, -1, text);

        protected internal void EmitMessage(int id, int code, string text)
        {
            EmitProviderError(new ProviderError(this.framework.Clock.DateTime, ProviderErrorType.Message, this.id, id, code, text));
        }

        protected internal void EmitWarning(string text) => EmitWarning(-1, -1, text);

        protected internal void EmitWarning(int id, int code, string text)
        {
            EmitProviderError(new ProviderError(this.framework.Clock.DateTime, ProviderErrorType.Warning, this.id, id, code, text));
        }

        protected internal void EmitError(string text) => EmitError(-1, -1, text);

        protected internal void EmitError(Exception exception) => EmitError(exception.ToString());

        protected internal void EmitError(string format, params object[] args) => EmitError(string.Format(format, args));

        protected internal void EmitError(int id, int code, string text)
        {
            EmitProviderError(new ProviderError(this.framework.Clock.DateTime, ProviderErrorType.Error, this.id, id, code, text));
        }

        #endregion

        #region Pipe Management

        private void OpenHistricalQueue()
        {
            if (this.historicalQueue == null)
            {
                this.historicalQueue = new EventQueue(EventQueueId.All, EventQueueType.Master, EventQueuePriority.Normal, 1024, this.framework.EventBus);
                this.historicalQueue.Name = $"{this.name} historical queue";
                this.historicalQueue.Enqueue(new OnQueueOpened(this.historicalQueue));
                this.framework.EventBus.HistoricalPipe.Add(this.historicalQueue);
            }
        }

        private void CloseHistricalQueue()
        {
            if (this.historicalQueue != null)
            {
                this.historicalQueue.Enqueue(new OnQueueClosed(this.historicalQueue));
                this.historicalQueue = null;
            }
        }

        private void OpenInstrumentQueue()
        {
            if (this.instrumentQueue == null)
            {
                this.instrumentQueue = new EventQueue(EventQueueId.All, EventQueueType.Master, EventQueuePriority.Normal, 1024, this.framework.EventBus);
                this.instrumentQueue.Name = $"{this.name} instrument queue";
                this.instrumentQueue.Enqueue(new OnQueueOpened(this.instrumentQueue));
                this.framework.EventBus.ServicePipe.Add(this.instrumentQueue);
            }
        }

        private void CloseInstrumentQueue()
        {
            if (this.instrumentQueue != null)
            {
                this.instrumentQueue.Enqueue(new OnQueueClosed(this.instrumentQueue));
                this.instrumentQueue = null;
            }
        }

        #endregion

        [Conditional("DEBUG")]
        private void WriteDebugInfoInternal(string format, params object[] args)
        {
            Console.WriteLine($"[{Name}] {string.Format(format, args)}");
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

        public override string ToString() => $"provider id = {Id} {Name} ({Description}) {Url}";
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
