// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

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

    public class Provider : IProvider
    {
        public byte Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public string Name
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
}
