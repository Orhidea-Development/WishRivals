using Oxide.Core.Plugins;
using System;
using VLB;

namespace Oxide.Plugins
{
    public class ConfigSetup
    {


        private readonly WishInfrastructure _plugin;
        public PluginConfig Config { get; set; }


        internal ConfigSetup(WishInfrastructure plugin)
        {
            plugin.PrintToConsole("Init");
            _plugin = plugin;
            Init();
        }

        private void Init()
        {
            _plugin.PrintToConsole("Config init");

            Config = _plugin.Config.ReadObject<PluginConfig>();

            _plugin.PrintToConsole("Config init 1");


        }
        public static PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                sql_host = "localhost",
                sql_port = 1234,
                sql_db = "rust",
                sql_user = "admin",
                sql_pass = "password"
            };
        }
    }
}
