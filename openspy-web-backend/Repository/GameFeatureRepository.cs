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
    public class GameFeatureRepository : IRepository<EntitledGameFeature, UserLookup>
    {
        public GameFeatureRepository()
        {
        }
        public Task<IEnumerable<EntitledGameFeature>> Lookup(UserLookup lookup)
        {
            var result = new List<EntitledGameFeature>();
            var entry = new EntitledGameFeature {
                EntitlementExpirationDays = -1,
                GameFeatureId = 2590,
                Status = 0,
                EntitlementExpirationDate = null,
                Message = null

            };
            result.Add(entry);

            return Task.Run(() => {
                return (IEnumerable<EntitledGameFeature>)result;
            });
        }
        public Task<bool> Delete(UserLookup lookup)
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
