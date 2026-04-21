using System;
using MySql.Data.MySqlClient;

namespace VAWCV5Tubod.Connection
{
    internal static class UserLogService
    {
        private const string UnknownUsername = "Unknown User";
        private static bool tableChecked;

        public static void Log(
            string? username,
            string action,
            string entityType,
            int entityId,
            string description)
        {
            try
            {
                InsertLog(username, action, entityType, entityId, description);
            }
            catch (MySqlException ex) when (ex.Number == 1146)
            {
                TryCreateTableAndInsert(username, action, entityType, entityId, description);
            }
            catch
            {
                // Logging must never block the main system action.
            }
        }

        private static void TryCreateTableAndInsert(
            string? username,
            string action,
            string entityType,
            int entityId,
            string description)
        {
            try
            {
                EnsureTableExists();
                InsertLog(username, action, entityType, entityId, description);
            }
            catch
            {
                // Logging must never block the main system action.
            }
        }

        private static void EnsureTableExists()
        {
            if (tableChecked)
            {
                return;
            }

            const string query = """
                CREATE TABLE IF NOT EXISTS user_logs (
                    log_id INT NOT NULL AUTO_INCREMENT,
                    username VARCHAR(100) NOT NULL,
                    action VARCHAR(100) NOT NULL,
                    entity_type VARCHAR(100) NOT NULL,
                    entity_id INT NOT NULL DEFAULT 0,
                    description TEXT NULL,
                    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (log_id)
                );
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using MySqlCommand command = new(query, connection);
            command.ExecuteNonQuery();
            tableChecked = true;
        }

        private static void InsertLog(
            string? username,
            string action,
            string entityType,
            int entityId,
            string description)
        {
            const string query = """
                INSERT INTO user_logs (`username`, `action`, `entity_type`, `entity_id`, `description`, `timestamp`)
                VALUES (@username, @action, @entityType, @entityId, @description, NOW());
                """;

            using MySqlConnection connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using MySqlCommand command = new(query, connection);
            command.Parameters.Add("@username", MySqlDbType.VarChar).Value = NormalizeUsername(username);
            command.Parameters.Add("@action", MySqlDbType.VarChar).Value = action;
            command.Parameters.Add("@entityType", MySqlDbType.VarChar).Value = entityType;
            command.Parameters.Add("@entityId", MySqlDbType.Int32).Value = entityId;
            command.Parameters.Add("@description", MySqlDbType.Text).Value = description;
            command.ExecuteNonQuery();
        }

        private static string NormalizeUsername(string? username)
        {
            string normalizedUsername = username?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return normalizedUsername;
            }

            return string.IsNullOrWhiteSpace(Environment.UserName)
                ? UnknownUsername
                : Environment.UserName;
        }
    }
}
