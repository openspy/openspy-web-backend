using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoreWeb.Models
{
    public class UserLookup
    {
        public int? id;
        private string _email;
        public string email { get { return _email; } set { _email  = value.ToLower();} }
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

        private string _email;
        public string Email { get { return _email; } set { _email  = value.ToLower();} }
        [JsonIgnoreAttribute]
        public string Password { get; set; }
        public int? Videocard1ram { get; set; }
        public int? Videocard2ram { get; set; }
        public int? Cpuspeed { get; set; }
        public int? Cpubrandid { get; set; }
        public int? Connectionspeed { get; set; }
        public bool? Hasnetwork { get; set; }
        public int Partnercode { get; set; }
        public System.Int64 Publicmask { get; set; }
        public bool EmailVerified { get; set; }
        public bool Deleted { get; set; }

        [JsonIgnoreAttribute]
        public ICollection<Profile> Profiles { get; set; }
    }
}
