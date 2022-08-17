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
            Console.WriteLine("WishStatistics Plugin started");
            Subscribe("OnUserChat");
            

        }

    }
}
