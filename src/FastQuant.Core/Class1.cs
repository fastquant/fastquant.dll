// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SmartQuant
{
    public enum ChartStyle
    {
        Bar,
        Line,
        Candle
    }

    public class Direction
    {
        public const sbyte Undefined = -1;
        public const sbyte Plus = 0;
        public const sbyte ZeroPlus = 1;
        public const sbyte Minus = 2;
        public const sbyte ZeroMinus = 3;
    }

    public enum RequestResult
    {
        Completed,
        Cancelled,
        Error,
    }
}