using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WishStatistics.Utils;

namespace Oxide.Plugins
{
    public partial class WishStatistics
    {

        //Scrap Gambling
        void OnBigWheelLoss(BigWheelGame wheel, Item scrap)
        {
            Database.SetPlayerData(scrap.text.ToString(), "ScrapLost", Database.GetPlayerDataRaw<int>(scrap.text, "ScrapLost") + scrap.amount);
        }
        object OnBigWheelWin(BigWheelGame wheel, Item scrap, BigWheelBettingTerminal terminal, int multiplier)
        {
            Database.SetPlayerData(scrap.text.ToString(), "ScrapWon", Database.GetPlayerDataRaw<int>(scrap.text, "ScrapWon") + scrap.amount);
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

            if (info == null) return;
            if (player == info.Initiator) return;

            var attacker = info.InitiatorPlayer;

            if (info == null || player == null || player.IsNpc || player.userID == attacker.userID ||
            (attacker.Team == player.Team && attacker.Team != null))
                return;

            Database.SetPlayerData(player.UserIDString.ToString(), "Deaths", Database.GetPlayerDataRaw<int>(player.UserIDString, "Deaths") + 1);

            if (attacker == null || attacker.IsNpc || player.userID == attacker.userID ||
            (attacker.Team == player.Team && attacker.Team != null))
                return;

            Database.SetPlayerData(attacker.UserIDString.ToString(), "Kills", Database.GetPlayerDataRaw<int>(attacker.UserIDString, "Kills") + 1);
        }
        //End Player Kills & Deaths


        //Damage done
        object OnEntityTakeDamage(BasePlayer entity, HitInfo info)
        {
            if (info == null || entity == null || info?.HitEntity == null || entity.IsNpc)
                return null;

            var attacker = info.InitiatorPlayer;
            if (attacker == null || attacker.IsNpc || entity.userID == attacker.userID ||
            (attacker.Team == entity.Team && attacker.Team != null))
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

        //Hoodie Stuff
        void OnPlayerConnected(BasePlayer player)
        {
            Puts("OnPlayerConnected works!");
            
        }
        void OnUserRespawn(IPlayer player)
        {
            Puts("OnUserRespawn works!");
        }
        void OnUserRespawned(IPlayer player)
        {
            Puts("OnUserRespawned works!");
        }
        //End Hoodie Stuff
    }
}