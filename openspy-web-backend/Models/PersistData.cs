using System;
using System.Collections.Generic;
using CoreWeb.Filters;
using CoreWeb.Models;
using Newtonsoft.Json;

namespace CoreWeb.Models
{
    public class PersistDataLookup
    {
        public GameLookup gameLookup;
        public ProfileLookup profileLookup;
        public int? PersistType { get; set; }
        public int? DataIndex { get; set; }
};
    public partial class PersistData
    {
        public int Id { get; set; }
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? Modified { get; set; }
        public byte[] Base64Data { get; set; }
        public int? PersistType { get; set; }
        public int? DataIndex { get; set; }
        public int? Profileid { get; set; }
        public int Gameid { get; set; }

        public Profile Profile { get; set; }
    }
}
