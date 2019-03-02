using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CoreWeb.Database;
using CoreWeb.Models;

namespace CoreWeb.Repository
{
    public class GroupRepository : IRepository<Group, GroupLookup>
    {
        private GamemasterDBContext gameMasterDb;
        private GroupCacheDatabase groupCacheDatabase;
        private IRepository<Game, GameLookup> gameRepository;
        public GroupRepository(GamemasterDBContext gameMasterDb, IRepository<Game, GameLookup> gameRepository, GroupCacheDatabase gameCacheDatabase)
        {
            this.gameRepository = gameRepository;
            this.gameMasterDb = gameMasterDb;
            this.groupCacheDatabase = gameCacheDatabase;
        }
        public async Task<IEnumerable<Group>> Lookup(GroupLookup lookup)
        {
            if(lookup.id.HasValue)
            {
                var results = await gameMasterDb.Group.Where(b => b.Groupid == lookup.id.Value).ToListAsync();
                return results;
            } else if(lookup.gameid.HasValue)
            {
                var results = await gameMasterDb.Group.Where(b => b.Gameid == lookup.gameid).ToListAsync();
                return results;
            } else
            {
                return await gameMasterDb.Group.ToListAsync();
            }
            return null;
        }
        public Task<bool> Delete(GroupLookup lookup)
        {
            return Task.Run(async () =>
            {
                var groups = (await Lookup(lookup)).ToList();
                foreach (var group in groups)
                {
                    gameMasterDb.Remove<Group>(group);
                }
                var num_modified = await gameMasterDb.SaveChangesAsync();
                return groups.Count > 0 && num_modified > 0;
            });
        }
        public Task<Group> Update(Group model)
        {
            return Task.Run(async () =>
            {
                var entry = gameMasterDb.Update<Group>(model);
                await gameMasterDb.SaveChangesAsync();
                return entry.Entity;
            });
        }
        public async Task<Group> Create(Group model)
        {
            var entry = await gameMasterDb.AddAsync<Group>(model);
            var num_modified = await gameMasterDb.SaveChangesAsync();
            return entry.Entity;
        }
        public async Task SyncToRedis()
        {
            var db = groupCacheDatabase.GetDatabase();
            var lookup = new GroupLookup();
            var groups = await Lookup(lookup);

            groupCacheDatabase.FlushDatabase();

            foreach (var group in groups)
            {
                var gamelookup = new GameLookup();
                gamelookup.id = group.Gameid;
                var game = (await gameRepository.Lookup(gamelookup)).FirstOrDefault();
                if (game == null) continue;
                var group_key = game.Gamename + ":" + group.Groupid + ":";
                db.HashSet(group_key, "gameid", group.Gameid.ToString());
                db.HashSet(group_key, "groupid", group.Groupid.ToString());
                db.HashSet(group_key, "maxwaiting", group.Maxwaiting.ToString());
                db.HashSet(group_key, "hostname", group.Name.ToString());

                var custkey_name = group_key + "custkeys";

                if (group.Other == null || group.Other.Length == 0) continue;

                var keys = group.Other.Substring(1).Split('\\');
                string key = "";
                for(int i=0;i<keys.Length;i++)
                {
                    if(i % 2 != 0)
                    {
                        db.HashSet(custkey_name, key, keys[i]);
                    } else
                    {
                        key = keys[i];
                    }
                }
            }
        }
    }
}
