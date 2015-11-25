
namespace SmartQuant
{


    public class ProviderManager
    {
        public IDataSimulator DataSimulator { get; internal set; }
        public IExecutionSimulator ExecutionSimulator { get; internal set; }
    }
}