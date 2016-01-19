using System;
using System.Collections.Generic;

namespace SmartQuant
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

        private Dictionary<IGroupListener, List<int>> dictionary_0= new Dictionary<IGroupListener, List<int>>();

        private IdArray<List<IGroupListener>> idArray_0 = new IdArray<List<IGroupListener>>();

        private List<IGroupListener> list_0 = new List<IGroupListener>();

        public GroupDispatcher(Framework framework)
        {
            this.framework = framework;
            this.framework.EventManager.Dispatcher.NewGroup += new GroupEventHandler(this.method_3);
            this.framework.EventManager.Dispatcher.NewGroupEvent += new GroupEventEventHandler(this.method_2);
            this.framework.EventManager.Dispatcher.NewGroupUpdate += new GroupUpdateEventHandler(this.method_1);
            this.framework.EventManager.Dispatcher.FrameworkCleared += new FrameworkEventHandler(this.method_0);
        }

        public void AddListener(IGroupListener listener)
        {
            lock (this)
            {
                this.list_0.Add(listener);
                this.dictionary_0[listener] = new List<int>();
                for (int i = 0; i < this.framework.GroupManager.GroupList.Count; i++)
                {
                    this.method_4(listener, this.framework.GroupManager.GroupList[i]);
                }
            }
        }

        private void method_0(object sender, FrameworkEventArgs e)
        {
            lock (this)
            {
                foreach (IGroupListener current in this.list_0)
                {
                    current.Queue.Enqueue(new OnFrameworkCleared(e.Framework));
                }
                this.idArray_0.Clear();
                foreach (List<int> current2 in this.dictionary_0.Values)
                {
                    current2.Clear();
                }
            }
        }

        private void method_1(object object_0, GroupUpdateEventAgrs groupUpdateEventAgrs_0)
        {
            lock (this)
            {
                if (groupUpdateEventAgrs_0.GroupUpdate.GroupId != -1)
                {
                    List<IGroupListener> list = this.idArray_0[groupUpdateEventAgrs_0.GroupUpdate.GroupId];
                    if (list != null)
                    {
                        foreach (IGroupListener current in list)
                        {
                            current.OnNewGroupUpdate(groupUpdateEventAgrs_0.GroupUpdate);
                        }
                    }
                }
            }
        }

        private void method_2(object object_0, GroupEventEventAgrs groupEventEventAgrs_0)
        {
            lock (this)
            {
                if (groupEventEventAgrs_0.GroupEvent.Group != null)
                {
                    Group group = this.framework.GroupManager.Groups[groupEventEventAgrs_0.GroupEvent.Group.Id];
                    if (group != null)
                    {
                        group.OnNewGroupEvent(groupEventEventAgrs_0.GroupEvent);
                        List<IGroupListener> list = this.idArray_0[groupEventEventAgrs_0.GroupEvent.Group.Id];
                        if (list != null)
                        {
                            foreach (IGroupListener current in list)
                            {
                                current.Queue.Enqueue(groupEventEventAgrs_0.GroupEvent);
                            }
                        }
                    }
                }
            }
        }

        private void method_3(object object_0, GroupEventAgrs groupEventAgrs_0)
        {
            foreach (IGroupListener current in this.list_0)
            {
                this.method_4(current, groupEventAgrs_0.Group);
            }
        }

        private void method_4(IGroupListener igroupListener_0, Group group_0)
        {
            if (igroupListener_0.OnNewGroup(group_0))
            {
                List<IGroupListener> list = this.idArray_0[group_0.Id];
                if (list == null)
                {
                    list = new List<IGroupListener>();
                    this.idArray_0[group_0.Id] = list;
                }
                this.dictionary_0[igroupListener_0].Add(group_0.Id);
                list.Add(igroupListener_0);
                foreach (GroupEvent current in group_0.Events)
                {
                    igroupListener_0.Queue.Enqueue(current);
                }
            }
        }

        public void RemoveListener(IGroupListener listener)
        {
            lock (this)
            {
                this.list_0.Remove(listener);
                foreach (int current in this.dictionary_0[listener])
                {
                    this.idArray_0[current].Remove(listener);
                }
                this.dictionary_0.Remove(listener);
            }
        }
    }
}