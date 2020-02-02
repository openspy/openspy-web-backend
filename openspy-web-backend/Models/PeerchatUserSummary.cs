using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{

	public class PeerchatUserSummary
    {
		public int Id { get; set; }
		public string Nick { get; set; }
		public string Username { get; set; }
		public string Hostname { get; set; }
		public string Realname { get; set; }
		public string Address { get; set; }
		public int Gameid{ get; set; }
		public int Profileid{ get; set; }
	}
	public class PeerchatChannelUserSummary
	{
		public string ChannelName { get; set; }
		public PeerchatUserSummary UserSummary { get; set; }
	}
}
