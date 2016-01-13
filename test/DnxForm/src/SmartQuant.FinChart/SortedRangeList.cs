// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections;

namespace SmartQuant.FinChart
{
    public class SortedRangeList : ICollection
    {
        private SortedList list;

        public ArrayList this [int index]
        {
            get
            {
                return this.list.GetByIndex(index) as ArrayList;
            }
        }

        public ArrayList this [DateTime dateTime]
        {
            get
            {
                return this.list[dateTime] as ArrayList;
            }
        }

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

        public SortedRangeList()
        {
            this.list = new SortedList();
        }

        public SortedRangeList(bool right)
        {
            this.list = new SortedList();
            right = true;
        }

        public void Add(IDateDrawable item)
        {
            if (this.list.ContainsKey(item.DateTime))
                (this.list[item.DateTime] as ArrayList).Add(item);
            else
                this.list.Add(item.DateTime, new ArrayList() { item });
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public int GetNextIndex(DateTime dateTime)
        {
            var willAt = WillAtIndex(dateTime);
            return willAt == this.list.Count ? -1 : willAt;
        }

        public int GetPrevIndex(DateTime dateTime)
        {
            var willAt = WillAtIndex(dateTime);
            return willAt == 0 ? -1 : willAt - 1;
        }

        public DateTime GetDateTime(int index)
        {
            return (DateTime)this.list.GetKey(index);
        }

        public bool Contains(DateTime dateTime)
        {
            return this.list.ContainsKey(dateTime);
        }

        public void CopyTo(Array array, int index)
        {
            this.list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.Values.GetEnumerator();
        }

        private int WillAtIndex(DateTime dateTime)
        {
            if (this.list.ContainsKey(dateTime))
                return this.list.IndexOfKey(dateTime);
            this.list.Add(dateTime, null);
            int i = this.list.IndexOfKey(dateTime);
            this.list.Remove(dateTime);
            return i;
        }
    }
}
