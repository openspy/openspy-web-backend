using System;
using System.Collections.Generic;
using CoreWeb.Models;
namespace CoreWeb.Models
{
    public class SendMessageRequest
    {
        public BuddyLookup lookup;
        public String message;
        public int type;
        public System.DateTime? time;
    };
    public class BuddyLookup
    {
        public ProfileLookup SourceProfile;
        public ProfileLookup TargetProfile;
        public bool? reverseLookup;
        public String addReason;
        public bool? silent;
    };
    public partial class Buddy
    {
        public int Id { get; set; }
        public int? ToProfileid { get; set; }
        public int? FromProfileid { get; set; }

        public Profile FromProfile { get; set; }
        public Profile ToProfile { get; set; }
    }
}
