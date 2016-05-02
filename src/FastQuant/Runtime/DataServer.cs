// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace FastQuant
{
    public enum SaveMode
    {
        Add,
        Append,
    }

    public class DataServer : IDisposable
    {
        protected Framework framework;
        protected bool tapeMode;

        public bool TapeMode
        {
            get
            {
                return this.tapeMode;
            }
            set
            {
                this.tapeMode = value;
            }
        }

        public DataServer(Framework framework)
        {
            this.framework = framework;
            this.tapeMode = false;
        }

        public virtual void Open()
        {
            // no-op
        }

        public virtual void Close()
        {
            // no-op
        }

        public virtual void Flush()
        {
            // no-op
        }

        public virtual void Save(Instrument instrument, DataObject obj, SaveMode option = SaveMode.Add)
        {
            // no-op
        }

        public virtual DataSeries GetDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            return null;
        }

        public virtual DataSeries GetDataSeries(string name)
        {
            return null;
        }

        public virtual List<DataSeries> GetDataSeriesList(Instrument instrument = null, string pattern = null)
        {
            return null;
        }

        public virtual void DeleteDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60L)
        {
            // no-op
        }

        public virtual void DeleteDataSeries(string name)
        {
            // no-op
        }

        public virtual DataSeries AddDataSeries(Instrument instrument, byte type, BarType barType = BarType.Time, long barSize = 60)
        {
            return null;
        }

        public virtual DataSeries AddDataSeries(string name)
        {
            return null;
        }

        public virtual void Dispose()
        {
            Close();
        }

        public virtual void Refresh()
        {
            // noop
        }
    }
}