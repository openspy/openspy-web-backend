using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class PresenceProfileLookup
    {
        public ProfileLookup profileLookup;
        public bool? buddyLookup;
        public bool? blockLookup;
    };
    public class PresenceProfileStatus
    {
        public Profile profile;
        public String IP;
        public UInt16 Port;
        public UInt32 quietFlags;
        public String locationText;
        public String statusText;
        public UInt32 statusFlags;
    }
}
