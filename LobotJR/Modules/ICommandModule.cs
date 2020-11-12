using LobotJR.Command;
using System.Collections.Generic;

namespace LobotJR.Modules
{
    /// <summary>
    /// Modules for organizing and grouping commands.
    /// </summary>
    public interface ICommandModule
    {
        /// <summary>
        /// The name of the module used to group commands.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// A collection containing all commands within this module.
        /// </summary>
        IEnumerable<CommandHandler> Commands { get; }
        /// <summary>
        /// A collection of sub modules contained within this module.
        /// </summary>
        IEnumerable<ICommandModule> SubModules { get; }
    }
}
