using CommandSystem;
using System.Collections.Generic;
using System;

namespace UnifiedEconomy.Command.Admin
{

    /// <summary>
    /// The main parent command.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class EcoCommand : ParentCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EcoCommand"/> class.
        /// </summary>
        public EcoCommand()
        {
            LoadGeneratedCommands();
        }

        public bool SanitizeResponse => false;

        /// <inheritdoc/>
        public override string Command { get; } = "economy";

        /// <inheritdoc/>
        public override string[] Aliases { get; } = { "eco" };

        /// <inheritdoc/>
        public override string Description { get; } = "Command for the economy";

        /// <inheritdoc/>
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new EcoGiveCommand());
            RegisterCommand(new EcoTakeCommand());
        }

        /// <inheritdoc/>
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Invalid subcommand! Available: give, take";
            return false;
        }
    }
}
