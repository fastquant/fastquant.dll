using System;
using System.Collections;
using System.Drawing;

namespace SmartQuant.FinChart
{
    public class ColorSeries : ICollection
    {
        private SortedList list = new SortedList();

        public bool IsSynchronized => this.list.IsSynchronized;

        public int Count => this.list.Count;

        public object SyncRoot => this.list.SyncRoot;

        public void CopyTo(Array array, int index) => this.list.CopyTo(array, index);

        public IEnumerator GetEnumerator() => this.list.Values.GetEnumerator();

        public void AddColor(DateTime date, Color color) => this.list.Add(date, color);
    }

    public class SortedRangeList : ICollection
    {
        private SortedList list = new SortedList();

        public ArrayList this [int index] => this.list.GetByIndex(index) as ArrayList;

        public ArrayList this [DateTime dateTime] => this.list[dateTime] as ArrayList;

        public bool IsSynchronized => this.list.IsSynchronized;

        public int Count => this.list.Count;

        public object SyncRoot => this.list.SyncRoot;

        public SortedRangeList()
        {
        }

        public SortedRangeList(bool right)
        {
        }

        public void Add(IDateDrawable item)
        {
            if (Contains(item.DateTime))
                this[item.DateTime].Add(item);
            else
                this.list.Add(item.DateTime, new ArrayList() { item });
        }

        public void Clear() => this.list.Clear();

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

        public DateTime GetDateTime(int index) => (DateTime)this.list.GetKey(index);

        public bool Contains(DateTime dateTime) => this.list.ContainsKey(dateTime);

        public void CopyTo(Array array, int index) => this.list.CopyTo(array, index);

        public IEnumerator GetEnumerator() => this.list.Values.GetEnumerator();

        private int WillAtIndex(DateTime dateTime)
        {
            if (Contains(dateTime))
                return this.list.IndexOfKey(dateTime);
            this.list.Add(dateTime, null);
            int i = this.list.IndexOfKey(dateTime);
            this.list.Remove(dateTime);
            return i;
        }
    }
}
