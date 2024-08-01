namespace UnifiedEconomy.Database.Config
{
    using System.ComponentModel;

    public class EconomyConfig
    {

        /// <summary>
        /// Gets or sets the Money the player will start with.
        /// </summary>
        [Description("Startup Money for the economy")]
        public float StartupMoney { get; set; } = 100.0f;
    }
}
