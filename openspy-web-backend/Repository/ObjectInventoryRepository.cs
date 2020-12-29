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
        public ObjectInventoryRepository()
        {
        }
        public Task<IEnumerable<ObjectInventoryItem>> Lookup(ObjectInventoryLookup lookup)
        {
            var result = new List<ObjectInventoryItem>();
            if(lookup.DomainId.CompareTo("eagames") == 0 && lookup.SubdomainId.CompareTo("bf2142") == 0 && lookup.PartitionKey.CompareTo("online_content") == 0 && lookup.ObjectIds.Contains("bf2142_bp1")) {
                var entry = new ObjectInventoryItem {
                    ObjectId = "bf2142_bp1",
                    EditionNo = 0,
                    DateEntitled = DateTime.Now.AddDays(-7),
                    EntitleId = 114793868,
                    UseCount = 0
                };
                result.Add(entry);

                entry = new ObjectInventoryItem {
                    ObjectId = "bf2142_bp1",
                    EditionNo = 0,
                    DateEntitled = DateTime.Now.AddDays(-7),
                    EntitleId = 245445381,
                    UseCount = 0
                };
                result.Add(entry);
            }

            return Task.Run(() => {
                return (IEnumerable<ObjectInventoryItem>)result;
            });
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
