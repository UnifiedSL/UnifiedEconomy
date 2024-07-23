namespace UnifiedEconomy.Database.Impl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Exiled.API.Features;
    using Newtonsoft.Json;

    public class JsonDB : UEDatabase
    {
        private string _filePath;

        /// <summary>
        /// Gets or sets the database id.
        /// </summary>
        public override string Id { get; set; } = "Json";

        /// <summary>
        /// Connects to the JSON database.
        /// </summary>
        /// <param name="connectionString">The file path to the JSON database.</param>
        public override void ConnectDB(string connectionString)
        {
            _filePath = connectionString;

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "{}");
            }

            var database = ReadDatabase();
            foreach (var entry in database)
            {
                Database[entry.Key] = entry.Value;
            }
        }


        /// <summary>
        /// Saves a user to the JSON database.
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
                    // Remove player data from cache
                    if (Database.ContainsKey(playerId))
                    {
                        Log.Debug($"Removed player {playerId} from cache.");
                    }
                    else
                    {
                        Log.Warn($"Player with ID {playerId} not found in the cache for removal.");
                    }

                    // Save the updated cache to the JSON file
                    WriteDatabase(Database);
                    return true;
                }
                else
                {
                    // Check if the player exists in the cache
                    if (Database.ContainsKey(playerId))
                    {
                        // Player already exists, no need to add or modify
                        Log.Debug($"Player {playerId} already exists in cache. No changes made.");
                        return true;
                    }
                    else
                    {
                        // Player is not in the cache; check the JSON file
                        var fileData = ReadDatabase();
                        if (fileData.TryGetValue(playerId, out var playerData))
                        {
                            // Load data from the JSON file into the cache
                            Database[playerId] = playerData;
                            Log.Debug($"Loaded player {playerId} data from JSON file into cache.");
                        }
                        else
                        {
                            // New player, create new data
                            playerData = new PlayerData
                            {
                                Balance = 100.0f, // Example balance, replace with actual data
                            };

                            Database[playerId] = playerData;
                            Log.Debug($"Added new player {playerId} to cache.");
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
        /// Reads a user from the JSON database.
        /// </summary>
        /// <param name="player">The player that needs to be read from the database.</param>
        /// <returns>The player data.</returns>
        public override PlayerData ReadUser(Player player)
        {
            try
            {
                var playerId = player.UserId;

                // Check if player data is in the cache
                if (Database.TryGetValue(playerId, out var data))
                {
                    return data;
                }

                // If not in cache, check the JSON file
                var fileData = ReadDatabase();
                if (fileData.TryGetValue(playerId, out data))
                {
                    // Update cache with data from JSON file
                    Database[playerId] = data;
                    Log.Debug($"Loaded player {playerId} data from JSON file into cache.");
                    return data;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates the user inside the database cache.
        /// </summary>
        /// <param name="player">The player who needs to be updated.</param>
        /// <param name="data">The data to update.</param>
        /// <returns>If the action was successful.</returns>
        public override bool UpdateUser(Player player, PlayerData data)
        {
            try
            {
                var playerId = player.UserId;

                if (Database.ContainsKey(playerId))
                {
                    Database[playerId] = data;
                    Log.Debug($"Updated player {playerId} data in cache.");
                    return true;
                }
                else
                {
                    Log.Warn($"Player with ID {playerId} not found in the database.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update user: {ex.Message}");
                return false;
            }
        }

        private Dictionary<string, PlayerData> ReadDatabase()
        {
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(json) ?? new Dictionary<string, PlayerData>();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read database: {ex.Message}");
                return new Dictionary<string, PlayerData>();
            }
        }

        private void WriteDatabase(Dictionary<string, PlayerData> database)
        {
            try
            {
                var json = JsonConvert.SerializeObject(database, Formatting.Indented);
                File.WriteAllText(_filePath, json);
                Log.Debug("Database written to file successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write database: {ex.Message}");
            }
        }
    }
}
