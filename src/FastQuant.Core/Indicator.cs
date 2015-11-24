// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SmartQuant
{
    namespace Indicators
    {
        public enum IndicatorStyle
        {
            QuantStudio,
            MetaStock
        }
    }

    public class Indicator : TimeSeries
    {
        protected ISeries input;
        protected bool calculate;

        public Indicator(ISeries input)
        {
            this.input = input;
        }

        protected virtual void Init()
        {
        }

        protected virtual void Calculate()
        {
        }

        public virtual void Calculate(int index)
        {
        }

        public void Clear() 
        {
        }
    }
}