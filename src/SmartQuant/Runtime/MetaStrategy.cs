using System.Collections.Generic;

namespace SmartQuant
{
    public class MetaStrategy : Strategy
    {
        internal List<Strategy> list_1 = new List<Strategy>();

        private IdArray<List<Strategy>> idArray_3 = new IdArray<List<Strategy>>();

        private IdArray<Strategy> idArray_4 = new IdArray<Strategy>();

        private IdArray<Strategy> idArray_5 = new IdArray<Strategy>();

        public MetaStrategy(Framework framework, string name) : base(framework, name)
        {
        }

        public void Add(Strategy strategy)
        {
            this.list_1.Add(strategy);
            strategy.Portfolio.Parent = Portfolio;
            foreach (Instrument current in strategy.Instruments)
            {
                List<Strategy> list;
                if (this.idArray_3[current.Id] == null)
                {
                    list = new List<Strategy>();
                    this.idArray_3[current.Id] = list;
                }
                else
                {
                    list = this.idArray_3[current.Id];
                }
                list.Add(strategy);
                if (!Instruments.Contains(current))
                {
                    Instruments.Add(current);
                }
            }
        }
    }

}
