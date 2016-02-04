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
        private EventQueue[] attached = new EventQueue[1024];

        private int attachedCount;

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
            for (int i = 0; i < this.attachedCount; i++)
                this.attached[i] = null;
            this.attachedCount = 0;
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
            this.attached[this.attachedCount++] = q;
        }

        public void Detach(EventBus bus)
        {
            string name = "attached " + bus.framework.Name;
            var index = Array.FindIndex(this.attached, 0, this.attachedCount, q => q.Name == name);
            if (index != -1)
            {
                var found = this.attached[index];
                for (int i = index; i < this.attachedCount - 1; ++i)
                    this.attached[i] = this.attached[i + 1];
                this.attachedCount--;
                bus.DataPipe.Remove(found);
            }
            else
                Console.WriteLine($"EventBus::Detach Can not find attached bus queue : {bus.framework.Name}");
        }

        public Event Dequeue()
        {
            return Dequeue_origin();
        }
        private Event Dequeue_me()
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
                        for (int i = 0; i < this.attachedCount; i++)
                            if (e.TypeId != EventType.OnQueueOpened && e.TypeId != EventType.OnQueueClosed)
                                this.attached[i].Enqueue(e);
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


        private Event Dequeue_origin()
        {
            if (this.Mode == EventBusMode.Simulation)
            {
                while (true)
                {
                    if (!DataPipe.IsEmpty() && this.@event == null)
                    {
                        Event @event = DataPipe.Read();
                        if (@event.dateTime < this.framework.Clock.DateTime)
                        {
                            if (@event.TypeId != 205 && @event.TypeId != 206 && @event.TypeId != 229)
                            {
                                if (@event.TypeId != 230)
                                {
                                    Console.WriteLine(string.Concat(new object[]
                                    {
                                "EventBus::Dequeue Skipping: ",
                                @event,
                                " ",
                                @event.dateTime,
                                " <> ",
                                this.framework.Clock.DateTime
                                    }));
                                    continue;
                                }
                            }
                            @event.dateTime = this.framework.Clock.DateTime;
                            this.@event = @event;
                        }
                        else
                        {
                            this.@event = @event;
                        }
                    }
                    if (!ExecutionPipe.IsEmpty())
                    {
                        goto IL_25D;
                    }
                    if (!LocalClockEventQueue.IsEmpty() && this.@event != null)
                    {
                        if (ReminderOrder == ReminderOrder.Before)
                        {
                            if (LocalClockEventQueue.PeekDateTime() <= this.@event.DateTime)
                            {
                                break;
                            }
                        }
                        else if (LocalClockEventQueue.PeekDateTime() < this.@event.DateTime)
                        {
                            goto IL_275;
                        }
                    }
                    if (!ExchangeClockEventQueue.IsEmpty() && this.@event != null && (this.@event.TypeId == 2 || this.@event.TypeId == 3 || this.@event.TypeId == 4))
                    {
                        if (ReminderOrder == ReminderOrder.Before)
                        {
                            if (ExchangeClockEventQueue.PeekDateTime() <= ((Tick)this.@event).ExchangeDateTime)
                            {
                                goto Block_12;
                            }
                        }
                        else if (ExchangeClockEventQueue.PeekDateTime() < ((Tick)this.@event).ExchangeDateTime)
                        {
                            goto IL_28D;
                        }
                    }
                    if (!CommandPipe.IsEmpty())
                    {
                        goto IL_299;
                    }
                    if (!ServicePipe.IsEmpty())
                    {
                        goto IL_2A5;
                    }
                    if (this.@event != null)
                    {
                        goto IL_2B1;
                    }
                    Thread.Sleep(1);
                }
                return LocalClockEventQueue.Read();
                Block_12:
                return ExchangeClockEventQueue.Read();
                IL_25D:
                return ExecutionPipe.Read();
                IL_275:
                return LocalClockEventQueue.Read();
                IL_28D:
                return ExchangeClockEventQueue.Read();
                IL_299:
                return CommandPipe.Read();
                IL_2A5:
                return ServicePipe.Read();
                IL_2B1:
                Event event2 = this.@event;
                this.@event = null;
                for (int i = 0; i < this.attachedCount; i++)
                {
                    if (event2.TypeId != 205 && event2.TypeId != 206)
                    {
                        this.attached[i].Enqueue(event2);
                    }
                }
                return event2;
            }
            while (true)
            {
                if (!DataPipe.IsEmpty() && this.@event == null)
                {
                    this.@event = DataPipe.Read();
                }
                if (!LocalClockEventQueue.IsEmpty())
                {
                    if (ReminderOrder == ReminderOrder.Before)
                    {
                        if (LocalClockEventQueue.PeekDateTime() <= this.framework.Clock.DateTime)
                        {
                            break;
                        }
                    }
                    else if (LocalClockEventQueue.PeekDateTime() < this.framework.Clock.DateTime)
                    {
                        goto IL_4AF;
                    }
                }
                if (!ExchangeClockEventQueue.IsEmpty() && this.@event != null && (this.@event.TypeId == 2 || this.@event.TypeId == 3 || this.@event.TypeId == 4))
                {
                    if (ReminderOrder == ReminderOrder.Before)
                    {
                        if (ExchangeClockEventQueue.PeekDateTime() <= ((Tick)this.@event).ExchangeDateTime)
                        {
                            goto Block_34;
                        }
                    }
                    else if (ExchangeClockEventQueue.PeekDateTime() < ((Tick)this.@event).ExchangeDateTime)
                    {
                        goto IL_4C7;
                    }
                }
                if (!ExecutionPipe.IsEmpty())
                {
                    goto IL_4D3;
                }
                if (!CommandPipe.IsEmpty())
                {
                    goto IL_4DF;
                }
                if (!ServicePipe.IsEmpty())
                {
                    goto IL_4EB;
                }
                if (this.@event != null)
                {
                    goto IL_4F7;
                }
                switch (this.IdleMode)
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
            return LocalClockEventQueue.Read();
            Block_34:
            return ExchangeClockEventQueue.Read();
            IL_4AF:
            return LocalClockEventQueue.Read();
            IL_4C7:
            return ExchangeClockEventQueue.Read();
            IL_4D3:
            return ExecutionPipe.Read();
            IL_4DF:
            return CommandPipe.Read();
            IL_4EB:
            return ServicePipe.Read();
            IL_4F7:
            Event result = this.@event;
            this.@event = null;
            return result;
        }

        [NotOriginal]
        internal void TryWakeUpIdle()
        {
            if (IdleMode == EventBusIdleMode.Wait)
                this.hasWorkEvent.Set();
        }
    }
}