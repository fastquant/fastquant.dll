// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace SmartQuant
{
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

    public interface IProvider
    {
        ProviderStatus Status { get; }

        bool IsConnected { get; }

        bool IsDisconnected { get; }

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

    public class Provider : IProvider
    {
        protected internal byte id;
        protected internal string name;
        protected internal string description;
        protected internal string url;

        [Category("Information")]
        public byte Id => this.id;

        [Category("Information")]
        public string Name => this.name;

        [Category("Information")]
        public string Description => this.description;

        [Category("Information")]
        public string Url => this.url;

        public bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDisconnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ProviderStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public bool Connect(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
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

        public bool IsExecutionProvider=> ProviderType.HasFlag(ProviderType.ExecutionProvider);

        public bool IsFundamentalProvider=> ProviderType.HasFlag(ProviderType.FundamentalProvider);

        public bool IsInstrumentProvider=>ProviderType.HasFlag(ProviderType.InstrumentProvider);

        public bool IsHistoricalDataProvider=> ProviderType.HasFlag(ProviderType.HistoricalDataProvider);

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
}
