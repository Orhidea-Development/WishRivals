using Oxide.Core;
using WishInfrastructure;

namespace Oxide.Plugins
{
    public class TeamsService
    {
        private readonly RustPlugin _rustPlugin;
        private readonly DatabaseClient _dbClient;

        public TeamsService(RustPlugin rustPlugin, DatabaseClient dbClient)
        {
            _rustPlugin = rustPlugin;
            _dbClient = dbClient;
            RelationshipManager.maxTeamSize = 30;
        }
        //0 nav creatota 1 ir creatota nav leaderis ienacis serveri 2 viss safe
        public int TeamExists(ulong teamId)
        {
            return _dbClient.GetClanDataRaw<int>(teamId.ToString(), "Exists");
        }
        public void InitTeamJoin(ulong playerId, ulong teamId)
        {
            _dbClient.SetPlayerData(playerId.ToString(), "TeamId", teamId.ToString());

        }
        public bool IsCaptain(ulong playerId, ulong teamId)
        {
            return _dbClient.GetClanDataRaw<string>(teamId.ToString(), "CaptainID") == playerId.ToString();

        }
        public void InitTeamCreation(ulong playerId, ulong teamId)
        {
            _dbClient.SetPlayerData(playerId.ToString(), "TeamId", teamId.ToString());
            _dbClient.SetClanData(teamId.ToString(), "CaptainID", playerId.ToString());
            _dbClient.SetClanData(teamId.ToString(), "Exists", 1);
        }
        public ulong GetPlayersTeam(ulong playerId)
        {
            return _dbClient.GetPlayerDataRaw<ulong>(playerId.ToString(), "TeamId");
        }

        public void CreateTeam(ulong playerId, ulong teamId)
        {
            Interface.Oxide.LogDebug($"Creating team with id {teamId}, and leader {playerId}");
            var player = BasePlayer.FindByID(playerId);

            if (player == null)
            {
                Interface.Oxide.LogWarning("Cant find player with id {0}", player.userID);
                return;
            }

            if (PreparePlayerToJoinRequiredTeam(player, teamId))
            {
                return;
            }

            RelationshipManager.PlayerTeam aTeam = RelationshipManager.ServerInstance.CreateTeam();
            aTeam.teamLeader = player.userID;
            aTeam.AddPlayer(player);

            //Domats ka dc role id izmantot ka teamId
            aTeam.teamID = teamId;
            aTeam.teamName = "Team " + player.displayName;

            player.TeamUpdate();

            _dbClient.SetPlayerData(player.userID.ToString(), "TeamId", aTeam.teamID.ToString());
            _dbClient.SetPlayerData(player.userID.ToString(), "HasTeam", 1);

            _dbClient.SetClanData(aTeam.teamID.ToString(), "CaptainID", player.userID.ToString());
            _dbClient.SetClanData(aTeam.teamID.ToString(), "CaptainName", player.displayName);
            _dbClient.SetClanData(aTeam.teamID.ToString(), "exists", 1);


        }


        public void AddPlayer(ulong playerId, ulong teamId)
        {
            var player = BasePlayer.FindByID(playerId);

            if (player == null)
            {
                Interface.Oxide.LogWarning("Cant find player with id {0}", player.userID);
                return;
            }

            if (PreparePlayerToJoinRequiredTeam(player, teamId))
            {
                return;
            }

            var team = RelationshipManager.ServerInstance.FindTeam(teamId);
            if (team == null)
            {
                Interface.Oxide.LogWarning("{0} tried to join team {1} but it does not exist", player.displayName, teamId);
                return;
            }
            team.AddPlayer(player);
            player.TeamUpdate();
        }
        public static bool CanJoinTeam(BasePlayer player, ulong teamId)
        {

            if (player.currentTeam == 0UL)
            {
                return true;
            }
            if (IsAlreadyInRequiredTeam(player, teamId))
            {
                Interface.Oxide.LogWarning("Player already is in the requested team {0}", player.userID);
                return false;

            }
            Interface.Oxide.LogWarning("Player {0} already has a team", player.userID);
            return false;
        }
        private bool PreparePlayerToJoinRequiredTeam(BasePlayer player, ulong teamId)
        {
            if (!CanJoinTeam(player, teamId))
            {
                if (IsAlreadyInRequiredTeam(player, teamId))
                {
                    return true;
                }
                player.ClearTeam();
            }
            return false;
        }
        private static bool IsAlreadyInRequiredTeam(BasePlayer player, ulong teamId)
        {
            return player.currentTeam == teamId;
        }


    }
}
