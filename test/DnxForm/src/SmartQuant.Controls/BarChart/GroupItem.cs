using System;
using System.Collections.Generic;
using SmartQuant.Charting;
using System.Drawing;
using SmartQuant.FinChart;

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

    public class GroupItem2
    {
        public string Name { get; private set; }

        public int PadNumber { get; private set; }

        public string Format { get; private set; }

        public string SelectorKey { get; private set; }

        public bool IsColor { get; private set; }

        public Color Color { get; private set; }

        public bool IsStyle { get; private set; }

        public SimpleDSStyle Style { get; private set; }

        public Dictionary<int, object> Table { get; private set; }

        public GroupItem2(Group group)
        {
            Name = group.Name;
            PadNumber = (int)group.Fields["Pad"].Value;
            Format = group.Fields.ContainsKey("Format") ? (string)group.Fields["Format"].Value : "F2";
            SelectorKey = (string)group.Fields["SelectorKey"].Value;
            if (group.Fields.ContainsKey("Color"))
            {
                IsColor = true;
                Color = (Color)group.Fields["Color"].Value;
            }
            if (group.Fields.ContainsKey("Style"))
            {
                IsStyle = true;
                if (group.Fields["Style"].Value.ToString() == "Line")
                    Style = SimpleDSStyle.Line;
                else if (group.Fields["Style"].Value.ToString() == "Bar")
                    Style = SimpleDSStyle.Bar;
                else if (group.Fields["Style"].Value.ToString() == "Circle")
                    Style = SimpleDSStyle.Circle;
            }
            Table = new Dictionary<int, object>();
        }
    }
}

