// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace SmartQuant
{
    public enum ClockMode
    {
        Realtime,
        Simulation
    }

    public enum ClockResolution
    {
        Normal,
        High
    }

    public enum ClockType
    {
        Local,
        Exchange
    }

    class ReminderEventQueue : SortedEventQueue
    {
        public ReminderEventQueue() : base(EventQueueId.Reminder, EventQueueType.Master, EventQueuePriority.Normal)
        {
        }

        //TODO: the logic is twisted! make var events private!
        public void Remove(ReminderCallback callback, DateTime dateTime)
        {
            lock (this)
            {
                for (int i = 0; i < Count; i++)
                {
                    var reminder = (Reminder)this.events[i];
                    if (reminder.Callback == callback && reminder.DateTime == dateTime)
                    {
                        this.events.RemoveAt(i);
                        if (i == 0 && this.events.Count != 0)
                        {
                            this.dateTime = this.events[0].DateTime;
                        }
                        break;
                    }
                }
            }
        }
    }

    public class Clock
    {
        private DateTime dateTime;
        private ClockMode mode;

        private Framework framework;
        private bool isStandalone;
        private long initTicks;
        private Stopwatch stopwatch;
        private Thread thread;

        internal IEventQueue ReminderEventQueue { get; } = new ReminderEventQueue();

        public ClockResolution Resolution { get; set; }

        public ClockMode Mode
        {
            get { return this.mode; }
            set
            {
                if (this.mode != value)
                {
                    this.mode = value;
                    this.dateTime = this.mode == ClockMode.Simulation ? DateTime.MinValue : this.dateTime;
                }
            }
        }

        internal ClockType Type { get; set; }

        public DateTime DateTime
        {
            get
            {
                if (Type == ClockType.Exchange)
                    return this.dateTime;

                if (this.mode == ClockMode.Simulation)
                    return this.dateTime;

                if (Resolution == ClockResolution.Normal)
                    return DateTime.Now;

                return new DateTime(this.initTicks + (long)((double)this.stopwatch.ElapsedTicks / Stopwatch.Frequency * TimeSpan.TicksPerSecond));
            }
            internal set
            {
                if (Type == ClockType.Exchange && value != this.dateTime)
                {
                    if (value < this.dateTime)
                    {
                        Console.WriteLine("Clock::DateTime (Exchange) incorrect set order");
                        return;
                    }
                    this.dateTime = value;
                }
                else
                {
                    if (this.mode != ClockMode.Simulation)
                    {
                        Console.WriteLine("Clock::DateTime Can not set dateTime because Clock is not in the Simulation mode");
                        return;
                    }
                    if (value != this.dateTime)
                    {
                        if (value < this.dateTime)
                        {
                            Console.WriteLine("Clock::DateTime (Local) incorrect set order");
                            return;
                        }
                        if (this.isStandalone)
                        {
                            throw new NotImplementedException();
                            //while (!ReminderEventQueue.IsEmpty() && ReminderEventQueue.PeekDateTime() < value)
                            //{
                            //    var reminder = (Reminder)ReminderEventQueue.Read();
                            //    this.dateTime = reminder.DateTime;
                            //    reminder.Execute();
                            //}
                        }
                        this.dateTime = value;
                    }
                }
            }
        }

        public long Ticks
        {
            get
            {
                if (this.mode == ClockMode.Simulation)
                    return DateTime.Ticks;

                if (Resolution == ClockResolution.Normal)
                    return DateTime.Now.Ticks;

                return this.initTicks + (long)((double)this.stopwatch.ElapsedTicks / Stopwatch.Frequency * TimeSpan.TicksPerSecond);
            }
        }

        public Clock(Framework framework, ClockType type = ClockType.Local, bool isStandalone = false)
        {
            this.framework = framework;
            Type = type;
            this.isStandalone = isStandalone;
            this.dateTime = DateTime.MinValue;
            this.mode = this.framework.Mode == FrameworkMode.Realtime ? ClockMode.Realtime : ClockMode.Simulation;
            this.initTicks = DateTime.Now.Ticks;
            this.stopwatch = Stopwatch.StartNew();
            if (this.isStandalone)
            {
                throw new NotImplementedException("don't know when to use it");
                //this.thread = new Thread(() => {
                //    Console.WriteLine($"{DateTime.Now} Clock thread started");
                //    bool imminent = false;
                //    while (true)
                //    {
                //        if (Mode == ClockMode.Realtime)
                //        {
                //            if (!ReminderEventQueue.IsEmpty())
                //            {
                //                long ticks1 = ReminderEventQueue.PeekDateTime().Ticks;
                //                long ticks2 = this.framework.Clock.Ticks;
                //                if (ticks1 <= ticks2)
                //                {
                //                    ((Reminder)ReminderEventQueue.Read()).Execute();
                //                }
                //                else if (ticks1 - ticks2 < 15000)
                //                {
                //                    imminent = true;
                //                }
                //            }
                //            if (imminent)
                //            {
                //                Thread.Sleep(0);
                //                //Thread.SpinWait(1);Thread.Sleep(1);
                //                //System.Threading.Tasks.Task.Delay(3000).Wait()
                //            }
                //            else
                //            {
                //                Thread.Sleep(1);
                //            }
                //        }
                //        else
                //        {
                //            Thread.Sleep(10);
                //        }
                //    }
                //});
                //this.thread.Name = "Clock Thread";
                //this.thread.IsBackground = true;
                //this.thread.Start();
            }
        }

        public bool AddReminder(ReminderCallback callback, DateTime dateTime, object data = null)
        {
            return AddReminder(new Reminder(callback, dateTime, data) {Clock = this});
        }

        public Reminder AddReminderWithHandler(ReminderCallback callback, DateTime dateTime, object data = null)
        {
            var reminder = new Reminder(callback, dateTime, data) {Clock = this};
            return AddReminder(reminder) ? reminder : null;
        }

        public bool AddReminder(Reminder reminder)
        {
            if (reminder.DateTime < this.dateTime)
            {
                Console.WriteLine($"Clock::AddReminder ({Type}) Can not set reminder to the past. Clock datetime = {DateTime.ToString("dd.MM.yyyy HH:mm:ss.ffff")} Reminder datetime = {reminder.DateTime.ToString("dd.MM.yyyy HH: mm:ss.ffff")} Reminder object = {reminder.Data}");
                return false;
            }
            reminder.Clock = this;
            ReminderEventQueue.Enqueue(reminder);
            return true;
        }

        public void RemoveReminder(ReminderCallback callback, DateTime dateTime)
        {
            ((ReminderEventQueue)ReminderEventQueue).Remove(callback, dateTime);
        }

        public void Clear()
        {
            this.dateTime = DateTime.MinValue;
        }

        public string GetModeAsString() => this.mode == ClockMode.Realtime ? "Realtime" : this.mode == ClockMode.Simulation ? "Simulation" : "Undefined";
    }
}