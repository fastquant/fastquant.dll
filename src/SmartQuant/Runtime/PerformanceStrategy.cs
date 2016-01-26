// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace SmartQuant
{
    public class PerformanceProvider : Provider, IDataProvider, IExecutionProvider
    {
        public PerformanceProvider(Framework framework) : base(framework)
        {
        }
    }


    public class PerformanceStrategy : InstrumentStrategy
    {
        private Stopwatch stopwatch = new Stopwatch();

        public PerformanceStrategy(Framework framework) : base(framework, "PerformanceStrategy")
        {
        }

        protected internal override void OnStrategyStart()
        {
            var data = new Trade();
            this.stopwatch.Start();
            (DataProvider as PerformanceProvider).EmitData(data, true);
        }

        protected override void OnTrade(Instrument instrument, Trade trade)
        {
            this.stopwatch.Stop();
        }
    }
}
