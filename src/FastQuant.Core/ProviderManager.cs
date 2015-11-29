
using System;
using System.Linq;

namespace SmartQuant
{


    public class ProviderManager
    {
        public ProviderList Providers { get; } = new ProviderList();

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

        public void DisconnectAll() => Providers.TakeWhile(provider => provider.IsConnected).ToList().ForEach(provider=>provider.Disconnect());
    }
}