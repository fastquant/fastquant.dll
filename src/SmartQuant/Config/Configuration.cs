// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public override string ToString() => $"{TypeName} {(X64 ? 1 : 0)}";
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


#if SMARTQUANT_COMPAT
    [XmlRoot("Configuration")]
    public class Configuration
    {
        public const string FILENAME_DATA = "data.quant";
        public const string FILENAME_INSTRUMENTS = "instruments.quant";
        public const string FILENAME_ORDERS = "orders.quant";
        public const string FILENAME_PORTFOLIOS = "portfolios.quant";

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
        [XmlElement("CheckUpdates")]
        public bool CheckUpdates;
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

        public string ServerFactoryType { get; set; }
        public string DefaultDataSimulator { get; set; }
        public string DefaultExecutionSimulator { get; set; }

        public void AddDefaultStreamers()
        {
            var types = new string[] {};

            foreach (var name in types)
            {
                Type t = Type.GetType(name);
                if (t != null)
                    Streamers.Add(new StreamerPlugin(t.FullName));
            }
        }

        public void AddDefaultProviders()
        {
            //var types = new Dictionary<string, bool>();
            //foreach (var pair in types)
            //{
            //    Type t = Type.GetType(pair.Key);
            //    if (t != null)
            //        Providers.Add(new ProviderPlugin(t.FullName, pair.Value));
            //}
        }

        public static Configuration DefaultConfiguaration()
        {
            var c = new Configuration();

            c.IsInstrumentFileLocal = true;
            c.InstrumentFileHost = "127.0.0.1";
            c.InstrumentFileName = Path.Combine(Installation.DataDir.FullName, "instruments.quant");
            c.IsDataFileLocal = true;
            c.DataFileHost = "127.0.0.1";
            c.DataFileName = Path.Combine(Installation.DataDir.FullName, "data.quant");

            c.IsOrderFileLocal = true;
            c.OrderFileHost = "127.0.0.1";
            c.OrderFilePort = 1000;
            c.OrderFileName = Path.Combine(Installation.DataDir.FullName, "orders.quant");

            c.DefaultCurrency = "USD";
            c.DefaultExchange = "SMART";
            c.DefaultDataProvider = "QuantRouter";
            c.DefaultExecutionProvider = "QuantRouter";
            c.ProviderManagerFileName = Path.Combine(Installation.ConfigDir.FullName, "providermanager.xml");

            c.Streamers = new List<StreamerPlugin>();
            c.AddDefaultStreamers();
            c.Providers = new List<ProviderPlugin>();
            c.AddDefaultProviders();

            c.ServerFactoryType = "DefaultServerFactory";
            return c;
        }
    }
#else
    public class Configuration
    {
        private static JObject configData;

        static Configuration()
        {
            var json = File.ReadAllText(Path.Combine(Installation.ConfigDir.FullName, "configuration.json"));
            configData = JObject.Parse(json);
        }

        public string DefaultDataProvider => Get("DefaultDataProvider");
        public string DefaultExecutionProvider => Get("DefaultExecutionProvider");
        public string DataFileName => Get("DataFileName");
        public string InstrumentFileName => Get("InstrumentFileName");
        public string OrderFileName => Get("OrderFileName");
        public string PortfolioFileName => Get("PortfolioFileName");

        private static string Get(string key) => (string)configData[key];

        public static Configuration DefaultConfiguaration()
        {
            var c = new Configuration();
            return c;
        }
    }
#endif
}
