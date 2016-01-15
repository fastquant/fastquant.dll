using System.Collections.Generic;
using System.IO;
using System;

namespace SmartQuant
{
    public class FileDataServer : DataServer
    {
        private string host;
        private DataFile dataFile;
        private bool opened;

        private IdArray<IdArray<DataSeries>> idArray_1 = new IdArray<IdArray<DataSeries>>();
        private IdArray<IdArray<Dictionary<long, DataSeries>>> idArray_2 = new IdArray<IdArray<Dictionary<long, DataSeries>>>();
        private IdArray<DataSeries>[] idArray_0;

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
                this.idArray_0 = new IdArray<DataSeries>[128];
                for (int i = 0; i < this.idArray_0.Length; i++)
                    this.idArray_0[i] = new IdArray<DataSeries>();
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
            {
                return this.method_0(instrument, barType, barSize, false);
            }
            var dataSeries = this.idArray_0[type][instrument.Id];
            if (dataSeries == null)
            {
                string name = DataSeriesNameHelper.GetName(instrument, type);
                dataSeries = (this.dataFile.Get(name) as DataSeries);
                this.idArray_0[type][instrument.Id] = dataSeries;
            }
            return dataSeries;
        }

        public override List<DataSeries> GetDataSeriesList(Instrument instrument = null, string pattern = null)
        {
            var list = new List<DataSeries>();
            foreach (var key in this.dataFile.Keys.Values)
            {
                if (key.TypeId == ObjectType.DataSeries  && (instrument == null || !(DataSeriesNameHelper.GetSymbol(key.Name) != instrument.Symbol)) && (pattern == null || key.Name.Contains(pattern)))
                {
                    list.Add(GetDataSeries(key.Name));
                }
            }
            return list;
        }

        public override DataSeries AddDataSeries(string name)
        {
            DataSeries dataSeries = GetDataSeries(name);
            if (dataSeries == null)
            {
                if (this.host == null)
                {
                    dataSeries = new DataSeries(name);
                }
                else
                {
                    throw new NotImplementedException();
                    //dataSeries = new NetDataSeries(name);
                }
                this.dataFile.Write(name, dataSeries);
            }
            return dataSeries;
        }

        public override DataSeries AddDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            if (type == DataObjectType.Bar)
            {
                return this.method_0(instrument, barType, barSize, true);
            }
            DataSeries dataSeries = this.idArray_0[type][instrument.Id];
            if (dataSeries == null)
            {
                string name = DataSeriesNameHelper.GetName(instrument, type);
                dataSeries = GetDataSeries(name);
                if (dataSeries == null)
                {
                    if (this.host == null)
                    {
                        dataSeries = new DataSeries(name);
                    }
                    else
                    {
                        throw new NotImplementedException();
                        //dataSeries = new NetDataSeries(name);
                    }
                    this.dataFile.Write(name, dataSeries);
                }
                this.idArray_0[type][instrument.Id] = dataSeries;
            }
            return dataSeries;
        }

        public override void DeleteDataSeries(string name)
        {
            DataSeries dataSeries = GetDataSeries(name);
            if (dataSeries != null)
            {
                for (int i = 0; i < this.idArray_0.Length; i++)
                {
                    for (int j = 0; j < this.idArray_0[i].Size; j++)
                    {
                        if (this.idArray_0[i][j] == dataSeries)
                        {
                            this.idArray_0[i].Remove(j);
                        }
                    }
                }
                this.dataFile.Delete(name);
            }
        }

        public override void DeleteDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            DataSeries dataSeries;
            if (type == DataObjectType.Bar)
            {
                var iId = instrument.Id;
                dataSeries = this.method_0(instrument, barType, barSize, false);
                if (barType == BarType.Time && barSize <= 86400L)
                {
                    DataSeries dataSeries2 = this.idArray_1[iId][(int)barSize] = null;
                }
                else
                {
                    if (this.idArray_2[(int)type] == null)
                    {
                        this.idArray_2[(int)type] = new IdArray<Dictionary<long, DataSeries>>();
                    }
                    if (this.idArray_2[(int)type][iId] == null)
                    {
                        this.idArray_2[(int)type][iId] = new Dictionary<long, DataSeries>();
                    }
                    this.idArray_2[(int)type][iId].Remove(barSize);
                }
                this.dataFile.Delete(DataSeriesNameHelper.GetName(instrument, barType, barSize));
                return;
            }
            dataSeries = this.idArray_0[(int)type][instrument.Id];
            if (dataSeries != null)
            {
                this.idArray_0[(int)type].Remove(instrument.Id);
            }
            this.dataFile.Delete(DataSeriesNameHelper.GetName(instrument, type));
        }

        public override void Save(Instrument instrument, DataObject obj, SaveMode option = SaveMode.Add)
        {
            throw new NotImplementedException();
        }

        private DataSeries method_0(Instrument instrument_0, BarType barType_0, long long_0, bool bool_1)
        {
            DataSeries dataSeries;
            var iId = instrument_0.Id;
            if (barType_0 == BarType.Time && long_0 <= TimeSpan.TicksPerDay/TimeSpan.TicksPerSecond)
            {
                if (this.idArray_1[iId] == null)
                {
                    this.idArray_1[iId] = new IdArray<DataSeries>();
                }
                dataSeries = this.idArray_1[iId][(int)long_0];
            }
            else
            {
                if (this.idArray_2[(int)barType_0] == null)
                {
                    this.idArray_2[(int)barType_0] = new IdArray<Dictionary<long, DataSeries>>(1000);
                }
                if (this.idArray_2[(int)barType_0][iId] == null)
                {
                    this.idArray_2[(int)barType_0][iId] = new Dictionary<long, DataSeries>();
                }
                this.idArray_2[(int)barType_0][iId].TryGetValue(long_0, out dataSeries);
            }
            if (dataSeries == null)
            {
                string name = DataSeriesNameHelper.GetName(instrument_0, barType_0, long_0);
                dataSeries = GetDataSeries(name);
                if (dataSeries == null & bool_1)
                {
                    if (this.host == null)
                    {
                        dataSeries = new DataSeries(name);
                    }
                    else
                    {
                        throw new NotImplementedException();
                        //dataSeries = new NetDataSeries(name);
                    }
                    this.dataFile.Write(name, dataSeries);
                    this.dataFile.Flush();
                }
                if (dataSeries != null)
                {
                    if (barType_0 == BarType.Time && long_0 <= TimeSpan.TicksPerDay/TimeSpan.TicksPerSecond)
                    {
                        this.idArray_1[iId][(int)long_0] = dataSeries;
                    }
                    else
                    {
                        this.idArray_2[(int)barType_0][iId][long_0] = dataSeries;
                    }
                }
            }
            return dataSeries;
        }
    }
}