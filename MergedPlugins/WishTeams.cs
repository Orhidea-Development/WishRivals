//Reference: WishInfrastructure
#define DEBUG
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities;
using Oxide.Ext.Discord.Entities.Gatway;
using Oxide.Ext.Discord.Entities.Gatway.Events;
using Oxide.Ext.Discord.Entities.Messages;
using Oxide.Ext.Discord.Libraries.Command;
using System.Collections.Generic;
using System.Linq;
using WishInfrastructure;


//WishTeams created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("WishTeams", "Latvish", "0.0.1")]
    [Description("WishTeams")]
    public partial class WishTeams : RustPlugin
    {
        #region DiscordHook.cs
        private const string _apiToken = "MTA0NDY1NzUyODA1Njg0MDI5Mg.GRYxs_.zewj6IbADCy_EMxjWxlTFZfRc_PhvBNruGrJw0";
        private readonly DiscordCommand _dcCommands = Interface.Oxide.GetLibrary<DiscordCommand>();
        
        private void InitDiscordClient()
        {
            var settings = new DiscordSettings()
            {
                ApiToken = _apiToken,
                LogLevel = Ext.Discord.Logging.DiscordLogLevel.Debug,
            };
            settings.Intents |= GatewayIntents.GuildMessages;
            settings.Intents |= GatewayIntents.DirectMessages;
            settings.Intents |= GatewayIntents.Guilds;
            settings.Intents |= GatewayIntents.GuildIntegrations;
            settings.Intents |= GatewayIntents.GuildPresences;
            settings.Intents |= GatewayIntents.GuildMembers;
            settings.Intents |= GatewayIntents.GuildMessageTyping;
            settings.Intents |= GatewayIntents.GuildWebhooks;
            settings.Intents |= GatewayIntents.DirectMessageTyping;
            
            
            RegisterDiscordLangCommand(nameof(RegisterCommand), LangKeys.RegisterCommand, true, true, null
            //new List<Snowflake>() { (Snowflake)897528550083665985 }
            );
            _client.Connect(settings);
            
        }
        
        //private void RegisterCommands()
        //{
            
        //}
        
        [HookMethod(DiscordExtHooks.OnDiscordGatewayReady)]
        public void OnDiscordGatewayReady(GatewayReadyEvent ready)
        {
            Puts($"Bot connected to:{ready.Guilds.FirstOrDefault().Value.Name}");
        }
        
        private void RegisterCommand(DiscordMessage message, string cmd, string[] args)
        {
            Interface.Oxide.LogDebug($"Recieved a command1");
            
            message.Author.SendDirectMessage(_client, $"You have been whitelisted {args[0]}");
            permission.GrantUserPermission(args[0], permAllow, this);
        }
        
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [LangKeys.RegisterCommand] = "register",
                
            }, this);
        }
        public void RegisterDiscordLangCommand(string command, string langKey, bool direct, bool guild, List<Snowflake> allowedChannels)
        {
            if (direct)
            {
                _dcCommands.AddDirectMessageLocalizedCommand(langKey, this, command);
            }
            
            if (guild)
            {
                // _dcCommands.AddGuildCommand("register", this, new List<Snowflake>() { (Snowflake)897528550083665985}, command);
                _dcCommands.AddGuildLocalizedCommand(langKey, this, allowedChannels, command);
            }
        }
        private static class LangKeys
        {
            public const string RegisterCommand = nameof(RegisterCommand);
        }
        #endregion

        #region WishTeams.cs
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
        #endregion

        #region TeamsService.cs
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
        #endregion

    }

}
