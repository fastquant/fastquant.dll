// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SmartQuant
{
    public interface IEventQueue
    {
        byte Id { get; }

        byte Type { get; }

        bool IsSynched { get; set; }

        string Name { get; }

        byte Priority { get; }

        long Count { get; }

        long FullCount { get; }

        long EmptyCount { get; }

        Event Peek();

        DateTime PeekDateTime();

        Event Read();

        void Write(Event obj);

        Event Dequeue();

        void Enqueue(Event obj);

        bool IsEmpty();

        bool IsFull();

        void Clear();

        void ResetCounts();
    }

    public class EventQueueId
    {
        public const byte All = 0;
        public const byte Data = 1;
        public const byte Execution = 2;
        public const byte Reminder = 3;
        public const byte Service = 4;
    }

    public class EventQueuePriority
    {
        public const byte Highest = 0;
        public const byte High = 1;
        public const byte Normal = 2;
        public const byte Low = 3;
        public const byte Lowest = 4;

    }

    public class EventQueueType
    {
        public const byte Master = 0;
        public const byte Slave = 1;
    }

    // This implemetation is not thread-safe. Use carefully!
    public class EventQueue : IComparable<IEventQueue>, IEventQueue
    {
        private volatile int readPosition;
        private volatile int writePosition;
        private Event[] events;
        internal EventBus bus;

        public byte Id { get; private set; }

        public byte Type { get; private set; }

        public int Size { get; private set; }

        public bool IsSynched { get; set; }

        public string Name { get; internal set; }

        public byte Priority { get; private set; }

        public long Count => EnqueueCount - DequeueCount;

        public long EnqueueCount { get; private set; }

        public long DequeueCount { get; private set; }

        public long FullCount { get; private set; }

        public long EmptyCount { get; private set; }

        public EventQueue(byte id = EventQueueId.All, byte type = EventQueueType.Master, byte priority = EventQueuePriority.Normal, int size = 100000, EventBus bus = null)
        {
            Id = id;
            Type = type;
            Priority = priority;
            Size = size;
            this.bus = bus;
            this.events = new Event[Size];
        }

        public void Clear()
        {
            this.readPosition = this.writePosition = 0;
            EmptyCount = FullCount = EnqueueCount = DequeueCount = 0;
            Array.Clear(this.events, 0, this.events.Length);
        }

        public Event Peek() => this.events[this.readPosition];

        public DateTime PeekDateTime() => Peek().DateTime;

        public Event Read()
        {
            Event e = Peek();
            this.readPosition = (this.readPosition + 1) % Size;
            ++DequeueCount;
            return e;
        }

        public void Write(Event obj)
        {
            if (obj == null)
            {
                Console.WriteLine($"EventQueue::Write Error. Can not write null object to the queue {Name}");
                return;
            }

            this.events[this.writePosition] = obj;
            this.writePosition = (this.writePosition + 1) % Size;
            ++EnqueueCount;
            if (Count == 1 && this.bus != null && this.bus.IdleMode == EventBusIdleMode.Wait)
            {
                this.bus.manualResetEventSlim_0.Set();
            }
        }

        public Event Dequeue()
        {
            while (IsEmpty())
            {
                ++EmptyCount;
                Thread.Sleep(1);
            }
            return Read();
        }

        public void Enqueue(Event obj)
        {
            while (IsFull())
            {
                ++FullCount;
                Thread.Sleep(1);
            }
            Write(obj);
        }

        public bool IsEmpty() => this.readPosition == this.writePosition;

        public bool IsFull() => (this.writePosition + 1) % Size == this.readPosition;


        public void ResetCounts() => FullCount = EmptyCount = 0;

        public int CompareTo(IEventQueue other) => PeekDateTime().CompareTo(other.PeekDateTime());

        public override string ToString() => $"Id: {Id} Count = {Count} Enqueue = {EnqueueCount} Dequeue = {DequeueCount}";

        internal void Enqueue(Event[] events)
        {
            foreach (var e in events)
                Enqueue(e);
        }
    }

    public class EventSortedSet : IEnumerable
    {
        private List<Event> events = new List<Event>();

        public string Name { get; set; }

        public string Description { get; set; }

        public EventSortedSet()
        {
        }

        public int Count => this.events.Count;

        public Event this[int index] => this.events[index];

        public void Add(Event e)
        {
            // Don't care what finding algorithm it uses at the moment.
            var i = this.events.FindIndex(new Predicate<Event>(evt => evt.DateTime > e.DateTime));
            if (i == -1)
                this.events.Add(e);
            else
                this.events.Insert(i, e);
        }

        public void Clear()
        {
            this.events.Clear();
        }

        internal Event Pop()
        {
            var e = this[0];
            this.events.RemoveAt(0);
            return e;
        }

        public IEnumerator GetEnumerator()
        {
            return this.events.GetEnumerator();
        }
    }

    public class SortedEventQueue : IComparable<IEventQueue>, IEventQueue
    {
        internal EventSortedSet events;
        internal DateTime dateTime;

        public byte Id { get; private set; }

        public byte Type { get; private set; }

        public bool IsSynched { get; set; }

        public string Name { get; private set; }

        public byte Priority { get; private set; }

        public long Count
        {
            get
            {
                return this.events.Count;
            }
        }

        public long EmptyCount
        {
            get
            {
                throw new NotImplementedException("Not implemented in SortedEventQueue");
            }
        }

        public long FullCount
        {
            get
            {
                throw new NotImplementedException("Not implemented in SortedEventQueue");
            }
        }

        public SortedEventQueue(byte id, byte type = EventQueueType.Master, byte priority = EventQueuePriority.Normal)
        {
            Id = id;
            Type = type;
            Priority = priority;
            this.events = new EventSortedSet();
        }

        public Event Peek()
        {
            lock (this)
                return this.events[0];
        }

        public DateTime PeekDateTime()
        {
            return this.dateTime;
        }

        public Event Read()
        {
            Event e;
            lock (this)
            {
                e = this.events.Pop();
                if (this.events.Count > 0)
                    this.dateTime = this.events[0].DateTime;
            }
            return e;
        }

        public void Write(Event e)
        {
            throw new NotImplementedException("Not implemented in SortedEventQueue");
        }

        public Event Dequeue()
        {
            return Read();
        }

        public void Enqueue(Event e)
        {
            lock (this)
            {
                this.events.Add(e);
                this.dateTime = this.events[0].DateTime;
            }
        }

        public bool IsEmpty()
        {
            return this.events.Count == 0;
        }

        public bool IsFull()
        {
            return false;
        }

        public void Clear()
        {
            this.events.Clear();
        }

        public void ResetCounts()
        {
            // no-op
        }

        public int CompareTo(IEventQueue other)
        {
            return PeekDateTime().CompareTo(other.PeekDateTime());
        }
    }
}