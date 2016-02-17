// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace FastQuant
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
            var dataFile = GetFromCache(name);
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
            lock (this)
            {
                var dataFile = GetFromCache(name);
                if (dataFile == null)
                {
                    Console.WriteLine($"{DateTime.Now} Opening file : {name}");
                    dataFile = new DataFile(Path.Combine(this.path, name), this.smanager);
                    dataFile.Open(mode);
                    this.dataFiles.Add(name, dataFile);
                }
                return dataFile;
            }
        }

        public DataSeries GetSeries(string fileName, string seriesName)
        {
            var file = GetFile(fileName, FileMode.OpenOrCreate);
            var series = (DataSeries)file.Get(seriesName);
            if (series == null)
            {
                series = new DataSeries(seriesName);
                file.Write(seriesName, series);
            }
            return series;
        }

        private DataFile GetFromCache(string name)
        {
            DataFile dataFile;
            this.dataFiles.TryGetValue(name, out dataFile);
            return dataFile;
        }
    }
}