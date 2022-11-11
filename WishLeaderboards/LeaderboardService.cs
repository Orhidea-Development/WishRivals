using System.Collections.Generic;
using WishInfrastructure;

namespace Oxide.Plugins
{
    public class LeaderboardService
    {
        private List<Leaderboard> _leaderboards;
        private readonly DatabaseClient _databaseClient;

        public LeaderboardService(DatabaseClient databaseClient)
        {

            _databaseClient = databaseClient;
            RegisterLeaderboards();
        }
        public void UpdateLeaderboards()
        {
            _leaderboards.ForEach(lb => lb.Update());
        }
        public List<Leaderboard> GetLeaderboards()
        {
            return _leaderboards;
        }
        private void RegisterLeaderboards()
        {
            _leaderboards = new List<Leaderboard>()
            {
                InitLeaderboard("Shots"),
                InitLeaderboard("Headshots"),
                InitLeaderboard("Damage"),
                InitLeaderboard("Hits"),
                InitLeaderboard("Kills"),
                InitLeaderboard("ScrapWon"),
                InitLeaderboard("ScrapLost"),
            };
        }
        private Leaderboard InitLeaderboard(string name)
        {
            return new Leaderboard(_databaseClient, name);
        }
    }
}
