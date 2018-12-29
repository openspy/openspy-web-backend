using System;
using System.Collections.Generic;

namespace CoreWeb.Models
{
    public partial class PersistKeyedData
    {
        public int Id { get; set; }
        public string KeyName { get; set; }
        public byte[] KeyValue { get; set; }
        public int? Profileid { get; set; }
        public int? Gameid { get; set; }
        public int? PersistType { get; set; }
        public int? DataIndex { get; set; }
        public DateTime? Modified { get; set; }

        public Profile Profile { get; set; }
    }
}
