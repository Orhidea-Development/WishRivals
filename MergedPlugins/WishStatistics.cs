#define DEBUG
using Oxide.Core.Libraries.Covalence;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


//WishStatistics created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish", "0.0.2")]
    [Description("WishStatistics")]
    public partial class WishStatistics : RustPlugin
    {
        #region WishStatistics.cs
        void Init()
        {
            Console.WriteLine("WishStatistics Plugin started");
            Subscribe("OnUserChat");
            
            
        }
        #endregion

        #region EventListeners\PlayerConnected.cs
        object OnUserChat(IPlayer player, string message)
        {
            Puts("OnPlayerCommand works!");
            Server.Broadcast($"{player.Name} connected to the server, debug msg from WISH statistics  PlayerConnected");
            
            return null;
        }
        #endregion

    }

}
