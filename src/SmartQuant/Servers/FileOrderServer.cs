namespace SmartQuant
{
    public class FileOrderServer : OrderServer
    {
        public FileOrderServer(Framework framework, string fileName, string host = null, int port = -1) : base(framework)
        {
        }
    }
}