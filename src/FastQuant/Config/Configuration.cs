// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace FastQuant
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
}

namespace FastQuant
{
#if SMARTQUANT_COMPAT
    [XmlRoot("Configuration")]
    public partial class Configuration
    {
        public const string FILENAME_DATA = "data.quant";
        public const string FILENAME_INSTRUMENTS = "instruments.quant";
        public const string FILENAME_ORDERS = "orders.quant";
        public const string FILENAME_PORTFOLIOS = "portfolios.quant";

        [XmlElement("IsOutputLogEnabled")]
        public bool IsOutputLogEnabled;

        [XmlElement("OutputLogFileName")]
        public string OutputLogFileName;

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
        [XmlElement("PortfolioFileName")]
        public string PortfolioFileName;
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

        //public void AddDefaultStreamers()
        //{
        //    var types = new string[] {};
        //    foreach (var name in types)
        //    {
        //        Type t = Type.GetType(name);
        //        if (t != null)
        //            Streamers.Add(new StreamerPlugin(t.FullName));
        //    }
        //}

        //public void AddDefaultProviders()
        //{
        //    //var types = new Dictionary<string, bool>();
        //    //foreach (var pair in types)
        //    //{
        //    //    Type t = Type.GetType(pair.Key);
        //    //    if (t != null)
        //    //        Providers.Add(new ProviderPlugin(t.FullName, pair.Value));
        //    //}
        //}

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
    using Newtonsoft.Json.Linq;

    public partial class Configuration
    {
        private static JObject configData;

        static Configuration()
        {
            var json = File.ReadAllText(Path.Combine(Installation.ConfigDir.FullName, "configuration.json"));
            configData = JObject.Parse(json);
        }

        public string DefaultDataProvider => GetString("DefaultDataProvider");
        public string DefaultExecutionProvider => GetString("DefaultExecutionProvider");
        public string DataFileName => GetString("DataFileName");
        public string InstrumentFileName => GetString("InstrumentFileName");
        public string OrderFileName => GetString("OrderFileName");
        public string PortfolioFileName => GetString("PortfolioFileName");
        public bool IsOutputLogEnabled => GetBool("IsOutputLogEnabled");
        public string OutputLogFileName => GetString("OutputLogFileName");

        private static string GetString(string key) => (string)configData[key];
        private static bool GetBool(string key) => (bool)configData[key];
        private static int GetInteger(string key) => (int)configData[key];
        private static double GetDouble(string key) => (double)configData[key];

        public static Configuration DefaultConfiguaration()
        {
            return new Configuration();
        }

        public List<ProviderPlugin> Providers = new List<ProviderPlugin>();

        public List<StreamerPlugin> Streamers = new List<StreamerPlugin>();
    }
#endif
    public partial class Configuration
    {
        public void SetDefaultDataConfiguration()
        {
            //this.IsDataFileLocal = false;
            //this.DataFileHost = "127.0.0.1";
            //this.DataFilePort = 1000;
            //this.DataFileName = Installation.DataDir.FullName + "\\data.quant";
        }

        public void SetDefaultInstrumentConfiguration()
        {
            //this.IsInstrumentFileLocal = false;
            //this.InstrumentFileHost = "127.0.0.1";
            //this.InstrumentFilePort = 1000;
            //this.InstrumentFileName = Installation.DataDir.FullName + "\\instruments.quant";
        }

        public void SetDefaultOrderConfiguration()
        {
            //this.IsOrderFileLocal = false;
            //this.OrderFileHost = "127.0.0.1";
            //this.OrderFilePort = 1000;
            //this.OrderFileName = Installation.DataDir.FullName + "\\orders.quant";
            //this.OrderServer = "FileOrderServer";
            //this.OrderConnectionString = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=" + Installation.DataDir.FullName + "\\orders.mdb;";
        }

        public void SetDefaultPortfolioConfiguration()
        {
            //this.IsPortfolioFileLocal = false;
            //this.PortfolioFileHost = "127.0.0.1";
            //this.PortfolioFilePort = 1000;
            //this.PortfolioFileName = Installation.DataDir.FullName + "\\portfolios.quant";
        }

        public void AddDefaultProviders()
        {
        }

        public void AddDefaultStreamers()
        {
        }
    }
}
