// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace FastQuant.Optimization
{
    public class Objective
    {
        public double Value { get; set; }
    }

    public class OptimizationParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public object LowerBound { get; set; }

        public object UpperBound { get; set; }

        public object Step { get; set; }

        public OptimizationParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public OptimizationParameter(string name, object lowerBound, object upperBound, object step)
        {
            Name = name;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Step = step;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class OptimizationParameterAttribute : Attribute
    {
        public double LowerBound { get; }

        public double UpperBound { get; }

        public double Step { get; }

        public OptimizationParameterAttribute(double lowerBound, double upperBound, double step = 1.0)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Step = step;
        }
    }

    public class OptimizationParameterSet : IEnumerable<OptimizationParameter>
    {
        private List<OptimizationParameter> parameters = new List<OptimizationParameter>();

        public int Count => this.parameters.Count;

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

    public class Optimizer
    {
        public OptimizationParameterSet GetParameters(Strategy strategy)
        {
            OptimizationParameterSet optimizationParameterSet = new OptimizationParameterSet();
            PropertyInfo[] properties =
                strategy.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                   BindingFlags.SetProperty);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    Attribute attribute = (Attribute) customAttributes[j];
                    if (attribute is OptimizationParameterAttribute)
                    {
                        OptimizationParameterAttribute optimizationParameterAttribute =
                            (OptimizationParameterAttribute) attribute;
                        OptimizationParameter parameter = new OptimizationParameter(propertyInfo.Name,
                            optimizationParameterAttribute.LowerBound, optimizationParameterAttribute.UpperBound,
                            optimizationParameterAttribute.Step);
                        optimizationParameterSet.Add(parameter);
                    }
                }
            }
            return optimizationParameterSet;
        }

        public void SetParameters(Strategy strategy, OptimizationParameterSet parameters)
        {
            int num = 0;
            PropertyInfo[] properties =
                strategy.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                                   BindingFlags.SetProperty);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo propertyInfo = properties[i];
                object[] customAttributes = propertyInfo.GetCustomAttributes(false);
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    Attribute attribute = (Attribute) customAttributes[j];
                    if (attribute is OptimizationParameterAttribute)
                    {
                        if (propertyInfo.Name != parameters[num].Name)
                        {
                            throw new Exception("Can not set parameter. Wrong parameter order.");
                        }
                        propertyInfo.SetValue(strategy, (double) parameters[num].Value);
                        num++;
                    }
                }
            }
        }
    }

    //public class SimulatedAnnealingOptimizer : Optimizer
    //{
    //    static SimulatedAnnealingOptimizer()
    //    {
    //        LicenseManager.Validate(typeof (SimulatedAnnealingOptimizer));
    //    }

    //    public SimulatedAnnealingOptimizer()
    //    {
    //        Class59.tAHXU0wz2AVX3();
    //        this.simulatedAnnealing_0 = new SimulatedAnnealing();
    //        base..ctor();
    //    }

    //    private double method_0(Vector vector_0)
    //    {
    //        for (int i = 0; i < vector_0.int_0; i++)
    //        {
    //            this.optimizationParameterSet_0[i].Value = vector_0[i];
    //        }
    //        Framework framework = this.strategy_0.framework;
    //        Strategy strategy = (Strategy) Activator.CreateInstance(this.strategy_0.GetType(), new object[]
    //        {
    //            framework,
    //            "strategy_ " + this.int_0++
    //        });
    //        foreach (Instrument current in this.strategy_0.Instruments)
    //        {
    //            strategy.AddInstrument(current);
    //        }
    //        base.SetParameters(strategy, this.optimizationParameterSet_0);
    //        framework.strategyManager_0.StartStrategy(strategy, StrategyMode.Backtest);
    //        while (strategy.strategyStatus_0 != StrategyStatus.Stopped)
    //        {
    //            Thread.Sleep(10);
    //        }
    //        double result = -strategy.Objective();
    //        framework.PortfolioManager.Clear();
    //        framework.OrderManager.Clear();
    //        framework.StrategyManager.Clear();
    //        return result;
    //    }

    //    public void Optimize(Strategy strategy)
    //    {
    //        this.strategy_0 = strategy;
    //        this.optimizationParameterSet_0 = base.GetParameters(strategy);
    //        int count = this.optimizationParameterSet_0.Count;
    //        Vector vector = new Vector(count);
    //        Vector vector2 = new Vector(count);
    //        for (int i = 0; i < count; i++)
    //        {
    //            vector[i] = (double) this.optimizationParameterSet_0[i].LowerBound;
    //            vector2[i] = (double) this.optimizationParameterSet_0[i].UpperBound;
    //        }
    //        this.simulatedAnnealing_0.Run(new Func<Vector, double>(this.method_0), vector, vector2, null);
    //    }

    //    public int NC
    //    {
    //        get { return this.simulatedAnnealing_0.NC; }
    //        set { this.simulatedAnnealing_0.NC = value; }
    //    }

    //    public int NT
    //    {
    //        get { return this.simulatedAnnealing_0.NT; }
    //        set { this.simulatedAnnealing_0.NT = value; }
    //    }

    //    private int int_0;

    //    private OptimizationParameterSet optimizationParameterSet_0;

    //  //  private SimulatedAnnealing simulatedAnnealing_0;

    //    private Strategy strategy_0;
    //}

    public class MulticoreOptimizer
    {
        private Stopwatch stopwatch = new Stopwatch();

        public long Elapsed => this.stopwatch.ElapsedMilliseconds;

        public long EventCount { get; private set; }

        public OptimizationParameterSet Optimize(Strategy strategy, InstrumentList instruments,
            OptimizationUniverse universe, int bunch = -1)
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
            Console.WriteLine(
                $"Processed {EventCount} events in {Elapsed} msec - {EventCount/(Elapsed*1000d)} event/sec");
            return max;
        }

        private void Optimize(Strategy strategy, InstrumentList instruments, OptimizationUniverse universe,
            int nFrameworks, int nStrategies)
        {
            throw new NotImplementedException();
        }
    }
}
