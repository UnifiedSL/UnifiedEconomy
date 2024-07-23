namespace UnifiedEconomy.Database.Config
{
    using System.ComponentModel;
    using System.IO;
    using Exiled.API.Features;

    public class DBConfig
    {

        [Description("Database Id to use")]
        public string DatabaseId { get; set; } = "Json";

        [Description("Connection URI")]
        public string ConnectionURI { get; set; } = Path.Combine(Paths.Exiled, "database.json");

        [Description("Will people with DNT be saved?")]
        public bool DNT { get; set; } = true;
    }
}
