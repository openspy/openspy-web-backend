using CoreWeb.Database;
using CoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Repository
{
    public class CdKeyRepository : IRepository<CdKey, CdKeyLookup>
    {
        private KeymasterDBContext keyMasterDb;
        private IRepository<Profile, ProfileLookup> profileRepository;
        public CdKeyRepository(KeymasterDBContext keyMasterDb, IRepository<Profile, ProfileLookup> profileRepository)
        {
            this.keyMasterDb = keyMasterDb;
            this.profileRepository = profileRepository;
        }
        public async Task<CdKey> Create(CdKey model)
        {
            var entry = await keyMasterDb.AddAsync<CdKey>(model);
            var num_modified = await keyMasterDb.SaveChangesAsync();
            return entry.Entity;
        }

        public Task<bool> Delete(CdKeyLookup lookup)
        {
            return Task.Run(async () =>
            {
                var cdkeys = (await Lookup(lookup)).ToList();
                foreach (var cdkey in cdkeys)
                {
                    keyMasterDb.Remove<CdKey>(cdkey);
                }
                var num_modified = await keyMasterDb.SaveChangesAsync();
                return cdkeys.Count > 0 && num_modified > 0;
            });
        }

        public async Task<IEnumerable<CdKey>> Lookup(CdKeyLookup lookup)
        {
            if (lookup.Id.HasValue)
            {
                var results = await keyMasterDb.CdKey.Where(b => b.Id == lookup.Id.Value).ToListAsync();
                return results;
            } else if(lookup.Cdkey != null)
            {
                var results = await keyMasterDb.CdKey.Where(b => b.Cdkey == lookup.Cdkey && b.Gameid == lookup.Gameid).ToListAsync();
                return results;
            }
            else
            {
                var results = await keyMasterDb.CdKey.ToListAsync();
                return results;
            }
        }

        public Task<CdKey> Update(CdKey model)
        {
            return Task.Run(async () =>
            {
                var entry = keyMasterDb.Update<CdKey>(model);
                await keyMasterDb.SaveChangesAsync();
                return entry.Entity;
            });
        }

        public async Task<bool> LookupFailItNotFound(CdKeyLookup lookup)
        {
            var results = await keyMasterDb.CdKeyRules.Where(b => b.Gameid == lookup.Gameid.Value).FirstOrDefaultAsync();
            if(results != null)
            {
                return results.Failifnotfound;
            }
            return false;
        }
        public async Task<bool> AssociateCDKeyToProfile(CdKeyLookup cdKeyLookup, Profile profile)
        {
            var cdkeyResults = (await Lookup(cdKeyLookup)).FirstOrDefault();
            if (cdkeyResults == null) return false;

            var cdkeyAssociation = new ProfileCdKey();
            cdkeyAssociation.Cdkeyid = cdkeyResults.Id;
            cdkeyAssociation.Profileid = profile.Id;

            await keyMasterDb.ProfileCdKey.AddAsync(cdkeyAssociation);
            var num_modified = await keyMasterDb.SaveChangesAsync();
            return num_modified > 0;

        }
        public async Task<Profile> LookupProfileFromCDKey(CdKeyLookup lookup)
        {
            var cdkeyResults = (await Lookup(lookup)).FirstOrDefault();
            if (cdkeyResults == null) return null;

            var profileResults = await keyMasterDb.ProfileCdKey.Where(b => b.Cdkeyid == cdkeyResults.Id).FirstOrDefaultAsync();
            var profileLookup = new ProfileLookup();

            profileLookup.id = profileResults.Profileid;
            return (await profileRepository.Lookup(profileLookup)).FirstOrDefault();
        }
    }
}
