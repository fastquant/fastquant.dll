using System;
using System.IO;

namespace SmartQuant
{
    public class CommandType
    {
        public const int GetInformation = 0;
        public const int GetInstruments = 1;
        public const int AddInstrument = 2;
        public const int DeleteInstrument = 3;
        public const int GetProviders = 4;
        public const int ConnectProvider = 5;
        public const int DisconnectProvider = 6;
        public const int StartStrategy = 7;
        public const int StopStrategy = 8;
        public const int StartScenario = 9;
        public const int StopScenario = 10;
        public const int ReadText = 11;
        public const int WriteText = 12;
        public const int AddStrategyInstrument = 13;
        public const int RemoveStrategyInstrument = 14;
        public const int AddStrategyInstrumentBySymbol = 15;
        public const int RemoveStrategyInstrumentBySymbol = 16;
        public const int GetStrategyManagerStatus = 17;
        //public const int GetStrategyManagerStatus_ = 18;
        public const int SetProperties = 19;
        public const int AddAlgorithm = 20;
        public const int DeleteAlgorithm = 21;
        public const int GetPortfolio = 22;
        public const int GetPortfolioList = 23;
        public const int CancelOrder = 24;
        public const int ReplaceOrder = 25;
        public const int AdjustAccountMoney = 26;
        public const int GetStrategyParameters = 27;
        public const int SendStrategyCommand = 28;
        public const int GetUserList = 29;
        public const int GetUser = 30;
        public const int AddUser = 31;
        public const int DeleteUser = 32;
        public const int UpdateUser = 33;
        public const int SubscribeData = 34;
        public const int UnsubscribeData = 35;
        public const int ChangeProperty = 36;
        public const int SendOrder = 37;
        public const int ClosePosition = 38;
    }

    public partial class Message : Event
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

    public class ResponseType
    {
        public const int Confirmation = 0;

        public const int Failure = 1;

        public const int Information = 2;

        public const int Instruments = 3;

        public const int Providers = 4;

        public const int Text = 5;

        public const int StrategyManagerStatus = 6;

        //public const int StrategyManagerStatus_ = 7;

        public const int Algorithms = 8;

        public const int Portfolio = 9;

        public const int PortfolioList = 10;

        public const int UserList = 11;

        public const int User = 12;
    }

    public partial class Response : Message
    {
        public override byte TypeId => EventType.Response;

        public int CommandId { get; }

        public Response()
        {
        }

        public Response(Command command)
        {
            CommandId = command.Id;
            ReceiverId = command.ReceiverId;
        }

        public Response(DateTime dateTime, int type, int id, int commandId, int senderId, int receiverId) : base(dateTime, type, id, senderId, receiverId)
        {
            CommandId = commandId;
        }

        #region Extra
        public Response(DateTime dateTime, int type, int id, int commandId, int senderId, int receiverId, ObjectTable fields) : base(dateTime, type, id, senderId, receiverId, fields)
        {
            CommandId = commandId;
        }
        #endregion
    }

    public partial class Command : Message
    {
        public override byte TypeId => EventType.Command;

        public Command()
        {
        }

        public Command(int type) : base(type)
        {
        }

        public Command(int type, object data0) : base(type, data0)
        {
        }


        public Command(int type, object data0, object data1) : base(type, data0, data1)
        {
        }

        public Command(int type, object data0, object data1, object data2) : base(type, data0, data1, data2)
        {
        }

        public Command(DateTime dateTime, int type, int id, int senderId, int receiverId) : base(dateTime, type, id, senderId, receiverId)
        {
        }

        #region Extra
        public Command(DateTime dateTime, int type, int id, int senderId, int receiverId, ObjectTable fields) : base(dateTime, type, id, senderId, receiverId, fields)
        {
        }
        #endregion
    }

    public delegate void CommandEventHandler(object sender, Command command);
}