// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace FastQuant
{
    public class EventLogger
    {
        protected internal string name;
        protected internal Framework framework;

        public string Name => this.name;

        public EventLogger()
        {
        }

        public EventLogger(Framework framework, string name)
        {
            this.framework = framework;
            this.name = name;
            this.framework.EventLoggerManager.Add(this);
        }

        public virtual void OnEvent(Event e)
        {
            // noop
        }
    }

    public class ConsoleEventLogger : EventLogger
    {
        public ConsoleEventLogger(Framework framework)
            : base(framework, "Console")
        {
        }

        public override void OnEvent(Event e)
        {
            if (e == null || e.TypeId == EventType.Bid || e.TypeId == EventType.Ask || e.TypeId == EventType.Trade || e.TypeId == EventType.Bar)
                return;
            Console.WriteLine($"Event {e.TypeId} {e.GetType()}");
        }
    }

    public class DataSeriesEventLogger : EventLogger
    {
        private DataSeries series;
        private IdArray<bool> gates = new IdArray<bool>(256);
        private DateTime dateTime;

        public DataSeriesEventLogger(Framework framework, DataSeries series)
            : base(framework, "DataSeriesEventLogger")
        {
            this.series = series;
        }

        public DataSeriesEventLogger(DataSeries series)
            : base(Framework.Current, "DataSeriesEventLogger")
        {
            this.series = series;
        }

        public void Enable(byte typeId) => this.gates[typeId] = true;

        public void Disable(byte typeId) => this.gates[typeId] = false;

        public override void OnEvent(Event e)
        {
            if (this.gates[e.TypeId])
            {

                if (e.DateTime >= this.dateTime)
                {
                    this.dateTime = e.dateTime;
                    this.series.Add((DataObject)e);
                }
                else
                {
                    Console.WriteLine($"!{e} = {e.DateTime} <> {this.dateTime}");
                }
            }
        }
    }

    public class EventLoggerManager
    {
        private readonly Dictionary<string, EventLogger> loggers = new Dictionary<string, EventLogger>();

        public void Add(EventLogger logger) => this.loggers[logger.Name] = logger;

        public EventLogger GetLogger(string name)
        {
            EventLogger result;
            this.loggers.TryGetValue(name, out result);
            return result;
        }
    }
}
