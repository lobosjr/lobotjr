using LobotJR.Data;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Runs the logic for the fishing system.
    /// </summary>
    public class FishingSystem : ISystem
    {
        private readonly Random Random = new Random();
        private readonly int[] Chances = new int[] { 40, 70, 95, 99, 100 };

        private readonly IRepository<Fish> FishData;
        private readonly IRepository<Fisher> Fishers;
        private readonly IRepository<Catch> Leaderboard;

        private readonly AppSettings Settings;
        /// <summary>
        /// Event handler for events related to a specific user.
        /// </summary>
        /// <param name="fisher">The fisher object for the user.</param>
        public delegate void FisherEventHandler(Fisher fisher);
        /// <summary>
        /// Event handler for events related to the leaderboard.
        /// </summary>
        /// <param name="catchData">The catch data the leaderboard was updated with.</param>
        public delegate void LeaderboardEventHandler(Catch catchData);

        /// <summary>
        /// Event fired when a user hooks a fish.
        /// </summary>
        public event FisherEventHandler FishHooked;
        /// <summary>
        /// Event fired when a user's hooked fish gets away.
        /// </summary>
        public event FisherEventHandler FishGotAway;
        /// <summary>
        /// Event fired when a user's catch sets a new record.
        /// </summary>
        public event LeaderboardEventHandler NewGlobalRecord;

        /// <summary>
        /// The tournament system that manages fishing tournaments.
        /// </summary>
        public TournamentSystem Tournament { get; set; }

        public FishingSystem(
            IRepository<Fish> fishData,
            IRepository<Fisher> fishers,
            IRepository<Catch> leaderboard,
            IRepository<TournamentResult> tournamentResults,
            IRepository<AppSettings> appSettings)
        {
            FishData = fishData;
            Fishers = fishers;
            Leaderboard = leaderboard;

            Settings = appSettings.Read().First();
            Tournament = new TournamentSystem(fishers, tournamentResults, appSettings);
        }

        private void OnFishHooked(Fisher fisher)
        {
            if (FishHooked != null)
            {
                FishHooked(fisher);
            }
        }

        private void OnFishGotAway(Fisher fisher)
        {
            if (FishGotAway!= null)
            {
                FishGotAway(fisher);
            }
        }

        private void OnNewGlobalRecord(Catch catchData)
        {
            if (NewGlobalRecord != null)
            {
                NewGlobalRecord(catchData);
            }
        }

        /// <summary>
        /// Gets a fisher object for a given user id.
        /// </summary>
        /// <param name="userId">The twitch id for the user.</param>
        /// <returns>The fisher object for the user.</returns>
        public Fisher GetFisherById(string userId)
        {
            return Fishers.Read(x => x.UserId.Equals(userId)).FirstOrDefault();
        }

        public Fisher CreateFisher(string userId)
        {
            var fisher = new Fisher()
            {
                UserId = userId,
                Records = new List<Catch>()
            };
            Fishers.Create(fisher);
            Fishers.Commit();
            return fisher;
        }

        /// <summary>
        /// Gets the global leaderboard records.
        /// </summary>
        /// <returns>A collection of catch data containing the largest catch of
        /// each fish.</returns>
        public IEnumerable<Catch> GetLeaderboard()
        {
            return Leaderboard.Read();
        }

        /// <summary>
        /// Deletes a fish from a user's records.
        /// </summary>
        /// <param name="fisher">The fisher object for the user.</param>
        /// <param name="fish">The catch data for the record to remove.</param>
        public void DeleteFish(Fisher fisher, Catch fish)
        {
            fisher.Records.Remove(fish);
            Fishers.Update(fisher);
            Fishers.Commit();
        }

        /// <summary>
        /// Calculates the exact length, weight, and point value of a fish
        /// being caught.
        /// </summary>
        /// <param name="fisher">The fisher object for the user catching the
        /// fish.</param>
        /// <returns>The catch object with the calculated data values.</returns>
        public Catch CalculateFishSizes(Fisher fisher)
        {
            var catchData = new Catch();
            catchData.UserId = fisher.UserId;
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
                OnNewGlobalRecord(catchData);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Casts the line out for a user, starting the fishing process.
        /// </summary>
        /// <param name="userId">The twitch id of the user to begin fishing for.</param>
        public void Cast(string userId)
        {
            var fisher = GetFisherById(userId);
            var hookTime = DateTime.Now;
            if (Tournament.IsRunning)
            {
                hookTime.AddSeconds(Random.Next(Settings.FishingTournamentCastMinimum, Settings.FishingTournamentCastMaximum + 1));
            }
            else
            {
                hookTime.AddSeconds(Random.Next(Settings.FishingCastMinimum, Settings.FishingCastMaximum + 1));
            }
            if (fisher == null)
            {
                fisher = new Fisher()
                {
                    UserId = userId,
                    IsFishing = true,
                    HookedTime = hookTime
                };
                Fishers.Create(fisher);
            }
            else
            {
                fisher.IsFishing = true;
                fisher.HookedTime = hookTime;
                Fishers.Update(fisher);
            }
            Fishers.Commit();
        }

        /// <summary>
        /// Attempts to hook a fish.
        /// </summary>
        /// <param name="fisher">The fisher to update.</param>
        /// <returns>True if a fish was hooked.</returns>
        public bool HookFish(Fisher fisher)
        {
            var index = -1;
            var rarities = FishData.Read().Select(x => x.Rarity).Distinct().ToList();
            if (Settings.FishingUseWeights)
            {
                index = Random.WeightedRandom(rarities.Select(x => (double)x.Weight).ToList());
            }
            else
            {
                index = Random.NextNormalIndex(rarities.Count);
            }
            if (index >= 0)
            {
                var rarityId = rarities[index].Id;
                var fishList = FishData.Read(x => x.Rarity.Id == rarityId).ToList();
                var fish = fishList[Random.Next(0, fishList.Count)];
                fisher.Hooked = fish;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to have the hooked fish escape.
        /// </summary>
        /// <param name="fisher">The fisher to update.</param>
        public void UnhookFish(Fisher fisher)
        {
            fisher.Hooked = null;
            fisher.HookedTime = null;
            fisher.IsFishing = false;
        }

        /// <summary>
        /// Catches the fish a user has hooked. If no fish is hooked, the user
        /// will reel in the empty line.
        /// </summary>
        /// <param name="fisher">The fisher that is trying to catch.</param>
        /// <returns>The catch data for this fish.</returns>
        public Catch CatchFish(Fisher fisher)
        {
            var catchData = default(Catch);
            if (fisher != null && fisher.Hooked != null)
            {
                catchData = CalculateFishSizes(fisher);
                if (Tournament.IsRunning)
                {
                    UpdatePersonalLeaderboard(fisher, catchData);
                    UpdateGlobalLeaderboard(catchData);
                    Tournament.AddTournamentPoints(fisher.UserId, catchData.Points);
                }
            }
            fisher.IsFishing = false;
            fisher.Hooked = null;
            fisher.HookedTime = null;
            return catchData;
        }

        /// <summary>
        /// Runs all active fishers to process hooking and releasing events.
        /// </summary>
        public void Process(bool broadcasting)
        {
            var messages = new Dictionary<string, IEnumerable<string>>();
            foreach (var fisher in Fishers.Read(x => x.IsFishing))
            {
                if (fisher != null
                    && fisher.IsFishing
                    && fisher.Hooked == null
                    && DateTime.Now >= fisher.HookedTime)
                {
                    if (HookFish(fisher))
                    {
                        OnFishHooked(fisher);
                    }
                }
                if (fisher != null
                    && fisher.Hooked != null
                    && fisher.HookedTime.HasValue
                    && DateTime.Now >= fisher.HookedTime.Value.AddSeconds(Settings.FishingHookLength))
                {
                    UnhookFish(fisher);
                    OnFishGotAway(fisher);
                }
            }

            Tournament.Process(broadcasting);
        }
    }
}
