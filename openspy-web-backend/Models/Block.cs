using System;
using System.Collections.Generic;

namespace CoreWeb.Models
{
    public partial class Block
    {
        public int Id { get; set; }
        public int? ToProfileid { get; set; }
        public int? FromProfileid { get; set; }

        public Profile FromProfile { get; set; }
        public Profile ToProfile { get; set; }
    }
}
