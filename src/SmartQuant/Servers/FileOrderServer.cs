using System;
using System.Collections.Generic;
using System.IO;

namespace SmartQuant
{
    public class FileOrderServer : OrderServer
    {
        private DataFile dataFile;

        private DataSeries dataSeries;

        private IdArray<DataSeries> serieses = new IdArray<DataSeries>();

        private int count;

        public string Host { get; }

        public int Port { get; }

        public FileOrderServer(Framework framework, string fileName, string host = null, int port = -1) : base(framework)
        {
            Host = host;
            Port = port;
            this.dataFile = host == null ? new DataFile(fileName, framework.StreamerManager) : new NetDataFile(fileName, host, port, framework.StreamerManager);
        }

        public override void Open()
        {
            if (!this.isOpen)
            {
                this.dataFile.Open(FileMode.OpenOrCreate);
                this.isOpen = true;
            }
        }

        public override void Close()
        {
            if (this.isOpen)
            {
                this.dataFile.Close();
                this.isOpen = false;
            }
        }

        public override void Clear()
        {
        }

        public override void Delete(string name)
        {
            this.dataFile.Delete(name);
            if (name == this.seriesName)
                this.dataSeries = null;
        }

        public override void Flush()
        {
            if (this.isOpen)
            {
                this.dataFile.Flush();
            }
        }

        public override int Get(string seriesName)
        {
            if (this.idByName.ContainsKey(seriesName))
                return this.idByName[seriesName];

            var dataSeries = (DataSeries)this.dataFile.Get(seriesName);
            if (dataSeries == null)
            { 
                dataSeries = Host == null ? new DataSeries(seriesName) : new NetDataSeries(seriesName);
                this.dataFile.Write(seriesName, dataSeries);
            }
            int num = this.count++;
            this.idByName[seriesName] = num;
            this.nameById[num] = seriesName;
            this.serieses[num] = dataSeries;
            return num;
        }

        public override List<ExecutionMessage> Load(string name = null)
        {
            if (name == null)
                name = this.seriesName;
            var list = new List<ExecutionMessage>();
            var series = (DataSeries)this.dataFile.Get(name);
            if (series != null)
            {
                for (long i = 0; i < series.Count; i++)
                {
                    var message = (ExecutionMessage)series[i];
                    message.IsLoaded = true;
                    list.Add(message);
                }
            }
            return list;
        }

        public override void Save(ExecutionMessage message, int id = -1)
        {
            DataSeries series;
            if (id == -1)
            {
                if (this.dataSeries == null)
                {
                    int id2 = Get(this.seriesName);
                    this.dataSeries = this.serieses[id2];
                    series = this.dataSeries;
                }
                else
                {
                    series = this.dataSeries;
                }
            }
            else
            {
                series = this.serieses[id];
                if (series == null)
                {
                    Console.WriteLine($"FileOrderServer::Save Error. Series with id does not exist : {id}");
                    return;
                }
            }
            series.Add(message);
        }
    }
}