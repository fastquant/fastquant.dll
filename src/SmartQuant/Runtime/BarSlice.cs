// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class BarSlice : Event
    {
        public override byte TypeId => EventType.BarSlice;

        public long Size { get; }

        public BarSlice(Bar bar) : this(bar.DateTime, bar.Size)
        {
        }

        public BarSlice(DateTime dateTime, long size) : base(dateTime)
        {
            Size = size;
        }

        public override string ToString() => $"Bar slice {Size}";
    }

    class Slice
    {
        internal List<Bar> Bars;
        internal DateTime DateTime;
        internal int int_0;
    }

    public class BarSliceFactory
    {
        private Framework framework;
        private IdArray<Slice> slices = new IdArray<Slice>(86400);

        public BarSliceFactory(Framework framework)
        {
            this.framework = framework;
        }

        public void Clear()
        {
            this.slices.Clear();
        }

        internal void method_1(Bar bar)
        {
            var slice = this.slices[(int)bar.Size];
            if (slice == null)
                return;

            if (--slice.int_0 == 0)
            {
                this.framework.EventServer.OnEvent(new BarSlice(bar));
                slice.DateTime = DateTime.MinValue;
                foreach (var b in slice.Bars)
                    this.framework.EventServer.OnEvent(b);
                slice.Bars.Clear();
            }
        }

        internal bool method_0(Bar bar)
        {
            var slice = this.slices[(int)bar.Size];
            if (slice == null)
            {
                slice = new Slice();
                this.slices[(int)bar.Size] = slice;
            }
            if (slice.DateTime == bar.OpenDateTime)
            {
                slice.Bars.Add(bar);
                return false;
            }
            if (slice.int_0 == 0)
            {
                slice.DateTime = bar.OpenDateTime.AddSeconds(bar.Size);
            }
            slice.int_0++;
            return true;
        }
    }
}
