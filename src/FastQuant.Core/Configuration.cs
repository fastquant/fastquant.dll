// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace SmartQuant
{
    public class ProviderPlugin
    {
        [XmlElement("TypeName")]
        public string TypeName { get; set; }

        [XmlElement("X64")]
        public bool X64 { get; set; }

        public ProviderPlugin()
        {
        }

        public ProviderPlugin(string typeName, bool x64 = false)
        {
            TypeName = typeName;
            X64 = x64;
        }

        public override string ToString() => $"{TypeName} {(X64 ? 1 : 0).ToString()}";
    }

    public class StreamerPlugin
    {
        [XmlElement("TypeName")]
        public string TypeName { get; set; }

        public StreamerPlugin()
        {
        }

        public StreamerPlugin(string typeName)
        {
            TypeName = typeName;
        }

        public override string ToString() => TypeName;
    }

    [XmlRoot("Configuration")]
    public class Configuration
    {
        [XmlElement("IsInstrumentFileLocal")]
        public bool IsInstrumentFileLocal;
        [XmlElement("InstrumentFileHost")]
        public string InstrumentFileHost;
        [XmlElement("InstrumentFilePort")]
        public int InstrumentFilePort;
        [XmlElement("InstrumentFileName")]
        public string InstrumentFileName;
        [XmlElement("IsDataFileLocal")]
        public bool IsDataFileLocal;
        [XmlElement("DataFileHost")]
        public string DataFileHost;
        [XmlElement("DataFilePort")]
        public int DataFilePort;
        [XmlElement("DataFileName")]
        public string DataFileName;
        [XmlElement("IsOrderFileLocal")]
        public bool IsOrderFileLocal;
        [XmlElement("OrderFileHost")]
        public string OrderFileHost;
        [XmlElement("OrderFilePort")]
        public int OrderFilePort;
        [XmlElement("OrderFileName")]
        public string OrderFileName;
        [XmlElement("DefaultCurrency")]
        public string DefaultCurrency;
        [XmlElement("DefaultExchange")]
        public string DefaultExchange;
        [XmlElement("DefaultDataProvider")]
        public string DefaultDataProvider;
        [XmlElement("DefaultExecutionProvider")]
        public string DefaultExecutionProvider;
        [XmlElement("ProviderManagerFileName")]
        public string ProviderManagerFileName;
        [XmlArrayItem("Streamer")]
        [XmlArray("Streamers")]
        public List<StreamerPlugin> Streamers;
        [XmlArray("Providers")]
        [XmlArrayItem("Provider")]
        public List<ProviderPlugin> Providers;
        [XmlElement("QuantControllerHost")]
        public string QuantControllerHost;
        [XmlElement("QuantControllerPort")]
        public int QuantControllerPort;
        [XmlElement("QuantControllerUsername")]
        public string QuantControllerUsername;
        [XmlElement("QuantControllerPassword")]
        public string QuantControllerPassword;
        [XmlElement("QuantControllerUpdateStatusInterval")]
        public int QuantControllerUpdateStatusInterval;
        [XmlElement("QuantControllerAutoConnect")]
        public bool QuantControllerAutoConnect;

        public void AddDefaultStreamers()
        {
            var types = new string[]
            {
                "SmartQuant.DataObjectStreamer",
                "SmartQuant.InstrumentStreamer",
                "SmartQuant.AltIdStreamer",
                "SmartQuant.LegStreamer", 
                "SmartQuant.BidStreamer",
                "SmartQuant.AskStreamer", 
                "SmartQuant.QuoteStreamer", 
                "SmartQuant.TradeStreamer",
                "SmartQuant.BarStreamer", 
                "SmartQuant.Level2SnapshotStreamer",
                "SmartQuant.NewsStreamer",
                "SmartQuant.FundamentalStreamer",
                "SmartQuant.DataSeriesStreamer",
                "SmartQuant.ExecutionCommandStreamer",
                "SmartQuant.ExecutionReportStreamer",
                "SmartQuant.PositionStreamer",
                "SmartQuant.PortfolioStreamer",
                "SmartQuant.ObjectTableStreamer",
                "SmartQuant.DoubleStreamer",
                "SmartQuant.Int16Streamer",
                "SmartQuant.Int32Streamer",
                "SmartQuant.Int64Streamer",
                "SmartQuant.StringStreamer",
                "SmartQuant.TimeSeriesItemStreamer"
            };

            foreach (var name in types)
            {
                Type t = Type.GetType(name);
                if (t != null)
                    Streamers.Add(new StreamerPlugin(t.FullName));
            }
        }

        public void AddDefaultProviders()
        {
            var types = new Dictionary<string, bool>();
            types.Add("SmartQuant.QR.QuantRouter", true);
            types.Add("SmartQuant.QR2014.QuantRouter2014", true);

            types.Add("SmartQuant.QB.QuantBase", true);
            types.Add("SmartQuant.IB.IBTWS", true);
            types.Add("SmartQuant.TT.TTFIX", true);
            types.Add("SmartQuant.OEC.OpenECry", true);
            types.Add("SmartQuant.IQ.IQFeed", true);
            types.Add("SmartQuant.MNI.MNIProvider", true);
            types.Add("SmartQuant.CX.Currenex", true);
            types.Add("SmartQuant.HS.Hotspot", true);

            foreach (var pair in types)
            {
                Type t = Type.GetType(pair.Key);
                if (t != null)
                    Providers.Add(new ProviderPlugin(t.FullName, pair.Value));
            }
        }

        public static Configuration DefaultConfiguaration()
        {
            Configuration configuration = new Configuration();

            configuration.IsInstrumentFileLocal = true;
            configuration.InstrumentFileHost = "127.0.0.1";
            configuration.InstrumentFileName = Path.Combine(Installation.DataDir.FullName, "instruments.quant");
            configuration.IsDataFileLocal = true;
            configuration.DataFileHost = "127.0.0.1";
            configuration.DataFileName = Path.Combine(Installation.DataDir.FullName, "data.quant");

            configuration.IsOrderFileLocal = true;
            configuration.OrderFileHost = "127.0.0.1";
            configuration.OrderFilePort = 1000;
            configuration.OrderFileName = Path.Combine(Installation.DataDir.FullName, "orders.quant");

            configuration.DefaultCurrency = "USD";
            configuration.DefaultExchange = "SMART";
            configuration.DefaultDataProvider = "QuantRouter";
            configuration.DefaultExecutionProvider = "QuantRouter";
            configuration.ProviderManagerFileName = Path.Combine(Installation.ConfigDir.FullName, "providermanager.xml");

            configuration.Streamers = new List<StreamerPlugin>();
            configuration.AddDefaultStreamers();
            configuration.Providers = new List<ProviderPlugin>();
            configuration.AddDefaultProviders();

            return configuration;
        }
    }
}
