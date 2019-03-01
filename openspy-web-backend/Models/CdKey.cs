using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class CdKeyLookup
    {
        public int? Id { get; set; }
        public String Cdkey { get; set; }
        public String CdkeyHash { get; set; }
        public ProfileLookup profileLookup;
        public int? Gameid { get; set; }
    };
    public class CdKey
    {
        public int Id { get; set; }
        public String Cdkey { get; set; }
        public String CdkeyHash { get; set; }
        public bool InsertedByUser { get; set; }
        public int Gameid { get; set; }
    }
}
