using Oxide.Core.Libraries.Covalence;
using System;

namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish", "0.0.2")]
    [Description("WishStatistics")]

    public partial class WishStatistics : RustPlugin
    {
        void Init()
        {
            
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");


        }

    }
}
