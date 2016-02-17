using System;
using System.Collections.Generic;

namespace FastQuant
{
    public interface IGroupListener
    {
        bool OnNewGroup(Group group);

        void OnNewGroupEvent(GroupEvent groupEvent);

        void OnNewGroupUpdate(GroupUpdate groupUpdate);

        PermanentQueue<Event> Queue { get; }
    }

    public class GroupEventEventAgrs : EventArgs
    {
        public GroupEvent GroupEvent { get; }

        public GroupEventEventAgrs(GroupEvent groupEvent)
        {
            GroupEvent = groupEvent;
        }
    }

    public class GroupEventAgrs : EventArgs
    {
        public Group Group { get; }

        public GroupEventAgrs(Group group)
        {
            Group = group;
        }
    }

    public class GroupUpdateEventAgrs : EventArgs
    {
        public GroupUpdate GroupUpdate { get; }

        public GroupUpdateEventAgrs(GroupUpdate groupUpdate)
        {
            GroupUpdate = groupUpdate;
        }
    }

    public delegate void GroupEventHandler(object sender, GroupEventAgrs args);

    public delegate void GroupEventEventHandler(object sender, GroupEventEventAgrs args);

    public delegate void GroupUpdateEventHandler(object sender, GroupUpdateEventAgrs args);

    public class GroupDispatcher
    {
        private Framework framework;

        private readonly Dictionary<IGroupListener, List<int>> groupIdsByListener = new Dictionary<IGroupListener, List<int>>();

        private readonly IdArray<List<IGroupListener>> listenersByGroupId = new IdArray<List<IGroupListener>>();

        private readonly List<IGroupListener> listeners = new List<IGroupListener>();

        public GroupDispatcher(Framework framework)
        {
            this.framework = framework;
            this.framework.EventManager.Dispatcher.NewGroup += OnNewGroup;
            this.framework.EventManager.Dispatcher.NewGroupEvent += OnNewGroupEvent;
            this.framework.EventManager.Dispatcher.NewGroupUpdate += OnNewGroupUpdate;
            this.framework.EventManager.Dispatcher.FrameworkCleared += OnFrameworkCleared;
        }

        public void AddListener(IGroupListener listener)
        {
            lock (this)
            {
                this.listeners.Add(listener);
                this.groupIdsByListener[listener] = new List<int>();
                foreach(var group in this.framework.GroupManager.GroupList)
                    ProcessGroup(listener, group);
            }
        }

        public void RemoveListener(IGroupListener listener)
        {
            lock (this)
            {
                this.listeners.Remove(listener);
                foreach (var id in this.groupIdsByListener[listener])
                    this.listenersByGroupId[id].Remove(listener);
                this.groupIdsByListener.Remove(listener);
            }
        }

        private void OnFrameworkCleared(object sender, FrameworkEventArgs e)
        {
            lock (this)
            {
                foreach (var listener in this.listeners)
                    listener.Queue.Enqueue(new OnFrameworkCleared(e.Framework));
                this.listenersByGroupId.Clear();
                foreach (var list in this.groupIdsByListener.Values)
                    list.Clear();
            }
        }

        private void OnNewGroupUpdate(object sender, GroupUpdateEventAgrs args)
        {
            lock (this)
            {
                if (args.GroupUpdate.GroupId != -1)
                {
                    var list = this.listenersByGroupId[args.GroupUpdate.GroupId];
                    list?.ForEach(l => l.OnNewGroupUpdate(args.GroupUpdate));
                }
            }
        }

        private void OnNewGroupEvent(object sender, GroupEventEventAgrs args)
        {
            lock (this)
            {
                if (args.GroupEvent.Group != null)
                {
                    var id = args.GroupEvent.Group.Id;
                    var group = this.framework.GroupManager.Groups[id];
                    if (group != null)
                    {
                        group.OnNewGroupEvent(args.GroupEvent);
                        var list = this.listenersByGroupId[id];
                        list?.ForEach(l => l.Queue.Enqueue(args.GroupEvent));
                    }
                }
            }
        }

        private void OnNewGroup(object sender, GroupEventAgrs args)
        {
            foreach (var listener in this.listeners)
                ProcessGroup(listener, args.Group);
        }

        private void ProcessGroup(IGroupListener listener, Group group)
        {
            if (listener.OnNewGroup(group))
            {
                var list = this.listenersByGroupId[group.Id] = this.listenersByGroupId[group.Id] ?? new List<IGroupListener>();
                list.Add(listener);
                this.groupIdsByListener[listener].Add(group.Id);
                foreach (var e in group.Events)
                    listener.Queue.Enqueue(e);
            }
        }
    }
}