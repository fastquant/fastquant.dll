// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace SmartQuant
{
    public class OrderServer
    {
        private Framework framework;
        protected string seriesName = "messages";
        protected int seriesId;
        protected bool isOpen;
        protected IdArray<string> nameById = new IdArray<string>(1024);
        protected Dictionary<string, int> idByName = new Dictionary<string, int>();

        public virtual string SeriesName
        {
            get
            {
                return this.seriesName;
            }
            set
            {
                if (this.seriesName != value)
                {
                    this.seriesName = value;
                    this.seriesId = Get(this.seriesName);
                }
            }
        }

        public OrderServer(Framework framework)
        {
            this.framework = framework;
        }

        public virtual void Dispose()
        {
            Close();
        }

        public virtual void Clear()
        {
        }

        public virtual void Flush()
        {
        }

        public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }

        public virtual int Get(string seriesName)
        {
            return -1;
        }

        public virtual List<ExecutionMessage> Load(string seriesName = null)
        {
            return null;
        }

        public virtual void Save(ExecutionMessage message, int id = -1)
        {
        }

        public virtual void Delete(string seriesName)
        {
        }
    }
}