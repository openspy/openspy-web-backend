using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Models
{
    public class PlayerProgress
    {
        public string pageKey;
        public Profile profile;
        public Game game;
        public object data;
        public decimal modified;
    }
    public class PlayerProgressLookup
    {
        public string objectId;
        public string pageKey;
        public ProfileLookup profileLookup;
        public GameLookup gameLookup;
    }
}
