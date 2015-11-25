// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace SmartQuant
{
    public enum EventBusIdleMode
    {
        Spin,
        Sleep,
        Wait
    }

    public enum EventBusMode
    {
        Realtime,
        Simulation
    }

    public class EventBus
    {
        private Framework framework;

        internal EventBusIdleMode IdleMode { get; set; } = EventBusIdleMode.Wait;
        internal EventBusMode Mode { get; set; } = EventBusMode.Simulation;
        public EventPipe DataPipe { get; private set; }
        public EventPipe ServicePipe { get; private set; }
        public EventPipe HistoricalPipe { get; private set; }
        public EventPipe ExecutionPipe { get; private set; }

        public EventBus(Framework framework)
        {
            this.framework = framework;
            Mode = framework.Mode == FrameworkMode.Realtime ? EventBusMode.Realtime : EventBusMode.Simulation；
            this.inputEventPipe = new EventPipe(framework, false);
            DataPipe = new EventPipe(framework, false);
            ExecutionPipe = new EventPipe(framework, false);
            ServicePipe  = new EventPipe(framework, false);
            HistoricalPipe = new EventPipe(framework, false);
        }
    }
}