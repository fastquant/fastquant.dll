// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class EventTreeItem
    {
        internal IEventQueue ieventQueue_0;
        internal DateTime dateTime_0;
        internal EventTreeItem eventTreeItem_0;
        internal EventTreeItem eventTreeItem_1;
        internal EventTreeItem eventTreeItem_2;
        internal EventTreeItem eventTreeItem_3;
        internal EventTreeItem eventTreeItem_4;

        public EventTreeItem(IEventQueue queue)
        {
            this.ieventQueue_0 = queue;
            this.eventTreeItem_4 = this;
        }
    }

    public class EventTree
    {
        internal EventTreeItem eventTreeItem_0;
        internal EventTreeItem eventTreeItem_1;

        public bool IsEmpty()
        {
            if (this.eventTreeItem_0 == null && this.eventTreeItem_1 == null)
                return true;
            if (this.eventTreeItem_1 == null)
                return false;
            if (this.eventTreeItem_1.ieventQueue_0.IsEmpty())
                return true;
            this.method_0(this.eventTreeItem_1);
            this.eventTreeItem_1 = (EventTreeItem)null;
            return false;
        }

        public Event Read()
        {
            if (this.eventTreeItem_1 != null)
                throw new Exception("EventTree::Read Can not read from a tree with empty queue");
            EventTreeItem eventTreeItem_2 = this.eventTreeItem_0;
            EventTreeItem eventTreeItem1 = (EventTreeItem)null;
            for (; eventTreeItem_2.eventTreeItem_0 != null; eventTreeItem_2 = eventTreeItem_2.eventTreeItem_0)
                eventTreeItem1 = eventTreeItem_2;
            if (eventTreeItem_2.eventTreeItem_3 != null)
            {
                EventTreeItem eventTreeItem2 = eventTreeItem_2.eventTreeItem_4;
                eventTreeItem_2.eventTreeItem_4.eventTreeItem_2.eventTreeItem_3 = (EventTreeItem)null;
                eventTreeItem_2.eventTreeItem_4 = eventTreeItem_2.eventTreeItem_4.eventTreeItem_2;
                eventTreeItem_2 = eventTreeItem2;
            }
            else
            {
                if (eventTreeItem1 != null)
                    eventTreeItem1.eventTreeItem_0 = eventTreeItem_2.eventTreeItem_1;
                else
                    this.eventTreeItem_0 = eventTreeItem_2.eventTreeItem_1;
                eventTreeItem_2.eventTreeItem_1 = (EventTreeItem)null;
            }
            Event @event = eventTreeItem_2.ieventQueue_0.Read();
            if ((int)@event.TypeId == 206)
            {
                if (this.eventTreeItem_0 == null)
                    @event = (Event)new OnSimulatorStop();
            }
            else if (eventTreeItem_2.ieventQueue_0.IsEmpty())
                this.eventTreeItem_1 = eventTreeItem_2;
            else
                this.method_0(eventTreeItem_2);
            return @event;
        }

        private void method_0(EventTreeItem eventTreeItem_2)
        {
            eventTreeItem_2.eventTreeItem_1 = (EventTreeItem)null;
            eventTreeItem_2.eventTreeItem_0 = (EventTreeItem)null;
            eventTreeItem_2.eventTreeItem_2 = (EventTreeItem)null;
            eventTreeItem_2.eventTreeItem_3 = (EventTreeItem)null;
            eventTreeItem_2.eventTreeItem_4 = eventTreeItem_2;
            eventTreeItem_2.dateTime_0 = eventTreeItem_2.ieventQueue_0.PeekDateTime();
            if (this.eventTreeItem_0 == null)
            {
                this.eventTreeItem_0 = eventTreeItem_2;
            }
            else
            {
                EventTreeItem eventTreeItem1 = this.eventTreeItem_0;
                EventTreeItem eventTreeItem2 = (EventTreeItem)null;
                while (!(eventTreeItem1.dateTime_0 == eventTreeItem_2.dateTime_0))
                {
                    if (eventTreeItem_2.dateTime_0 > eventTreeItem1.dateTime_0)
                    {
                        if (eventTreeItem1.eventTreeItem_1 != null)
                        {
                            eventTreeItem2 = eventTreeItem1;
                            eventTreeItem1 = eventTreeItem1.eventTreeItem_1;
                        }
                        else
                        {
                            eventTreeItem1.eventTreeItem_1 = eventTreeItem_2;
                            return;
                        }
                    }
                    else if (eventTreeItem1.eventTreeItem_0 != null)
                    {
                        eventTreeItem2 = eventTreeItem1;
                        eventTreeItem1 = eventTreeItem1.eventTreeItem_0;
                    }
                    else
                    {
                        eventTreeItem1.eventTreeItem_0 = eventTreeItem_2;
                        return;
                    }
                }
                eventTreeItem_2.eventTreeItem_3 = eventTreeItem1;
                eventTreeItem1.eventTreeItem_2 = eventTreeItem_2;
                eventTreeItem_2.eventTreeItem_4 = eventTreeItem1.eventTreeItem_4;
                eventTreeItem_2.eventTreeItem_1 = eventTreeItem1.eventTreeItem_1;
                eventTreeItem_2.eventTreeItem_0 = eventTreeItem1.eventTreeItem_0;
                if (eventTreeItem2 != null)
                {
                    if (eventTreeItem_2.dateTime_0 > eventTreeItem2.dateTime_0)
                        eventTreeItem2.eventTreeItem_1 = eventTreeItem_2;
                    else
                        eventTreeItem2.eventTreeItem_0 = eventTreeItem_2;
                }
                else
                    this.eventTreeItem_0 = eventTreeItem_2;
            }
        }

        public void Add(IEventQueue queue)
        {
            if (queue.IsEmpty())
                throw new Exception("EventTree::Add Can not add queue, the queue is empty : " + queue.Name);
            EventTreeItem eventTreeItem_2 = new EventTreeItem(queue);
            if (this.eventTreeItem_0 == null)
                this.eventTreeItem_0 = eventTreeItem_2;
            else
                this.method_0(eventTreeItem_2);
        }

        public void Remove(IEventQueue queue)
        {
            Console.WriteLine("EventTree::Remove is called");
        }

        public void Clear()
        {
            this.eventTreeItem_0 = (EventTreeItem)null;
            this.eventTreeItem_1 = (EventTreeItem)null;
        }
    }
}
