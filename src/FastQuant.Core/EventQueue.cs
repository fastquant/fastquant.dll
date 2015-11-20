// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

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

    public class EventQueue : IComparable<IEventQueue>, IEventQueue
    {
        public long Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long EmptyCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long FullCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynched
        {
            get
            {
                throw new NotImplementedException();
            }

            set
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

        public byte Priority
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(IEventQueue other)
        {
            throw new NotImplementedException();
        }

        public Event Dequeue()
        {
            throw new NotImplementedException();
        }

        public void Enqueue(Event obj)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public bool IsFull()
        {
            throw new NotImplementedException();
        }

        public Event Peek()
        {
            throw new NotImplementedException();
        }

        public DateTime PeekDateTime()
        {
            throw new NotImplementedException();
        }

        public Event Read()
        {
            throw new NotImplementedException();
        }

        public void ResetCounts()
        {
            throw new NotImplementedException();
        }

        public void Write(Event obj)
        {
            throw new NotImplementedException();
        }
    }
}