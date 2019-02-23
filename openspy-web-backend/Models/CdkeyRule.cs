using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class CdkeyRule
    {
        public int Id { get; set; }
        public int Gameid { get; set; }
        public bool Failifnotfound { get; set; }
    }
}
