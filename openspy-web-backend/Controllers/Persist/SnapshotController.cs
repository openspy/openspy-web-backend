using CoreWeb.Models;
using CoreWeb.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWeb.Controllers.Persist
{
    [Route("v1/[controller]")]
    [Authorize(Policy = "Persist")]
    public class SnapshotController
    {
        private IRepository<PlayerProgress, PlayerProgressLookup> playerProgressRepository;
        private IRepository<Leaderboard, LeaderboardLookup> leaderboardRepository;
        private IRepository<Snapshot, SnapshotLookup> snapshotRepository;
        public SnapshotController(IRepository<PlayerProgress, PlayerProgressLookup> playerProgressRepository, IRepository<Leaderboard, LeaderboardLookup> leaderboardRepository, IRepository<Snapshot, SnapshotLookup> snapshotRepository)
        {
            this.leaderboardRepository = leaderboardRepository;
            this.playerProgressRepository = playerProgressRepository;
            this.snapshotRepository = snapshotRepository;
        }
        [HttpPost("LookupPlayerProgress")]
        public Task<IEnumerable<PlayerProgress>> GetPlayerProgress([FromBody] PlayerProgressLookup request)
        {
            return playerProgressRepository.Lookup(request);
        }

        [Authorize(Policy = "PersistWrite")]
        [HttpPost("SetPlayerProgress")]
        public Task<bool> SetPlayerProgress([FromBody] PlayerProgressSet request)
        {
            return ((PlayerProgressRepository)playerProgressRepository).SetData(request);
        }

        [HttpPost("LookupLeaderboard")]
        public async Task<Leaderboard> LookupLeaderboard([FromBody] LeaderboardLookup request)
        {
            return (await leaderboardRepository.Lookup(request)).FirstOrDefault();
        }
        [HttpPost("LookupSnapshot")]
        public async Task<IEnumerable<Snapshot>> LookupSnapshot([FromBody] SnapshotLookup request)
        {
            return await (snapshotRepository.Lookup(request));
        }
        [HttpPost("RequeueSnapshots")]
        public async Task RequeueSnapshots([FromBody] SnapshotLookup request)
        {
            await ((SnapShotRepository)snapshotRepository).RequeueSnapshots(request);
        }
    }
}
