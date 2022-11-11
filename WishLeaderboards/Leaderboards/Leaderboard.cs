using System.Collections.Generic;
using WishInfrastructure;

namespace Oxide.Plugins
{
    public class Leaderboard
    {
        private readonly DatabaseClient _databaseClient;
        private readonly string _leaderboardName;
        private  List<KeyValuePair<string, int>> _playersWithValues;
        
        protected Leaderboard(DatabaseClient databaseClient, string leaderboard)
        {
            _databaseClient = databaseClient;
            _leaderboardName = leaderboard;
        }
        public List<KeyValuePair<string, int>> GetLeaderboard()
        {
            return _databaseClient.GetLeaderboard<int>(_leaderboardName);

        }
        public void Update()
        {
            _playersWithValues = _databaseClient.GetLeaderboard<int>(_leaderboardName);
        }
    }
}
