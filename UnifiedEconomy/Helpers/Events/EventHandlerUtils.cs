namespace UnifiedEconomy.Helpers.Events
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs.Interfaces;
    using Exiled.Events.Features;
    using Exiled.Loader;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnifiedEconomy.Helpers.Extension;

    public static class EventHandlerUtils
    {

        private static readonly List<Tuple<EventInfo, Delegate>> _dynamicHandlers = new List<Tuple<EventInfo, Delegate>>();
        private static bool isHandlerAdded;

        public static void AddEventHandlers()
        {
            var EventsAssembly = Loader.Plugins.FirstOrDefault(x => x.Name == "Exiled.Events");

            if (EventsAssembly == null)
            {
                Log.Warn($"Exiled.Events not found. Skipping AddEventHandlers.");
                return;
            }

            foreach (var eventClass in EventsAssembly.Assembly.GetTypes().Where(x => x.Namespace == "Exiled.Events.Handlers"))
            {
                foreach (PropertyInfo propertyInfo in eventClass.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    Type eventType = propertyInfo.PropertyType;

                    if (propertyInfo.PropertyType == typeof(Event))
                    {
                        continue;
                    }

                    if (eventType.IsGenericType && eventType.GetGenericTypeDefinition() == typeof(Event<>))
                    {
                        Type eventArgsType = eventType.GetGenericArguments().FirstOrDefault();

                        if (eventArgsType != null && typeof(IPlayerEvent).IsAssignableFrom(eventArgsType))
                        {
                            Log.Debug($"{eventArgsType.Name} is Registred");
                            EventInfo eventInfo = eventType.GetEvent("InnerEvent", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            Delegate handler = typeof(EventHandlerUtils)
                            .GetMethod(nameof(EventHandlerUtils.MessageHandler))
                            .MakeGenericMethod(eventInfo.EventHandlerType.GenericTypeArguments)
                            .CreateDelegate(typeof(CustomEventHandler<>)
                            .MakeGenericType(eventInfo.EventHandlerType.GenericTypeArguments));

                            MethodInfo addMethod = eventInfo.GetAddMethod(true);
                            addMethod.Invoke(propertyInfo.GetValue(null), new[] { handler });

                            _dynamicHandlers.Add(new Tuple<EventInfo, Delegate>(eventInfo, handler));
                        }
                    }
                }
            }
        }

        public static void RemoveEventHandlers()
        {
            if (!isHandlerAdded) return;

            for (int i = 0; i < _dynamicHandlers.Count; i++)
            {
                Tuple<EventInfo, Delegate> tuple = _dynamicHandlers[i];
                EventInfo eventInfo = tuple.Item1;
                Delegate handler = tuple.Item2;

                if (eventInfo.DeclaringType != null)
                {
                    MethodInfo removeMethod = eventInfo.DeclaringType.GetMethod($"remove_{eventInfo.Name}", BindingFlags.Instance | BindingFlags.NonPublic);
                    removeMethod.Invoke(null, new object[] { handler });
                }
                else
                {
                    MethodInfo removeMethod = eventInfo.GetRemoveMethod(true);
                    removeMethod.Invoke(null, new[] { handler });
                }
                _dynamicHandlers.Remove(tuple);
            }
            isHandlerAdded = false;
        }

        public static void MessageHandler<T>(T ev) where T : IExiledEvent
        {
            Type eventType = ev.GetType();
            string eventname = eventType.Name.Replace("EventArgs", string.Empty);

            IPlayerEvent playerevent = (IPlayerEvent)ev;

            Log.Debug($"{eventname} is going off");

            if (UEMain.Singleton.Config.Economy.EventMoney.TryGetValue(eventname, out float coins))
            {
                playerevent.Player.AddBalance(coins);
                Log.Debug($"Added balance to {playerevent.Player.Nickname} +{coins}");
            }
        }
    }
}
