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

        private int queueCount;

        private Event @event;

        internal IEventQueue LocalClockEventQueue { get; set; }
        internal IEventQueue ExchangeClockEventQueue { get; set; }

        public EventBusIdleMode IdleMode { get; set; } = EventBusIdleMode.Wait;
        public EventBusMode Mode { get; set; } = EventBusMode.Simulation;
        private ManualResetEventSlim hasWorkEvent = new ManualResetEventSlim(false);

        internal EventPipe CommandPipe { get; }
        public EventPipe DataPipe { get; }
        public EventPipe ExecutionPipe { get; }
        public EventPipe ServicePipe { get; }
        public EventPipe HistoricalPipe { get; }

        internal ReminderOrder ReminderOrder { get; set; }

        public EventBus(Framework framework)
        {
            this.framework = framework;
            Mode = framework.Mode == FrameworkMode.Realtime ? EventBusMode.Realtime : EventBusMode.Simulation;
            CommandPipe = new EventPipe(framework, false);
            DataPipe = new EventPipe(framework, false);
            ExecutionPipe = new EventPipe(framework, false);
            ServicePipe = new EventPipe(framework, false);
            HistoricalPipe = new EventPipe(framework, false);
        }

        public void Clear()
        {
            this.@event = null;
            CommandPipe.Clear();
            DataPipe.Clear();
            ServicePipe.Clear();
            HistoricalPipe.Clear();
            ExecutionPipe.Clear();
            for (int i = 0; i < this.queueCount; i++)
                this.queues[i] = null;
            this.queueCount = 0;
        }

        public void ResetCounts()
        {
            // noop
        }

        public void Attach(EventBus bus)
        {
            var q = new EventQueue(EventQueueId.Data, EventQueueType.Master, EventQueuePriority.Normal, 25600, null)
            {
                IsSynched = true,
                Name = $"attached {bus.framework.Name}"
            };
            q.Enqueue(new OnQueueOpened(q));
            bus.DataPipe.Add(q);
            this.queues[this.queueCount++] = q;
        }

        public void Detach(EventBus bus)
        {
            string name = "attached " + bus.framework.Name;
            var index = Array.FindIndex(this.queues, 0, this.queueCount, q => q.Name == name);
            if (index != -1)
            {
                var found = this.queues[index];
                for (int i = index; i < this.queueCount-1; ++i)
                    this.queues[i] = this.queues[i + 1];
                this.queueCount--;
                bus.DataPipe.Remove(found);
            }
            else
                Console.WriteLine($"EventBus::Detach Can not find attached bus queue : {bus.framework.Name}");
        }

        public Event Dequeue()
        {
            if (Mode == EventBusMode.Simulation)
            {
                while (true)
                {
                    if (!DataPipe.IsEmpty() && this.@event == null)
                    {
                        var e = DataPipe.Read();
                        if (e.dateTime < this.framework.Clock.DateTime)
                        {
                            if (e.TypeId != EventType.OnQueueOpened && e.TypeId != EventType.OnQueueClosed && e.TypeId != EventType.OnSimulatorStop)
                            {
                                if (e.TypeId != EventType.OnSimulatorProgress)
                                {
                                    Console.WriteLine($"EventBus::Dequeue Skipping: {e} {e.dateTime} <> {this.framework.Clock.DateTime}");
                                    continue;
                                }
                            }
                            e.DateTime = this.framework.Clock.DateTime;
                            this.@event = e;
                        }
                        else
                            this.@event = e;
                    }

                    if (!ExecutionPipe.IsEmpty())
                        return this.ExecutionPipe.Read();

                    if (!LocalClockEventQueue.IsEmpty() && this.@event != null)
                    {
                        if (ReminderOrder == ReminderOrder.Before)
                        {
                            if (LocalClockEventQueue.PeekDateTime() <= this.@event.DateTime)
                                return LocalClockEventQueue.Read();
                        }
                        else if (this.LocalClockEventQueue.PeekDateTime() < this.@event.DateTime)
                            return LocalClockEventQueue.Read();
                    }

                    if (!LocalClockEventQueue.IsEmpty() && this.@event != null && (this.@event.TypeId == EventType.Bid || this.@event.TypeId == EventType.Ask || this.@event.TypeId == EventType.Trade))
                    {
                        if (ReminderOrder == ReminderOrder.Before)
                        {
                            if (ExchangeClockEventQueue.PeekDateTime() <= ((Tick)this.@event).ExchangeDateTime)
                                return this.ExchangeClockEventQueue.Read();
                        }
                        else if (ExchangeClockEventQueue.PeekDateTime() < ((Tick)this.@event).ExchangeDateTime)
                            return ExchangeClockEventQueue.Read();
                    }

                    if (!CommandPipe.IsEmpty())
                        return CommandPipe.Read();

                    if (!ServicePipe.IsEmpty())
                        return ServicePipe.Read();

                    if (this.@event != null)
                    {
                        // forward the event to externally attached queues
                        var e = this.@event;
                        this.@event = null;
                        for (int i = 0; i < this.queueCount; i++)
                            if (e.TypeId != EventType.OnQueueOpened && e.TypeId != EventType.OnQueueClosed)
                                this.queues[i].Enqueue(e);
                        return e;
                    }

                    // still not getting any event, try later
                    Thread.Sleep(1);
                }
            }

            // Mode == EventBusMode.Realtime
            while (true)
            {
                if (!DataPipe.IsEmpty() && this.@event == null)
                    this.@event = DataPipe.Read();

                if (!LocalClockEventQueue.IsEmpty())
                {
                    if (ReminderOrder == ReminderOrder.Before)
                    {
                        if (LocalClockEventQueue.PeekDateTime() <= this.framework.Clock.DateTime)
                            return LocalClockEventQueue.Read();
                    }
                    else if (LocalClockEventQueue.PeekDateTime() < this.framework.Clock.DateTime)
                        return LocalClockEventQueue.Read();
                }

                if (!ExchangeClockEventQueue.IsEmpty() && this.@event != null && (this.@event.TypeId == EventType.Bid || this.@event.TypeId == EventType.Ask || this.@event.TypeId == EventType.Trade))
                {
                    if (ReminderOrder == ReminderOrder.Before)
                    {
                        if (ExchangeClockEventQueue.PeekDateTime() <= ((Tick)this.@event).ExchangeDateTime)
                            return this.ExchangeClockEventQueue.Read();
                    }
                    else if (ExchangeClockEventQueue.PeekDateTime() < ((Tick)this.@event).ExchangeDateTime)
                        return ExchangeClockEventQueue.Read();
                }

                if (!ExecutionPipe.IsEmpty())
                    return ExecutionPipe.Read();

                if (!CommandPipe.IsEmpty())
                    return CommandPipe.Read();

                if (!ServicePipe.IsEmpty())
                    return ServicePipe.Read();

                if (this.@event != null)
                {
                    var result = this.@event;
                    this.@event = null;
                    return result;
                }

                // still not getting an event, go idle for a while
                switch (IdleMode)
                {
                    case EventBusIdleMode.Sleep:
                        Thread.Sleep(1);
                        break;
                    case EventBusIdleMode.Wait:
                        this.hasWorkEvent.Reset();
                        this.hasWorkEvent.Wait(1);
                        break;
                }
            }
        }
    }
}