// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace SmartQuant
{
    public class GroupEvent : DataObject
    {
        public Group Group { get; set; }

        public Event Obj { get; private set; }

        public int GroupId { get; private set; }

        public override byte TypeId => DataObjectType.GroupEvent;

        public GroupEvent(Event obj, Group group)
        {
            Obj = obj;
            Group = group;
            GroupId = group != null ? group.Id : -1;
        }

        internal GroupEvent(Event obj, int groupId)
        {
            Obj = obj;
            GroupId = groupId;
        }
    }

    public class Group : DataObject
    {
        public override byte TypeId => DataObjectType.Group;

        public int Id { get; internal set; }

        public string Name { get; private set; }

        public Framework Framework { get; internal set; }

        public Dictionary<string, GroupField> Fields { get; private set; }

        public List<GroupEvent> Events { get; private set; }

        public Group(string name)
        {
            Name = name;
            Fields = new Dictionary<string, GroupField>();
            Events = new List<GroupEvent>();
        }

        public void Add(string name, byte type, object value)
        {
            Add(new GroupField(name, type, value));
        }

        public void Add(string name, Color color)
        {
            Add(new GroupField(name, DataObjectType.Color, color));
        }

        public void Add(string name, string value)
        {
            Add(new GroupField(name, DataObjectType.String, value));
        }

        public void Add(string name, int value)
        {
            Add(new GroupField(name, DataObjectType.Int, value));
        }

        public void Add(string name, bool value)
        {
            Add(new GroupField(name, DataObjectType.Boolean, value));
        }

        public void Add(string name, DateTime dateTime)
        {
            Add(new GroupField(name, DataObjectType.DateTime, dateTime));
        }

        public void Remove(string fieldName)
        {
            Fields.Remove(fieldName);
        }

        public void OnNewGroupEvent(GroupEvent groupEvent)
        {
            Events.Add(groupEvent);
        }

        private void Add(GroupField groupField)
        {
            Fields[groupField.Name] = groupField;
            groupField.Group = this;
        }
    }

    public class GroupField
    {
        internal Group Group { get; set; }

        private object value;

        public string Name { get; private set; }

        public byte Type { get; private set; }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.value == value)
                    return;
                object oldValue = this.value;
                this.value = value;
                //Group.Framework.EventServer.OnLog(new GroupUpdate(Group.Id, Name, Type, this.value, oldValue, GroupUpdateType.FieldUpdated));
            }
        }

        public GroupField(string name, byte type, object value)
        {
            Name = name;
            Type = type;
            this.value = value;
        }
    }

    public enum GroupUpdateType
    {
        FieldAdded,
        FieldRemoved,
        FieldUpdated
    }

    public class GroupUpdate : DataObject
    {
        public override byte TypeId => DataObjectType.GroupUpdate;

        public int GroupId { get; private set; }

        public string FieldName { get; private set; }

        public GroupUpdateType UpdateType { get; private set; }

        public byte FieldType { get; private set; }

        public object Value { get; private set; }

        public object OldValue { get; private set; }

        public GroupUpdate(int groupId, string fieldName, byte fieldType, object value, object oldValue, GroupUpdateType updateType)
        {
            GroupId = groupId;
            FieldName = fieldName;
            FieldType = fieldType;
            Value = value;
            OldValue = oldValue;
            UpdateType = updateType;
        }
    }

    public class GroupManager
    {
        private Framework framework;
        private int counter;

        public List<Group> GroupList { get; } = new List<Group>();

        public IdArray<Group> Groups { get; } = new IdArray<Group>(1024);

        public GroupManager(Framework framework)
        {
            this.framework = framework;
        }

        public void Clear()
        {
            Groups.Clear();
            GroupList.Clear();
            this.counter = 0;
        }

        public void Add(Group group)
        {
            group.Id = Interlocked.Increment(ref this.counter);
            Groups.Add(group.Id, group);
            GroupList.Add(group);
            group.Framework = this.framework;
            this.framework.EventServer.OnLog(group);
        }
    }
}