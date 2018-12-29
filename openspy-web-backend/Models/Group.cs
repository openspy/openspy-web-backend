using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class GroupLookup
    {
        public int? id;
        public int? gameid;
    };

    public class Group
    {
        public int Gameid { get; set; }
        public int Groupid { get; set; }
        public int Maxwaiting { get; set; }
        public string Name { get; set; }
        public string Other { get; set; }
    }
}
