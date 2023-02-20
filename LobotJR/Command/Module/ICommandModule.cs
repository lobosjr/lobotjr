using System.Collections.Generic;

namespace LobotJR.Command.Module
{
    /// <summary>
    /// Handler for push notification events.
    /// </summary>
    /// <param name="userId">The user to push to, or null for public pushes.</param>
    /// <param name="commandResult">The CommandResult object to process.</param>
    public delegate void PushNotificationHandler(string userId, CommandResult commandResult);

    /// <summary>
    /// Modules for organizing and grouping commands.
    /// </summary>
    public interface ICommandModule
    {
        /// <summary>
        /// Event that this module will raise when a push notification needs to be sent.
        /// </summary>
        event PushNotificationHandler PushNotification;

        /// <summary>
        /// The name of the module used to group commands.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A collection containing all commands within this module.
        /// </summary>
        IEnumerable<CommandHandler> Commands { get; }
    }
}
