
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
            //if (provider is IDataProvider)
            //{
            //    this.providerList_1.Add(provider);
            //}
            //if (provider is GInterface3)
            //{
            //    this.providerList_2.Add(provider);
            //}
            //if (provider is IHistoricalDataProvider)
            //{
            //    this.providerList_4.Add(provider);
            //}
            //if (provider is GInterface5)
            //{
            //    this.providerList_3.Add(provider);
            //}
            //if (provider is INewsProvider)
            //{
            //    this.providerList_5.Add(provider);
            //}
            //if (provider is GInterface1)
            //{
            //    this.providerList_6.Add(provider);
            //}
            //this.LoadSettings(provider);
            this.framework.EventServer.OnProviderAdded(provider);
        }

        public IDataProvider GetDataProvider(int id)
        {
            throw new NotImplementedException();
        }

        public IDataProvider GetDataProvider(string name)
        {
            throw new NotImplementedException();
        }

        public IExecutionProvider GetExecutionProvider(int id)
        {
            throw new NotImplementedException();
        }

        public IExecutionProvider GetExecutionProvider(string name)
        {
            throw new NotImplementedException();
        }

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
            throw new NotImplementedException();
        }

        public void LoadSettings(IProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}