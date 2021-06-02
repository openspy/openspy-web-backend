using CoreWeb.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class UsermodeLookup {
        public int? Id {get; set;}
        public string channelmask {get; set;}
        public string hostmask {get; set;}
        public string machineid { get; set; }
        public int? profileid {get; set;}
    };
    public class UsermodeRecord
    {
        public int Id { get; set; }
        public string channelmask { get; set; }
        public string hostmask { get; set; }
        public string comment { get; set; }
        public string machineid { get; set; }
        public int? profileid { get; set; }
        public int modeflags { get; set; }
        [JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan? expiresIn
        {
            get
            {
                if(expiresAt.HasValue)
                {
                    return expiresAt.Value.Subtract(DateTime.UtcNow);
                }
                return null;
            }
        }
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? expiresAt { get; set; }

        public string setByNick { get; set; }
        public string setByHost { get; set; }
        public int setByPid { get; set; }
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime setAt { get; set; } = DateTime.UtcNow;
        public bool isGlobal {get; set;}
    }
}
