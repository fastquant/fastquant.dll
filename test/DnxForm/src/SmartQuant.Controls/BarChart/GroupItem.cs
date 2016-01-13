// Licensed under the Apache License, Version 2.0. 
// Copyright (c) Alex Lee. All rights reserved.

using System;
using System.Collections.Generic;
using SmartQuant.Charting;

namespace SmartQuant.Controls
{
    class GroupItem
    {
        public Dictionary<int, Tuple<Viewer, object>> Table { get; private set; }

        public int PadNumber { get; set; }

        public string Format { get; set; }

        public GroupItem(Group group)
        {
            Table = new Dictionary<int, Tuple<Viewer, object>>();
            PadNumber = (int)group.Fields["Pad"].Value;
            Format = group.Fields.ContainsKey("Format") ? (string)group.Fields["Format"].Value : "F2";
        }
    }
}

