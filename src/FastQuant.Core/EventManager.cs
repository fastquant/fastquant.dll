namespace SmartQuant
{
    public class EventFilter
    {
        private Framework framework;

        public EventFilter(Framework framework)
        {
            this.framework = framework;
        }

        public virtual Event Filter(Event e)
        {
            return e;
        }
    }

    public enum EventManagerStatus
    {
        Running,
        Paused,
        Stopping,
        Stopped
    }

    public class EventManager
    {
        public BarFactory BarFactory { get; internal set; }
    }
}