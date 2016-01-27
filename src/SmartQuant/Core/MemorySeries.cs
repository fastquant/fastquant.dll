using System;
using System.Collections.Generic;

namespace SmartQuant 
{
    class DataObjectComparer : IComparer<DataObject>
    {
        public int Compare(DataObject x, DataObject y) => x.DateTime.CompareTo(y.DateTime);
    }

    internal enum Option
    {
        Exact,
        Prev,
        Next
    }

    internal static class DataObjectListSearcher
    {
        public static int GetIndex(List<DataObject> list, DateTime dateTime, DateTime firstDateTime, DateTime lastDateTime, Option option)
        {
            if (dateTime < firstDateTime)
                return option == Option.Exact || option == Option.Prev ? -1 : 0;
            if (dateTime > lastDateTime)
                return option == Option.Exact || option == Option.Next ? -1 : list.Count - 1;

            var i = list.BinarySearch(new DataObject() { DateTime = dateTime }, new DataObjectComparer());
            if (i >= 0)
                return i;
            else if (option == Option.Next)
                return ~i;
            else if (option == Option.Prev)
                return ~i - 1;
            return -1; // option == Option.Exact
        }
    }

    public class MemorySeries : IDataSeries
    {
        private readonly List<DataObject> list = new List<DataObject>();

        public DataObject this[long index] => this.list[(int)index];

        public long Count => (long)this.list.Count;

        public DateTime DateTime1 => this.list[0].DateTime;

        public DateTime DateTime2 => this.list[this.list.Count - 1].DateTime;

        public string Description { get; }

        public string Name { get; }

        public MemorySeries() : this(null, null)
        {
        }
        public MemorySeries(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public void Add(DataObject obj)
        {
            if (this.list.Count != 0 && !(obj.dateTime >= this.list[this.list.Count - 1].dateTime))
            {
                this.list.Insert((int)this.GetIndex(obj.dateTime, SearchOption.Next), obj);
                return;
            }
            this.list.Add(obj);
        }

        public void Clear()=> this.list.Clear();

        public bool Contains(DateTime dateTime)=> GetIndex(dateTime, SearchOption.ExactFirst) != -1;

        public long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev)
        {
            if (option == SearchOption.ExactLast)
                throw new NotSupportedException();

            if (dateTime < DateTime1)
                return option == SearchOption.ExactFirst || option == SearchOption.Prev ? -1 : 0;
            if (dateTime > DateTime2)
                return option == SearchOption.ExactFirst || option == SearchOption.Next ? -1 : Count - 1;

            var i = this.list.BinarySearch(new DataObject { DateTime = dateTime }, new DataObjectComparer());
            if (i >= 0)
                return i;
            if (option == SearchOption.Next)
                return ~i;
            if (option == SearchOption.Prev)
                return ~i - 1;
            return -1; // option == IndexOption.Null
        }

        public void Remove(long index) => this.list.RemoveAt((int)index);
    }
}