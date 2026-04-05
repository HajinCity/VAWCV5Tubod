using MySql.Data.MySqlClient;

namespace VAWCV5Tubod.Connection
{
    internal static class DbConnectionFactory
    {
        private const string ConnectionStringEnvironmentVariable = "VAWC_DB_CONNECTION_STRING";
        private const string DefaultConnectionString =
            "server=localhost;port=3307;database=vawcmanagementsystem;uid=root;pwd=3rystAl4o8#12;";

        public static MySqlConnection CreateConnection()
        {
            string? connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = DefaultConnectionString;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No database connection string is configured. Set VAWC_DB_CONNECTION_STRING or update DbConnectionFactory.cs.");
            }

            return new MySqlConnection(connectionString);
        }
    }
}
