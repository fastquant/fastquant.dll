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
        public void Dispose()
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