using Oxide.Core;

namespace Oxide.Plugins
{
    public partial class WishTeams
    {
        object CanUserLogin(string name, string id)
        {
            Interface.Oxide.LogDebug($"Checking if user {name} can login");
            if (!IsWhitelisted(id))
            {
                return "You are not whitelisted";
            }
            var userId = ulong.Parse(id);
            var teamId = _teamsService.GetPlayersTeam(userId);
            if (_teamsService.TeamExists(teamId) != 2)
            {
                if (!_teamsService.IsCaptain(userId, teamId))
                {
                    return "Ask your leader to join server first";
                }
            }
            return null;
        }
        bool IsWhitelisted(string id)
        {
            Interface.Oxide.LogDebug($"Checking if user can login {permission.UserHasPermission(id, permAllow)}");
            return permission.UserHasPermission(id, permAllow);
        }
        void Whitelist(string id)
        {
            permission.GrantUserPermission(id, permAllow, this);
        }
    }
}
