// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class DataObjectType
    {
        public const byte DataObject = 0;
        public const byte Tick = 1;
        public const byte Bid = 2;
        public const byte Ask = 3;
        public const byte Trade = 4;
        public const byte Quote = 5;
        public const byte Bar = 6;
        public const byte Level2 = 7;
        public const byte Level2Snapshot = 8;
        public const byte Level2Update = 9;
        public const byte Fill = 10;
        public const byte TimeSeriesItem = 11;
        public const byte Order = 12;
        public const byte ExecutionReport = 13;
        public const byte ExecutionCommand = 14;
        public const byte Reminder = 15;
        public const byte StrategyEvent = 16;
        public const byte Text = 17;
        public const byte DataSeries = 18;
        public const byte FieldList = 19;
        public const byte StrategyStatus = 20;
        public const byte ProviderError = 21;
        public const byte Fundamental = 22;
        public const byte News = 23;
        public const byte ObjectTable = 24;
        public const byte Position = 25;
        public const byte Portfolio = 26;
        public const byte Output = 27;
        public const byte Group = 50;
        public const byte GroupUpdate = 51;
        public const byte GroupEvent = 52;
        public const byte Message = 53;
        public const byte Command = 54;
        public const byte Response = 55;
        public const byte ResponseEvent = 60;
        public const byte OnFrameworkCleared = 99;
        public const byte OnPositionOpened = 110;
        public const byte OnPositionClosed = 111;
        public const byte OnPositionChanged = 112;
        public const byte OnFill = 113;
        public const byte OnTransaction = 114;
        public const byte OnExecutionCommand = 115;
        public const byte OnExecutionReport = 116;
        public const byte OnSendOrder = 117;
        public const byte OnPendingNewOrder = 118;
        public const byte OnNewOrder = 119;
        public const byte OnOrderStatusChanged = 120;
        public const byte OnOrderPartiallyFilled = 121;
        public const byte OnOrderFilled = 122;
        public const byte OnOrderReplaced = 123;
        public const byte OnOrderCancelled = 124;
        public const byte OnOrderRejected = 125;
        public const byte OnOrderExpired = 126;
        public const byte OnOrderCancelRejected = 127;
        public const byte OnOrderReplaceRejected = 128;
        public const byte OnOrderDone = 129;
        public const byte OnOrderManagerCleared = 130;
        public const byte OnInstrumentDefinition = 131;
        public const byte OnInstrumentDefintionEnd = 132;
        public const byte OnHistoricalData = 133;
        public const byte OnHistoricalDataEnd = 134;
        public const byte OnPortfolioAdded = 135;
        public const byte OnPortfolioRemoved = 136;
        public const byte OnPortfolioParentChanged = 137;
        public const byte HistoricalData = 138;
        public const byte HistoricalDataEnd = 139;
        public const byte BarSlice = 140;
        public const byte OnStrategyEvent = 141;
        public const byte AccountData = 142;
        public const byte AccountTransaction = 143;
        public const byte OnStrategyAdded = 144;
        public const byte OnPropertyChanged = 145;
        public const byte User = 146;
        public const byte String = 150;
        public const byte Int64 = 151;
        public const byte Int32 = 152;
        public const byte Int = 152;
        public const byte DateTime = 153;
        public const byte Char = 154;
        public const byte Boolean = 155;
        public const byte Color = 156;
        public const byte Byte = 157;
        public const byte Double = 158;
        public const byte Int16 = 159;
        public const byte Array = 160;
        public const byte OnException = 161;
        public const byte AccountReport = 162;
        public const byte OnConnect = 201;
        public const byte OnDisconnect = 202;
        public const byte OnSubscribe = 203;
        public const byte OnUnsubscribe = 204;
        public const byte OnQueueOpened = 205;
        public const byte OnQueueClosed = 206;
        public const byte OnEventManagerStarted = 207;
        public const byte OnEventManagerStopped = 208;
        public const byte OnEventManagerPaused = 209;
        public const byte OnEventManagerResumed = 210;
        public const byte OnEventManagerStep = 211;
        public const byte OnUserCommand = 212;
        public const byte Parameter = 213;
        public const byte ParameterList = 214;
        public const byte OnLogin = 215;
        public const byte OnLogout = 216;
        public const byte OnLoggedIn = 217;
        public const byte OnLoggedOut = 218;
        public const byte OnLoginRejected = 219;
        public const byte OnHeartbeat = 220;
        public const byte OnInstrumentAdded = 221;
        public const byte OnInstrumentDeleted = 222;
        public const byte OnProviderAdded = 223;
        public const byte OnProviderRemoved = 224;
        public const byte OnProviderConnected = 225;
        public const byte OnProviderDisconnected = 226;
        public const byte OnProviderStatusChanged = 227;
        public const byte OnSimulatorStart = 228;
        public const byte OnSimulatorStop = 229;
        public const byte OnSimulatorProgress = 230;
        public const byte OpenQuantInfo = 231;
        public const byte QuantBaseInfo = 232;
        public const byte QuantRouterInfo = 233;
        public const byte QuantDataInfo = 234;

        // something that EventType does not have
        public const byte ClientStatus = 240;
        public const byte ClientStatusRequest = 241;
        public const byte ClientInfo = 242;
        public const byte StrategyLog = 243;
        public const byte StrategyLogList = 244;
        public const byte StrategyRunListRequest = 245;
        public const byte StrategyRunListReponse = 246;
        public const byte DownloadHistoricalBacktestRequest = 247;
        public const byte DownloadHistoricalBacktestResponse = 248;
        public const byte StartOfBacktest = 249;
        public const byte EndOfBacktest = 250;
        public const byte SolutionStatus = 251;
        public const byte Attribute = 252;
        public const byte TimeSpan = 253;
        public const byte SeriesReset = 254;
    }

    public class ObjectType
    {
        public const byte DataSeries = 101;
        public const byte ObjectKeyList = 102;
        public const byte FreeKeyList = 103;
        public const byte ObjectKeyIdArray = 104;
        public const byte DataKeyIdArray = 105;
        public const byte Instrument = 106;
        public const byte AltId = 107;
        public const byte Leg = 108;
    }

    public class DataObject : Event
    {
        public override byte TypeId => DataObjectType.DataObject;

        public DataObject()
        {
        }

        public DataObject(DateTime dateTime) : base(dateTime)
        {
        }

        public DataObject(DataObject obj) : base(obj.DateTime)
        {
        }
    }
}