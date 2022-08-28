using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Plugins
{
    public partial class WishStatistics
    {

        //Scrap Gambling
        void OnBigWheelLoss(BigWheelGame wheel, Item scrap)
        {
            Database.SetPlayerData(scrap.GetOwnerPlayer().UserIDString.ToString(), "ScrapLost", Database.GetPlayerDataRaw<int>(scrap.GetOwnerPlayer().UserIDString, "ScrapLost") + scrap.amount);
            Server.Broadcast($" player: {scrap.GetOwnerPlayer().UserIDString}, {scrap.amount}  amount");
        }
        object OnBigWheelWin(BigWheelGame wheel, Item scrap, BigWheelBettingTerminal terminal, int multiplier)
        {
            Database.SetPlayerData(scrap.GetOwnerPlayer().UserIDString.ToString(), "ScrapWon", Database.GetPlayerDataRaw<int>(scrap.GetOwnerPlayer().UserIDString, "ScrapWon") + scrap.amount);
            Server.Broadcast($" player: {scrap.GetOwnerPlayer().UserIDString}, {scrap.amount}  amount");
            return null;
        }

        object CanMoveItem(Item item, PlayerInventory playerLoot, uint targetContainer, int targetSlot, int amount)
        {
            if (item.info.shortname == "scrap")
            {
                item.text = playerLoot.baseEntity.UserIDString;
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
        //Scrap Gaming End


        //Player Kills & Deaths
        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            if (info == null || player == null || player.IsNpc)
                return;

            Database.SetPlayerData(player.UserIDString.ToString(), "Deaths", Database.GetPlayerDataRaw<int>(player.UserIDString, "Deaths") + 1);

            var attacker = info.InitiatorPlayer;
            if (attacker == null || attacker.IsNpc)
                return;

            Database.SetPlayerData(attacker.UserIDString.ToString(), "Kills", Database.GetPlayerDataRaw<int>(attacker.UserIDString, "Kills") + 1);
        }
        //End Player Kills & Deaths


        //Damage done
        object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (info == null || entity == null || info?.HitEntity == null || entity.IsNpc)
                return null;

            var attacker = info.InitiatorPlayer;
            if (attacker == null || attacker.IsNpc)
                return null;

            Database.SetPlayerData(attacker.UserIDString.ToString(), "Hits", Database.GetPlayerDataRaw<int>(attacker.UserIDString, "Hits") + 1);

            if (!info.HitEntity.IsNpc)
            {
                NextTick(() =>
                {
                    var damages = info?.damageTypes?.Total() ?? 0f;
                    Database.SetPlayerData(attacker.UserIDString.ToString(), "Damage", (int)(Database.GetPlayerDataRaw<int>(attacker.UserIDString, "Damage") + damages));
                });

                if (info.isHeadshot)
                {
                    Database.SetPlayerData(attacker.UserIDString.ToString(), "Headshots", Database.GetPlayerDataRaw<int>(attacker.UserIDString, "Headshots") + 1);
                }
            }


            return null;
        }
        //End Damage done

        //Accuracy
        //Shots:hits 
        void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (projectile == null || player == null)
                return;

            Database.SetPlayerData(player.UserIDString.ToString(), "Shots", Database.GetPlayerDataRaw<int>(player.UserIDString, "Shots") + 1);
        }

        //End Accuracy
    }
}