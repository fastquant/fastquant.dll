// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections;
using System.Drawing;

namespace SmartQuant.FinChart
{
    public class ColorSeries : ICollection
    {
        private SortedList list;

        public bool IsSynchronized
        {
            get
            {
                return this.list.IsSynchronized;
            }
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.list.SyncRoot;
            }
        }

        public ColorSeries()
        {
            this.list = new SortedList();
        }

        public void CopyTo(Array array, int index)
        {
            this.list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.Values.GetEnumerator();
        }

        public void AddColor(DateTime date, Color color)
        {
            this.list.Add(date, color);
        }
    }
}
