using Oxide.Core.Libraries.Covalence;
using System;

namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish", "0.0.2")]
    [Description("WishStatistics")]

    public partial class WishStatistics : RustPlugin
    {
        [PluginReference] WishInfrastructure infrastructure;

        private DatabaseClient Database { get; set; }

        void Init()
        {
            Database = new DatabaseClient("WishStatistics", infrastructure);
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");


        }

    }
}
