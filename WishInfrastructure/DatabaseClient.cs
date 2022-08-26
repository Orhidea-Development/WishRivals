namespace Oxide.Plugins
{
    public class DatabaseClient
    {
        private string _tableName;
        private readonly WishInfrastructure _plugin;

        public DatabaseClient(string tableName, WishInfrastructure plugin)
        {
            _tableName = tableName;
            _plugin = plugin;
        }

        public string TestMethod()
        {
            return _tableName;
        }
    }
}
