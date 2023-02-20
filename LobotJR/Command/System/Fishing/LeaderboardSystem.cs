using LobotJR.Command.Model.Fishing;
using LobotJR.Data;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Command.System.Fishing
{
    /// <summary>
    /// Runs the tournament logic for the fishing system.
    /// </summary>
    public class LeaderboardSystem : ISystem
    {
        private readonly IRepository<LeaderboardEntry> Leaderboard;
        private readonly IRepository<Catch> PersonalLeaderboard;

        /// <summary>
        /// Event handler for events related to the leaderboard.
        /// </summary>
        /// <param name="catchData">The catch data the leaderboard was updated with.</param>
        public delegate void LeaderboardEventHandler(LeaderboardEntry catchData);

        /// <summary>
        /// Event fired when a user's catch sets a new record.
        /// </summary>
        public event LeaderboardEventHandler NewGlobalRecord;

        public LeaderboardSystem(
            IRepository<Catch> personalLeaderboard,
            IRepository<LeaderboardEntry> leaderboard)
        {
            PersonalLeaderboard = personalLeaderboard;
            Leaderboard = leaderboard;
        }

        /// <summary>
        /// Gets the global leaderboard records.
        /// </summary>
        /// <returns>A collection of catch data containing the largest catch of
        /// each fish.</returns>
        public IEnumerable<LeaderboardEntry> GetLeaderboard()
        {
            return Leaderboard.Read();
        }

        /// <summary>
        /// Gets the personal leaderboard for a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns>A collection of records for the user.</returns>
        public IEnumerable<Catch> GetPersonalLeaderboard(string userId)
        {
            return PersonalLeaderboard.Read(x => x.UserId.Equals(userId)).OrderBy(x => x.FishId);
        }

        /// <summary>
        /// Gets the personal leaderboard for a user.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns>A collection of records for the user.</returns>
        public Catch GetUserRecordForFish(string userId, Fish fish)
        {
            return PersonalLeaderboard.Read(x => x.UserId.Equals(userId) && x.Fish.Equals(fish)).FirstOrDefault();
        }

        /// <summary>
        /// Updates the personal leaderboard with new data if the catch object
        /// would set a new record.
        /// </summary>
        /// <param name="userId">The user id of the user catching the
        /// fish.</param>
        /// <param name="catchData">An object with catch data to use for the
        /// update.</param>
        /// <returns>Whether or not the leaderboard was updated.</returns>
        public bool UpdatePersonalLeaderboard(string userId, Catch catchData)
        {
            if (userId == null || catchData == null)
            {
                return false;
            }

            var record = PersonalLeaderboard.Read(x => x.UserId.Equals(userId) && x.Fish.Equals(catchData.Fish)).FirstOrDefault();
            if (record == null || record.Weight < catchData.Weight)
            {
                if (record == null)
                {
                    PersonalLeaderboard.Create(catchData);
                }
                else
                {
                    record.CopyFrom(catchData);
                    PersonalLeaderboard.Update(record);
                }
                PersonalLeaderboard.Commit();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a fish from a user's records.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="index">The index of the fish to remove.</param>
        public void DeleteFish(string userId, int index)
        {
            if (userId != null)
            {
                var records = PersonalLeaderboard.Read(x => x.UserId.Equals(userId)).OrderBy(x => x.FishId);
                if (index >= 0 && records.Count() > index)
                {
                    var record = records.ElementAt(index);
                    PersonalLeaderboard.Delete(record);
                    PersonalLeaderboard.Commit();
                }
            }
        }

        /// <summary>
        /// Updates the global leaderboard with new data if the catch object
        /// would set a new record.
        /// </summary>
        /// <param name="catchData">An object with catch data to use for the
        /// update.</param>
        /// <returns>Whether or not the leaderboard was updated.</returns>
        public bool UpdateGlobalLeaderboard(Catch catchData)
        {
            if (catchData == null)
            {
                return false;
            }

            var entry = new LeaderboardEntry()
            {
                Fish = catchData.Fish,
                Length = catchData.Length,
                Weight = catchData.Weight,
                UserId = catchData.UserId
            };
            var record = Leaderboard.Read(x => x.Fish.Equals(catchData.Fish)).FirstOrDefault();
            if (record == null || record.Weight < catchData.Weight)
            {
                if (record == null)
                {
                    Leaderboard.Create(entry);
                }
                else
                {
                    record.CopyFrom(entry);
                    Leaderboard.Update(record);
                }
                Leaderboard.Commit();
                NewGlobalRecord?.Invoke(entry);
                return true;
            }
            return false;
        }

        public void Process(bool broadcasting)
        {
        }
    }
}
