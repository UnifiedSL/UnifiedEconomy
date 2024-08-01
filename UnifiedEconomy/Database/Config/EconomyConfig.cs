namespace UnifiedEconomy.Database.Config
{
    using Amazon.Runtime.Internal.Transform;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class EconomyConfig
    {

        /// <summary>
        /// Gets or sets the Money the player will start with.
        /// </summary>
        [Description("Startup Money for the economy")]
        public float StartupMoney { get; set; } = 100.0f;

        public Dictionary<string, float> EventMoney { get; set; } = new Dictionary<string, float>()
        {
            { "PickingUpItem", 2f },
        };
    }
}
