namespace Oxide.Plugins
{
    public partial class WishStatistics
    {
        [ChatCommand("statistics")]
        private void stats(BasePlayer player, string command, string[] args)
        {
            _guiService.ActivateGui(player);
        }

        [ConsoleCommand("stats_close")]
        private void stats_close(ConsoleSystem.Arg arg)
        {
            _guiService.DestroyGui(BasePlayer.FindByID(ulong.Parse(arg.Args[0])));
        }

        
    }
}
