// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    namespace Indicators
    {
        public enum IndicatorStyle
        {
            QuantStudio,
            MetaStock
        }

        public enum RegressionDistanceMode
        {
            Time,
            Index
        }
    }

    public class Indicator : TimeSeries
    {
        protected ISeries input;
        protected bool calculate;

        public bool AutoUpdate { get; set; } = true;

        public Indicator(ISeries input)
        {
            this.input = input;
            this.input.Indicators.Add(this);
            this.calculate = true;
        }

        protected virtual void Init()
        {
            // noop
        }

        public void Attach() => this.input.Indicators.Add(this);

        public void Detach() => this.input.Indicators.Remove(this);

        protected virtual void Calculate()
        {
            if (this.calculate)
            {
                this.calculate = false;
                (this.input as Indicator)?.Calculate();
                for (var i = 0; i < this.input.Count; i++)
                    Update(i);
            }
        }

        public virtual void Calculate(int index)
        {
            // noop
        }

        public override int Count
        {
            get
            {
                Calculate();
                return base.Count;
            }
        }

        public override double First
        {
            get
            {
                Calculate();
                return base.First;
            }
        }

        public override DateTime FirstDateTime
        {
            get
            {
                Calculate();
                return base.FirstDateTime;
            }
        }

        public override double Last
        {
            get
            {
                Calculate();
                return base.Last;
            }
        }

        public override DateTime LastDateTime
        {
            get
            {
                Calculate();
                return base.LastDateTime;
            }
        }

        public override double this[int index]
        {
            get
            {
                Calculate();
                return base[index];
            }
        }

        public override double this[int index, BarData barData]
        {
            get
            {
                Calculate();
                return base[index, barData];
            }
        }

        public override DateTime GetDateTime(int index)
        {
            Calculate();
            return base.GetDateTime(index);
        }

        public override int GetIndex(DateTime datetime, IndexOption option = IndexOption.Null)
        {
            Calculate();
            return base.GetIndex(datetime, option);
        }

        public override double GetMax(DateTime dateTime1, DateTime dateTime2)
        {
            Calculate();
            return base.GetMax(dateTime1, dateTime2);
        }

        public override double GetMin(DateTime dateTime1, DateTime dateTime2)
        {
            Calculate();
            return base.GetMin(dateTime1, dateTime2);
        }

        internal void Update(int index)
        {
            if (this.calculate)
                Calculate();
            else
                Calculate(index);
        }
    }
}