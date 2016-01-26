using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace SmartQuant
{
    public class FileDataServer : DataServer
    {
        private string host;
        private DataFile dataFile;
        private bool opened;

        private IdArray<IdArray<DataSeries>> byIId_BSize = new IdArray<IdArray<DataSeries>>();
        private IdArray<IdArray<Dictionary<long, DataSeries>>> byType_IId_BSize = new IdArray<IdArray<Dictionary<long, DataSeries>>>();
        private IdArray<DataSeries>[] byType;

        public FileDataServer(Framework framework, string fileName, string host = null, int port = -1) : base(framework)
        {
            this.host = host;
            this.dataFile = new DataFile(fileName, framework.StreamerManager);
        }

        public override void Open()
        {
            if (!this.opened)
            {
                this.dataFile.Open(FileMode.OpenOrCreate);
                this.byType = new IdArray<DataSeries>[128];
                for (int i = 0; i < this.byType.Length; i++)
                    this.byType[i] = new IdArray<DataSeries>();
                this.opened = true;
            }
        }

        public override void Close()
        {
            if (this.opened)
            {
                this.dataFile.Close();
                this.opened = false;
            }
        }

        public override void Flush() => this.dataFile.Flush();

        public override DataSeries GetDataSeries(string name) => (DataSeries)this.dataFile.Get(name);

        public override DataSeries GetDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            if (type == DataObjectType.Bar)
                return GetBarDataSeriesInCache(instrument, barType, barSize, false);

            var series = this.byType[type][instrument.Id];
            if (series == null)
            {
                string name = DataSeriesNameHelper.GetName(instrument, type);
                series = (this.dataFile.Get(name) as DataSeries);
                this.byType[type][instrument.Id] = series;
            }
            return series;
        }

        public override List<DataSeries> GetDataSeriesList(Instrument instrument = null, string pattern = null)
        {
            var keys = this.dataFile.Keys.Values.Where(k => k.TypeId == ObjectType.DataSeries && (instrument == null || DataSeriesNameHelper.GetSymbol(k.Name) == instrument.Symbol) && (pattern == null || k.Name.Contains(pattern)));
            return keys.Select(k => GetDataSeries(k.Name)).ToList();
        }

        public override DataSeries AddDataSeries(string name)
        {
            var series = GetDataSeries(name);
            if (series == null)
            {
                series = CreateDataSeries(name);
                this.dataFile.Write(name, series);
            }
            return series;
        }

        public override DataSeries AddDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            if (type == DataObjectType.Bar)
                return GetBarDataSeriesInCache(instrument, barType, barSize, true);

            var series = this.byType[type][instrument.Id];
            if (series == null)
            {
                string name = DataSeriesNameHelper.GetName(instrument, type);
                series = GetDataSeries(name);
                if (series == null)
                {
                    series = CreateDataSeries(name);
                    this.dataFile.Write(name, series);
                }
                this.byType[type][instrument.Id] = series;
            }
            return series;
        }

        public override void DeleteDataSeries(string name)
        {
            var series = GetDataSeries(name);
            if (series != null)
            {
                for (int i = 0; i < this.byType.Length; i++)
                    for (int j = 0; j < this.byType[i].Size; j++)
                        if (this.byType[i][j] == series)
                            this.byType[i].Remove(j);
                this.dataFile.Delete(name);
            }
        }

        public override void DeleteDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            DataSeries dataSeries;
            if (type == DataObjectType.Bar)
            {
                var iId = instrument.Id;
                dataSeries = GetBarDataSeriesInCache(instrument, barType, barSize, false);
                if (barType == BarType.Time && barSize <= TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond)
                {
                    this.byIId_BSize[iId][(int)barSize] = null;
                }
                else
                {
                    this.byType_IId_BSize[type] = this.byType_IId_BSize[type] ?? new IdArray<Dictionary<long, DataSeries>>();
                    this.byType_IId_BSize[type][iId] = this.byType_IId_BSize[type][iId] ?? new Dictionary<long, DataSeries>();
                    this.byType_IId_BSize[type][iId].Remove(barSize);
                }
                this.dataFile.Delete(DataSeriesNameHelper.GetName(instrument, barType, barSize));
            }
            else
            {
                dataSeries = this.byType[type][instrument.Id];
                if (dataSeries != null)
                    this.byType[type].Remove(instrument.Id);
                this.dataFile.Delete(DataSeriesNameHelper.GetName(instrument, type));
            }

        }

        public override void Save(Instrument instrument, DataObject obj, SaveMode option = SaveMode.Add)
        {
            throw new NotImplementedException();
        }

        private DataSeries GetBarDataSeriesInCache(Instrument instrument, BarType barType, long barSize, bool createNotExist)
        {
            var iId = instrument.Id;
            DataSeries dataSeries;
            if (barType == BarType.Time && barSize <= TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond)
            {
                this.byIId_BSize[iId] = this.byIId_BSize[iId] ?? new IdArray<DataSeries>();
                dataSeries = this.byIId_BSize[iId][(int)barSize];
            }
            else
            {
                this.byType_IId_BSize[(int)barType] = this.byType_IId_BSize[(int)barType] ?? new IdArray<Dictionary<long, DataSeries>>();
                this.byType_IId_BSize[(int)barType][iId] = this.byType_IId_BSize[(int)barType][iId] ?? new Dictionary<long, DataSeries>();
                this.byType_IId_BSize[(int)barType][iId].TryGetValue(barSize, out dataSeries);
            }
            if (dataSeries != null)
                return dataSeries;

            string name = DataSeriesNameHelper.GetName(instrument, barType, barSize);
            dataSeries = GetDataSeries(name);
            if (dataSeries == null && createNotExist)
            {
                dataSeries = CreateDataSeries(name);
                this.dataFile.Write(name, dataSeries);
                this.dataFile.Flush();
            }

            // Save to cache
            if (barType == BarType.Time && barSize <= TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond)
                this.byIId_BSize[iId][(int)barSize] = dataSeries;
            else
                this.byType_IId_BSize[(int)barType][iId][barSize] = dataSeries;

            return dataSeries;
        }

        private DataSeries CreateDataSeries(string name)
        {
            var s = this.host == null ? new DataSeries(name) : new NetDataSeries(name);
            return s;
        }
    }
}