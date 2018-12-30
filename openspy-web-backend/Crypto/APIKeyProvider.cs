using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Crypto
{
    public class SignedKeyData
    {
        public string name;
        public List<string> roles;
        public long? expiresAt;
    };
    public class APIKeyProvider : RSAProvider
    {
        public APIKeyProvider(String pemBase64) : base(pemBase64)
        {

        }
    }
}
