using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoreWeb.Models
{
    public class UserLookup
    {
        public int? id;
        public String email;
        public int? partnercode;
    }
    public class User
    {
        public const int PARTNERID_GAMESPY = 0;
        public const int PARTNERID_IGN = 10;
        public const int PARTNERID_EA = 20;
        public User()
        {
            Profiles = new HashSet<Profile>();
        }

        public int Id { get; set; }
        public string Email { get; set; }
        [JsonIgnoreAttribute]
        public string Password { get; set; }
        public int? Videocard1ram { get; set; }
        public int? Videocard2ram { get; set; }
        public int? Cpuspeed { get; set; }
        public int? Cpubrandid { get; set; }
        public int? Connectionspeed { get; set; }
        public sbyte? Hasnetwork { get; set; }
        public int Partnercode { get; set; }
        public int Publicmask { get; set; }
        public sbyte EmailVerified { get; set; }
        public sbyte Deleted { get; set; }

        [JsonIgnoreAttribute]
        public ICollection<Profile> Profiles { get; set; }
    }
}
