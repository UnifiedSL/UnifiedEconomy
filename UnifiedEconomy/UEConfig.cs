namespace UnifiedEconomy
{
    using Exiled.API.Interfaces;
    using UnifiedEconomy.Database.Config;

    /// <summary>
    /// Config class for UnifiedEconomy.
    /// </summary>
    public class UEConfig : IConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether is Enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether is Debug Enabled.
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets Database Main Config.
        /// </summary>
        public DBConfig Database { get; set; } = new ();

        /// <summary>
        /// Gets or sets main Economy settings.
        /// </summary>
        public EconomyConfig Economy { get; set; } = new();
    }
}
