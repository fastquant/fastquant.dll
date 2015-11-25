namespace SmartQuant
{
    public class EventPipe
    {
        private Framework framework;
        private bool v;

        public EventPipe(Framework framework, bool v)
        {
            this.framework = framework;
            this.v = v;
        }
    }
}