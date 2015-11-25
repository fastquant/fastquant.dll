// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace SmartQuant.Optimization
{
    public class Objective
    {
        public double Value { get; set; }
    }

    public class OptimizationParameter
    {
        public string Name { get; }

        public object Value { get; }

        public OptimizationParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class OptimizationParameterSet : IEnumerable<OptimizationParameter>
    {
        private List<OptimizationParameter> parameters = new List<OptimizationParameter>();

        public double Objective { get; set; }

        public Global Global { get; set; } = new Global();

        public OptimizationParameter this[int index] => this.parameters[index];

        public void Add(OptimizationParameter parameter) => this.parameters.Add(parameter);

        public void Add(string name, object value) => this.parameters.Add(new OptimizationParameter(name, value));

        public IEnumerator<OptimizationParameter> GetEnumerator() => this.parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.parameters.GetEnumerator();

        public override string ToString() => string.Join(" ", this.parameters.Select(p => $"{p.Name} = {p.Value}"));
    }

    public class OptimizationUniverse : IEnumerable<OptimizationParameterSet>
    {
        private List<OptimizationParameterSet> sets = new List<OptimizationParameterSet>();

        public int Count => this.sets.Count;

        public OptimizationParameterSet this[int index] => this.sets[index];

        public void Add(OptimizationParameterSet parameter) => this.sets.Add(parameter);

        public void Clear() => this.sets.Clear();

        public IEnumerator<OptimizationParameterSet> GetEnumerator() => this.sets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.sets.GetEnumerator();
    }

    public class MulticoreOptimizer
    {
        private Stopwatch stopwatch = new Stopwatch();

        public long Elapsed
        {
            get
            {
                return this.stopwatch.ElapsedMilliseconds;
            }
        }

        public long EventCount { get; private set; }

        public OptimizationParameterSet Optimize(Strategy strategy, InstrumentList instruments, OptimizationUniverse universe, int bunch = -1)
        {
            EventCount = 0;
            this.stopwatch.Start();
            int int_1 = bunch != -1 ? bunch : universe.Count;
            int int_0 = 0;
            while (int_0 + int_1 < universe.Count)
            {
                Optimize(strategy, instruments, universe, int_0, int_1);
                int_0 += int_1;
            }
            Optimize(strategy, instruments, universe, int_0, universe.Count - int_0);
            int maxIndex = 0;
            for (int i = 1; i < universe.Count; ++i)
                if (universe[i].Objective > universe[maxIndex].Objective)
                    maxIndex = i;

            Console.WriteLine("Best Objective {0}  Objective = {1}", universe[maxIndex], universe[maxIndex].Objective);
            Console.WriteLine("Optimization done");
            this.stopwatch.Stop();
            Console.WriteLine("Processed {0} events in {1} msec - {2} event/sec", EventCount, this.stopwatch.ElapsedMilliseconds, EventCount / (this.stopwatch.ElapsedMilliseconds * 1000d));
            return universe[maxIndex];
        }

        private void Optimize(Strategy strategy, InstrumentList instruments, OptimizationUniverse universe, int nFrameworks, int nStrategies)
        {
            var frameworks = new Framework[nStrategies];
            var strategies = new Strategy[nStrategies];
            for (int i = 0; i < nStrategies; ++i)
            {
                frameworks[i] = i != 0 ? new Framework(string.Format("framework {0}", i), frameworks[i - 1].EventBus, strategy.framework.InstrumentServer, null) : strategy.framework;
                strategies[i] = (Strategy)Activator.CreateInstance(strategy.GetType(), new object[] { frameworks[i], string.Format("strategy {0}", i) });
                foreach (var info in strategy.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
					if (info.GetCustomAttributes(typeof(ParameterAttribute), true).Count() > 0)
                        info.SetValue(strategies[i], info.GetValue(strategy));
                }
            }

            for (int i = 0; i < nStrategies; ++i)
            {
                foreach (var parameter in universe[nFrameworks + i])
                {
                    if (!(parameter.Name == "Bar") && !(parameter.Name == "TimeBar"))
                    {
                        if (parameter.Name == "TickBar")
                        {
                            foreach (var instrument in instruments)
                                frameworks[i].EventManager.BarFactory.Add(instrument, BarType.Tick, (long)parameter.Value, BarInput.Trade, ClockType.Local);
                        }
                        else if (parameter.Name == "VolumeBar")
                        {
                            foreach (var instrument in instruments)
                                frameworks[i].EventManager.BarFactory.Add(instrument, BarType.Volume, (long)parameter.Value, BarInput.Trade, ClockType.Local);
                        }
                        else if (parameter.Name == "RangeBar")
                        {
                            foreach (var instrument in instruments)
                                frameworks[i].EventManager.BarFactory.Add(instrument, BarType.Range, (long)parameter.Value, BarInput.Trade, ClockType.Local);
                        }
                        else
                        {
                            var field = strategies[i].GetType().GetField(parameter.Name, BindingFlags.Instance | BindingFlags.Public
								#if !DNXCORE50
								| BindingFlags.GetField | BindingFlags.SetField
								#endif
							);
                            if (field != null)
                                field.SetValue(strategies[i], parameter.Value);
                            else
                                Console.WriteLine("Optimizer::Optimize Can not set field with name " + parameter.Name);
                        }
                    }
                    else
                    {
                        foreach (var instrument in instruments)
                            frameworks[i].EventManager.BarFactory.Add(instrument, BarType.Time, (long)parameter.Value, BarInput.Trade, ClockType.Local);
                    }
                }
            }

            foreach (var s in strategies)
                s.AddInstruments(instruments);

            for (int i = nStrategies - 1; i >= 0; --i)
                frameworks[i].StrategyManager.StartStrategy(strategies[i], StrategyMode.Backtest);

            bool done;
            do
            {
                done = strategies.All(s => s.Status == StrategyStatus.Stopped);
                Thread.Sleep(10);
            }
            while (!done);

            for (int i = 0; i < nStrategies; ++i)
            {
                universe[i].Objective = strategies[i].Objective();
                Console.WriteLine("{0} Objective = {1}", universe[i], universe[i].Objective);
            }

            EventCount = frameworks.Sum(framework => framework.EventManager.EventCount);

            for (int i = 0; i < nStrategies; ++i)
            {
                frameworks[i] = null;
                strategies[i] = null;
            }
            strategy.framework.Clear();
        }
    }
}
