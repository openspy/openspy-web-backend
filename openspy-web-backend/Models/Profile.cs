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
        public int? userId;
        public String nick;
        public String uniquenick;
        public int? namespaceid;
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
        public sbyte Deleted { get; set; }
        public sbyte Admin { get; set; }

        public User User { get; set; }

        public ICollection<Block> BlocksFromProfile { get; set; }
        public ICollection<Block> BlocksToProfile { get; set; }
        public ICollection<Buddy> BuddiesFromProfile { get; set; }
        public ICollection<Buddy> BuddiesToProfile { get; set; }
        public ICollection<PersistData> PersistData { get; set; }
        public ICollection<PersistKeyedData> PersistKeyedData { get; set; }
    }
}
