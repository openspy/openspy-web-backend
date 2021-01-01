using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using CoreWeb.Filters;
namespace CoreWeb.Models.EA
{
    public class EntitledGameFeatureLookup {
        public GameLookup gameLookup {get; set;}
        public ProfileLookup profileLookup {get; set;}
    }
    public class EntitledGameFeature
    {
        public int GameFeatureId { get; set; }
        public int Status { get; set; }
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public System.DateTime? EntitlementExpirationDate { get; set; }
        public string Message { get; set; }
        public int? EntitlementExpirationDays { get; set; }
    }
}
