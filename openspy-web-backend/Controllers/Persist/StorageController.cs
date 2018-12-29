using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers.Persist
{
    public class SetDataRequest
    {
        public String base64Data;
        public Dictionary<String, String> keyValueList;
        public ProfileLookup profileLookup;
        public UserLookup userLookup;
        public GameLookup gameLookup;
    }
    public class GetDataRequest
    {
        public ProfileLookup profileLookup;
        public UserLookup userLookup;
        public GameLookup gameLookup;
        public int data_index;
        public int data_type;
        public System.DateTime? modified_since;
    }
    public class NewGameRequest
    {
        public ProfileLookup profileLookup;
        public UserLookup userLookup;
        public GameLookup gameLookup;
    };
    public class AddGameSnapshotRequest
    {
        public ProfileLookup profileLookup;
        public UserLookup userLookup;
        public GameLookup gameLookup;
        public Dictionary<String, String> keyValueList;
        public bool? complete;
        public String game_identifier;
    };
    [Route("v1/Persist/[controller]")]
    [ApiController]
    public class StorageController : Controller
    {
        [HttpPut("SetKVData")]
        public void SetPersistKeyedData([FromBody] SetDataRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("GetKVData")]
        public void GetPersistKeyedData([FromBody] GetDataRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("SetData")]
        public void SetPersistData([FromBody] SetDataRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("GetData")]
        public void GetPersistData([FromBody] GetDataRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("NewGame")]
        public void DeclareNewGame([FromBody] NewGameRequest request)
        {
            throw new NotImplementedException();
        }
        [HttpPut("AddGameSnapshot")]
        public void AddGameSnapshot([FromBody] AddGameSnapshotRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
