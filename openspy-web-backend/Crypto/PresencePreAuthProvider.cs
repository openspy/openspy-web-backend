using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Crypto
{
    public class PresencePreAuthProvider : RSAProvider
    {
        public PresencePreAuthProvider(String pemBase64) : base(pemBase64)
        {

        }
    }
}
