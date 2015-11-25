// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class BarSlice : Event
    {
        public override byte TypeId => EventType.BarSlice;

        public long Size { get; private set; }

        public BarSlice(Bar bar)
            : this(bar.dateTime, bar.Size)
        {
        }

        public BarSlice(DateTime dateTime, long size)
            : base(dateTime)
        {
            Size = size;
        }

        public override string ToString() => $"Bar slice {Size}";
    }

    class Slice
    {
        internal List<Bar> bars;
        DateTime dateTime;
    }

    public class BarSliceFactory
    {
        private Framework framework;
        private IdArray<Slice> slices;

        public BarSliceFactory(Framework framework)
        {
            this.framework = framework;
            this.slices = new IdArray<Slice>(86400);
        }

        public void Clear()
        {
            slices.Clear();
        }
    }
}
