using System;
using System.Collections.Generic;
using CoreWeb.Models;

namespace CoreWeb.Models
{
    public partial class PersistData
    {
        public int Id { get; set; }
        public DateTimeOffset? Modified { get; set; }
        public byte[] Base64Data { get; set; }
        public int? PersistType { get; set; }
        public int? DataIndex { get; set; }
        public int? Profileid { get; set; }
        public int Gameid { get; set; }

        public Profile Profile { get; set; }
    }
}
