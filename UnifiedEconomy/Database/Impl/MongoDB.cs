﻿namespace UnifiedEconomy.Database.Impl
{
    using System;
    using Exiled.API.Features;
    using global::MongoDB.Driver;
    using UnifiedEconomy.Helpers;

    public class MongoDB : UEDatabase
    {
        private IMongoCollection<PlayerData> playerDataCollection;

        /// <summary>
        /// Gets or sets the database id.
        /// </summary>
        public override string Id { get; set; } = "MongoDB";

        /// <summary>
        /// Connects to the MongoDB database.
        /// </summary>
        /// <param name="connectionString">The connection string to the MongoDB server.</param>
        public override void ConnectDB(string connectionString)
        {
            var mongoUrl = new MongoUrl(connectionString);
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(mongoUrl.DatabaseName);
            playerDataCollection = database.GetCollection<PlayerData>("PlayerData");
        }

        /// <summary>
        /// Saves a user to the MongoDB database.
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
                    // Remove player data from cache and database
                    if (Database.ContainsKey(playerId))
                    {
                        Database.Remove(playerId);
                        UEUtils.Debug($"Removed player {playerId} from cache");
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
                        // Player already exists, no need to add or modify
                        UEUtils.Debug($"Player {playerId} already exists in cache. No changes made.");
                        return true;
                    }
                    else
                    {
                        // Player is not in the cache; check the database
                        var playerData = playerDataCollection.Find(pd => pd.Id == playerId).FirstOrDefault();
                        if (playerData != null)
                        {
                            // Load data from the database into the cache
                            Database[playerId] = playerData;
                            UEUtils.Debug($"Loaded player {playerId} data from MongoDB into cache.");
                        }
                        else
                        {
                            // New player, create new data
                            playerData = new PlayerData
                            {
                                Id = playerId,
                                Balance = UEMain.Singleton.Config.Economy.StartupMoney,
                            };

                            Database[playerId] = playerData;
                            playerDataCollection.InsertOne(playerData);
                            UEUtils.Debug($"Added new player {playerId} to database.");
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
        /// Reads a user from the MongoDB database.
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

                // If not in cache, check the MongoDB
                data = playerDataCollection.Find(pd => pd.Id == playerId).FirstOrDefault();
                if (data != null)
                {
                    // Update cache with data from MongoDB
                    Database[playerId] = data;
                    UEUtils.Debug($"Loaded player {playerId} data from MongoDB into cache.");
                    return data;
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
        /// Updates the user inside the MongoDB database.
        /// </summary>
        /// <param name="player">The player who needs to be updated.</param>
        /// <param name="data">The data to update.</param>
        /// <returns>If the action was successful.</returns>
        public override bool UpdateUser(Player player, PlayerData data)
        {
            try
            {
                var playerId = player.UserId;

                // Ensure the data has the correct ID
                data.Id = playerId;

                var updateResult = playerDataCollection.ReplaceOne(
                    pd => pd.Id == playerId,
                    data,
                    new ReplaceOptions { IsUpsert = true });

                if (updateResult.MatchedCount > 0 || updateResult.UpsertedId != null)
                {
                    Database[playerId] = data;
                    UEUtils.Debug($"Updated player {playerId} data in database and cache.");
                    return true;
                }
                else
                {
                    ServerConsole.AddLog($"[UnifiedEconomy] Failed to update player {playerId} data. No documents modified.", ConsoleColor.Red);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog($"[UnifiedEconomy] Failed to update user: {ex.Message}", ConsoleColor.DarkRed);
                return false;
            }
        }
    }
}
