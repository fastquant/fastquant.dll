// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace FastQuant.Optimization
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

        public Global Global { get; set; } = new Global();

        public double Objective { get; set; }

        public OptimizationParameter this[int index] => this.parameters[index];

        public void Add(OptimizationParameter parameter) => this.parameters.Add(parameter);

        public void Add(string name, object value) => Add(new OptimizationParameter(name, value));

        public IEnumerator<OptimizationParameter> GetEnumerator() => this.parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.parameters.GetEnumerator();

        public override string ToString() => string.Join(" ", this.parameters.Select(p => $"{p.Name} = {p.Value}"));
    }

    public class OptimizationUniverse : IEnumerable<OptimizationParameterSet>
    {
        private readonly List<OptimizationParameterSet> sets = new List<OptimizationParameterSet>();

        public int Count => this.sets.Count;

        public OptimizationParameterSet this[int index] => this.sets[index];

        public void Add(OptimizationParameterSet parameter) => this.sets.Add(parameter);

        public void Clear() => this.sets.Clear();

        public IEnumerator<OptimizationParameterSet> GetEnumerator() => this.sets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class MulticoreOptimizer
    {
        private Stopwatch stopwatch = new Stopwatch();

        public long Elapsed => this.stopwatch.ElapsedMilliseconds;

        public long EventCount { get; private set; }

        public OptimizationParameterSet Optimize(Strategy strategy, InstrumentList instruments, OptimizationUniverse universe, int bunch = -1)
        {
            if (universe.Count == 0)
            {
                Console.WriteLine("Optimization failed, universe is empty");
                return new OptimizationParameterSet();
            }

            EventCount = 0;
            this.stopwatch.Start();
            throw new NotImplementedException();
            this.stopwatch.Stop();
            var max = universe.OrderBy(u => u.Objective).Last();
            Console.WriteLine($"Best Objective {max}  Objective = {max.Objective}");
            Console.WriteLine("Optimization done");
            Console.WriteLine($"Processed {EventCount} events in {Elapsed} msec - {EventCount/(Elapsed*1000d)} event/sec");
            return max;
        }

        private void Optimize(Strategy strategy, InstrumentList instruments, OptimizationUniverse universe, int nFrameworks, int nStrategies)
        {
            throw new NotImplementedException();
        }
    }
}
