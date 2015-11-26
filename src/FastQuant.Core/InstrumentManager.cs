namespace SmartQuant
{
    public class InstrumentManager
    {
        private Framework framework;
        private InstrumentServer server;

        public InstrumentList Instruments { get; } = new InstrumentList();

        public InstrumentManager(Framework framework, InstrumentServer server)
        {
            this.framework = framework;
            this.server = server;
        }

    }
}