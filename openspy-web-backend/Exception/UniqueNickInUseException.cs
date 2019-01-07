using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class UniqueNickInUseException : IApplicationException
    {
        public UniqueNickInUseException(int ?profileId, int ?userId = null) : base("profile", "UniqueNickInUse")
        {
            if(profileId.HasValue)
            {
                extraData["profileid"] = profileId.Value;
            }
            if(userId.HasValue)
            {
                extraData["userid"] = userId.Value;
            }
        }
    }
}
