using System;
using System.Collections.Generic;
using CoreWeb.Models;
namespace CoreWeb.Models
{
    public partial class Buddy
    {
        public int Id { get; set; }
        public int? ToProfileid { get; set; }
        public int? FromProfileid { get; set; }

        public Profile FromProfile { get; set; }
        public Profile ToProfile { get; set; }
    }
}
