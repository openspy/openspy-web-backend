using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreWeb.Crypto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers
{
    public class SignKeyRequest
    {
        public string name;
        public List<string> roles;
        public int? expiresInSecs;
    }
    [Route("api/[controller]")]
    public class APIKeyController : Controller
    {
        private APIKeyProvider rsaProvider;
        public APIKeyController(APIKeyProvider rsaProvider)
        {
            this.rsaProvider = rsaProvider;
        }
        // GET: api/<controller>
        [HttpPost("GenerateKey")]
        public string GenerateKey([FromBody] SignKeyRequest keyData)
        {
            SignedKeyData data = new SignedKeyData();
            data.roles = keyData.roles;
            data.name = keyData.name;
            if(keyData.expiresInSecs.HasValue)
            {
                data.expiresAt = DateTime.Now.AddSeconds(keyData.expiresInSecs.Value).ToFileTimeUtc();
            }                
            var json_data = JsonConvert.SerializeObject(data);
            var encrypted_buff = rsaProvider.EncryptData(Encoding.UTF8.GetBytes(json_data));
            return Convert.ToBase64String(encrypted_buff);
        }
        [HttpPost("DecryptKey")]
        public SignedKeyData GenerateKey([FromBody] string keyData)
        {
            var encrypted_buff = Convert.FromBase64String(keyData);
            var decrypted_buff = rsaProvider.DecryptData(encrypted_buff);
            var signedData = JsonConvert.DeserializeObject<SignedKeyData>(Encoding.ASCII.GetString(decrypted_buff));
            return signedData;
        }
    }
}
