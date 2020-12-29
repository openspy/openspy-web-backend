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
    public class GameFeatureRepository : IRepository<EntitledGameFeature, EntitledGameFeatureLookup>
    {
        private IRepository<Game, GameLookup> gameRepository;
        public GameFeatureRepository(IRepository<Game, GameLookup> gameRepository)
        {
            this.gameRepository = gameRepository;
        }
        public async Task<IEnumerable<EntitledGameFeature>> Lookup(EntitledGameFeatureLookup lookup)
        {
            var game = (await gameRepository.Lookup(lookup.gameLookup)).FirstOrDefault();
            var result = new List<EntitledGameFeature>();
            if(game != null && game.Id == 1324) { //stella/bf2142
                var entry = new EntitledGameFeature {
                    EntitlementExpirationDays = -1,
                    GameFeatureId = 2590,
                    Status = 0,
                    EntitlementExpirationDate = null,
                    Message = null
                };
                result.Add(entry);
            }
            return (IEnumerable<EntitledGameFeature>)result;
        }
        public Task<bool> Delete(EntitledGameFeatureLookup lookup)
        {
            throw new NotImplementedException();
        }
        public Task<EntitledGameFeature> Update(EntitledGameFeature model)
        {
            throw new NotImplementedException();
        }
        public Task<EntitledGameFeature> Create(EntitledGameFeature model)
        {
            throw new NotImplementedException();
        }
    }
}
