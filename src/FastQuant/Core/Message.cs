using System;

namespace FastQuant
{
    public class Message : Event
    {
        private static int counter;

        public override byte TypeId => EventType.Message;

        public int Id { get; }

        public int Type { get; set; }

        public int SenderId { get; set; } = -1;

        public int ReceiverId { get; set; } = -1;

        public ObjectTable Fields { get; } = new ObjectTable();

        public Message() : base(DateTime.Now)
        {
            Id = counter++;
        }

        public Message(int type)
        {
            Type = type;
        }

        public Message(int type, object data0) : this(type)
        {
            Fields[0] = data0;
        }

        public Message(int type, object data0, object data1) : this(type, data0)
        {
            Fields[1] = data1;
        }

        public Message(int type, object data0, object data1, object data2) : this(type, data0, data1)
        {
            Fields[2] = data2;
        }

        public Message(DateTime dateTime, int type, int id, int senderId, int receiverId) : base(dateTime)
        {
            Type = type;
            SenderId = senderId;
            ReceiverId = receiverId;
        }

        public object this[int index]
        {
            get
            {
                return Fields[index];
            }
            set
            {
                Fields[index] = value;
            }
        }

        #region Extra

        public Message(DateTime dateTime, int type, int id, int senderId, int receiverId, ObjectTable fields) : base(dateTime)
        {
            Type = type;
            Id = id;
            SenderId = senderId;
            ReceiverId = receiverId;
            Fields = fields;
        }

        #endregion
    }
}