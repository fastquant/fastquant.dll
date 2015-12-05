// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class EventTreeItem
    {
        public EventTreeItem(IEventQueue queue)
        {
            this.queue = queue;
            this.head = this;
        }

        internal DateTime dateTime;

        internal EventTreeItem left;

        internal EventTreeItem right;

        internal EventTreeItem next;

        internal EventTreeItem prev;

        internal EventTreeItem head;

        internal IEventQueue queue;
    }

    public class EventTree
    {
        internal EventTreeItem first;
        internal EventTreeItem last;

        public void Add(IEventQueue queue)
        {
            if (queue.IsEmpty())
                throw new Exception($"{nameof(EventTree)}::{nameof(Add)} Can not add queue, the queue is empty : {queue.Name}");

            var item = new EventTreeItem(queue);
            this.method_0(item);
        }

        public void Clear()
        {
            this.first = this.last = null;
        }

        public bool IsEmpty()
        {
            if (this.first == null && this.last == null)
            {
                return true;
            }
            if (this.last == null)
            {
                return false;
            }
            if (this.last.queue.IsEmpty())
            {
                return true;
            }
            this.method_0(this.last);
            this.last = null;
            return false;
        }

        private void method_0(EventTreeItem item)
        {
            if (this.first == null)
            {
                this.first = item;
                return;
            }

            item.right = item.left = item.next = item.prev = null;
            item.head = item;
            item.dateTime = item.queue.PeekDateTime();

            EventTreeItem current = this.first;
            EventTreeItem last_current = null;
            while (current.dateTime != item.dateTime)
            {
                if (item.dateTime > current.dateTime)
                {
                    if (current.right == null)
                    {
                        current.right = item;
                        return;
                    }
                    last_current = current;
                    current = current.right;
                }
                else
                {
                    if (current.left == null)
                    {
                        current.left = item;
                        return;
                    }
                    last_current = current;
                    current = current.left;
                }
            }

            item.prev = current;
            current.next = item;
            item.head = current.head;
            item.right = current.right;
            item.left = current.left;
            if (last_current == null)
            {
                this.first = item;
                return;
            }

            if (item.dateTime > last_current.dateTime)
                last_current.right = item;
            else 
                last_current.left = item;
        }

        public Event Read()
        {
            if (this.last != null)
                throw new Exception("EventTree::Read Can not read from a tree with empty queue");

            // Move to the last two items
            EventTreeItem current = this.first;
            EventTreeItem last_current = null;
            while (current.left != null)
            {
                last_current = current;
                current = current.left;
            }

            if (current.prev != null)
            {
                EventTreeItem eventTreeItem_ = current.head;
                current.head.next.prev = null;
                current.head = current.head.next;
                current = eventTreeItem_;
            }
            else
            {
                if (last_current != null)
                {
                    last_current.left = current.right;
                }
                else
                {
                    this.first = current.right;
                }
                current.right = null;
            }

            Event @event = current.queue.Read();
            if (@event.TypeId == EventType.OnQueueClosed)
            {
                if (this.first == null)
                {
                    @event = new OnSimulatorStop();
                }
            }
            else if (current.queue.IsEmpty())
            {
                this.last = current;
            }
            else
            {
                this.method_0(current);
            }
            return @event;
        }

        public void Remove(IEventQueue queue)
        {
            Console.WriteLine("EventTree::Remove is called");
        }

    }

}
