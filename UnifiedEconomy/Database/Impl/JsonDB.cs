namespace UnifiedEconomy.Database.Impl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Exiled.API.Features;
    using Newtonsoft.Json;
    using UnifiedEconomy.Helpers;

    public class JsonDB : UEDatabase
    {
        private string filePath;

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
            filePath = connectionString;

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }

            var database = ReadDatabase();
            foreach (var entry in database)
            {
                Database[entry.Id] = entry;
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
                    // Remove player data from cache and JSON file
                    if (Database.ContainsKey(playerId))
                    {
                        Database.Remove(playerId);
                        UEUtils.Debug($"Removed player {playerId} from cache and JSON file.");
                        return true;
                    }
                    else
                    {
                        ServerConsole.AddLog($"[UnifiedEconomy] Player with ID {playerId} not found in the cache for removal.", ConsoleColor.Red);
                        return false;
                    }
                }
                else
                {
                    // Check if the player exists in the cache
                    if (Database.ContainsKey(playerId))
                    {
                        // Player already exists, no need to add or modify the ID
                        UEUtils.Debug($"Player {playerId} already exists in cache. No changes made to ID.");
                        return true;
                    }
                    else
                    {
                        // Player is not in the cache; check the JSON file
                        var fileData = ReadDatabase();
                        var playerData = fileData.Find(pd => pd.Id == playerId);
                        if (playerData != null)
                        {
                            // Load data from the JSON file into the cache
                            Database[playerId] = playerData;
                            UEUtils.Debug($"Loaded player {playerId} data from JSON file into cache.");
                        }
                        else
                        {
                            // New player, create new data (including the ID only for the first time)
                            playerData = new PlayerData
                            {
                                Id = playerId, // ID is set here only once and never changes
                                Balance = UEMain.Singleton.Config.Economy.StartupMoney,
                            };

                            Database[playerId] = playerData;
                            fileData.Add(playerData);
                            WriteDatabase(fileData);
                            UEUtils.Debug($"Added new player {playerId} to JSON file.");
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog($"[UnifiedEconomy] Failed to save user: {ex.Message}", ConsoleColor.DarkRed);
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
                var playerData = fileData.Find(pd => pd.Id == playerId);
                if (playerData != null)
                {
                    // Update cache with data from JSON file
                    Database[playerId] = playerData;
                    UEUtils.Debug($"Loaded player {playerId} data from JSON file into cache.");
                    return playerData;
                }

                ServerConsole.AddLog($"[UnifiedEconomy] Player with ID {playerId} not found in the database.", ConsoleColor.Red);
                return null;
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog($"[UnifiedEconomy] Failed to read user: {ex.Message}", ConsoleColor.DarkRed);
                return null;
            }
        }

        /// <summary>
        /// Updates the user inside the database cache.
        /// </summary>
        /// <param name="player">The player who needs to be updated.</param>
        /// <param name="data">The data to update. The ID field will not be updated.</param>
        /// <returns>If the action was successful.</returns>
        public override bool UpdateUser(Player player, PlayerData data)
        {
            try
            {
                var playerId = player.UserId;

                if (Database.ContainsKey(playerId))
                {
                    // Ensure the ID is not modified
                    data.Id = playerId; // Prevent changing the ID by enforcing the current player ID

                    Database[playerId] = data;
                    WriteDatabase(Database.Values);
                    UEUtils.Debug($"Updated player {playerId} data in cache and JSON file (without changing ID).");
                    return true;
                }
                else
                {
                    ServerConsole.AddLog($"[UnifiedEconomy] Player with ID {playerId} not found in the database.", ConsoleColor.Red);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update user: {ex.Message}");
                return false;
            }
        }

        private List<PlayerData> ReadDatabase()
        {
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<PlayerData>>(json) ?? new List<PlayerData>();
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog($"[UnifiedEconomy] Failed to read database: {ex.Message}", ConsoleColor.DarkRed);
                return new List<PlayerData>();
            }
        }

        private void WriteDatabase(IEnumerable<PlayerData> database)
        {
            try
            {
                var json = JsonConvert.SerializeObject(database, Formatting.Indented);
                File.WriteAllText(filePath, json);
                UEUtils.Debug("Database written to file successfully.");
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog($"[UnifiedEconomy] Failed to write database: {ex.Message}", ConsoleColor.DarkRed);
            }
        }
    }
}