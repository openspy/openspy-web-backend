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
        public SnapshotController(IRepository<PlayerProgress, PlayerProgressLookup> playerProgressRepository, IRepository<Leaderboard, LeaderboardLookup> leaderboardRepository)
        {
            this.leaderboardRepository = leaderboardRepository;
            this.playerProgressRepository = playerProgressRepository;
        }
        [HttpPost("LookupPlayerProgress")]
        public Task<IEnumerable<PlayerProgress>> GetPlayerProgress([FromBody] PlayerProgressLookup request)
        {
            return playerProgressRepository.Lookup(request);
        }

        [HttpPost("LookupLeaderboard")]
        public Task<IEnumerable<Leaderboard>> LookupLeaderboard([FromBody] LeaderboardLookup request)
        {
            return leaderboardRepository.Lookup(request);
        }
    }
}
