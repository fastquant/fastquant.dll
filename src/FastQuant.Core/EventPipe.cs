using System;
using System.Threading;

namespace SmartQuant
{
    public class EventPipe
    {
        private Framework framework;
        private bool threaded;
        private Thread thread;
        private LinkedList<IEventQueue> queues = new LinkedList<IEventQueue>();
        private EventTree tree = new EventTree();

        private EventQueue hubQueue;

        public int Count => queues.Count;

        public EventPipe(Framework framework, bool threaded = false)
        {
            this.framework = framework;
            this.threaded = threaded;
            if (this.threaded)
            {
                this.hubQueue = new EventQueue(EventQueueId.All, EventQueueType.Master, EventQueuePriority.Normal,
                    100000, null);
                this.thread = new Thread(new ThreadStart(Run));
                this.thread.Name = "Event Pipe Thread";
                this.thread.IsBackground = true;
                this.thread.Start();
            }
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

        public Event Dequeue() => null;

        private void Run()
        {
            while (true)
            {
                if (this.tree.IsEmpty())
                    Thread.Sleep(1);
                else
                    this.hubQueue.Enqueue(this.tree.Read());
            }
        }

        public Event Read()
        {
            throw new NotImplementedException();
        }
    }
}