namespace UnifiedEconomy.Command.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using UnifiedEconomy.Helpers.Extension;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class EcoGiveCommand : ICommand
    {
        public bool SanitizeResponse => false;

        /// <inheritdoc/>
        public string Command { get; } = "give";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "g" };

        /// <inheritdoc/>
        public string Description { get; } = "Gives the specified player/s the amount of money.";

        public string Permission { get; } = string.IsNullOrEmpty(UEMain.Singleton.Config.Economy.PermissionForAdminCommand) ? string.Empty : UEMain.Singleton.Config.Economy.PermissionForAdminCommand + ".give";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!string.IsNullOrEmpty(Permission) && !Player.Get(sender).CheckPermission(Permission))
            {
                response = "You don't have the required permission!";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: eco give <PlayerID/*/me> <money>";
                return false;
            }

            int playerId = -1;

            if (arguments.At(0) != "*" &&
                arguments.At(0) != "me" &&
                !int.TryParse(arguments.At(0), out playerId))
            {
                response = "You need to input:\n - A valid paramater (*,me,PlayerId)\n - A number";
                return false;
            }

            if (!float.TryParse(arguments.At(1), out float duration))
            {
                response = "You need to input:\n - A valid paramater (*,me,PlayerId)\n - A number";
                return false;
            }

            List<Player> players = new();

            if (playerId == -1 && arguments.At(0) == "*")
            {
                players = Player.List.Where(player => !player.IsNPC && player.IsAlive).ToList();
            }
            else if (playerId == -1 && arguments.At(0) == "me")
            {
                players.Add(Player.Get(sender));
            }
            else
            {
                players.Add(Player.Get(playerId));
            }

            if (players.FirstOrDefault() is null)
            {
                response = "Player is not online!";
                return false;
            }

            players.ForEach(player => player.AddBalance(duration));

            response = $"You have added {duration}$ to {(players.Count == 1 ? players.FirstOrDefault().Nickname : (players.Count + " players"))}!";
            return true;
        }
    }
}
