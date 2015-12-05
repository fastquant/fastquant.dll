// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SmartQuant
{
    // This implemetation is not thread-safe. Use carefully!
    public class EventQueue : IComparable<IEventQueue>, IEventQueue
    {
        private volatile int readPosition;
        private volatile int writePosition;
        private Event[] events;
        internal EventBus bus;

        public byte Id { get; }

        public byte Type { get; }

        public int Size { get; }

        public bool IsSynched { get; set; }

        public string Name { get; set; }

        public byte Priority { get; }

        public long Count => EnqueueCount - DequeueCount;

        public long EnqueueCount { get; private set; }

        public long DequeueCount { get; private set; }

        public long FullCount { get; private set; }

        public long EmptyCount { get; private set; }

        public EventQueue(byte id = EventQueueId.All, byte type = EventQueueType.Master, byte priority = EventQueuePriority.Normal, int size = 102400, EventBus bus = null)
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
            var e = Peek();
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
            var i = this.events.FindIndex(evt => evt.DateTime > e.DateTime);
            if (i == -1)
                this.events.Add(e);
            else
                this.events.Insert(i, e);
        }

        public void Clear()
        {
            this.events.Clear();
        }

        internal void RemoveAt(int index)
        {
            this.events.RemoveAt(0);
        }

        public IEnumerator GetEnumerator()
        {
            return this.events.GetEnumerator();
        }
    }

    public class SortedEventQueue : IComparable<IEventQueue>, IEventQueue
    {
        protected internal EventSortedSet events = new EventSortedSet();
        protected internal DateTime dateTime;

        public byte Id { get; }

        public byte Type { get; }

        public bool IsSynched { get; set; }

        public string Name { get; }

        public byte Priority { get; }

        public long Count => this.events.Count;

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
        }

        public void Clear()
        {
            this.events.Clear();
        }

        public Event Peek()
        {
            lock (this)
                return this.events[0];
        }

        public DateTime PeekDateTime() => this.dateTime;

        public Event Read()
        {
            Event e;
            lock (this)
            {
                e = this.events[0];
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

        public bool IsEmpty() => this.events.Count == 0;

        public bool IsFull() => false;

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