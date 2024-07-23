namespace UnifiedEconomy
{
    using Exiled.API.Features;
    using System;
    using System.Collections.Generic;
    using UnifiedEconomy.Database;
    using UnifiedEconomy.Helpers;
    using UnifiedEconomy.Integration;

    /// <summary>
    /// Main Class for UnifiedEconomy.
    /// </summary>
    public class UEMain : Plugin<UEConfig, UETranslation>
    {

        private readonly Dictionary<string, UEDatabase> registeredDatabase = new();
        public static UEDatabase CurrentDatabase = null;
        public static UEMain Singleton { get; private set; }

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Singleton = this;

            foreach (Type type in UEUtils.GetDerivedClasses<UEDatabase>())
            {
                UEDatabase db = (UEDatabase)Activator.CreateInstance(type);

                if (db == null)
                {
                    continue;
                }

                if (registeredDatabase.ContainsKey(db.Id))
                {
                    Log.Warn("The plugin is trying to registering a database with the same Id as another one");
                    continue;
                }

                registeredDatabase.Add(db.Id, db);
            }

            CurrentDatabase = registeredDatabase[Config.Database.DatabaseId];
            CurrentDatabase.ConnectDB(Config.Database.ConnectionURI);

            EventHandler.SubscribeEvents();

            ScriptedEventsIntegration.AddCustomActions();

            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            EventHandler.UnsubscribeEvents();

            ScriptedEventsIntegration.UnregisterCustomActions();

            Singleton = null!;
            CurrentDatabase = null!;
            registeredDatabase.Clear();

            base.OnDisabled();
        }
    }
}
