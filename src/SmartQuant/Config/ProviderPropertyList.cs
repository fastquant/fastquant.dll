// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SmartQuant
{
    [XmlRoot("settings")]
    public struct XmlProviderManagerSettings
    {
        [XmlArrayItem("provider")]
        [XmlArray("providers")]
        public List<XmlProvider> Providers;
    }

    public struct XmlProvider
    {
        [XmlElement("id")]
        public int ProviderId;

        [XmlElement("instance")]
        public int InstanceId;

        [XmlArrayItem("property")]
        [XmlArray("properties")]
        public List<XmlProviderProperty> Properties;
    }

    public struct XmlProviderProperty
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlText]
        public string Value;
    }

    public class ProviderPropertyList
    {
        private readonly Dictionary<string, string> _properties  = new Dictionary<string, string>();

        public void SetValue(string name, string value)
        {
            _properties[name] = value;
        }

        public string GetStringValue(string name, string defaultValue)
        {
            string s;
            return _properties.TryGetValue(name, out s) ? s : defaultValue;
        }

        public ProviderPropertyList()
        {
        }

        internal ProviderPropertyList(IEnumerable<XmlProviderProperty> properties)
        {
            foreach (var p in properties)
                SetValue(p.Name, p.Value);
        }

        internal List<XmlProviderProperty> ToXmlProviderProperties() => _properties.Select(p => new XmlProviderProperty {Name = p.Key, Value = p.Value}).ToList();
    }
}
