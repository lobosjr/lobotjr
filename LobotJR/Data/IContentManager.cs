using LobotJR.Command.Model.Fishing;

namespace LobotJR.Data
{
    /// <summary>
    /// Collection of repositories for data access.
    /// </summary>
    public interface IContentManager
    {
        IRepository<Fish> FishData { get; }
    }
}
