// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

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
        private readonly List<OptimizationParameter> _parameters = new List<OptimizationParameter>();

        public int Count => _parameters.Count;

        public Global Global { get; set; } = new Global();

        public double Objective { get; set; }

        public OptimizationParameter this[int index] => _parameters[index];

        public void Add(OptimizationParameter parameter) => _parameters.Add(parameter);

        public void Add(string name, object value) => Add(new OptimizationParameter(name, value));

        public IEnumerator<OptimizationParameter> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _parameters.GetEnumerator();

        public override string ToString() => string.Join(" ", _parameters.Select(p => $"{p.Name} = {p.Value}"));
    }

    public class OptimizationUniverse : IEnumerable<OptimizationParameterSet>
    {
        private readonly List<OptimizationParameterSet> _sets = new List<OptimizationParameterSet>();

        public int Count => _sets.Count;

        public OptimizationParameterSet this[int index] => _sets[index];

        public void Add(OptimizationParameterSet parameter) => _sets.Add(parameter);

        public void Clear() => _sets.Clear();

        public IEnumerator<OptimizationParameterSet> GetEnumerator() => _sets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class OptimizationPortfolio { }

    public class OptimizationProgress
    {
        private int juSsjZ22RM = 100;

        public List<OptimizationResult> AllResults { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public bool IsElapsedTimeAvailable { get; set; }

        public List<OptimizationResult> LastResults { get; set; }

        public double ProgressValue { get; set; }

        public TimeSpan RemainingTime { get; set; }

        public DateTime StartTime { get; set; }

        public List<OptimizationResult> TopResults { get; set; }

        public OptimizationProgress()
        {
            StartTime = DateTime.Now;
            AllResults = new List<OptimizationResult>();
            TopResults = new List<OptimizationResult>();
            LastResults = new List<OptimizationResult>();
        }

        public OptimizationProgress(double value, List<OptimizationResult> lastResults)
        {
            ProgressValue = value;
            LastResults = lastResults;
        }

        public void SetProgress(double value)
        {
            if (value < 0.0 || value > 1.0)
                throw new Exception("OptimizationProgress: progress < 0 || > 1");

            ProgressValue = value;
            if (value == 0.0)
            {
                StartTime = DateTime.Now;
                IsElapsedTimeAvailable = false;
            }
            else if (value == 1.0)
            {
                RemainingTime = new TimeSpan(0);
            }
            else
            {
                IsElapsedTimeAvailable = true;
                ElapsedTime = DateTime.Now - StartTime;
                long ticks = Convert.ToInt64(ElapsedTime.Ticks * (1.0 - value) / value);
                RemainingTime = new TimeSpan(ticks);
            }
        }

        public void AddResults(List<OptimizationResult> results)
        {
            AllResults.AddRange(results);
            jQIsZpt75A(results);
            LastResults = results;
        }

        private void jQIsZpt75A(List<OptimizationResult> results)
        {
            TopResults.AddRange(results);
            TopResults.Sort();
            for (var i = TopResults.Count - 1; i >= this.juSsjZ22RM; i--)
                TopResults.RemoveAt(i);
        }
    }

    public class OptimizationProgressEventArgs : EventArgs
    {
        public OptimizationProgressEventArgs(OptimizationProgress optimizationProgress)
        {
            OptimizationProgress = optimizationProgress;
        }

        public OptimizationProgress OptimizationProgress { get; set; }
    }

    public class OptimizationStatistics
    {
        public OptimizationStatistics(PortfolioStatistics statistics)
        {
            if (statistics == null)
                return;
            NetProfit = statistics.Items[PortfolioStatisticsType.NetProfit - 1].TotalValue;
            MaxDDPct = statistics.Items[8].TotalValue;
            ProfitFactor = statistics.Items[9].TotalValue;
            Trades = Convert.ToInt32(statistics.Items[11].TotalValue);
            ProfitablePct = statistics.Items[14].TotalValue;
            AvgTrade = statistics.Items[18].TotalValue;
            PayoffRatio = statistics.Items[21].TotalValue;
            AvgDailyReturnPct = statistics.Items[37].TotalValue;
            AvgAnnualReturnPct = statistics.Items[38].TotalValue;
            SharpeRatio = statistics.Items[43].TotalValue;
        }

        public double AvgAnnualReturnPct { get; set; }

        public double AvgDailyReturnPct { get; set; }

        public double AvgTrade { get; set; }

        public double MaxDDPct { get; set; }

        public double NetProfit { get; set; }

        public double PayoffRatio { get; set; }

        public double ProfitablePct { get; set; }

        public double ProfitFactor { get; set; }

        public double SharpeRatio { get; set; }

        public int Trades { get; set; }
    }

    public class OptimizationResult : IComparable<OptimizationResult>
    {
        public OptimizationResult(OptimizationParameterSet parameterSet, PortfolioStatistics statistics)
        {
            OptimizationParameterSet = parameterSet;
            OptimizationStatistics = new OptimizationStatistics(statistics);
        }

        public int CompareTo(OptimizationResult other) => other?.OptimizationParameterSet.Objective.CompareTo(OptimizationParameterSet.Objective) ?? 1;

        public override string ToString() => $"{OptimizationParameterSet.Objective.ToString("F2")} | {OptimizationParameterSet}";

        public OptimizationParameterSet OptimizationParameterSet { get; set; }

        public OptimizationStatistics OptimizationStatistics { get; set; }
    }

    public class OptimizationScenario : Scenario
    {
        public OptimizationScenario(Framework framework) : base(framework)
        {
        }

        public virtual void OnOptimizationStep(List<OptimizationResult> results)
        {
        }
    }

    public class OptimizerSettings
    {
        public OptimizerSettingsPropertyList GetProperties()
        {
            var optimizerSettingsPropertyList = new OptimizerSettingsPropertyList();
            foreach (var propertyInfo in GetType().GetProperties().Where(p=> p.CanRead && p.CanWrite))
            {

                    var customAttributes = propertyInfo.GetCustomAttributes(typeof(TypeConverterAttribute));
                    TypeConverter typeConverter;
                    if (customAttributes.Count() > 0)
                    {
                        typeConverter = (TypeConverter)Activator.CreateInstance(Type.GetType(((TypeConverterAttribute)customAttributes.First<Attribute>()).ConverterTypeName));
                    }
                    else
                    {
                        typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    }
                    if (typeConverter != null && typeConverter.CanConvertTo(typeof(string)) && typeConverter.CanConvertFrom(typeof(string)))
                    {
                        object value = propertyInfo.GetValue(this, null);
                        optimizerSettingsPropertyList.Items.Add(new OptimizerSettingsProperty(propertyInfo.Name, typeConverter.ConvertToInvariantString(value)));
                    }
            }
            return optimizerSettingsPropertyList;
        }

        public void SetProperties(OptimizerSettingsPropertyList propertyList)
        {
            foreach (var propertyInfo in GetType().GetProperties().Where(p=> p.CanRead && p.CanWrite))
            {

                    IEnumerable<Attribute> customAttributes = propertyInfo.GetCustomAttributes(typeof(TypeConverterAttribute));
                    TypeConverter typeConverter;
                    if (customAttributes.Count() > 0)
                    {
                        typeConverter = (TypeConverter)Activator.CreateInstance(Type.GetType(((TypeConverterAttribute)customAttributes.First<Attribute>()).ConverterTypeName));
                    }
                    else
                    {
                        typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    }
                    if (typeConverter != null && typeConverter.CanConvertFrom(typeof(string)))
                    {
                        string value = propertyList.GetValue(propertyInfo.Name);
                        if (value != null && typeConverter.IsValid(value))
                        {
                            propertyInfo.SetValue(this, typeConverter.ConvertFromInvariantString(value), null);
                        }
                    }
            }
        }

        [Category("Concurrency")]
        public int Bunch { get; set; } = 10;

        [Browsable(false)]
        public bool IsDefaultOptimizer { get; set; }

        [Browsable(false)]
        public bool IsRaiseEvents { get; set; } = true;

        [Browsable(false)]
        public bool IsSilentMode { get; set; }
    }
    public class OptimizerSettingsList
    {
        public List<OptimizerSettingsPropertyList> Items { get; set; }=new List<OptimizerSettingsPropertyList>();
    }

    public class OptimizerSettingsProperty
    {
        public OptimizerSettingsProperty()
        {
        }

        public OptimizerSettingsProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class OptimizerSettingsPropertyList
    {
        public string GetValue(string name) => Items.FirstOrDefault(n => n.Name == name)?.Value;

        public List<OptimizerSettingsProperty> Items { get; set; } = new List<OptimizerSettingsProperty>();

        public string OptimizerName { get; set; }
    }


    public class Optimizer
    {
        internal Framework Framework { get; set; }
        private Framework framework;

        public virtual string Name => string.Empty;

        public OptimizationUniverse OptimizationUniverse { get; set; }

        public virtual OptimizerSettings Settings { get; } = new OptimizerSettings();

        public virtual int UniverseCount => 0;

        public OptimizationProgress Progress { get; set; } = new OptimizationProgress();

        public Optimizer(Framework framework)
        {
            this.framework = framework;
        }

        public OptimizationParameterSet GetParameters(object obj)
        {
            var parameters = new OptimizationParameterSet();
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

            foreach (var parameter in properties.SelectMany(property => property.GetCustomAttributes(false).OfType<OptimizationParameterAttribute>().Select(attribute
                => new OptimizationParameter(property.Name, attribute.LowerBound, attribute.UpperBound, attribute.Step))))
            {
                parameters.Add(parameter);
            }
            return parameters;
        }


        public virtual OptimizationParameterSet Optimize(Scenario scenario)
        {
            return null;
        }

        public virtual OptimizationParameterSet Optimize(Strategy strategy, OptimizationUniverse universe = null)
        {
            return null;
        }

        public void SetParameters(object obj, OptimizationParameterSet parameters)
        {
            var num = 0;
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var property in properties.SelectMany(property => property.GetCustomAttributes(false).OfType<OptimizationParameterAttribute>(), (property, attribute) => property))
            {
                if (property.Name != parameters[num].Name)
                    throw new Exception("Can not set parameter. Wrong parameter order.");
                property.SetValue(obj, parameters[num].Value);
                num++;
            }
        }

        protected void EmitOptimizationProgress(double progress, List<OptimizationResult> results)
        {
            if (!Settings.IsRaiseEvents)
                return;
            Progress.AddResults(results);
            Progress.SetProgress(progress);
            this.framework.EventServer.OnOptimizationProgress(Progress);
        }

        protected void EmitOptimizationStart()
        {
            if (!Settings.IsRaiseEvents)
                return;
            Progress = new OptimizationProgress();
            this.framework.EventServer.OnOptimizationStart();
        }

        protected void EmitOptimizationStop()
        {
            if (!Settings.IsRaiseEvents)
                return;
            Progress.SetProgress(1.0);
            this.framework.EventServer.OnOptimizationStop();
        }
    }

    public class SimulatedAnnealingOptimizer : Optimizer
    {
        public SimulatedAnnealingOptimizer(Framework framework) : base(framework)
        {
        }
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

        //      private SimulatedAnnealing simulatedAnnealing_0;

        //    private Strategy strategy_0;
    }

    public class MulticoreOptimizer : Optimizer
    {
        public MulticoreOptimizer(Framework framework) : base(framework)
        {
        }
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public long Elapsed => _stopwatch.ElapsedMilliseconds;

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
            _stopwatch.Start();
            throw new NotImplementedException();
            _stopwatch.Stop();
            var max = universe.OrderBy(u => u.Objective).Last();
            Console.WriteLine($"Best Objective {max}  Objective = {max.Objective}");
            Console.WriteLine("Optimization done");
            Console.WriteLine(
                $"Processed {EventCount} events in {Elapsed} msec - {EventCount / (Elapsed * 1000d)} event/sec");
            return max;
        }

        private void Optimize(Strategy strategy, InstrumentList instruments, OptimizationUniverse universe,
            int nFrameworks, int nStrategies)
        {
            throw new NotImplementedException();
        }
    }
}
