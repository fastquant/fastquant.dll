using System;
using System.Threading;

namespace SmartQuant
{
    public class EventPipe
    {
        private Framework framework;
        private LinkedList<IEventQueue> list = new LinkedList<IEventQueue>();
        private EventTree tree = new EventTree();

        internal bool Threaded { get; }

        public int Count => this.list.Count;

        public EventPipe(Framework framework, bool threaded = false)
        {
            this.framework = framework;
            Threaded = threaded;
            if (threaded)
                throw new NotSupportedException();
        }

        public void Add(IEventQueue queue)
        {
            if (queue.IsSynched)
                this.tree.Add(queue);
            else
                this.list.Add(queue);
        }

        public void Remove(IEventQueue queue)
        {
            if (queue.IsSynched)
                this.tree.Remove(queue);
            else
                this.list.Remove(queue);
        }

        public void Clear()
        {
            this.list.Clear();
            this.tree.Clear();
        }

        public Event Dequeue() => null;

        public bool IsEmpty()
        {
            if (this.list.Count != 0)
                for (var q = this.list.First; q != null; q = q.Next)
                    if (!q.Data.IsEmpty())
                        return false;

            return this.tree.IsEmpty();
        }

        public Event Read()
        {
            var node = this.list.First;
            LinkedListNode<IEventQueue> lastNode = null;
            while (node != null)
            {
                var q = node.Data;
                if (!q.IsEmpty())
                {
                    var e = q.Read();
                    if (e.TypeId == EventType.OnQueueClosed)
                    {
                        if (lastNode == null)
                            this.list.First = node.Next;
                        else
                            lastNode.Next = node.Next;
                        this.list.Count--;
                    }
                    return e;
                }
                lastNode = node;
                node = node.Next;
            }

            return this.tree.IsEmpty() ? null : this.tree.Read();
        }
    }
}