using LobotJR.Data;

namespace LobotJR.Test.Mocks
{
    public static class DataUtils
    {

        /// <summary>
        /// Clears personal leaderboard records for a specific user.
        /// </summary>
        /// <param name="manager">The data manager to manipulate.</param>
        /// <param name="userId">The id of the user to clear.</param>
        public static void ClearFisherRecords(SqliteRepositoryManager manager, string userId)
        {
            var records = manager.Catches.Read(x => x.UserId.Equals(userId));
            foreach (var record in records)
            {
                manager.Catches.Delete(record);
            }
            manager.Catches.Commit();
        }
    }
}
