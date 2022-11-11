using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Diagnostics;

namespace Oxide.Plugins
{

    public partial class WishStatistics
    {
        private List<KeyValuePair<string, int>> _proofOfConceptLeaderboards;
        void OnServerSave()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Interface.Oxide.LogDebug($"START Performing database save WishStatistics");
            Database.SavePlayerDatabase();

            if (_proofOfConceptLeaderboards != null)
            {
                Interface.Oxide.LogDebug($"Proof of concept lb");

                for (int i = 0; i < _proofOfConceptLeaderboards.Count; i++)
                {
                    Interface.Oxide.LogDebug($" {i}) {_proofOfConceptLeaderboards[i].Key} {_proofOfConceptLeaderboards[i].Value} hits");

                }

            }
            _proofOfConceptLeaderboards = Database.GetLeaderboard<int>("Hits");

            stopwatch.Stop();
            Interface.Oxide.LogDebug($"END Performing database save WishStatistics - {stopwatch.ElapsedMilliseconds}ms");
        }

        void OnUserConnected(IPlayer player)
        {
            if (!Database.IsKnownPlayer(player.Id))
            {
                Database.LoadPlayer(player.Id);
            }

            Database.SetPlayerData(player.Id, "name", player.Name);

        }
    }
}
