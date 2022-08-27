using WishInfrastructure;

namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish", "0.0.2")]
    [Description("WishStatistics")]

    public partial class WishStatistics : RustPlugin
    {
        private ConfigSetup _config;

        public static DatabaseClient Database { get; set; }

        void Init()
        {
            _config = new ConfigSetup(this);
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");
            Database = new DatabaseClient("WishStats", this, _config.ConfigFile.DatabaseConfig);

        }
        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }

    }
}
