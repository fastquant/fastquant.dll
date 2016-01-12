
using System;
using System.Linq;

namespace SmartQuant
{
    public class ProviderManager
    {
        private Framework framework;

        public ProviderList Providers { get; } = new ProviderList();

        public IDataSimulator DataSimulator { get; internal set; }

        public IExecutionSimulator ExecutionSimulator { get; internal set; }

        public ProviderManager(Framework framework, IDataSimulator dataSimulator = null, IExecutionSimulator executionSimulator = null)
        {
            this.framework = framework;
        }

        public IDataProvider GetDataProvider(string name)
        {
            throw new NotImplementedException();
        }

        public IExecutionProvider GetExecutionProvider(string name)
        {
            throw new NotImplementedException();
        }

        public void DisconnectAll() => Providers.TakeWhile(provider => provider.IsConnected).ToList().ForEach(provider=>provider.Disconnect());

        public IProvider GetProvider(int id)
        {
            return Providers.GetById(id);
        }

        public IProvider GetProvider(string name)
        {
            return Providers.GetByName(name);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}