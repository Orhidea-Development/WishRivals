using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using WishInfrastructure;

namespace Oxide.Plugins
{
    [Info("DatabaseUsageTemplate", "Latvish", "0.0.1")]
    [Description("DatabaseUsageTemplate")]

    public partial class DatabaseUsageTemplate : RustPlugin
    {
        private ConfigSetup _config;

        public static DatabaseClient Database { get; set; }

        void Init()
        {
            _config = new ConfigSetup(this);

            Database = new DatabaseClient("DatabaseUsageTemplate", this, _config.ConfigFile.DatabaseConfig);

            Database.SetupDatabase();

            Subscribe("OnServerSave");
            Subscribe("OnUserConnected");

        }

        [ChatCommand("savetest")]
        private void SaveTest(BasePlayer player, string wipeday, string[] args)
        {
            Interface.Oxide.LogDebug($"Adding test values {player.UserIDString}");

            Database.SetPlayerData(player.UserIDString.ToString(), "TestDataString", "worked");

            Database.SetPlayerData(player.UserIDString.ToString(), "TestDataInt", Database.GetPlayerDataRaw<int>(player.UserIDString, "TestDataInt") + 1);

            Interface.Oxide.LogDebug($"Performing database save");

            Database.SavePlayerDatabase();
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }

    }
}
