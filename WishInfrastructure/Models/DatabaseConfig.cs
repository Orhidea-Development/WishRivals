namespace WishInfrastructure.Models
{
    public class DatabaseConfig
    {

        public string sql_host { get; set; }
        public int sql_port { get; set; }
        public string sql_db { get; set; }
        public string sql_user { get; set; }
        public string sql_pass { get; set; }

        public static DatabaseConfig GetDefaultDatabaseConfig()
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
}
