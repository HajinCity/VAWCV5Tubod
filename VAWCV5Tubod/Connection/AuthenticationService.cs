using MySql.Data.MySqlClient;
using VAWCV5Tubod.Domain;

namespace VAWCV5Tubod.Connection
{
    internal sealed class AuthenticationService
    {
        public async Task<Users?> AuthenticateAsync(string username, string password)
        {
            string normalizedUsername = username.Trim();

            if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            const string query = """
                SELECT userId, username, password, firstname, middlename, lastname, position
                FROM users
                WHERE username = @username
                LIMIT 1;
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new(query, connection);
            command.Parameters.Add("@username", MySqlDbType.VarChar).Value = normalizedUsername;

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            string storedPassword = reader["password"]?.ToString() ?? string.Empty;

            if (!PasswordSecurity.Matches(password, storedPassword))
            {
                return null;
            }

            return new Users
            {
                UserId = Convert.ToInt32(reader["userId"]),
                Username = reader["username"].ToString() ?? "",
                FirstName = reader["firstname"].ToString() ?? "",
                MiddleName = reader["middlename"].ToString() ?? "",
                LastName = reader["lastname"].ToString() ?? "",
                Position = reader["position"].ToString() ?? ""
            };
        }

        public async Task<Users?> GetRememberedUserAsync(int userId, string username)
        {
            string normalizedUsername = username.Trim();

            if (userId <= 0 || string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return null;
            }

            const string query = """
                SELECT userId, username, firstname, middlename, lastname, position
                FROM users
                WHERE userId = @userId AND username = @username
                LIMIT 1;
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            using MySqlCommand command = new(query, connection);
            command.Parameters.Add("@userId", MySqlDbType.Int32).Value = userId;
            command.Parameters.Add("@username", MySqlDbType.VarChar).Value = normalizedUsername;

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Users
            {
                UserId = Convert.ToInt32(reader["userId"]),
                Username = reader["username"].ToString() ?? "",
                FirstName = reader["firstname"].ToString() ?? "",
                MiddleName = reader["middlename"].ToString() ?? "",
                LastName = reader["lastname"].ToString() ?? "",
                Position = reader["position"].ToString() ?? ""
            };
        }
    }
}
