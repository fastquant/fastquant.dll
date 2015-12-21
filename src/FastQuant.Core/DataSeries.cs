using System;

namespace SmartQuant
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

    public class DataSeries : IDataSeries
    {
        private string seriesName;

        public string Name { get; }

        public string Description { get; }

        public bool CacheObjects { get; set; }

        public long Count { get; }

        public DataObject this[long index] => Get(index);

        public DataObject this[DateTime dateTime] => Get(dateTime);

        public DateTime DateTime1
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DateTime DateTime2
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public DataSeries(string name)
        {
            Name = name;
        }


        public void Add(DataObject obj)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public virtual bool Contains(DateTime dateTime) => GetIndex(dateTime, SearchOption.ExactFirst) != -1;

        public virtual long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev)
        {
            throw new NotImplementedException();
        }

        public void Remove(long index)
        {
            throw new NotImplementedException();
        }

        public virtual void Refresh()
        {
            // noop
        }

        public virtual void Update(long index, DataObject obj)
        {
            throw new NotImplementedException();
        }

        public virtual DataObject Get(long index)
        {
            throw new NotImplementedException();
        }

        public virtual DataObject Get(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public void Dump()
        {
        }
    }

    public class DataSeriesIterator
    {
        private DataSeries series;
        private long index1;
        private long index2;
        private long current;

        public DataSeriesIterator(DataSeries series, long index1 = -1, long index2 = -1)
        {
            this.series = series;
            this.index1 = index1 != -1 ? index1 : 0;
            this.index2 = index2 != -1 ? series.Count - 1 : 0;
            this.current = index1;
        }

        public DataObject GetNext()
        {
            return this.current > this.index2 ? null : this.series.Get(this.current++);
        }
    }

    public class DataSeriesListEventArgs
    {
        public DataSeries[] SeriesList { get; }

        public DataSeriesListEventArgs(params DataSeries[] seriesList)
        {
            SeriesList = seriesList;
        }
    }
}