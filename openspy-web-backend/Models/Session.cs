using CoreWeb.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class SessionLookup
    {
        public String sessionKey;
        public ProfileLookup profileLookup;
    };
    public class Session
    {
        public Profile profile;
        public User user;        
        [JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan? expiresIn;
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? expiresAt;
        public String sessionKey;
    };
}
