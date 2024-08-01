namespace UnifiedEconomy
{
    using Exiled.Events.EventArgs.Player;
    using UnifiedEconomy.Helpers.Events;

    public static class EventHandler
    {
        public static void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Verified += OnJoin;
            Exiled.Events.Handlers.Player.Left += OnQuit;

            //Exiled.Events Support
            EventHandlerUtils.AddEventHandlers();
        }

        public static void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Verified -= OnJoin;
            Exiled.Events.Handlers.Player.Left -= OnQuit;
        }

        public static void OnJoin(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC)
            {
                return;
            }

            if (ev.Player.DoNotTrack && !UEMain.Singleton.Config.Database.DNT)
            {
                return;
            }

            UEMain.CurrentDatabase.SaveUser(ev.Player);
        }

        public static void OnQuit(LeftEventArgs ev)
        {
            if (ev.Player.IsNPC)
            {
                return;
            }

            if (ev.Player.DoNotTrack && !UEMain.Singleton.Config.Database.DNT)
            {
                return;
            }

            UEMain.CurrentDatabase.SaveUser(ev.Player, true);
        }
    }
}
