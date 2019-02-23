using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class CdKey
    {
        public int Id { get; set; }
        public String Cdkey { get; set; }
        public bool UserInserted { get; set; }
        public int Gameid { get; set; }
    }
}
