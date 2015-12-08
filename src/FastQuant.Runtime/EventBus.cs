// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private EventQueue[] queues = new EventQueue[1024];
        private Event @event;
        private int queueCount;

        internal IEventQueue LocalClockEventQueue { get; set; }
        internal IEventQueue ExchangeClockEventQueue { get; set; }

        internal EventBusIdleMode IdleMode { get; set; } = EventBusIdleMode.Wait;
        internal EventBusMode Mode { get; set; }
        private ManualResetEventSlim hasWorkEvent = new ManualResetEventSlim(false);

        internal EventPipe CommandPipe { get; }
        public EventPipe DataPipe { get; }
        public EventPipe HistoricalPipe { get; }
        public EventPipe ExecutionPipe { get; }
        public EventPipe ServicePipe { get; }

        public EventBus(Framework framework)
        {
            this.framework = framework;
            Mode = framework.Mode == FrameworkMode.Realtime ? EventBusMode.Realtime : EventBusMode.Simulation;
            CommandPipe = new EventPipe(framework, false);
            DataPipe = new EventPipe(framework, false);
            ExecutionPipe = new EventPipe(framework, false);
            ServicePipe  = new EventPipe(framework, false);
            HistoricalPipe = new EventPipe(framework, false);
        }

        public void Clear()
        {
            @event = null;
            CommandPipe.Clear();
            DataPipe.Clear();
            ServicePipe.Clear();
            HistoricalPipe.Clear();
            ExecutionPipe.Clear();
            Parallel.ForEach(queues.Take(this.queueCount), queue => queue = null);
            this.queueCount = 0;
        }

        public void ResetCounts()
        {
            // noop
        }

        public void Attach(EventBus bus)
        {
            var q = new EventQueue(EventQueueId.Data, EventQueueType.Master, EventQueuePriority.Normal, 25600, null);
            q.IsSynched = true;
            q.Name = $"attached {bus.framework.Name}";
            q.Enqueue(new OnQueueOpened(q));
            this.queues[this.queueCount++] = q;
            bus.DataPipe.Add(q);
        }

        public void Detach(EventBus bus)
        {
            var name = $"attached {bus.framework.Name}";
            var index = Array.FindIndex(this.queues, q => q.Name == name);
            if (index < 0)
            {
                Console.WriteLine($"EventBus::Detach Can not find attached bus queue : {bus.framework.Name}");
                return;
            }
            var found = this.queues[index];
            for (int i = index; i < this.queueCount; ++i)
                this.queues[i] = this.queues[i + 1];
            bus.DataPipe.Remove(found);
        }

        public Event Dequeue()
        {
            throw new NotImplementedException();
        }

        private bool iStree(Event e)
        {
            return e.TypeId != EventType.OnQueueOpened && e.TypeId != EventType.OnQueueClosed && e.TypeId != EventType.OnSimulatorStop && e.TypeId != EventType.OnSimulatorProgress;
        }
    }
}