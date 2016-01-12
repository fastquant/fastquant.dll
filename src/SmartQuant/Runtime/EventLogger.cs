// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public class EventLogger
    {
        protected internal Framework framework;

        public string Name { get; private set; }

        public EventLogger(Framework framework, string name)
        {
            this.framework = framework;
            Name = name;
        }

        public virtual void OnEvent(Event e)
        {
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

        public void Enable(byte typeId)
        {
        }

        public void Disable(byte typeId)
        {
        }

        public override void OnEvent(Event e)
        {
        }
    }

    public class EventLoggerManager
    {
        private Dictionary<string, EventLogger> loggers = new Dictionary<string, EventLogger>();

        public void Add(EventLogger logger) => this.loggers[logger.Name] = logger;

        public EventLogger GetLogger(string name)
        {
            EventLogger result;
            this.loggers.TryGetValue(name, out result);
            return result;
        }
    }
}
