using Oxide.Core;
using System.Diagnostics;
using WishInfrastructure;


namespace Oxide.Plugins
{
    [Info("WishLeaderboards", "Latvish&Zurius", "0.0.2")]
    [Description("WishLeaderboards")]
    public partial class WishLeaderboards : RustPlugin
    {
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
    }
}
