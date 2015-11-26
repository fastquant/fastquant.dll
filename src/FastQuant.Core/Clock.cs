// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public enum ClockMode
    {
        Realtime,
        Simulation
    }

    public enum ClockResolution
    {
        Normal,
        High
    }

    public enum ClockType
    {
        Local,
        Exchange
    }

    public class Clock
    {
        public DateTime DateTime { get; set; }
    }
}