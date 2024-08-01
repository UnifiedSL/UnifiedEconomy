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

        [Description("Prefix for the economy permission, if empty no permission will be required")]
        public string PermissionForAdminCommand { get; set; } = "eco";

        [Description("Check https://github.com/Exiled-Team/EXILED/tree/dev/Exiled.Events/EventArgs/Player and remove EventArgs to get the event you need, check the example")]
        public Dictionary<string, float> EventMoney { get; set; } = new Dictionary<string, float>()
        {
            { "PickingUpItem", 2f },
        };
    }
}
