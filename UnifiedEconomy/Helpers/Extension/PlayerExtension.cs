namespace UnifiedEconomy.Helpers.Extension
{
    using Exiled.API.Features;
    using UnifiedEconomy.Database;

    /// <summary>
    /// Player extension class to simplify coding.
    /// </summary>
    public static class PlayerExtension
    {
        /// <summary>
        /// Gets the player from the database
        /// </summary>
        /// <param name="player">Player you want to add balance.</param>
        public static PlayerData GetPlayerFromDB(this Player player) => UEMain.CurrentDatabase.ReadUser(player);

        /// <summary>
        /// Save the player and overwrite new data.
        /// (NOTE PLEASE USE ADDBALANCE/REMOVEBALANCE)
        /// </summary>
        /// <param name="player">Player you want to add balance.</param>
        /// <param name="data">New/Updated data to the player.</param>
        /// <returns>if the action was succesfull.</returns>
        public static bool SavePlayer(this Player player, PlayerData data) => UEMain.CurrentDatabase.UpdateUser(player, data);

        /// <summary>
        /// Adds balance to the player.
        /// </summary>
        /// <param name="player">Player you want to add balance.</param>
        /// <param name="add">How much it should be added.</param>
        /// <returns>if the transaction was succesfull.</returns>
        public static bool AddBalance(this Player player, float add)
        {
            PlayerData updated = UEMain.CurrentDatabase.ReadUser(player) + new PlayerData() { Balance = add };
            return player.SavePlayer(updated);
        }

        /// <summary>
        /// Removes balance to the player.
        /// </summary>
        /// <param name="player">Player you want to remove balance.</param>
        /// <param name="remove">How much it should be removed.</param>
        /// <returns>if the transaction was succesfull.</returns>
        public static bool RemoveBalance(this Player player, float remove)
        {
            PlayerData updated = UEMain.CurrentDatabase.ReadUser(player) - new PlayerData() { Balance = remove };
            return player.SavePlayer(updated);
        }
    }
}
