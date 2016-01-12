namespace SmartQuant
{
    public class PortfolioServer
    {
        protected Framework framework;

        public PortfolioServer(Framework framework)
        {
            this.framework = framework;
        }

        public virtual void Close()
        {
        }

        public virtual void Delete(string name)
        {
        }

        public virtual void Dispose()
        {
            Close();
        }

        public virtual void Flush()
        {
        }

        public virtual Portfolio Load(string name)
        {
            return null;
        }

        public virtual void Open()
        {
        }

        public virtual void Save(Portfolio portfolio)
        {
        }

    }

}