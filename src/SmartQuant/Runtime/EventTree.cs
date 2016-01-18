// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class EventTreeItem
    {
        public EventTreeItem(IEventQueue queue)
        {
            EventQueue = queue;
            Head = this;
        }

        internal DateTime DateTime { get; set; }

        internal EventTreeItem Left { get; set; }

        internal EventTreeItem Right { get; set; }

        internal EventTreeItem Next { get; set; }

        internal EventTreeItem Prev { get; set; }

        internal EventTreeItem Head { get; set; }

        internal IEventQueue EventQueue { get; set; }
    }

    public class EventTree
    {
        internal EventTreeItem root;
        internal EventTreeItem last;

        public void Add(IEventQueue queue)
        {
            if (queue.IsEmpty())
                throw new Exception($"{nameof(EventTree)}::{nameof(Add)} Can not add queue, the queue is empty : {queue.Name}");

            var item = new EventTreeItem(queue);
            Arrange(item);
        }

        public void Clear()
        {
            this.root = this.last = null;
        }

        public bool IsEmpty()
        {
            if (this.root == null && this.last == null)
                return true;

            if (this.last == null)
                return false;

            if (this.last.EventQueue.IsEmpty())
                return true;

            Arrange(this.last);
            this.last = null;
            return false;
        }

        // TODO: need more comments
        private void Arrange(EventTreeItem item)
        {
            item.Right = item.Left = item.Next = item.Prev = null;
            item.Head = item;
            item.DateTime = item.EventQueue.PeekDateTime();

            if (this.root == null)
            {
                this.root = item;
                return;
            }

            EventTreeItem current = this.root;
            EventTreeItem last_current = null;
            while (item.DateTime != current.DateTime)
            {
                if (item.DateTime > current.DateTime)
                {
                    if (current.Right == null)
                    {
                        current.Right = item;
                        return;
                    }
                    last_current = current;
                    current = current.Right;
                }
                else
                {
                    if (current.Left == null)
                    {
                        current.Left = item;
                        return;
                    }
                    last_current = current;
                    current = current.Left;
                }
            }

            item.Prev = current;
            current.Next = item;
            item.Head = current.Head;
            item.Right = current.Right;
            item.Left = current.Left;

            if (last_current == null)
            {
                this.root = item;
                return;
            }

            if (last_current.DateTime < item.DateTime)
                last_current.Right = item;
            else 
                last_current.Left = item;
        }

        // TODO: need more comments
        public Event Read()
        {
            if (this.last != null)
                throw new Exception("EventTree::Read Can not read from a tree with empty queue");

            // Move to the last two items
            EventTreeItem current = this.root;
            EventTreeItem last_current = null;
            
            // find the leftmost node
            while (current.Left != null)
            {
                last_current = current;
                current = current.Left;
            }

            if (current.Prev != null)
            {
                var temp = current.Head;
                current.Head.Next.Prev = null;
                current.Head = current.Head.Next;
                current = temp;
            }
            else
            {
                if (last_current != null)
                    last_current.Left = current.Right;
                else
                    this.root = current.Right;
                current.Right = null;
            }

            var e = current.EventQueue.Read();
            if (e.TypeId == EventType.OnQueueClosed)
            {
                if (this.root == null)
                    e = new OnSimulatorStop();
            }
            else if (current.EventQueue.IsEmpty())
                this.last = current;
            else
                this.Arrange(current);
            return e;
        }

        public void Remove(IEventQueue queue) => Console.WriteLine("EventTree::Remove is called");
    }
}
