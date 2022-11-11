//Reference: WishInfrastructure
#define DEBUG
using Oxide.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WishInfrastructure;
using WishInfrastructure.Models;


//WishLeaderboards created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("WishLeaderboards", "Latvish&Zurius", "0.0.2")]
    [Description("WishLeaderboards")]
    public partial class WishLeaderboards : RustPlugin
    {
        #region WishLeaderboards.cs
        private ConfigSetup _config;
        
        public static DatabaseClient Database { get; set; }
        
        void Init()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Interface.Oxide.LogDebug($"START Init WishLeaderboards");
            _config = new ConfigSetup(this);
            
            SubscribeToEvents();
            
            InitInfrastructure();
            stopwatch.Stop();
            Interface.Oxide.LogDebug($"ÉND Init WishLeaderboards {stopwatch.ElapsedMilliseconds}ms");
            
        }
        
        private void InitInfrastructure()
        {
            Database = new DatabaseClient("WishStats", this, _config.ConfigFile.DatabaseConfig);
        }
        
        private void SubscribeToEvents()
        {
            
        }
        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
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
                    DatabaseConfig = GetDefaultDatabaseConfig(),
                };
            }
            
            private static DatabaseConfig GetDefaultDatabaseConfig()
            {
                return new DatabaseConfig
                {
                    sql_host = "localhost",
                    sql_port = 1234,
                    sql_db = "rust",
                    sql_user = "admin",
                    sql_pass = "password"
                };
            }
        }
        #endregion

    }

}
