//Reference: WishInfrastructure
#define DEBUG
using Oxide.Core;
using System.Collections.Generic;
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
        public static LeaderboardService LbService { get; set; }
        
        
        void Init()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Interface.Oxide.LogDebug($"START Init WishLeaderboards");
            _config = new ConfigSetup(this);
            
            SubscribeToEvents();
            
            InitInfrastructure();
            
            LbService = new LeaderboardService(Database);
            
            stopwatch.Stop();
            Interface.Oxide.LogDebug($"END Init WishLeaderboards {stopwatch.ElapsedMilliseconds}ms");
            
        }
        
        private void InitInfrastructure()
        {
            Database = new DatabaseClient("WishStats", this, _config.ConfigFile.DatabaseConfig);
            Database.SetupDatabase();
        }
        
        private void SubscribeToEvents()
        {
            Subscribe("OnServerSave");
        }
        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }
        #endregion

        #region EventListeners\ServerEvents.cs
        void OnServerSave()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Interface.Oxide.LogDebug($"START Updating leaderboards");
            
            LbService.UpdateLeaderboards();
            stopwatch.Stop();
            
            Interface.Oxide.LogDebug($"STOP Updating leaderboards {stopwatch.ElapsedMilliseconds}ms");
            
        }
        #endregion

        #region Leaderboard.cs
        public class Leaderboard
        {
            private readonly DatabaseClient _databaseClient;
            private readonly string _leaderboardName;
            private  List<KeyValuePair<string, int>> _playersWithValues;
            
            public Leaderboard(DatabaseClient databaseClient, string leaderboard)
            {
                _databaseClient = databaseClient;
                _leaderboardName = leaderboard;
                Update();
            }
            public List<KeyValuePair<string, int>> GetLeaderboard()
            {
                return _databaseClient.GetLeaderboard<int>(_leaderboardName);
            }
            public void Update()
            {
                _playersWithValues = _databaseClient.GetLeaderboard<int>(_leaderboardName);
            }
        }
        #endregion

        #region LeaderboardService.cs
        public class LeaderboardService
        {
            private List<Leaderboard> _leaderboards;
            private readonly DatabaseClient _databaseClient;
            
            public LeaderboardService(DatabaseClient databaseClient)
            {
                
                _databaseClient = databaseClient;
                RegisterLeaderboards();
            }
            public void UpdateLeaderboards()
            {
                _leaderboards.ForEach(lb => lb.Update());
            }
            public List<Leaderboard> GetLeaderboards()
            {
                return _leaderboards;
            }
            private void RegisterLeaderboards()
            {
                _leaderboards = new List<Leaderboard>()
                {
                    InitLeaderboard("Shots"),
                    InitLeaderboard("Headshots"),
                    InitLeaderboard("Damage"),
                    InitLeaderboard("Hits"),
                    InitLeaderboard("Kills"),
                    InitLeaderboard("ScrapWon"),
                    InitLeaderboard("ScrapLost"),
                };
            }
            private Leaderboard InitLeaderboard(string name)
            {
                return new Leaderboard(_databaseClient, name);
            }
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
