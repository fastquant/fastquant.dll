// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartQuant
{
    public class StreamerManager
    {
        private readonly Dictionary<Type, ObjectStreamer> streamsByType = new Dictionary<Type, ObjectStreamer>();
        private readonly IdArray<ObjectStreamer> streamsById = new IdArray<ObjectStreamer>();

        public StreamerManager()
        {
            var names = new[] {
                "DataObjectStreamer", "FreeKeyListStreamer", "ObjectKeyListStreamer", "ObjectTableStreamer",
                "DataSeriesStreamer", "DataKeyIdArrayStreamer"
            }.Select(s => $"{nameof(SmartQuant)}.{s}");

            foreach (var n in names)
            {
                var t = Type.GetType(n);
                if (t == null)
                    throw new ArgumentNullException($"Can't found type: {n}");

                var streamer = (ObjectStreamer)Activator.CreateInstance(t);
                Add(streamer);
            }
        }

        public void Add(ObjectStreamer streamer)
        {
            var s = this.streamsById[streamer.typeId];
            if (s != null && s.type != streamer.type)
                throw new Exception($"StreamerManager::Add Streamer with the same typeId but different type is already registered in the StreamerManager {s.typeId} {s.type}");

            streamer.streamerManager = this;
            this.streamsById[streamer.typeId] = streamer;
            this.streamsByType[streamer.type] = streamer;
        }

        public void AddDefaultStreamers()
        {
            var streamers = new ObjectStreamer[]
            {
                new TickStreamer(),
                new TickStreamer(),
                new BidStreamer(),
                new AskStreamer(),
                new TradeStreamer(),
                new QuoteStreamer(),
                new BarStreamer(),
                new Level2Streamer(),
                new Level2SnapshotStreamer(),
                new Level2UpdateStreamer(),
                new FillStreamer(),
                new TimeSeriesItemStreamer(),
                new ExecutionReportStreamer(),
                new ExecutionCommandStreamer(),
                new TextInfoStreamer(),
                new FieldListStreamer(),
                new StrategyStatusStreamer(),
                new ProviderErrorStreamer(),
                new FundamentalStreamer(),
                new NewsStreamer(),
                new PositionStreamer(),
                new PortfolioStreamer(),
                new GroupStreamer(),
                new GroupUpdateStreamer(),
                new GroupEventStreamer(),
                new MessageStreamer(),
                new CommandStreamer(),
                new ResponseStreamer(),
                new InstrumentStreamer(),
                new AltIdStreamer(),
                new LegStreamer(),
                new AccountDataStreamer(),
                new AccountTransactionStreamer(),
                new UserStreamer(),
                new StringStreamer(),
                new Int64Streamer(),
                new Int32Streamer(),
                new DateTimeStreamer(),
                new CharStreamer(),
                new BooleanStreamer(),
                new ColorStreamer(),
                new ByteStreamer(),
                new DoubleStreamer(),
                new Int16Streamer(),
                new ArrayStreamer(),
                new AccountReportStreamer(),
                new OnSubscribeStreamer(),
                new OnUnsubscribeStreamer(),
                new ParameterStreamer(),
                new ParameterListStreamer(),
                new OnLoginStreamer(),
                new OnLogoutStreamer(),
                new OnLoggedInStreamer(),
                new OnLoggedOutStreamer(),
                new OnHeartbeatStreamer(),
                new OnProviderConnectedStreamer(),
                new OnProviderDisconnectedStreamer(),
                new AttributeStreamer(),
                new TimeSpanStreamer()
            };
            foreach (var s in streamers)
                Add(s);
        }

        public object Deserialize(BinaryReader reader)
        {
            var id = reader.ReadByte();
            var version = reader.ReadByte();
            var streamer = Get(id);
            return streamer.Read(reader, version);
        }

        public bool HasStreamer(object obj) => HasStreamer(obj.GetType());

        public bool HasStreamer(Type type) => this.streamsByType.ContainsKey(type);

        public bool HasStreamer(int typeId) => this.streamsById[typeId] != null;

        public void Remove(ObjectStreamer streamer)
        {
            this.streamsById.Remove(streamer.typeId);
            this.streamsByType.Remove(streamer.type);
        }

        public void Remove(byte typeId)
        {
            var streamer = this.streamsById[typeId];
            if (streamer != null)
                Remove(streamer);
        }

        public void Remove(Type type)
        {
            var streamer = this.streamsByType[type];
            if (streamer != null)
                Remove(streamer);
        }

        #region Extra Stuff

        public ObjectStreamer Get(Type type) => this.streamsByType[type];

        public void Get(Type type, out ObjectStreamer streamer) => this.streamsByType.TryGetValue(type, out streamer);

        public ObjectStreamer Get(byte typeId) => this.streamsById[typeId];

        #endregion

        public void Serialize(BinaryWriter writer, object obj)
        {
            var type = obj.GetType();
            var streamer = this.streamsByType[type];
            writer.Write(streamer.typeId);
            writer.Write(streamer.GetVersion(obj));
            streamer.Write(writer, obj);
        }

        public void Serialize(BinaryWriter writer, Event e)
        {
            var streamer = this.streamsById[e.TypeId];
            writer.Write(streamer.typeId);
            writer.Write(streamer.GetVersion(e));
            streamer.Write(writer, e);
        }

        public void Serialize(BinaryWriter writer, DataObject obj)
        {
            var streamer = this.streamsById[obj.TypeId];
            writer.Write(streamer.typeId);
            writer.Write(streamer.GetVersion(obj));
            streamer.Write(writer, obj);
        }
    }
}