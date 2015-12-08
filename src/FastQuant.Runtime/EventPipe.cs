using System;
using System.Threading;

namespace SmartQuant
{
    public class EventPipe
    {
        private Framework framework;
        private LinkedList<IEventQueue> queues = new LinkedList<IEventQueue>();
        private EventTree tree = new EventTree();

        public int Count => queues.Count;

        public EventPipe(Framework framework, bool threaded = false)
        {
            this.framework = framework;
            if (threaded)
                throw new NotSupportedException();
        }

        public void Add(IEventQueue queue)
        {
            if (queue.IsSynched)
                this.tree.Add(queue);
            else
                this.queues.Add(queue);
        }

        public void Remove(IEventQueue queue)
        {
            if (queue.IsSynched)
                this.queues.Remove(queue);
            else
                this.queues.Remove(queue);
        }

        public void Clear()
        {
            this.queues.Clear();
            this.tree.Clear();
        }

        public Event Dequeue()
        {
            throw new NotSupportedException();
        }

        public bool IsEmpty()
        {
            for (var q = this.queues.First; q != null; q = q.Next)
                if (!q.Data.IsEmpty())
                    return false;

            return this.tree.IsEmpty();
        }

        public Event Read()
        {
            var node = this.queues.First;
            var last_not_empty_q = node;

            LinkedListNode<IEventQueue> linkedListNode2 = null;
            while (node != null)
            {
                var q = node.Data;
                if (!q.IsEmpty())
                {
                    var @event = q.Read();
                    if (@event.TypeId == EventType.OnQueueClosed)
                    {
                        if (linkedListNode2 == null)
                        {
                            this.queues.First = node.Next;
                        }
                        else
                        {
                            linkedListNode2.Next = node.Next;
                        }
                        this.queues.Count--;
                    }
                    return @event;
                }
                linkedListNode2 = node;
                node = node.Next;
            }

            return this.tree.IsEmpty() ? null : this.tree.Read();
        }
    }
}