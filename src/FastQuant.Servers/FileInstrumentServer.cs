namespace SmartQuant
{
    public class FileInstrumentServer : InstrumentServer
    {
        public FileInstrumentServer(Framework framework, string fileName, string host = null, int port = -1): base(framework)
        {
        }
    }
}