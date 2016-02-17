using System;

namespace FastQuant
{
    public enum SearchOption
    {
        Next,
        Prev,
        ExactFirst,
        ExactLast
    }

    public interface IDataSeries
    {
        long Count { get; }

        string Name { get; }

        string Description { get; }

        DateTime DateTime1 { get; }

        DateTime DateTime2 { get; }

        DataObject this[long index] { get; }

        long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev);

        bool Contains(DateTime dateTime);

        void Add(DataObject obj);

        void Remove(long index);

        void Clear();
    }
}
