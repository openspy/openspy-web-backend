using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Exception
{
    public class UniqueNickInUseException : IApplicationException
    {
        public int ?profileId;
        public UniqueNickInUseException(int ?profileId) : base("profile", "UniqueNickInUse")
        {
            this.profileId = profileId;
        }
    }
}
