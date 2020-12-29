using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoreWeb.Models.EA
{
    public class ObjectInventoryLookup {
        public string DomainId {get; set;}
        public string SubdomainId {get; set;}
        public string PartitionKey {get; set;}
        public IEnumerable<string> ObjectIds { get; set; }

        public GameLookup gameLookup {get; set;}
        public UserLookup userLookup {get; set;}
    }
    public class ObjectInventoryItem
    {
        public string ObjectId { get; set; }
        public int EditionNo { get; set; }
        public DateTime DateEntitled { get; set; }
        public int UseCount { get; set; }
        public int EntitleId { get; set; }
    }
}