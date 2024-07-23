namespace UnifiedEconomy.Command
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnifiedEconomy.Helpers.Extension;

    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class BalanceCommand : ICommand
    {
        public bool SanitizeResponse => false;

        public string Command { get; } = "balance";

        public string[] Aliases { get; } = { };

        public string Description { get; } = "Get your balance.";

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender)
            {
                response = "You cannot execute this command!";
                return false;
            }

            Player player = Player.Get(sender);

            response = UEMain.Singleton.Translation.BalanceCommandResult.Replace("%money%", player.GetPlayerFromDB().Balance.ToString());
            return true;
        }
    }
}
