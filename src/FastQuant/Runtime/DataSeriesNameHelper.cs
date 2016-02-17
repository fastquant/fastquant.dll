using System;

namespace FastQuant
{

    public static class DataSeriesNameHelper
    {
        public static string GetName(Instrument instrument, byte type) => $"{instrument.Symbol}.{instrument.Id}.{GetDataTypeAsString(type)}";

        public static string GetName(Instrument instrument, BarType barType, long barSize)=> $"{instrument.Symbol}.{instrument.Id}.{GetBarTypeAsString(barType, barSize)}";

        public static bool TryGetBarTypeSize(DataSeries series, out BarType barType, out long barSize)
        {
            barType = BarType.Time;
            barSize = 0;
            var parts = series.Name.Split(new[] { '.' }, StringSplitOptions.None);
            return parts.Length >= 3 && GetDataType(series) == DataObjectType.Bar &&
                   Enum.TryParse<BarType>(parts[parts.Length - 3], out barType) &&
                   long.TryParse(parts[parts.Length - 2], out barSize);
        }

        public static Instrument GetInstrument(DataSeries series, Framework framework)
        {
            var parts = series.Name.Split(new[] { '.' }, StringSplitOptions.None);
            var id = GetDataType(series) == DataObjectType.Bar ? int.Parse(parts[parts.Length - 4]) : int.Parse(parts[parts.Length - 2]);
            return framework.InstrumentManager.GetById(id);
        }

        public static string GetSymbol(DataSeries series) => GetSymbol(series.Name);

        public static string GetSymbol(string seriesName)
        {
            var parts = seriesName.Split(new[] { '.' }, StringSplitOptions.None);
            var which = parts[parts.Length - 1] == "Bar" ? 4 : 2;
            return string.Join(".", parts, 0, parts.Length - which);
        }

        public static byte GetDataType(DataSeries series) => GetDataTypeFromName(series.Name);

        private static byte GetDataTypeFromName(string seriesName)
        {
            var parts = seriesName.Split(new[] { '.' }, StringSplitOptions.None);
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