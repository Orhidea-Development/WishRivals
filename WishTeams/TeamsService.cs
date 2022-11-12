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
            RelationshipManager.maxTeamSize = 100;
        }

        public void CreateTeam(ulong playerId, ulong teamId)
        {
            var player = BasePlayer.FindByID(playerId);
            if (!CanJoinTeam(player))
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

            _dbClient.SetPlayerDataSerialized<ulong>(player.userID.ToString(), "TeamId", aTeam.teamID);
        }


        public void AddPlayer(ulong playerId, ulong teamId)
        {
            var player = BasePlayer.FindByID(playerId);

            if (!CanJoinTeam(player))
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
        public static bool CanJoinTeam(BasePlayer player)
        {
            if (player is null)
            {
                Interface.Oxide.LogWarning("Cant find player with id {0}", player.userID);
                return false;
            }
            if (player.currentTeam != 0UL)
            {
                Interface.Oxide.LogWarning("Player {0} already has a team", player.userID);
                return false;
            }
            return true;
        }

    }
}
