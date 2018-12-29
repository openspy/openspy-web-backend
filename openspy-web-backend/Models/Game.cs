using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class GameLookup
    {
        public int? id;
        public String gamename;
    };

    public class Game
    {
        public int Id { get; set; }
        public string Gamename { get; set; }
        public string Secretkey { get; set; }
        public string Description { get; set; }
        public int Queryport { get; set; }
        public int Backendflags { get; set; }
        public int Disabledservices { get; set; }
        public string Keylist { get; set; }
        public string Keytypelist { get; set; }
    };
}
