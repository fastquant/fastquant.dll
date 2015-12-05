using System;
using System.Collections.Generic;
using System.IO;

namespace SmartQuant
{
    public class DataFileManager
    {
        private Dictionary<string, DataFile> dataFiles = new Dictionary<string, DataFile>();

        private StreamerManager smanager = new StreamerManager();

        private string path;

        public DataFileManager(string path)
        {
            this.path = path;
            this.smanager.AddDefaultStreamers();
        }

        public void Close()
        {
            foreach (var file in this.dataFiles.Values)
                file.Close();
        }

        public void Close(string name)
        {
            DataFile dataFile;
            this.dataFiles.TryGetValue(name, out dataFile);
            if (dataFile != null)
            {
                dataFile.Close();
                this.dataFiles.Remove(name);
            }
        }

        public void Delete(string fileName, string objectName)
        {
            var file = GetFile(fileName, FileMode.OpenOrCreate);
            file.Delete(objectName);
        }

        public DataFile GetFile(string name, FileMode mode = FileMode.OpenOrCreate)
        {
            DataFile result;
            lock (this)
            {
                DataFile dataFile;
                this.dataFiles.TryGetValue(name, out dataFile);
                if (dataFile == null)
                {
                    Console.WriteLine(DateTime.Now + " Opening file : " + name);
                    dataFile = new DataFile(Path.Combine(this.path, name), this.smanager);
                    dataFile.Open(mode);
                    this.dataFiles.Add(name, dataFile);
                }
                result = dataFile;
            }
            return result;
        }

        public DataSeries GetSeries(string fileName, string seriesName)
        {
            DataFile file = GetFile(fileName, FileMode.OpenOrCreate);
            DataSeries dataSeries = (DataSeries)file.Get(seriesName);
            if (dataSeries == null)
            {
                dataSeries = new DataSeries(seriesName);
                file.Write(seriesName, dataSeries);
            }
            return dataSeries;
        }
    }
}