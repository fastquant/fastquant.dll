// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class FrameworkEventArgs : EventArgs
    {
        public Framework Framework { get; private set; }

        public FrameworkEventArgs(Framework framework)
        {
            Framework = framework;
        }
    }

    public delegate void FrameworkEventHandler(object sender, FrameworkEventArgs args);

    public enum FrameworkMode
    {
        Simulation,
        Realtime
    }

    public class Framework : IDisposable
    {
        public static Framework Current { get; set; }
        public AccountDataManager AccountDataManager { get; internal set; }
        public Clock Clock { get; internal set; }
        public DataFileManager DataFileManager { get; internal set; }
        public DataManager DataManager { get; internal set; }
        public EventManager EventManager { get; internal set; }
        public GroupManager GroupManager { get; internal set; }
        public InstrumentManager InstrumentManager { get; internal set; }
        public FrameworkMode Mode { get; internal set; }
        public OrderManager OrderManager { get; internal set; }
        public ProviderManager ProviderManager { get; internal set; }
        public StatisticsManager StatisticsManager { get; internal set; }
        public StrategyManager StrategyManager { get; internal set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }

    public class FrameworkServer : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}