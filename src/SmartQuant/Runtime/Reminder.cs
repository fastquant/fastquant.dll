// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public enum ReminderOrder
    {
        Before,
        After
    }

    public delegate void ReminderCallback(DateTime dateTime, object data);

    public class Reminder : DataObject
    {

        public override byte TypeId => DataObjectType.Reminder;

        public ReminderCallback Callback { get; }

        public Clock Clock { get; internal set; }

        public object Data { set; get; }

        public Reminder(ReminderCallback callback, DateTime dateTime, object data)
            : base(dateTime)
        {
            Callback = callback;
            Data = data;
        }

        internal void Execute()
        {
            Callback(DateTime, Data);
        }

        public override string ToString() => $"{nameof(Reminder)} {DateTime}";
    }
}
