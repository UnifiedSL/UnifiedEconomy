namespace UnifiedEconomy.Database.Impl
{
    using System;
    using Exiled.API.Features;
    using MySql.Data.MySqlClient;
    using Newtonsoft.Json;

    public class MariaDB : UEDatabase
    {
        private string connectionString;
        private MySqlConnection connection;

        /// <summary>
        /// Gets or sets the database id.
        /// </summary>
        public override string Id { get; set; } = "MariaDB";

        /// <summary>
        /// Connects to the MariaDB database.
        /// </summary>
        /// <param name="connectionString">The connection string to the MariaDB server.</param>
        public override void ConnectDB(string connectionString)
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');

            this.connectionString = $"server={uri.Host};port={uri.Port};database={uri.AbsolutePath.TrimStart('/')};uid={userInfo[0]};pwd={userInfo[1]};";

            connection = new MySqlConnection(this.connectionString);
            connection.Open();

            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS PlayerData (
                    Id VARCHAR(255) PRIMARY KEY,
                    Balance FLOAT
                );";
            using var command = new MySqlCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Saves a user to the MariaDB database.
        /// </summary>
        /// <param name="player">The player that needs to be saved inside the database.</param>
        /// <param name="remove">Whether to remove the player from the database.</param>
        /// <returns>If the action was successful.</returns>
        public override bool SaveUser(Player player, bool remove = false)
        {
            try
            {
                var playerId = player.UserId;

                if (remove)
                {
                    if (Database.ContainsKey(playerId))
                    {
                        Database.Remove(playerId);
                        return true;
                    }
                    else
                    {
                        Log.Warn($"Player with ID {playerId} not found in the cache for removal.");
                        return false;
                    }
                }
                else
                {
                    if (Database.ContainsKey(playerId))
                    {
                        Log.Debug($"Player {playerId} already exists in cache. No changes made.");
                        return true;
                    }
                    else
                    {
                        var selectQuery = "SELECT * FROM PlayerData WHERE Id = @Id;";
                        using var selectCommand = new MySqlCommand(selectQuery, connection);
                        selectCommand.Parameters.AddWithValue("@Id", playerId);
                        using var reader = selectCommand.ExecuteReader();
                        if (reader.Read())
                        {
                            var playerData = new PlayerData
                            {
                                Id = reader.GetString("Id"),
                                Balance = reader.GetFloat("Balance"),
                            };
                            Database[playerId] = playerData;
                            Log.Debug($"Loaded player {playerId} data from MariaDB into cache.");
                        }
                        else
                        {
                            var playerData = new PlayerData
                            {
                                Id = playerId,
                                Balance = UEMain.Singleton.Config.Economy.StartupMoney,
                            };

                            Database[playerId] = playerData;
                            reader.Close(); // Ensure the reader is closed before inserting
                            var insertQuery = "INSERT INTO PlayerData (Id, Balance) VALUES (@Id, @Balance);";
                            using var insertCommand = new MySqlCommand(insertQuery, connection);
                            insertCommand.Parameters.AddWithValue("@Id", playerId);
                            insertCommand.Parameters.AddWithValue("@Balance", playerData.Balance);
                            insertCommand.ExecuteNonQuery();
                            Log.Debug($"Added new player {playerId} to database.");
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save user: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads a user from the MariaDB database.
        /// </summary>
        /// <param name="player">The player that needs to be read from the database.</param>
        /// <returns>The player data.</returns>
        public override PlayerData ReadUser(Player player)
        {
            try
            {
                var playerId = player.UserId;

                if (Database.TryGetValue(playerId, out var data))
                {
                    return data;
                }

                var selectQuery = "SELECT * FROM PlayerData WHERE Id = @Id;";
                using var selectCommand = new MySqlCommand(selectQuery, connection);
                selectCommand.Parameters.AddWithValue("@Id", playerId);
                using var reader = selectCommand.ExecuteReader();
                if (reader.Read())
                {
                    data = new PlayerData
                    {
                        Id = reader.GetString("Id"),
                        Balance = reader.GetFloat("Balance"),
                    };
                    Database[playerId] = data;
                    Log.Debug($"Loaded player {playerId} data from MariaDB into cache.");
                    return data;
                }

                Log.Warn($"Player with ID {playerId} not found in the database.");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates the user inside the MariaDB database.
        /// </summary>
        /// <param name="player">The player who needs to be updated.</param>
        /// <param name="data">The data to update.</param>
        /// <returns>If the action was successful.</returns>
        public override bool UpdateUser(Player player, PlayerData data)
        {
            try
            {
                var playerId = player.UserId;

                var updateQuery = "UPDATE PlayerData SET Balance = @Balance WHERE Id = @Id;";
                using var updateCommand = new MySqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@Id", playerId);
                updateCommand.Parameters.AddWithValue("@Balance", data.Balance);
                var rowsAffected = updateCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Database[playerId] = data;
                    Log.Debug($"Updated player {playerId} data in database and cache.");
                    return true;
                }
                else
                {
                    Log.Warn($"Failed to update player {playerId} data. No documents modified.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update user: {ex.Message}");
                return false;
            }
        }
    }
}
