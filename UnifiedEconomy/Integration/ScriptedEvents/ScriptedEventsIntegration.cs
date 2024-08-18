namespace UnifiedEconomy.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Amazon.Runtime.Internal.Util;
    using Exiled.API.Features;
    using Exiled.Loader;
    using UnifiedEconomy.Database;
    using UnifiedEconomy.Helpers;
    using UnifiedEconomy.Helpers.Extension;

    public static class ScriptedEventsIntegration
    {
        /// <summary>
        /// Gets the Scripted Events API.
        /// </summary>
        internal static Type API => Loader.GetPlugin("ScriptedEvents")?.Assembly?.GetType("ScriptedEvents.API.Features.ApiHelper");

        /// <summary>
        /// Gets a value indicating whether the Scripted Evetns API is available to be used.
        /// </summary>
        internal static bool CanInvoke => API is not null && AddAction is not null && RemoveAction is not null && APIGetPlayersMethod is not null;

        /// <summary>
        /// Gets the MethodInfo for checking if the ScriptModule was loaded.
        /// </summary>
        internal static MethodInfo ModuleLoadedMethod => API?.GetMethod("IsModuleLoaded");

        /// <summary>
        /// Gets a value indicating whether the ScriptModule of Scripted Events is loaded.
        /// </summary>
        internal static bool IsModuleLoaded => (bool)ModuleLoadedMethod.Invoke(null, Array.Empty<object>());

        /// <summary>
        /// Gets the MethodInfo for adding a custom action.
        /// </summary>
        internal static MethodInfo AddAction => API?.GetMethod("RegisterCustomAction");

        /// <summary>
        /// Gets the MethodInfo for removing a custom action.
        /// </summary>
        internal static MethodInfo RemoveAction => API?.GetMethod("UnregisterCustomAction");

        /// <summary>
        /// Gets the MethodInfo for removing a custom action.
        /// </summary>
        internal static MethodInfo APIGetPlayersMethod => API?.GetMethod("GetPlayers");

        /// <summary>
        /// Gets a list of custom actions registered.
        /// </summary>
        internal static List<string> CustomActions { get; } = new();

        /// <summary>
        /// Registers a custom action.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="action">The action implementation.</param>
        /// <remarks>
        /// Action implementation is Func<string[], Tuple<bool, string, object[]>>, where:
        ///
        ///   Tuple<string[], object> - the action input, where:
        ///     string[]   - The input to the action. Usually represented by single word strings, BUT can also include multiple words in one string.
        ///     object     - The script in which the action was ran.
        ///
        ///   Tuple<bool, string, object[]> - the action result, where:
        ///     bool       - Did action execute without any errors.
        ///     string     - The action response to the console when there was an error (can also be used when there was no error).
        ///     object[]   - optional values to return from an action, either strings or Player[]s, anything different will result in an error.
        /// </remarks>
#pragma warning restore SA1629 // Documentation text should end with a period

        public static void RegisterCustomAction(string name, Func<Tuple<string[], object>, Tuple<bool, string, object[]>> action)
        {
            try
            {
                AddAction.Invoke(null, new object[] { name, action });
                CustomActions.Add(name);
            }
            catch (Exception e)
            {
                Log.Error($"{e.Source} - {e.GetType().FullName} error: {e.Message}");
            }
        }

        /// <summary>
        /// Registers custom actions defined in the method.
        /// Used when plugin is enabled.
        /// </summary>
        public static async void RegisterCustomActions()
        {
            if (!CanInvoke)
            {
                ServerConsole.AddLog($"[UnifiedEconomy] SE integration: Scripted Events is either not present or outdated. Ignore this message if you're not using Scripted Events.", ConsoleColor.Red);
                return;
            }

            int tries = 0;
            while (!IsModuleLoaded)
            {
                tries++;
                UEUtils.Debug("ScriptedEvents is not yet loaded: Retrying in 1s");
                await Task.Delay(1000);

                if (tries > 10)
                {
                    ServerConsole.AddLog($"[UnifiedEconomy] ScriptedEvents integration error: ScriptedEvents' ScriptModule has not initialized.", ConsoleColor.Red);
                    return;
                }
            }

            RegisterCustomAction("UE_ADDMONEY", (Tuple<string[], object> input) =>
            {
                string[] arguments = input.Item1;
                object script = input.Item2;

                if (arguments.Length < 2)
                {
                    return new(false, "Missing argument: Player or Balance", null);
                }

                Player player = GetPlayers(arguments[0], script, 1).FirstOrDefault();

                if (player == null)
                {
                    return new(false, "An Error Occurred: Invalid Player", null);
                }

                if (float.TryParse(arguments.ElementAt(1), out float balance))
                {
                    bool result = player.AddBalance(balance);
                    return new(result, result ? "Successfully add the money" : "An Error Occurred", new[] { result.ToString() });
                }

                return new(true, string.Empty, null);
            });

            RegisterCustomAction("UE_MONEY", (Tuple<string[], object> input) =>
            {
                string[] arguments = input.Item1;
                object script = input.Item2;

                if (arguments.Length < 1)
                {
                    return new(false, "Missing argument: Player", null);
                }

                Player player = GetPlayers(arguments[0], script, 1).FirstOrDefault();

                if(player == null)
                {
                    return new(false, "An Error Occurred: Invalid Player", null);
                }

                PlayerData result = player.GetPlayerFromDB();

                if (result == null)
                {
                    return new(false, "An Error Occurred: Player is not registered in the database", null);
                }

                return new(true, "Successfully returned", new[] { result.Balance.ToString() });
            });

            RegisterCustomAction("UE_ADDMONEY_PLAYERS", (Tuple<string[], object> input) =>
            {
                string[] arguments = input.Item1;
                object script = input.Item2;

                if (arguments.Length < 2)
                {
                    return new(false, "Missing argument: Player or Balance", null);
                }

                if (!float.TryParse(arguments.ElementAt(1), out float balance))
                {
                    return new(false, "Missing argument: Not a valid number", null);
                }

                Player[] playerlist = GetPlayers(arguments[0], script, 1);

                int amount = 0;

                foreach(Player player in playerlist.ToList())
                {
                   bool result = player.AddBalance(balance);
                   if (result)
                   {
                        amount++;
                   }
                }

                return new(true, "Successfully returned", new[] { amount.ToString() });
            });
        }

        /// <summary>
        /// Registers custom actions defined previously.
        /// Used when plugin is disabled.
        /// </summary>
        public static void UnregisterCustomActions()
        {
            if (!CanInvoke)
            {
                return;
            }

            foreach (string name in CustomActions)
            {
                RemoveAction.Invoke(null, new object[] { name });
            }
        }

        /// <summary>
        /// Gets the MethodInfo for getting the players from a variable.
        /// </summary>
        /// <param name="input">The input to process.</param>
        /// <param name="script">The script as object.</param>
        /// <param name="max">The number of players to return (-1 for unlimited).</param>
        /// <returns>The list of players.</returns>
        internal static Player[] GetPlayers(string input, object script, int max = -1)
        {
            return (Player[])APIGetPlayersMethod.Invoke(null, new[] { input, script, max });
        }
    }
}
