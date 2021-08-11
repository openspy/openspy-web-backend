using CoreWeb.Database;
using CoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using CoreWeb.Exception;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

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
            using(MD5 md5 = MD5.Create())
            {
                StringBuilder sBuilder = new StringBuilder();
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(model.Cdkey));
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                model.CdkeyHash = sBuilder.ToString();
            }
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
                var results = await keyMasterDb.CdKey.Where(b => (b.Cdkey == lookup.Cdkey || b.CdkeyHash == lookup.Cdkey) && b.Gameid == lookup.Gameid).ToListAsync();
                return results;
            } else if(lookup.CdkeyHash != null)
            {
                var results = await keyMasterDb.CdKey.Where(b => b.CdkeyHash == lookup.CdkeyHash && b.Gameid == lookup.Gameid).ToListAsync();
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
            if (cdkeyResults == null)
            {
                var failIfNotFound = await LookupFailItNotFound(cdKeyLookup);
                if (failIfNotFound || !cdKeyLookup.Gameid.HasValue)
                    throw new BadCdKeyException();

                var insertRequest = new CdKey();
                insertRequest.InsertedByUser = true;
                insertRequest.Gameid = cdKeyLookup.Gameid.Value;
                insertRequest.Cdkey = cdKeyLookup.Cdkey;
                cdkeyResults = await Create(insertRequest);
            }

            var cdkeyAssociation = new ProfileCdKey();
            cdkeyAssociation.Cdkeyid = cdkeyResults.Id;
            cdkeyAssociation.Profileid = profile.Id;

            await keyMasterDb.ProfileCdKey.AddAsync(cdkeyAssociation);
            var num_modified = await keyMasterDb.SaveChangesAsync();
            return num_modified > 0;

        }
        public async Task<Profile> LookupProfileFromCDKey(CdKeyLookup cdKeylookup)
        {
            var cdkeyResults = (await Lookup(cdKeylookup)).FirstOrDefault();
            if (cdkeyResults == null) return null;

            var profileResults = await keyMasterDb.ProfileCdKey.Where(b => b.Cdkeyid == cdkeyResults.Id).ToListAsync();
            if(profileResults == null || profileResults.Count == 0) throw new NoSuchUserException(); //client is out of luck, no profile found for cd key

            //find first user id, this is the "owner" of the cdkey
            var userProfileLookup = new ProfileLookup();
            userProfileLookup.id = profileResults.First().Profileid;

            var userProfile = (await profileRepository.Lookup(userProfileLookup)).FirstOrDefault();
            if(userProfile == null) throw new NoSuchUserException();

            if(cdKeylookup.profileLookup == null) {
                var profile_cdkey_records = await keyMasterDb.ProfileCdKey.Where(b => b.Cdkeyid == cdkeyResults.Id).ToListAsync();
                var firstProfileAssociationRecord = profile_cdkey_records.FirstOrDefault();
                if(firstProfileAssociationRecord == null) throw new NoSuchUserException();

                //return first profile
                var profileLookup = new ProfileLookup();
                profileLookup.id = firstProfileAssociationRecord.Profileid;
                return (await profileRepository.Lookup(profileLookup)).FirstOrDefault();
            } else {
                //lock profile lookup to current user
                cdKeylookup.profileLookup.user = new UserLookup();
                cdKeylookup.profileLookup.user.id = userProfile.Userid;

                Profile result = (await profileRepository.Lookup(cdKeylookup.profileLookup)).FirstOrDefault();

                if(result != null)
                    return result;

                //create new profile for user
                result = new Profile();
                result.Userid = userProfile.Userid;
                result.Nick = cdKeylookup.profileLookup.nick;
                result.Uniquenick = cdKeylookup.profileLookup.uniquenick;
                result.Namespaceid = cdKeylookup.profileLookup.namespaceid ?? 0;

                return await profileRepository.Create(result);
            }

        }

        public async Task<CdKey> LookupCDKeyFromProfile(CdKeyLookup lookup)
        {
            var profile = (await profileRepository.Lookup(lookup.profileLookup)).FirstOrDefault();
            if (profile == null) throw new NoSuchUserException();

            //TODO: clean this up into linq join query
            var profile_cdkey_records = await keyMasterDb.ProfileCdKey.Where(b => b.Profileid == profile.Id).ToListAsync();
            foreach(var record in profile_cdkey_records)
            {
                var cdkeyLookup = new CdKeyLookup();
                cdkeyLookup.Id = record.Cdkeyid;
                var cdkey = (await Lookup(cdkeyLookup)).FirstOrDefault();
                if(cdkey.Gameid == lookup.Gameid)
                {
                    return cdkey;
                }
            }
            return null;
        }
    }
}
