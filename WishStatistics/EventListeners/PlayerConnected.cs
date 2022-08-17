using Oxide.Core.Libraries.Covalence;
using System;

namespace Oxide.Plugins
{
    public partial class WishStatistics
    {
        object OnUserChat(IPlayer player, string message)
        {
            Puts("OnPlayerCommand works!");
            Server.Broadcast($"{player.Name} connected to the server, debug msg from WISH statistics  PlayerConnected");

            return null;
        }

    }
}
