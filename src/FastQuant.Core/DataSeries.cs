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
        public DataObject this[long index]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(DataObject obj)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public long GetIndex(DateTime dateTime, SearchOption option = SearchOption.Prev)
        {
            throw new NotImplementedException();
        }

        public void Remove(long index)
        {
            throw new NotImplementedException();
        }

        public virtual void Refresh()
        {
        }

        public virtual DataObject Get(long index)
        {
            throw new NotImplementedException();
        }

        public virtual DataObject Get(DateTime dateTime)
        {
            throw new NotImplementedException();
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

    public static class DataSeriesNameHelper
    {
        public static string GetName(Instrument instrument, byte type)
        {
            return string.Format("{0}.{1}.{2}", instrument.Symbol, instrument.Id, GetDataTypeAsString(type));
        }

        public static string GetName(Instrument instrument, BarType barType, long barSize)
        {
            return string.Format("{0}.{1}.{2}", instrument.Symbol, instrument.Id, GetBarTypeAsString(barType, barSize));
        }

        public static bool TryGetBarTypeSize(DataSeries series, out BarType barType, out long barSize)
        {
            barType = BarType.Time;
            barSize = 0;
            string[] parts = series.Name.Split(new char[] { '.' }, StringSplitOptions.None);
            return parts.Length >= 3 && GetDataType(series) == DataObjectType.Bar && Enum.TryParse<BarType>(parts[parts.Length - 3], out barType) && long.TryParse(parts[parts.Length - 2], out barSize);
        }

        public static Instrument GetInstrument(DataSeries series, Framework framework)
        {
            string[] parts = series.Name.Split(new char[] { '.' }, StringSplitOptions.None);
            int id = GetDataType(series) == DataObjectType.Bar ? int.Parse(parts[parts.Length - 4]) : int.Parse(parts[parts.Length - 2]);
            return framework.InstrumentManager.GetById(id);
        }

        public static string GetSymbol(DataSeries series)
        {
            return GetSymbol(series.Name);
        }

        public static string GetSymbol(string seriesName)
        {
            string[] parts = seriesName.Split(new char[] { '.' }, StringSplitOptions.None);
            int which = parts[parts.Length - 1] == "Bar" ? 4 : 2;
            return string.Join(".", parts, 0, parts.Length - which);
        }

        public static byte GetDataType(DataSeries series)
        {
            return GetDataTypeFromName(series.Name);
        }

        private static byte GetDataTypeFromName(string seriesName)
        {
            string[] parts = seriesName.Split(new char[] { '.' }, StringSplitOptions.None);
            switch (parts[parts.Length - 1])
            {
                case "Tick":
                    return DataObjectType.Tick;
                case "Bid":
                    return DataObjectType.Bid;
                case "Ask":
                    return DataObjectType.Ask;
                case "Quote":
                    return DataObjectType.Quote;
                case "Trade":
                    return DataObjectType.Trade;
                case "Bar":
                    return DataObjectType.Bar;
                case "Level2":
                    return DataObjectType.Level2;
                case "News":
                    return DataObjectType.News;
                case "Fundamental":
                    return DataObjectType.Fundamental;
                default:
                    return DataObjectType.DataObject;
            }
        }

        private static string GetDataTypeAsString(byte dataType)
        {
            switch (dataType)
            {
                case DataObjectType.Tick:
                    return "Tick";
                case DataObjectType.Bid:
                    return "Bid";
                case DataObjectType.Ask:
                    return "Ask";
                case DataObjectType.Trade:
                    return "Trade";
                case DataObjectType.Quote:
                    return "Quote";
                case DataObjectType.Bar:
                    return "Bar";
                case DataObjectType.Level2:
                case DataObjectType.Level2Snapshot:
                case DataObjectType.Level2Update:
                    return "Level2";
                case DataObjectType.Fundamental:
                    return "Fundamental";
                case DataObjectType.News:
                    return "News";
                default:
                    return string.Empty;
            }
        }

        private static object GetBarTypeAsString(BarType barType, long barSize) => $"{barType}.{barSize}.Bar"; 
    }
}