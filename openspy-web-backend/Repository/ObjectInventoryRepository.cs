using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;
using CoreWeb.Models.EA;

namespace CoreWeb.Repository
{
    public class ObjectInventoryRepository : IRepository<ObjectInventoryItem, ObjectInventoryLookup>
    {
        private IRepository<Game, GameLookup> gameRepository;
        public ObjectInventoryRepository(IRepository<Game, GameLookup> gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        public async Task<IEnumerable<ObjectInventoryItem>> Lookup(ObjectInventoryLookup lookup)
        {
            var defaultDate = new DateTime(2008, 5, 1, 8, 30, 52);

            var result = new List<ObjectInventoryItem>();
            var game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
            if(game != null && game.Id == 1324) { //stella/bf2142
                if(lookup.DomainId.CompareTo("eagames") == 0 && lookup.SubdomainId.CompareTo("bf2142") == 0 && lookup.PartitionKey.CompareTo("online_content") == 0 && lookup.ObjectIds.Contains("bf2142_bp1")) {
                    var entry = new ObjectInventoryItem {
                        ObjectId = "bf2142_bp1",
                        EditionNo = 0,
                        DateEntitled = defaultDate,
                        EntitleId = 114793868,
                        UseCount = 0
                    };
                    result.Add(entry);

                    entry = new ObjectInventoryItem {
                        ObjectId = "bf2142_bp1",
                        EditionNo = 0,
                        DateEntitled = defaultDate,
                        EntitleId = 245445381,
                        UseCount = 0
                    };
                    result.Add(entry);
                }
            }

            return (IEnumerable<ObjectInventoryItem>)result;
        }
        public Task<bool> Delete(ObjectInventoryLookup lookup)
        {
            throw new NotImplementedException();
        }
        public Task<ObjectInventoryItem> Update(ObjectInventoryItem model)
        {
            throw new NotImplementedException();
        }
        public Task<ObjectInventoryItem> Create(ObjectInventoryItem model)
        {
            throw new NotImplementedException();
        }
    }
}
