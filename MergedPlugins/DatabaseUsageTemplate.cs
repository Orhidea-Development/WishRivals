//Reference: WishInfrastructure
#define DEBUG
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WishInfrastructure;
using WishInfrastructure.Models;


//DatabaseUsageTemplate created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("DatabaseUsageTemplate", "Latvish", "0.0.1")]
    [Description("DatabaseUsageTemplate")]
    public partial class DatabaseUsageTemplate : RustPlugin
    {
        #region DatabaseUsageTemplate.cs
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
        #endregion

        #region EventListeners\DatabaseSaveEvents.cs
        void OnServerSave()
        {
            Interface.Oxide.LogDebug($"Performing database save");
            Database.SavePlayerDatabase();
        }
        
        void OnUserConnected(IPlayer player)
        {
            if (!Database.IsKnownPlayer(player.Id))
            {
                Database.LoadPlayer(player.Id);
            }
            
            Database.SetPlayerData(player.Id, "name", player.Name);
        }
        #endregion

        #region Models\ConfigFile.cs
        public class ConfigFile
        {
            public DatabaseConfig DatabaseConfig { get; set; }
            
        }
        #endregion

        #region Startup\ConfigSetup.cs
        public class ConfigSetup
        {
            
            
            private readonly RustPlugin _plugin;
            
            public ConfigFile ConfigFile { get; set; }
            
            
            internal ConfigSetup(RustPlugin plugin)
            {
                _plugin = plugin;
                Init();
            }
            
            private void Init()
            {
                ConfigFile = _plugin.Config.ReadObject<ConfigFile>();
            }
            
            internal static object GetDefaultConfig()
            {
                return new ConfigFile()
                {
                    DatabaseConfig = DatabaseConfig.GetDefaultDatabaseConfig(),
                };
            }
            
        }
        #endregion

    }

}
