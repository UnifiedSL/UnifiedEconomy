using Exiled.API.Interfaces;
using Exiled.Loader;
using System.Reflection;
using System;
using System.Linq;
using Exiled.API.Features;
using UnifiedEconomy.Helpers.Extension;
using UnifiedEconomy.Database;

namespace UnifiedEconomy.Integration
{
    public static class ScriptedEventsIntegration
    {
        /// <summary>
        /// Gets the plugin representing ScriptedEvents, if it's installed.
        /// </summary>
        public static IPlugin<IConfig> ScriptedPlugin => Loader.Plugins.FirstOrDefault(plugin => plugin.Assembly.GetName().Name == "ScriptedEvents");

        /// <summary>
        /// Gets the <see cref="Assembly"/> representing ScriptedEvents, if it's installed.
        /// </summary>
        public static Assembly ScriptedAssembly => ScriptedPlugin?.Assembly ?? null;

        /// <summary>
        /// Gets a value indicating whether or not this server has Scripted Events on it.
        /// </summary>
        public static bool Enabled => ScriptedAssembly is not null;

        /// <summary>
        /// Gets the <see cref="Type"/> representing the "ApiHelper" class of Scripted Events.
        /// </summary>
        public static Type ApiHelper => ScriptedAssembly?.GetType("ScriptedEvents.API.Features.ApiHelper");

        /// <summary>
        /// Wrapper method to add the action using reflection.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="action">The action's function/return value.</param>
        public static void AddAction(string name, Func<string[], Tuple<bool, string>> action)
        {
            ApiHelper?.GetMethod("RegisterCustomAction").Invoke(null, new object[] { name, action });
        }

        /// <summary>
        /// This method is called in <see cref="MainPlugin.OnEnabled"/> to add the actions to Scripted Events.
        /// </summary>
        public static void AddCustomActions()
        {
            if (!Enabled)
            {
                return;
            }

            Func<string[], Tuple<bool, string>> addMoneyAction = (string[] arguments) =>
            {
                if (arguments.Length < 2)
                {
                    return new(false, "Missing argument: Player or Balance");
                }

                if(Player.TryGet(arguments.ElementAt(0), out Player player) && float.TryParse(arguments.ElementAt(1), out float balance))
                {
                    bool result = player.AddBalance(balance);
                    return new(result, result ? "Successfully add the money" : "An Error Occurred");
                }

                return new(true, string.Empty);
            };

            Func<string[], Tuple<bool, string>> getMoneyAction = (string[] arguments) =>
            {
                if (arguments.Length < 1)
                {
                    return new(false, "Missing argument: Player");
                }

                if (Player.TryGet(arguments.ElementAt(0), out Player player))
                {
                    PlayerData result = player.GetPlayerFromDB();

                    if(result == null)
                    {
                        return new(false, "An Error Occurred: Player is not registered in the database");
                    }

                    return new(true, result.Balance.ToString());
                }

                return new(false, "An Error Occurred: Player not found");
            };

            AddAction("UE_ADDMONEY", addMoneyAction);
            AddAction("UE_MONEY", getMoneyAction);
        }

        /// <summary>
        /// This method is called in <see cref="MainPlugin.OnDisabled"/> to remove the actions from Scripted Events.
        /// </summary>
        public static void UnregisterCustomActions()
        {
            ApiHelper?.GetMethod("UnregisterCustomActions").Invoke(null, new object[] { new[] { "RR_ADDREMARK", "RR_PAUSEREPORT" } });
        }
    }
}
