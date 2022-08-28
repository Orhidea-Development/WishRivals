//Reference: WishInfrastructure
#define DEBUG
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WishInfrastructure;
using WishInfrastructure.Models;


//WishStatistics created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("WishStatistics", "Latvish", "0.0.2")]
    [Description("WishStatistics")]
    public partial class WishStatistics : RustPlugin
    {
        #region WishStatistics.cs
        private ConfigSetup _config;
        
        public static DatabaseClient Database { get; set; }
        
        void Init()
        {
            _config = new ConfigSetup(this);
            Subscribe("OnBigWheelWin");
            Subscribe("OnBigWheelLoss");
            Subscribe("CanMoveItem");
            Subscribe("CanLootEntity");
            Subscribe("OnItemSplit");
            
            //Database
            Subscribe("OnServerSave");
            Subscribe("OnUserConnected");
            Database = new DatabaseClient("WishStats", this, _config.ConfigFile.DatabaseConfig);
            Database.SetupDatabase();
            
        }
        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }
        #endregion

        #region EventListeners\DatabaseSaveEvents.cs
        void OnServerSave()
        {
            Interface.Oxide.LogDebug($"Performing database save");
            Database.SavePlayerDatabase();
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

    }

}
