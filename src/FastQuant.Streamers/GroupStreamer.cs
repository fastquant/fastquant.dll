// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace SmartQuant
{
    public class GroupStreamer : ObjectStreamer
    {
        public GroupStreamer()
        {
            this.typeId = DataObjectType.Group;
            this.type = typeof(Group);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            string gname = reader.ReadString();
            reader.ReadInt32();
            var group = new Group(gname);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string name = reader.ReadString();
                byte type = reader.ReadByte();
                object obj = this.streamerManager.Deserialize(reader);
                group.Add(name, type, obj);
            }
            return group;
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            byte version = 0;
            var group = obj as Group;
            writer.Write(version);
            writer.Write(group.Name);
            writer.Write(group.Id);
            writer.Write(group.Fields.Count);
            foreach (var field in group.Fields.Values)
            {
                writer.Write(field.Name);
                writer.Write(field.Type);
                this.streamerManager.Serialize(writer, field.Value);
            }
        }
    }
}
