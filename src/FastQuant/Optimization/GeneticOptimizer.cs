using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;
using FastQuant.Quant;

namespace FastQuant.Optimization
{
    public delegate void OptimizationProgressEventHandler(object sender, OptimizationProgressEventArgs progress);

    public class GeneticOptimizer : Optimizer
    {
        public GeneticOptimizer(Framework framework) : base(framework)
        {
        }
    }

    public class OptimizationManager
    {
        public OptimizationManager(Framework framework)
        {
            this.framework = framework;
            this.optimizer = new MulticoreOptimizer(framework);
            Add(this.optimizer);
            Add(new GeneticMulticoreOptimizer(framework));
           // this.vXosfEOPtA();
        }

        public void Add(Optimizer optimizer)
        {
            Optimizers.Add(optimizer);
        }

        //private string CcQsHkys28()
        //{
        //    string optimizationManagerFileName = this.framework.Configuration.OptimizationManagerFileName;
        //    if (optimizationManagerFileName == null)
        //    {
        //        optimizationManagerFileName = Configuration.DefaultConfiguaration().OptimizationManagerFileName;
        //    }
        //    return optimizationManagerFileName;
        //}

        public virtual void OnOptimizationProgress()
        {
        }

        public virtual void OnOptimizationStart()
        {
        }

        public virtual void OnOptimizationStop()
        {
        }

        public OptimizationParameterSet Optimize(Strategy strategy, OptimizationUniverse universe = null)
        {
            return this.optimizer?.Optimize(strategy, universe);
        }

        public OptimizationParameterSet Optimize(Scenario scenario, OptimizationUniverse universe = null)
        {
            return this.optimizer?.Optimize(scenario);
        }

        public void Remove(Optimizer optimizer)
        {
            Optimizers.Remove(optimizer);
        }

        public void SaveSettings()
        {
            throw new NotImplementedException();
        }

        private OptimizerSettingsList Y28sGt3f4m()
        {
            OptimizerSettingsList optimizerSettingsList = new OptimizerSettingsList();
            foreach (var current in this.Optimizers)
            {
                var properties = current.Settings.GetProperties();
                properties.OptimizerName = current.Name;
                optimizerSettingsList.Items.Add(properties);
            }
            return optimizerSettingsList;
        }

        public Optimizer Optimizer
        {
            get
            {
                return this.optimizer;
            }
            set
            {
                this.optimizer = value;
                this.optimizer.Settings.IsDefaultOptimizer = true;
                this.optimizer.Framework = this.framework;
            }
        }

        public List<Optimizer> Optimizers { get; } = new List<Optimizer>();

        private Optimizer optimizer;

        private Framework framework;
    }


    public class GeneticMulticoreOptimizer : Optimizer
    {
        public GeneticMulticoreOptimizer(Framework framework) : base(framework)
        {
        }
    }
}