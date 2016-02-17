// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace FastQuant
{
    public class StrategyEventArgs : EventArgs
    {
        public Strategy Strategy { get; }

        public StrategyEventArgs(Strategy strategy)
        {
            Strategy = strategy;
        }
    }


    public enum StrategyMode
    {
        Backtest = 1,
        Paper,
        Live
    }

    public enum StrategyPersistence
    {
        None,
        Full,
        Save,
        Load
    }

    public enum StrategyStatus
    {
        Running,
        Stopped
    }

    public enum StrategyStatusType : byte
    {
        Started,
        Stopped
    }

    public class StrategyMethodAttribute
    {
    }
}