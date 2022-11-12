//Reference: WishInfrastructure
#define DEBUG
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


//WishRaidBlock created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("WishRaidBlock", "Latvish", "0.0.1")]
    [Description("WishRaidBlock")]
    public partial class WishRaidBlock : RustPlugin
    {
        #region ChatCommands.cs
        [ChatCommand("raidblock")]
        private void raidblock(BasePlayer player, string command, string[] args)
        {
            PrintToChat(player, lang.GetMessage("nodamage", this, player.UserIDString)
            .Replace("{starttime}", ShowTime(_raidBlockService.GetStartTime()))
            .Replace("{endtime}", ShowTime(_raidBlockService.GetEndTime())));
            
            if (!permission.UserHasPermission(player.UserIDString, adminPriv))
            return;
            
            if (args.Count() > 0)
            {
                
                
                if (args[0].ToLower() == "activate")
                {
                    _raidBlockService.Enable();
                    PrintToChat(lang.GetMessage("activate", this));
                    return;
                }
                
                if (args[0].ToLower() == "deactivate")
                {
                    _raidBlockService.Disable();
                    PrintToChat(lang.GetMessage("deactivate", this));
                    return;
                }
            }
        }
        #endregion

        #region Events.cs
        object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            try
            {
                if (entity == null) return null;
                if (info == null) return null;
                if (info.InitiatorPlayer == null) return null;
                if (!_raidBlockService.IsOn()) return null;                  // check if on or off
                if (entity is BasePlayer) return null;                          // damage to players ok!!
                if (entity.OwnerID == 0) return null;
                if (entity.OwnerID == info.InitiatorPlayer.userID) return null; // owner can damage own stuff
                //TODO ALLOW TEAM TO DESTROY SHIT
                if (info.InitiatorPlayer != null)
                {
                    
                    if ((bool)Config["RaidBlockInformPlayer"] == true)
                    {
                        if (!_raidBlockService.IsForceActivated())
                        {
                            PrintToChat(info.InitiatorPlayer, lang.GetMessage("nodamage", this, info.InitiatorPlayer.UserIDString)
                            .Replace("{starttime}", ShowTime(_raidBlockService.GetStartTime()))
                            .Replace("{endtime}", ShowTime(_raidBlockService.GetEndTime())));
                        }
                        else
                        {
                            PrintToChat(info.InitiatorPlayer, lang.GetMessage("activated", this, info.InitiatorPlayer.UserIDString));
                        }
                    }
                }
                
                info.damageTypes.ScaleAll(0.0f);                                // no damage
                return false;
            }
            catch (Exception ex)
            {
                PrintError("Error OnEntityTakeDamage: " + ex.Message);
                
            }
            return null;
        }
        #endregion

        #region WishRaidBlock.cs
        const string adminPriv = "Raid block.admin";
        private RaidBlockService _raidBlockService;
        void Init()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Interface.Oxide.LogDebug($"START Init WishRaidBlock");
            
            LoadConfig();
            _raidBlockService = new RaidBlockService(Config);
            LoadRaidBlock();
            SubscribeToEvents();
            
            stopwatch.Stop();
            Interface.Oxide.LogDebug($"END Init WishRaidBlock {stopwatch.ElapsedMilliseconds}ms");
            
        }
        void LoadRaidBlock()
        {
            try
            {
                RegisterMessages();
                
                if ((bool)Config["RaidBlockOn"])
                {
                    PrintWarning("Raid block starts at " + ShowTime(_raidBlockService.GetStartTime()));
                    PrintWarning("Raid block ends at  " + ShowTime(_raidBlockService.GetEndTime()));
                }
                else
                {
                    PrintWarning("Raid block is off.");
                }
                permission.RegisterPermission(adminPriv, this);
                
                PrintWarning(adminPriv + " privilidge is registered.");
                
            }
            catch (Exception ex)
            {
                PrintError($"Error Loaded: {ex.StackTrace}");
            }
            
        }
        protected override void LoadDefaultConfig() // Only called if the config file does not already exist
        {
            Interface.Oxide.LogWarning("Creating a new configuration file.");
            
            Config.Clear();
            
            Config["RaidBlockStart"] = "23:30";           // 8:30 AM
            Config["RaidBlockEnd"] = "12:00 AM";
            Config["RaidBlockOn"] = true;
            Config["RaidBlockInformPlayer"] = true;
            
            SaveConfig();
        }
        private void RegisterMessages()
        {
            MessageRegister messageRegister = new MessageRegister(this);
            messageRegister.Register();
        }
        
        private void SubscribeToEvents()
        {
            Subscribe("OnEntityTakeDamage");
        }
        string ShowTime(object TimeIn)
        {
            return DateTime.Parse(TimeIn.ToString()).ToString("hh:mm tt");
        }
        #endregion

        #region MessageRegister.cs
        public class MessageRegister
        {
            private readonly RustPlugin _plugin;
            private readonly Lang _lang;
            
            public MessageRegister(RustPlugin plugin)
            {
                _plugin = plugin;
                _lang = Interface.Oxide.GetLibrary<Lang>();
            }
            public void Register()
            {
                
                RegisterEnglishMessages();
            }
            
            private void RegisterEnglishMessages()
            {
                _lang.RegisterMessages(new Dictionary<string, string>
                {
                    { "localtime", "Local time is {localtime}." },
                    { "nodamage", "You cannot cause damage between {starttime} and {endtime}." },
                    { "activated", "You cannot cause damage while Raid block is activated." },
                    { "starts", "Raid block starts at {starts}." },
                    { "remains", "Raid block remains on for {remains}." },
                    { "onstatus", "Raid block is ON. It is active  from @ {starttime} until {endtime}" },
                    { "offstatus", "Raid block is {status}. It will NOT become active." },
                    { "duration", "Raid block duration is {duration} minutes." },
                    { "errorstart", "Error, please use 24 hour time format: i.e 08:00 for 8 am. 20:00 for 8pm." },
                    { "errormin", "Error, please enter an integer i.e: 60 for 60 minutes." },
                    { "errorhour", "Error, please enter an integer i.e: 2 for 180 minutes." },
                    { "activate", "- Raid Block is enabled"},
                    { "deactivate", "- Raid Block is disabled"},
                    { "info", "- Raid block {act} inform players when they are unable to damage."},
                    { "help1", "/lset start 8:00 am ~ Set start time for damage control." },
                    { "help2", "/lset minutes 60    ~ Set duration in minutes for damage control."},
                    { "help3", "/lset hours 12      ~ Set duration in hours for damage control."},
                    { "help4", "/lset activate      ~ Force damage control ON, ignore config."},
                    { "help5", "/lset info          ~ Toggle player message, when damage is denied."},
                    { "help6", "/lset deactivate    ~ use configured times."},
                    { "help7", "/lset off           ~ Turn off damage control."},
                    { "help8", "/lset on            ~ Turn on damage control during set times. "},
                    { "help9", "- starts at {starttime} ends at {endtime}. (Server's time)"}
                }, _plugin, "en");
            }
        }
        #endregion

        #region RaidBlockService.cs
        public class RaidBlockService
        {
            private readonly DynamicConfigFile _config;
            
            private bool _isActive;
            public RaidBlockService(DynamicConfigFile config)
            {
                _config = config;
            }
            
            public bool IsOn()
            {
                if (_isActive) return true;
                
                if ((bool)_config["RaidBlockOn"] == false) return false;
                
                return IsOnUsingConfigTime();
            }
            public bool IsForceActivated()
            {
                return _isActive;
            }
            public void Enable()
            {
                _isActive = true;
            }
            public void Disable()
            {
                _isActive = false;
            }
            
            private bool IsOnUsingConfigTime()
            {
                var start = GetStartTime();
                var end = GetEndTime();
                var now = GetLatvianTime().TimeOfDay;
                
                if (start <= end)
                {
                    return now >= start && now <= end;
                }
                return now >= start || now <= end;
            }
            
            public TimeSpan GetStartTime()
            {
                return GetTimeFromConfig("RaidBlockStart");
                
            }
            public TimeSpan GetEndTime()
            {
                return GetTimeFromConfig("RaidBlockEnd");
            }
            
            private TimeSpan GetTimeFromConfig(string key)
            {
                try
                {
                    var time = DateTime.Parse(_config[key].ToString()).TimeOfDay;
                    Interface.Oxide.LogDebug($"Raidblock: Read time from config {time}");
                    return time;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error GetTimeFromConfig {key}. {key}", ex);
                }
            }
            
            private static DateTime GetLatvianTime()
            {
                TimeZoneInfo eeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now, eeZone);
            }
        }
        #endregion

    }

}
