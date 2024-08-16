namespace UnifiedEconomy.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class UEUtils
    {
        public static void Debug(string message)
        {
            if (!UEMain.Singleton.Config.Debug)
            {
                return;
            }

            ServerConsole.AddLog($"[UnifiedEconomy] {message}.", ConsoleColor.Green);
        }

        public static List<Type> GetDerivedClasses<TBase>()
        {
            return Assembly.GetAssembly(typeof(TBase))
                           .GetTypes()
                           .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(TBase)))
                           .ToList();
        }
    }
}
