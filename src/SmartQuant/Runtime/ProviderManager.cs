
using System;
using System.Linq;

namespace SmartQuant
{
    public class ProviderManager
    {
        private IDataSimulator dataSimulator;
        private IExecutionSimulator executionSimulator;

        private Framework framework;

        public ProviderList Providers { get; } = new ProviderList();

        public IDataSimulator DataSimulator
        {
            get
            {
                return this.dataSimulator;
            }
            set
            {
                if (!Providers.Contains(value))
                    AddProvider(value);
                this.dataSimulator = value;
            }
        }


        public IExecutionSimulator ExecutionSimulator
        {
            get
            {
                return this.executionSimulator;
            }
            set
            {
                if (!Providers.Contains(value))
                    AddProvider(value);
                this.executionSimulator = value;
            }
        }

        public ProviderManager(Framework framework, IDataSimulator dataSimulator = null, IExecutionSimulator executionSimulator = null)
        {
            this.framework = framework;
            this.dataSimulator = dataSimulator ?? new DataSimulator(framework);
            AddProvider(this.dataSimulator);
            this.executionSimulator = executionSimulator ?? new ExecutionSimulator(framework);
            AddProvider(this.executionSimulator);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisconnectAll();
                Providers.TakeWhile(p => p is Provider).ToList().ForEach(p => ((Provider)p).Dispose());
            }
        }

        public void AddProvider(IProvider provider)
        {
            if (provider.Id > 100)
            {
                Console.WriteLine($"ProviderManager::AddProvider Error. Provider Id must be smaller than 100. You are trying to add provider with Id = {provider.Id}");
                return;
            }
            Providers.Add(provider);
            LoadSettings(provider);
            this.framework.EventServer.OnProviderAdded(provider);
        }

        public IDataProvider GetDataProvider(int id) => GetProvider(id) as IDataProvider;

        public IDataProvider GetDataProvider(string name) => GetProvider(name) as IDataProvider;

        public IExecutionProvider GetExecutionProvider(int id) => GetProvider(id) as IExecutionProvider;

        public IExecutionProvider GetExecutionProvider(string name) => GetProvider(name) as IExecutionProvider;

        public IFundamentalProvider GetFundamentalProvider(int id) => GetProvider(id) as IFundamentalProvider;
 
        public IFundamentalProvider GetFundamentalProvider(string name) => GetProvider(name) as IFundamentalProvider;

        public IHistoricalDataProvider GetHistoricalDataProvider(int id) => GetProvider(id) as IHistoricalDataProvider;

        public IHistoricalDataProvider GetHistoricalDataProvider(string name) => GetProvider(name) as IHistoricalDataProvider;

        public IInstrumentProvider GetInstrumentProvider(int id) => GetProvider(id) as IInstrumentProvider;

        public IInstrumentProvider GetInstrumentProvider(string name) => GetProvider(name) as IInstrumentProvider;

        public INewsProvider GetNewsProvider(int id) => GetProvider(id) as INewsProvider;

        public INewsProvider GetNewsProvider(string name) => GetProvider(name) as INewsProvider;

        public void SetDataSimulator(string name) => this.dataSimulator = GetProvider(name) as IDataSimulator;

        public void SetDataSimulator(int id) => this.dataSimulator = GetProvider(id) as IDataSimulator;

        public void SetExecutionSimulator(string name) => this.executionSimulator = GetProvider(name) as IExecutionSimulator;

        public void SetExecutionSimulator(int id) => this.executionSimulator = GetProvider(id) as IExecutionSimulator;

        public void DisconnectAll() => Providers.TakeWhile(provider => provider.IsConnected).ToList().ForEach(provider => provider.Disconnect());

        public IProvider GetProvider(int id) => Providers.GetById(id);

        public IProvider GetProvider(string name) => Providers.GetByName(name);

        public void Clear() => Providers.TakeWhile(p => p is Provider).ToList().ForEach(p => ((Provider)p).Clear());

        public void SaveSettings(IProvider provider)
        {
            //TODO: do something
        }

        public void LoadSettings(IProvider provider)
        {
            //TODO: do something
        }
    }
}