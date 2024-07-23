namespace UnifiedEconomy.Database
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;

    /// <summary>
    /// Base class for the databases.
    /// </summary>
    public abstract class UEDatabase
    {
        /// <summary>
        /// Gets the list of current Item Managers.
        /// </summary>
        public static Dictionary<string, PlayerData> Database { get; } = new();

        /// <summary>
        /// Gets or sets database id.
        /// </summary>
        public abstract string Id { get; set; }

        /// <summary>
        /// Base for defining how the database should connect or how.
        /// </summary>
        /// <param name="connectionString">if supported an URI</param>
        public virtual void ConnectDB(string connectionString)
        {
        }

        /// <summary>
        /// Save a user inside the database.
        /// </summary>
        /// <param name="player">The player that needs to be saved inside the database</param>
        /// <returns>if the action was done.</returns>
        public virtual bool SaveUser(Player player, bool remove = false)
        {
            return false;
        }

        /// <summary>
        /// Save a user inside the database.
        /// </summary>
        /// <param name="player">The player that needs to be saved inside the database</param>
        /// <returns>if the action was done.</returns>
        public virtual PlayerData ReadUser(Player player)
        {
            return Database[player.UserId];
        }

        /// <summary>
        /// Updates the user inside the database cache
        /// </summary>
        /// <param name="player">player who needs to be updated</param>
        /// <param name="data">data to add</param>
        /// <returns>if the action was done.</returns>
        public virtual bool UpdateUser(Player player, PlayerData data)
        {
            return false;
        }
    }
}
