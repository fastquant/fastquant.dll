// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    //public class EventTreeItem
    //{
    //    public EventTreeItem(IEventQueue queue)
    //    {
    //        EventQueue = queue;
    //        Head = this;
    //    }

    //    internal DateTime DateTime { get; set; }

    //    internal EventTreeItem Left { get; set; }

    //    internal EventTreeItem Right { get; set; }

    //    internal EventTreeItem Next { get; set; }

    //    internal EventTreeItem Prev { get; set; }

    //    internal EventTreeItem Head { get; set; }

    //    internal IEventQueue EventQueue { get; set; }
    //}

    //public class EventTree
    //{
    //    internal EventTreeItem root;
    //    internal EventTreeItem last;

    //    public void Add(IEventQueue queue)
    //    {
    //        if (queue.IsEmpty())
    //            throw new Exception($"{nameof(EventTree)}::{nameof(Add)} Can not add queue, the queue is empty : {queue.Name}");

    //        var item = new EventTreeItem(queue);
    //        Arrange(item);
    //    }

    //    public void Clear()
    //    {
    //        this.root = this.last = null;
    //    }

    //    public bool IsEmpty()
    //    {
    //        if (this.root == null && this.last == null)
    //            return true;

    //        if (this.last == null)
    //            return false;

    //        if (this.last.EventQueue.IsEmpty())
    //            return true;

    //        Arrange(this.last);
    //        this.last = null;
    //        return false;
    //    }

    //    // TODO: need more comments
    //    private void Arrange(EventTreeItem item)
    //    {
    //        item.Right = item.Left = item.Next = item.Prev = null;
    //        item.Head = item;
    //        item.DateTime = item.EventQueue.PeekDateTime();

    //        if (this.root == null)
    //        {
    //            this.root = item;
    //            return;
    //        }

    //        EventTreeItem current = this.root;
    //        EventTreeItem last_current = null;
    //        while (item.DateTime != current.DateTime)
    //        {
    //            if (item.DateTime > current.DateTime)
    //            {
    //                if (current.Right == null)
    //                {
    //                    current.Right = item;
    //                    return;
    //                }
    //                last_current = current;
    //                current = current.Right;
    //            }
    //            else
    //            {
    //                if (current.Left == null)
    //                {
    //                    current.Left = item;
    //                    return;
    //                }
    //                last_current = current;
    //                current = current.Left;
    //            }
    //        }

    //        item.Prev = current;
    //        current.Next = item;
    //        item.Head = current.Head;
    //        item.Right = current.Right;
    //        item.Left = current.Left;

    //        if (last_current == null)
    //        {
    //            this.root = item;
    //            return;
    //        }

    //        if (last_current.DateTime < item.DateTime)
    //            last_current.Right = item;
    //        else 
    //            last_current.Left = item;
    //    }

    //    // TODO: need more comments
    //    public Event Read()
    //    {
    //        if (this.last != null)
    //            throw new Exception("EventTree::Read Can not read from a tree with empty queue");

    //        // Move to the last two items
    //        EventTreeItem current = this.root;
    //        EventTreeItem last_current = null;

    //        // find the leftmost node
    //        while (current.Left != null)
    //        {
    //            last_current = current;
    //            current = current.Left;
    //        }

    //        if (current.Prev != null)
    //        {
    //            var temp = current.Head;
    //            current.Head.Next.Prev = null;
    //            current.Head = current.Head.Next;
    //            current = temp;
    //        }
    //        else
    //        {
    //            if (last_current != null)
    //                last_current.Left = current.Right;
    //            else
    //                this.root = current.Right;
    //            current.Right = null;
    //        }

    //        var e = current.EventQueue.Read();
    //        if (e.TypeId == EventType.OnQueueClosed)
    //        {
    //            if (this.root == null)
    //                e = new OnSimulatorStop();
    //        }
    //        else if (current.EventQueue.IsEmpty())
    //            this.last = current;
    //        else
    //            Arrange(current);
    //        return e;
    //    }

    //    public void Remove(IEventQueue queue) => Console.WriteLine("EventTree::Remove is called");
    //}

    public class EventTreeItem
    {
        public EventTreeItem(IEventQueue queue)
        {
            this.queue = queue;
            this.eventTreeItem_4 = this;
        }

        // Token: 0x04000343 RID: 835
        internal DateTime dateTime;

        // Token: 0x04000344 RID: 836
        internal EventTreeItem eventTreeItem_0;

        // Token: 0x04000345 RID: 837
        internal EventTreeItem eventTreeItem_1;

        // Token: 0x04000346 RID: 838
        internal EventTreeItem eventTreeItem_2;

        // Token: 0x04000347 RID: 839
        internal EventTreeItem eventTreeItem_3;

        // Token: 0x04000348 RID: 840
        internal EventTreeItem eventTreeItem_4;

        // Token: 0x04000342 RID: 834
        internal IEventQueue queue;
    }
    public class EventTree
    {
        public void Add(IEventQueue queue)
        {
            if (queue.IsEmpty())
            {
                throw new Exception("EventTree::Add Can not add queue, the queue is empty : " + queue.Name);
            }
            EventTreeItem eventTreeItem_ = new EventTreeItem(queue);
            if (this.eventTreeItem_0 == null)
            {
                this.eventTreeItem_0 = eventTreeItem_;
                return;
            }
            this.method_0(eventTreeItem_);
        }

        // Token: 0x060006AF RID: 1711 RVA: 0x0000688A File Offset: 0x00004A8A
        public void Clear()
        {
            this.eventTreeItem_0 = null;
            this.eventTreeItem_1 = null;
        }

        // Token: 0x060006AA RID: 1706 RVA: 0x0002ACDC File Offset: 0x00028EDC
        public bool IsEmpty()
        {
            if (this.eventTreeItem_0 == null && this.eventTreeItem_1 == null)
            {
                return true;
            }
            if (this.eventTreeItem_1 == null)
            {
                return false;
            }
            if (this.eventTreeItem_1.queue.IsEmpty())
            {
                return true;
            }
            this.method_0(this.eventTreeItem_1);
            this.eventTreeItem_1 = null;
            return false;
        }

        // Token: 0x060006AC RID: 1708 RVA: 0x0002AE0C File Offset: 0x0002900C
        private void method_0(EventTreeItem eventTreeItem_2)
        {
            eventTreeItem_2.eventTreeItem_1 = null;
            eventTreeItem_2.eventTreeItem_0 = null;
            eventTreeItem_2.eventTreeItem_2 = null;
            eventTreeItem_2.eventTreeItem_3 = null;
            eventTreeItem_2.eventTreeItem_4 = eventTreeItem_2;
            eventTreeItem_2.dateTime = eventTreeItem_2.queue.PeekDateTime();
            if (this.eventTreeItem_0 == null)
            {
                this.eventTreeItem_0 = eventTreeItem_2;
                return;
            }
            EventTreeItem eventTreeItem = this.eventTreeItem_0;
            EventTreeItem eventTreeItem2 = null;
            while (!(eventTreeItem.dateTime == eventTreeItem_2.dateTime))
            {
                if (eventTreeItem_2.dateTime > eventTreeItem.dateTime)
                {
                    if (eventTreeItem.eventTreeItem_1 == null)
                    {
                        eventTreeItem.eventTreeItem_1 = eventTreeItem_2;
                        return;
                    }
                    eventTreeItem2 = eventTreeItem;
                    eventTreeItem = eventTreeItem.eventTreeItem_1;
                }
                else
                {
                    if (eventTreeItem.eventTreeItem_0 == null)
                    {
                        eventTreeItem.eventTreeItem_0 = eventTreeItem_2;
                        return;
                    }
                    eventTreeItem2 = eventTreeItem;
                    eventTreeItem = eventTreeItem.eventTreeItem_0;
                }
            }
            eventTreeItem_2.eventTreeItem_3 = eventTreeItem;
            eventTreeItem.eventTreeItem_2 = eventTreeItem_2;
            eventTreeItem_2.eventTreeItem_4 = eventTreeItem.eventTreeItem_4;
            eventTreeItem_2.eventTreeItem_1 = eventTreeItem.eventTreeItem_1;
            eventTreeItem_2.eventTreeItem_0 = eventTreeItem.eventTreeItem_0;
            if (eventTreeItem2 == null)
            {
                this.eventTreeItem_0 = eventTreeItem_2;
                return;
            }
            if (eventTreeItem_2.dateTime > eventTreeItem2.dateTime)
            {
                eventTreeItem2.eventTreeItem_1 = eventTreeItem_2;
                return;
            }
            eventTreeItem2.eventTreeItem_0 = eventTreeItem_2;
        }

        // Token: 0x060006AB RID: 1707 RVA: 0x0002AD30 File Offset: 0x00028F30
        public Event Read()
        {
            if (this.eventTreeItem_1 != null)
            {
                throw new Exception("EventTree::Read Can not read from a tree with empty queue");
            }
            EventTreeItem eventTreeItem = this.eventTreeItem_0;
            EventTreeItem eventTreeItem2 = null;
            while (eventTreeItem.eventTreeItem_0 != null)
            {
                eventTreeItem2 = eventTreeItem;
                eventTreeItem = eventTreeItem.eventTreeItem_0;
            }
            if (eventTreeItem.eventTreeItem_3 != null)
            {
                EventTreeItem eventTreeItem_ = eventTreeItem.eventTreeItem_4;
                eventTreeItem.eventTreeItem_4.eventTreeItem_2.eventTreeItem_3 = null;
                eventTreeItem.eventTreeItem_4 = eventTreeItem.eventTreeItem_4.eventTreeItem_2;
                eventTreeItem = eventTreeItem_;
            }
            else
            {
                if (eventTreeItem2 != null)
                {
                    eventTreeItem2.eventTreeItem_0 = eventTreeItem.eventTreeItem_1;
                }
                else
                {
                    this.eventTreeItem_0 = eventTreeItem.eventTreeItem_1;
                }
                eventTreeItem.eventTreeItem_1 = null;
            }
            Event @event = eventTreeItem.queue.Read();
            if (@event.TypeId == EventType.OnQueueClosed)
            {
                if (this.eventTreeItem_0 == null)
                {
                    @event = new OnSimulatorStop();
                }
            }
            else if (eventTreeItem.queue.IsEmpty())
            {
                this.eventTreeItem_1 = eventTreeItem;
            }
            else
            {
                this.method_0(eventTreeItem);
            }
            return @event;
        }

        // Token: 0x060006AE RID: 1710 RVA: 0x0000687E File Offset: 0x00004A7E
        public void Remove(IEventQueue queue)
        {
            Console.WriteLine("EventTree::Remove is called");
        }

        // Token: 0x04000340 RID: 832
        internal EventTreeItem eventTreeItem_0;

        // Token: 0x04000341 RID: 833
        internal EventTreeItem eventTreeItem_1;
    }
}
