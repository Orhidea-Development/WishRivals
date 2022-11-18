using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WishStatistics.Utils
{
    internal class PlayerUtil
    {

        // Uz teams pl
        // Vjg chekot hoodie skin pec skina
        public bool checkHoodie(BasePlayer player)
        {
            var hoodie = (Item)ItemManager.CreateByItemID(1751045826, 1);

            if (isNaked(player))
                return false;

            if (player.inventory.containerWear.FindItemByItemID(1751045826) != null)
                return false;
            else

            return true;
        }

        //Kip uzvelk hoodie un locko slot.
        public void addHoodie(BasePlayer player, String team)
        {
            player.inventory.containerWear.GetSlot(1).LockUnlock(true); 
            Item hoodie = (Item)ItemManager.CreateByItemID(1751045826, 1);
            hoodie.MoveToContainer(player.inventory.containerWear, 1);

        }
        public bool isNaked(BasePlayer player)
        {
            return player.inventory.containerWear.IsEmpty();
        }
    }
}
