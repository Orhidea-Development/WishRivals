using WishInfrastructure;

namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish&Zurius", "0.0.2")]
    [Description("WishStatistics")]

    public partial class WishStatistics : RustPlugin
    {
        private ConfigSetup _config;

        public static DatabaseClient Database { get; set; }

        void Init()
        {
            _config = new ConfigSetup(this);

            SubscribeToEvents();
            InitInfrastructure();

        }

        private void InitInfrastructure()
        {
            Subscribe("OnServerSave");
            Subscribe("OnUserConnected");
            Database = new DatabaseClient("WishStats","WishStatsClans", this, _config.ConfigFile.DatabaseConfig);
            Database.SetupDatabase();
        }

        private void SubscribeToEvents()
        {
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }



    }
}
