namespace Demo365.Repository.Services
{
    /// <summary>
    /// no balancing
    /// use a single connection string for all items
    /// </summary>
    public class SimpleDbRouter : IDbRouter
    {
        private readonly string _connectionString;

        public SimpleDbRouter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetConnectionString(DbRouterSettings settings)
        {
            // ignore settings, just return default connection string
            return _connectionString;
        }
    }
}
