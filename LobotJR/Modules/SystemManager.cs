using LobotJR.Data;
using LobotJR.Modules.Fishing;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules
{
    /// <summary>
    /// Loads and manages the various systems.
    /// </summary>
    public class SystemManager : ISystemManager
    {
        /// <summary>
        /// Collection of all loaded systems.
        /// </summary>
        public List<ISystem> Systems { get; private set; }
        /// <summary>
        /// Repository manager with references to all repositories for run-time data.
        /// </summary>
        public IRepositoryManager RepositoryManager { get; private set; }
        /// <summary>
        /// Content manager with references to all content data.
        /// </summary>
        public IContentManager ContentManager { get; private set; }


        public SystemManager(IRepositoryManager repositoryManager, IContentManager contentManager)
        {
            Systems = new List<ISystem>();
            RepositoryManager = repositoryManager;
            ContentManager = contentManager;
        }

        /// <summary>
        /// Gets the system with the requested type.
        /// </summary>
        /// <typeparam name="T">The type of system to request.</typeparam>
        /// <returns>The loaded system of the given type, or null if none exists.</returns>
        public T Get<T>() where T : class, ISystem
        {
            return Systems.Where(x => x is T).FirstOrDefault() as T;
        }

        /// <summary>
        /// Loads and initializes all systems.
        /// </summary>
        public void LoadAllSystems()
        {
            Systems.Add(new FishingSystem(ContentManager.FishData, RepositoryManager.Fishers, RepositoryManager.FishingLeaderboard, RepositoryManager.TournamentResults, RepositoryManager.AppSettings));
        }

        /// <summary>
        /// Processes all loaded systems.
        /// </summary>
        /// <param name="broadcasting"></param>
        public void Process(bool broadcasting)
        {
            foreach (var system in Systems)
            {
                system.Process(broadcasting);
            }
        }
    }
}
