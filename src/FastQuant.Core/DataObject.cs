// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class DataObject : Event
    {
        public DataObject()
        {
        }

        public DataObject(DateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        public DataObject(DataObject obj)
        {
            this.dateTime = obj.dateTime;
        }
    }
}