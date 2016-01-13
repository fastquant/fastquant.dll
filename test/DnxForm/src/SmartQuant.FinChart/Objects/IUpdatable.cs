// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;

namespace SmartQuant.FinChart.Objects
{
    public interface IUpdatable
    {
        event EventHandler Updated;
    }
}
