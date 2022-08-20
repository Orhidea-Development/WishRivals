#define DEBUG
using CompanionServer.Handlers;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


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
            
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");
            
            
        }
        #endregion

        #region EventListeners\GamblingEvents.cs
        void OnBigWheelLoss(BigWheelGame wheel, Item scrap)
        {
            Server.Broadcast($" Lost: {scrap.text} owner, {scrap.amount}  amount");
        }
        object OnBigWheelWin(BigWheelGame wheel, Item scrap, BigWheelBettingTerminal terminal, int multiplier)
        {
            
            Server.Broadcast($" Win: {scrap.text} owner, {scrap.amount}  amount");
            
            
            return null;
        }
        object CanLootEntity(BasePlayer player, StorageContainer container)
        {
            if (container is BigWheelBettingTerminal)
            {
                Server.Broadcast($"true");
                Server.Broadcast($"{container.inventory.itemList.Count} item count");
                
                container.inventory.itemList.ForEach(x => x.text = player.displayName);
                
            }
            Puts("CanLootEntity works!");
            return null;
        }
        object CanMoveItem(Item item, PlayerInventory playerLoot, uint targetContainer, int targetSlot, int amount)
        {
            if (item.info.shortname == "scrap")
            {
                Server.Broadcast($" Can move: {playerLoot.baseEntity.displayName} owner,   tag {item.info.gameObject.tag}");
                item.text = playerLoot.baseEntity.displayName;
            }
            return null;
        }
        Item OnItemSplit(Item item, int amount)
        {
            if (item.info.shortname == "scrap")
            {
                Item newItem = ItemManager.CreateByItemID(item.info.itemid, amount, 0uL);
                newItem.text = item.text;
                return newItem;
            }
            return null;
            ;
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
