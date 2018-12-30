using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreWeb.Crypto;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Text;

namespace CoreWeb.Authentication
{
    public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOpts>
    {
        private APIKeyProvider apiKeyProvider;
        public ApiKeyAuthHandler(IOptionsMonitor<ApiKeyAuthOpts> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, APIKeyProvider apiKeyProvider)
                : base(options, logger, encoder, clock)
        {
            this.apiKeyProvider = apiKeyProvider;
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.Run(() =>
            {
                try {
                    var APIKey = Context.Request.Headers["APIKey"];
                    var encrypted_buff = Convert.FromBase64String(APIKey);
                    var DecryptedAPIKey = apiKeyProvider.DecryptData(encrypted_buff);
                    SignedKeyData signedData = JsonConvert.DeserializeObject<SignedKeyData>(Encoding.ASCII.GetString(DecryptedAPIKey));
                    if(signedData.expiresAt.HasValue)
                    {
                        DateTime expireTime = DateTime.FromFileTimeUtc(signedData.expiresAt.Value);
                        if(DateTime.Now > expireTime)
                        {
                            return AuthenticateResult.Fail("Expired Token");
                        }
                    }
                    var identity = new GenericIdentity("API");
                    identity.AddClaim(new System.Security.Claims.Claim("Origin", "Api"));

                    foreach (var role in signedData.roles)
                    {
                        identity.AddClaim(new System.Security.Claims.Claim("role", role));
                    }

                    var principal = new GenericPrincipal(identity, new[] { signedData.name });

                    var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
                 catch(System.Exception e)
                {
                    return AuthenticateResult.Fail("Invalid APIKey");
                }

            });
        }
    }
}
