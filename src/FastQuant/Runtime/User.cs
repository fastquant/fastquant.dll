using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FastQuant
{
    public class UserItem
    {
        public UserItem()
        {
        }

        public UserItem(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public UserItem(string name, bool value) : this(name, value.ToString())
        {
        }

        public UserItem(string name, int value) : this(name, value.ToString())
        {
        }

        public UserItem(string name, double value) : this(name, value.ToString())
        {
        }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Value")]
        public string Value { get; set; }
    }


    public class User : DataObject
    {
        [ReadOnly(true), XmlElement("Id")]
        public int Id { get; set; }

        [ReadOnly(true), XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }

        // Token: 0x1700003A RID: 58
        [XmlArray("Items"), XmlArrayItem("Item")]
        public List<UserItem> Items = new List<UserItem>();
    }

    public class UserList : IEnumerable<User>
    {
        public IEnumerator<User> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class UserManager
    {
        private Framework framework;
        private UserServer server;

        public UserManager(Framework framework, UserServer server)
        {
            this.framework = framework;
            this.server = server;
        }

        public void Load()
        {
            this.server.Load();
        }
    }

    public class UserServer
    {
        private Framework framework;

        public UserServer(Framework framework)
        {
            this.framework = framework;
        }

        public virtual UserList Load()
        {
            return null;
        }

        public virtual void Save(UserList clients)
        {
        }
    }
}