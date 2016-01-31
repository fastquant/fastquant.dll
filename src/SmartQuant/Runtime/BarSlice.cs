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

    class BarSliceItem
    {
        internal List<Bar> Bars = new List<Bar>();
        internal DateTime CloseDateTime= DateTime.MinValue;
        internal int barCount;
    }

    public class BarSliceFactory
    {
        private const int SecondsPerDay = (int)(TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);

        private Framework framework;

        private readonly IdArray<BarSliceItem> items = new IdArray<BarSliceItem>(SecondsPerDay);

        public BarSliceFactory(Framework framework)
        {
            this.framework = framework;
        }

        public void Clear()
        {
            this.items.Clear();
        }

        internal void OnBar(Bar bar)
        {
            var item = this.items[(int)bar.Size];
            if (item == null)
                return;

            if (--item.barCount == 0)
            {
                this.framework.EventServer.OnEvent(new BarSlice(bar));
                item.CloseDateTime = DateTime.MinValue;
                foreach (var b in item.Bars)
                    this.framework.EventServer.OnEvent(b);
                item.Bars.Clear();
            }
        }

        internal bool OnBarOpen(Bar bar)
        {
            var item = this.items[(int)bar.Size] = this.items[(int)bar.Size] ?? new BarSliceItem();
            if (item.CloseDateTime == bar.OpenDateTime)
            {
                item.Bars.Add(bar);
                return false;
            }
            if (item.barCount == 0)
                item.CloseDateTime = bar.OpenDateTime.AddSeconds(bar.Size);

            item.barCount++;
            return true;
        }
    }
}
