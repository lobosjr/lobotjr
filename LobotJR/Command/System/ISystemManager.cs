namespace LobotJR.Command.System
{
    public interface ISystemManager
    {
        /// <summary>
        /// Gets the system with the requested type.
        /// </summary>
        /// <typeparam name="T">The type of system to request.</typeparam>
        /// <returns>The loaded system of the given type, or null if none exists.</returns>
        T Get<T>() where T : class, ISystem;

        /// <summary>
        /// Processes all loaded systems.
        /// </summary>
        /// <param name="broadcasting">Whether or not the streamer is currently live.</param>
        void Process(bool broadcasting);
    }
}
