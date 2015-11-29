using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartQuant
{
    public class User : DataObject
    {
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
    { }
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