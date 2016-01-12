using System;
using System.IO;

namespace SmartQuant
{
    public class OutputManager
    {
        private Framework framework;

        public OutputManager(Framework framework, string path = null)
        {
            this.framework = framework;
        }

        public void Clear()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            //throw new NotImplementedException();
        }
    }
}