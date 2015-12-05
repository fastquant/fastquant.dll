using System;

namespace SmartQuant
{
    public interface IEventClient
    {
        int Id { get; }

        byte[] EventTypes { get; }

        bool IsOnEventEnabled { get; set; }

        bool IsOnQueueEnabled { get; set; }

        void OnEvent(Event e);

        void OnQueue();

        void Emit(Event e);
    }

    public class EventClient : IEventClient
    {
        private static int counter;
        private EventDispatcher dispatcher;

        public byte[] EventTypes { get; } = new byte[0];

        public int Id { get; }

        public bool IsOnEventEnabled { get; set; }

        public bool IsOnQueueEnabled { get; set; }

        public EventClient(EventDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            Id = counter++;
            dispatcher.Add(this);
        }

        public void Emit(Event e)
        {
            if (e.TypeId == EventType.Command)
                ((Command)e).SenderId = Id;

            this.dispatcher.Emit(e);
        }

        public void OnEvent(Event e)
        {
            // noop
        }

        public void OnQueue()
        {
            // noop
        }
    }
}