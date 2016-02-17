using System;
using System.IO;

namespace FastQuant
{
    public class MessageStreamer : ObjectStreamer
    {
        public MessageStreamer()
        {
            this.typeId = DataObjectType.Message;
            this.type = typeof(Message);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            var type = reader.ReadInt32();
            var id = reader.ReadInt32();
            var senderId = reader.ReadInt32();
            var receiverId = reader.ReadInt32();
            var fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            return new Message(dateTime, type, id, senderId, receiverId, fields);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var message = (Message)obj;
            writer.Write(message.DateTime.Ticks);
            writer.Write(message.Type);
            writer.Write(message.Id);
            writer.Write(message.SenderId);
            writer.Write(message.ReceiverId);
            this.streamerManager.Serialize(writer, message.Fields);
        }
    }

    public class CommandStreamer : ObjectStreamer
    {
        public CommandStreamer()
        {
            this.typeId = DataObjectType.Command;
            this.type = typeof(Command);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            var type = reader.ReadInt32();
            var id = reader.ReadInt32();
            var senderId = reader.ReadInt32();
            var receiverId = reader.ReadInt32();
            var fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            return new Command(dateTime, type, id, senderId, receiverId, fields);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var command = (Command)obj;
            writer.Write(command.DateTime.Ticks);
            writer.Write(command.Type);
            writer.Write(command.Id);
            writer.Write(command.SenderId);
            writer.Write(command.ReceiverId);
            this.streamerManager.Serialize(writer, command.Fields);
        }
    }

    public class ResponseStreamer : ObjectStreamer
    {
        public ResponseStreamer()
        {
            this.typeId = DataObjectType.Response;
            this.type = typeof(Response);
        }

        public override object Read(BinaryReader reader, byte version)
        {
            var dateTime = new DateTime(reader.ReadInt64());
            var type = reader.ReadInt32();
            var id = reader.ReadInt32();
            var commandId = reader.ReadInt32();
            var senderId = reader.ReadInt32();
            var receiverId = reader.ReadInt32();
            var fields = (ObjectTable)this.streamerManager.Deserialize(reader);
            return new Response(dateTime, type, id, commandId, senderId, receiverId, fields);
        }

        public override void Write(BinaryWriter writer, object obj)
        {
            var r = (Response)obj;
            writer.Write(r.DateTime.Ticks);
            writer.Write(r.Type);
            writer.Write(r.Id);
            writer.Write(r.CommandId);
            writer.Write(r.SenderId);
            writer.Write(r.ReceiverId);
            this.streamerManager.Serialize(writer, r.Fields);
        }
    }
}
