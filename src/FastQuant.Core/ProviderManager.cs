
using System;

namespace SmartQuant
{


    public class ProviderManager
    {
        public IDataSimulator DataSimulator { get; internal set; }
        public IExecutionSimulator ExecutionSimulator { get; internal set; }


        public IDataProvider GetDataProvider(string name)
        {
            throw new NotImplementedException();
        }
        public IExecutionProvider GetExecutionProvider(string name)
        {
            throw new NotImplementedException();
        }
    }
}