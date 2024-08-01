namespace UnifiedEconomy.Database
{
    /// <summary>
    /// General class for all the databases.
    /// </summary>
    public class PlayerData
    {
        /// <summary>
        /// Gets or sets the player's balance.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the player's balance.
        /// </summary>
        public float Balance { get; set; }

        public static PlayerData operator +(PlayerData f1, PlayerData f2)
        {
            return new() { Balance = f1.Balance + f2.Balance };
        }

        public static PlayerData operator -(PlayerData f1, PlayerData f2)
        {
            return new() { Balance = f1.Balance - f2.Balance };
        }
    }
}
