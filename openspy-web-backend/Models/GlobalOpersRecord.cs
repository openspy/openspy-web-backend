using CoreWeb.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class GlobalOpersLookup {
        public int profileid {get; set;}
    };
    public class GlobalOpersRecord
    {
        public int Id { get; set; }
        public int profileid { get; set; }
        public int operflags { get; set; }

        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime setAt { get; set; } = DateTime.UtcNow;
    }
}
