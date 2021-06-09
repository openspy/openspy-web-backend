using CoreWeb.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class ChanpropsLookup {
        public int? Id {get; set;}
        public string channelmask {get; set;}
    };
    public class ChanpropsRecord
    {
        public int Id { get; set; }
        public string channelmask { get; set; }
        public string password { get; set; }
        public string entrymsg { get; set; }
        public string comment { get; set; }
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? expiresAt { get; set; }
        public string groupname {get; set;}
        public int? limit {get; set;}
        public int modeflags {get; set;}
        public bool onlyOwner {get; set;}
        public string topic {get; set;}
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? setAt { get; set; }
        public string setByNick {get; set;}
        public int setByPid {get; set;}
        public string setByHost {get; set;}

        //not stored in db... just used for kick on create logic
        public bool? kickExisting {get; set;}
    }
}