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
        public String address;
        public System.UInt32 quietFlags;
        public String locationText;
        public String statusText;
        public System.UInt32 statusFlags;
    }
}
