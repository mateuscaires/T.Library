
namespace T.Entities
{
    public class DBConfig
    {
        public DBConfig()
        {
            ConnectionTimeout = 86400;
        }

        [Alias("Data Source")]
        public string DataSource { get; set; }
        [Alias("Initial Catalog")]
        public string InitialCatalog { get; set; }
        [Alias("User ID")]
        public string UserID { get; set; }
        public string Password { get; set; }
        public int ConnectionTimeout { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(DataSource) || string.IsNullOrWhiteSpace(InitialCatalog) || string.IsNullOrWhiteSpace(UserID) || string.IsNullOrWhiteSpace(Password))
                return false;
            return true;
        }
    }
}
