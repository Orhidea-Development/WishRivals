namespace Oxide.Plugins
{
    public partial class WishStatistics
    {
        void OnBigWheelLoss(BigWheelGame wheel, Item scrap)
        {
            Server.Broadcast($" Lost: {scrap.text} owner, {scrap.amount}  amount");
        }
        object OnBigWheelWin(BigWheelGame wheel, Item scrap, BigWheelBettingTerminal terminal, int multiplier)
        {
            Server.Broadcast($" Win: {scrap.text} owner, {scrap.amount}  amount");
            Server.Broadcast($" { Database.TestMethod()}");
            
            return null;
        }

        object CanMoveItem(Item item, PlayerInventory playerLoot, uint targetContainer, int targetSlot, int amount)
        {
            if (item.info.shortname == "scrap")
            {
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
        }
    }
}
