using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class PresenceProfileLookup
    {
        public ProfileLookup profileLookup; //profile buddies belong to
        public ProfileLookup targetLookup; //used for filtering down to a specific profile
        public bool? buddyLookup;
        public bool? blockLookup;
        public bool? reverseLookup;
    };
    public class PresenceProfileStatus
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ProfileLookup profileLookup;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Profile profile;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public User user;
        public String IP;
        public UInt16 Port;
        public UInt32 quietFlags;
        public String locationText;
        public String statusText;
        public UInt32 statusFlags;
    }
}
