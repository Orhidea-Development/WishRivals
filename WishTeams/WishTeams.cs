using Oxide.Core;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.Entities.Messages;

namespace Oxide.Plugins
{
    [Info("WishTeams", "Latvish", "0.0.1")]
    [Description("WishTeams")]

    public partial class WishTeams : RustPlugin
    {
        #region Class Fields

        [DiscordClient] private DiscordClient _client;
        const string permAllow = "whitelist.allow";

        #endregion
        void Init()
        {
            permission.RegisterPermission(permAllow, this);

        }

        private void OnServerInitialized()
        {
            InitDiscordClient();

        }
        void OnServerShutdown()
        {
            Interface.Oxide.LogDebug($"Shuting down WishTeams");
            _client.Disconnect();

        }
        object CanUserLogin(string name, string id)
        {
            Interface.Oxide.LogDebug($"Checking if user {name} can login");

            return !IsWhitelisted(id) ? "You are not whitelisted" : null;
        }
        bool IsWhitelisted(string id)
        {
            Interface.Oxide.LogDebug($"Checking if user can login {permission.UserHasPermission(id, permAllow)}");

            return permission.UserHasPermission(id, permAllow);
        }
    }
}

