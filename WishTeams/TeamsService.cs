using Oxide.Core;
using Oxide.Plugins;
using WishInfrastructure;

namespace WishTeams
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

        public void CreateTeam(ulong playerId, ulong teamId)
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

            RelationshipManager.PlayerTeam aTeam = RelationshipManager.ServerInstance.CreateTeam();
            aTeam.teamLeader = player.userID;
            aTeam.AddPlayer(player);

            //Domats ka dc role id izmantot ka teamId
            aTeam.teamID = teamId;
            aTeam.teamName = "Team " + player.displayName;

            player.TeamUpdate();

            _dbClient.SetPlayerData(player.userID.ToString(), "TeamId", aTeam.teamID.ToString());
            _dbClient.SetClanData(aTeam.teamID.ToString(), "CaptainID", player.userID.ToString());
            _dbClient.SetClanData(aTeam.teamID.ToString(), "CaptainName", player.displayName);


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
