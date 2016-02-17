// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace FastQuant
{
    public class BarFilterItem
    {
        public BarType BarType { get; set; }

        public long BarSize { get; set; }

        public BarFilterItem(BarType barType, long barSize)
        {
            BarType = barType;
            BarSize = barSize;
        }
    }

    public class BarFilter
    {
        public List<BarFilterItem> Items { get; } = new List<BarFilterItem>();

        public int Count => Items.Count;

        public void Add(BarType barType, long barSize)
        {
            if (!Contains(barType, barSize))
                Items.Add(new BarFilterItem(barType, barSize));
        }

        public bool Contains(BarType barType, long barSize) => Items.Any(item => item.BarType == barType && item.BarSize == barSize);

        public void Clear() => Items.Clear();
    }
}