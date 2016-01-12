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
}