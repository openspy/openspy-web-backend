using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CoreWeb.Models;
using CoreWeb.Repository;
using System.Text;
using CoreWeb.Exception;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreWeb.Controllers.Persist
{
    public class SetDataRequest
    {
        public String base64Data;
        public Dictionary<String, String> keyValueList;
        public ProfileLookup profileLookup;
        public GameLookup gameLookup;
        public int dataIndex;
        public int persistType;
    }
    public class GetDataRequest
    {
        public ProfileLookup profileLookup;
        public GameLookup gameLookup;
        public int dataIndex;
        public int persistType;
        public List<string> keys;
        public System.DateTime? modifiedSince;
    }
    public class NewGameRequest
    {
        public ProfileLookup profileLookup;
        public GameLookup gameLookup;
    };
    public class AddGameSnapshotRequest
    {
        public ProfileLookup profileLookup;
        public GameLookup gameLookup;
        public Dictionary<String, String> keyValueList;
        public bool? complete;
        public String _id;
    };
    [Route("v1/Persist/[controller]")]
    [Authorize(Policy = "Persist")]
    [ApiController]
    public class StorageController : Controller
    {
        public PersistDataRepository persistRepository;
        public PersistKeyedDataRepository persistKeyedRepository;
        private SnapShotRepository snapshotRepository;
        private IRepository<Profile, ProfileLookup> profileRepository;
        private IRepository<Game, GameLookup> gameRepository;
        public StorageController(IRepository<PersistData, PersistDataLookup> persistRepository, IRepository<PersistKeyedData, PersistKeyedDataLookup> persistKeyedRepository, IRepository<Profile, ProfileLookup> profileRepository, IRepository<Game, GameLookup> gameRepository, IRepository<Snapshot, SnapshotLookup> snapshotRepository)
        {
            this.persistKeyedRepository = (PersistKeyedDataRepository)persistKeyedRepository;
            this.persistRepository = (PersistDataRepository)persistRepository;
            this.profileRepository = profileRepository;
            this.gameRepository = gameRepository;
            this.snapshotRepository = (SnapShotRepository)snapshotRepository;
        }
        [HttpPut("SetKVData")]
        public async Task SetPersistKeyedData([FromBody] SetDataRequest request)
        {

            //update existing
            var lookup = new PersistKeyedDataLookup();
            lookup.gameLookup = request.gameLookup;
            lookup.profileLookup = request.profileLookup;
            lookup.persistType = request.persistType;
            lookup.dataIndex = request.dataIndex;
            lookup.keys = new List<string>();

            //find keys to be deleted
            foreach (var key in request.keyValueList)
            {
                if(key.Value == null || key.Value.Length == 0)
                {
                    lookup.keys.Add(key.Key);
                }
            }
            await persistKeyedRepository.Delete(lookup);
            lookup.keys.Clear();

            //find existing keys
            foreach (var key in request.keyValueList)
            {
                lookup.keys.Add(key.Key);
            }
            var found_keys = await persistKeyedRepository.Lookup(lookup);
            foreach(var found_key in found_keys)
            {
                found_key.Modified = DateTime.UtcNow;
                found_key.KeyValue = Convert.FromBase64String(request.keyValueList[found_key.KeyName]);
                await persistKeyedRepository.Update(found_key);
                request.keyValueList.RemoveKey(found_key.KeyName);
            }

            //create new
            foreach(var kvData in request.keyValueList)
            {
                var data = new PersistKeyedData();
                data.KeyName = kvData.Key;
                data.KeyValue = Encoding.ASCII.GetBytes(kvData.Value);
                data.Modified = DateTime.UtcNow;
                data.DataIndex = request.dataIndex;
                data.PersistType = request.persistType;

                var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
                var profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
                data.Gameid = game.Id;
                data.Profileid = profile.Id;
                await persistKeyedRepository.Create(data);
            }
        }

        [HttpPost("GetKVData")]
        public Task<IEnumerable<PersistKeyedData>> GetPersistKeyedData([FromBody] GetDataRequest request)
        {
            var lookup = new PersistKeyedDataLookup();
            lookup.gameLookup = request.gameLookup;
            lookup.profileLookup = request.profileLookup;
            lookup.persistType = request.persistType;
            lookup.dataIndex = request.dataIndex;
            lookup.keys = request.keys;
            return persistKeyedRepository.Lookup(lookup);
        }

        [HttpPut("SetData")]
        public async Task<PersistData> SetPersistData([FromBody] SetDataRequest request)
        {
            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();
            var profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();

            var lookup = new PersistDataLookup();
            lookup.profileLookup = request.profileLookup;
            lookup.gameLookup = request.gameLookup;
            lookup.DataIndex = request.dataIndex;
            lookup.PersistType = request.persistType;

            //delete request
            PersistData data_entry = null;
            if (request.base64Data.Length == 0)
            {
                bool success = await persistRepository.Delete(lookup);
                if(success)
                {
                    var data = new PersistData();
                    data.Modified = DateTime.UtcNow;
                    data.Profile = profile;
                    return data;
                }
            } else
            {
                data_entry = (await persistRepository.Lookup(lookup)).FirstOrDefault();
                if (data_entry != null)
                {
                    data_entry.Base64Data = Convert.FromBase64String(request.base64Data);
                    return await persistRepository.Update(data_entry);
                }
                else
                {
                    var data = new PersistData();
                    data.Base64Data = Convert.FromBase64String(request.base64Data);
                    data.DataIndex = request.dataIndex;
                    data.PersistType = request.persistType;
                    data.Profileid = profile.Id;
                    data.Gameid = game.Id;
                    return await persistRepository.Create(data);
                }
            }
            return null;
        }

        [HttpPost("GetData")]
        public async Task<IEnumerable<PersistData>> GetPersistData([FromBody] GetDataRequest request)
        {
            var lookup = new PersistDataLookup();
            lookup.profileLookup = request.profileLookup;
            lookup.gameLookup = request.gameLookup;
            lookup.DataIndex = request.dataIndex;
            lookup.PersistType = request.persistType;
            return await persistRepository.Lookup(lookup);
            
        }

        [HttpPut("NewGame")]
        public async Task<Snapshot> DeclareNewGame([FromBody] NewGameRequest request)
        {
            var snapshot = new Snapshot();
            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();
            var profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();
            snapshot.gameid = game.Id;
            snapshot.profileid = profile.Id;
            snapshot.ip = "127.0.0.1:666";
            return await snapshotRepository.Create(snapshot);
        }
        [HttpPut("AddGameSnapshot")]
        public async Task<bool> AddGameSnapshot([FromBody] AddGameSnapshotRequest request)
        {
            var game = (await gameRepository.Lookup(request.gameLookup)).FirstOrDefault();
            if (game == null) throw new ArgumentException();
            var profile = (await profileRepository.Lookup(request.profileLookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();

            var update = new SnapshotUpdate();
            update.data = request.keyValueList;
            update.profileid = profile.Id;
            update.gameid = game.Id;
            update.completed = request.complete.HasValue && request.complete.Value;
            
            return await snapshotRepository.AppendSnapshotUpdate(request._id, update);
            
        }
    }
}
