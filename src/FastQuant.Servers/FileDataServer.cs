using System.Collections.Generic;

namespace SmartQuant
{
    public class FileDataServer : DataServer
    {
        public FileDataServer(Framework framework, string fileName, string host = null, int port = -1):base(framework)
        {
        }
    }
}