using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Plugins
{
    [Info("Wish Infrastructure", "Latvish", "0.1")]
    [Description("Nice plugin to save stuff for players")]

    public class WishInfrastructure : RustPlugin
    {

        private ConfigSetup _config;
        void Init()
        {
            Puts("starting to save config");
            _config = new ConfigSetup(this);
            //SetupDatabase();
        }
        internal void PrintToConsole(string x)
        {
            Puts(x);

        }
        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(ConfigSetup.GetDefaultConfig(), true);
        }
    }
}
