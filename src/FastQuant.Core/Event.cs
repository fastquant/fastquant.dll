// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class Event
    {
        protected internal DateTime dateTime;

        public DateTime DateTime
        {
            get
            {
                return this.dateTime;
            }
            set
            {
                this.dateTime = value;
            }
        }

        public virtual byte TypeId => EventType.Event;

        public Event()
        {
        }

        public Event(DateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        public override string ToString() => $"{DateTime} {GetType()}";
    }
}
