namespace LobotJR.Modules
{
    public interface ISystemManager
    {
        /// <summary>
        /// Gets the system with the requested type.
        /// </summary>
        /// <typeparam name="T">The type of system to request.</typeparam>
        /// <returns>The loaded system of the given type, or null if none exists.</returns>
        T Get<T>() where T : class, ISystem;
    }
}
