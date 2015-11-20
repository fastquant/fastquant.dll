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
        public byte[] EventTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsOnEventEnabled
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

        public bool IsOnQueueEnabled
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

        public void Emit(Event e)
        {
            throw new NotImplementedException();
        }

        public void OnEvent(Event e)
        {
            throw new NotImplementedException();
        }

        public void OnQueue()
        {
            throw new NotImplementedException();
        }
    }
}