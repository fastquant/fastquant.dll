// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class OnSimulatorStart : DataObject
    {
        public override byte TypeId => DataObjectType.OnSimulatorStart;

        internal DateTime DateTime1 { get; }
        internal DateTime DateTime2 { get; }
        internal long Count { get; }

        public OnSimulatorStart(DateTime dateTime1, DateTime dateTime2, long count = 0)
        {
            DateTime = dateTime1;
            DateTime1 = dateTime1;
            DateTime2 = dateTime2;
            Count = count;
        }

        public override string ToString() => nameof(OnSimulatorStart);
    }

    public class OnSimulatorStop : DataObject
    {
        public override byte TypeId => DataObjectType.OnSimulatorStop;

        public OnSimulatorStop()
        {
            this.dateTime = DateTime.MinValue;
        }

        public override string ToString() => nameof(OnSimulatorStop);
    }

    public class OnSimulatorProgress : DataObject
    {
        internal long Count { get; }
        internal long Percent { get; }

        public OnSimulatorProgress()
        {
            this.dateTime = DateTime.MinValue;
        }

        public OnSimulatorProgress(long count, int percent) : this()
        {
            Count = count;
            Percent = percent;
        }

        public override string ToString() => nameof(OnSimulatorProgress);

        public override byte TypeId => DataObjectType.OnSimulatorProgress;
    }
}