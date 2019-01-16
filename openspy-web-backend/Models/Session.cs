using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class SessionLookup
    {
        public String key;
        public ProfileLookup profile;
    };
    public class Session
    {
        public Profile profile;
        public User user;
        public TimeSpan? expiresIn;
        public DateTime? expiresAt;
        public String sessionKey;
    };
}
