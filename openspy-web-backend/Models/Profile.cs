using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class ProfileLookup
    {
        public int? id;
        public String nick;
        public String uniquenick;
        public UserLookup user;
        public int? namespaceid;
        public int? partnercode;
    };

    public class Profile
    {
        public Profile()
        {
        }

        public int Id { get; set; }
        public string Nick { get; set; }
        public string Uniquenick { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Homepage { get; set; }
        public int? Icquin { get; set; }
        public int? Zipcode { get; set; }
        public int? Sex { get; set; }
        public int? Ooc { get; set; }
        public int? Ind { get; set; }
        public int? Inc { get; set; }
        public int? I1 { get; set; }
        public int? O1 { get; set; }
        public int? Mar { get; set; }
        public int? Chc { get; set; }
        public int? Conn { get; set; }
        public decimal? Lon { get; set; }
        public decimal? Lat { get; set; }
        public string Aimname { get; set; }
        public string Countrycode { get; set; }
        public int? Pic { get; set; }
        public int Userid { get; set; }
        public int Namespaceid { get; set; }
        public System.Int16 Deleted { get; set; }
        public System.Int16 Admin { get; set; }

        [JsonIgnoreAttribute]
        public User User { get; set; }

        [JsonIgnoreAttribute]
        public ICollection<Block> BlocksFromProfile { get; set; }
        [JsonIgnoreAttribute]
        public ICollection<Block> BlocksToProfile { get; set; }
        [JsonIgnoreAttribute]
        public ICollection<Buddy> BuddiesFromProfile { get; set; }
        [JsonIgnoreAttribute]
        public ICollection<Buddy> BuddiesToProfile { get; set; }
        [JsonIgnoreAttribute]
        public ICollection<PersistData> PersistData { get; set; }
        [JsonIgnoreAttribute]
        public ICollection<PersistKeyedData> PersistKeyedData { get; set; }
    }
}
