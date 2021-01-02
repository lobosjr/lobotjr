using LobotJR.Data;
using LobotJR.Modules.Fishing.Model;
using System;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    public class FishingEngine
    {
        private readonly Random Random = new Random();
        private readonly int[] Chances = new int[] { 40, 70, 95, 99, 100 };

        private readonly IRepository<Fisher> Fishers;
        private readonly IRepository<Catch> Leaderboard;

        public TournamentResult Tournament { get; set; }

        /// <summary>
        /// Calculates the exact length, weight, and point value of a fish
        /// being caught.
        /// </summary>
        /// <param name="fisher">The fisher object for the user catching the
        /// fish.</param>
        /// <returns>The catch object with the calculated data values.</returns>
        public Catch CalculateFishSizes(Fisher fisher)
        {
            // get fish data out of table
            var catchData = new Catch();
            catchData.Username = fisher.Username;
            catchData.Fish = fisher.Hooked;

            var weightRange = (catchData.Fish.MaximumWeight - catchData.Fish.MinimumWeight) / 5;
            var lengthRange = (catchData.Fish.MaximumLength - catchData.Fish.MaximumLength) / 5;
            var weightVariance = Random.NextDouble() * weightRange;
            var lengthVariance = Random.NextDouble() * lengthRange;

            var size = Random.NextDouble() * 100;
            var category = Chances.Where(x => size > x).Count();
            catchData.Length = (float)Math.Round(catchData.Fish.MinimumLength + lengthRange * category + lengthVariance, 2);
            catchData.Weight = (float)Math.Round(catchData.Fish.MinimumWeight + weightRange * category + weightVariance, 2);
            catchData.Points = (int)Math.Max(Math.Round(size), 1);

            return catchData;
        }

        /// <summary>
        /// Updates the personal leaderboard with new data if the catch object
        /// would set a new record.
        /// </summary>
        /// <param name="fisher">The fisher object for the user catching the
        /// fish.</param>
        /// <param name="catchData">An object with catch data to use for the
        /// update.</param>
        /// <returns>Whether or not the leaderboard was updated.</returns>
        public bool UpdatePersonalLeaderboard(Fisher fisher, Catch catchData)
        {
            var record = fisher.Records.Where(x => x.Fish.Equals(catchData.Fish)).FirstOrDefault();
            if (record == null || record.Weight < catchData.Weight)
            {
                if (record == null)
                {
                    fisher.Records.Add(catchData);
                }
                else
                {
                    record.CopyFrom(catchData);
                }
                Fishers.Update(fisher);
                Fishers.Commit();
                return true;
            }
            return false;
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
            var record = Leaderboard.Read(x => x.Fish.Equals(catchData.Fish)).FirstOrDefault();
            if (record == null || record.Weight < catchData.Weight)
            {
                if (record == null)
                {
                    Leaderboard.Create(catchData);
                }
                else
                {
                    record.CopyFrom(catchData);
                    Leaderboard.Update(record);
                }
                Leaderboard.Commit();
                return true;
            }
            return false;
        }

        public void CatchFish(string username)
        {
            var fisher = Fishers.Read(x => x.Username.Equals(username)).FirstOrDefault();
            if (fisher == null)
            {
                fisher = new Fisher() { Username = username };
                Fishers.Create(fisher);
                Fishers.Commit();
            }
            var catchData = CalculateFishSizes(fisher);
            if (Tournament != null)
            {
                UpdatePersonalLeaderboard(fisher, catchData);
                UpdateGlobalLeaderboard(catchData);
            }
        }
    }
}
