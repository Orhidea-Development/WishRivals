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

namespace Oxide.Plugins
{
    public partial class WishTeams
    {
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
    }

}