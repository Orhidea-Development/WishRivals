//Reference: WishInfrastructure
#define DEBUG
using Melanchall.DryWetMidi.Interaction;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WishInfrastructure;
using WishInfrastructure.Models;
using WishStatistics.Utils;


//WishStatistics created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish&Zurius", "0.0.2")]
    [Description("WishStatistics")]
    public partial class WishStatistics : RustPlugin
    {
        #region WishStatistics.cs
        private ConfigSetup _config;
        
        public static DatabaseClient Database { get; set; }
        
        void Init()
        {
            _config = new ConfigSetup(this);
            
            SubscribeToEvents();
            InitInfrastructure();
            
        }
        
        private void InitInfrastructure()
        {
            Subscribe("OnServerSave");
            Subscribe("OnUserConnected");
            Database = new DatabaseClient("WishStats","WishStatsClans", this, _config.ConfigFile.DatabaseConfig);
            Database.SetupDatabase();
        }
        
        private void SubscribeToEvents()
        {
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");
        }
        
        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }
        #endregion

        #region EventListeners\DatabaseSaveEvents.cs
        void OnServerSave()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Interface.Oxide.LogDebug($"START Performing database save WishStatistics");
            
            Database.SaveDatabase();
            
            stopwatch.Stop();
            Interface.Oxide.LogDebug($"END Performing database save WishStatistics - {stopwatch.ElapsedMilliseconds}ms");
        }
        
        void OnUserConnected(IPlayer player)
        {
            if (!Database.IsKnownPlayer(player.Id))
            {
                Database.LoadPlayer(player.Id);
            }
            
            Database.SetPlayerData(player.Id, "name", player.Name);
            
        }
        #endregion

        #region EventListeners\PlayerEvents.cs
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
        #endregion

        #region Models\ConfigFile.cs
        public class ConfigFile
        {
            public DatabaseConfig DatabaseConfig { get; set; }
            
        }
        #endregion

        #region Startup\ConfigSetup.cs
        public class ConfigSetup
        {
            
            
            private readonly RustPlugin _plugin;
            public ConfigFile ConfigFile { get; set; }
            
            
            internal ConfigSetup(RustPlugin plugin)
            {
                _plugin = plugin;
                Init();
            }
            
            private void Init()
            {
                ConfigFile = _plugin.Config.ReadObject<ConfigFile>();
            }
            
            internal static object GetDefaultConfig()
            {
                return new ConfigFile()
                {
                    DatabaseConfig = GetDefaultDatabaseConfig(),
                };
            }
            
            private static DatabaseConfig GetDefaultDatabaseConfig()
            {
                return new DatabaseConfig
                {
                    sql_host = "localhost",
                    sql_port = 1234,
                    sql_db = "rust",
                    sql_user = "admin",
                    sql_pass = "password"
                };
            }
        }
        #endregion

        #region Utils\PlayerUtil.cs
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
        #endregion

    }

}
