using LobotJR.Command.Model.Fishing;
using LobotJR.Data;
using LobotJR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Command.System.Fishing
{
    /// <summary>
    /// Runs the logic for the fishing system.
    /// </summary>
    public class FishingSystem : ISystem
    {
        private readonly Random Random = new Random();
        private readonly int[] Chances = new int[] { 40, 70, 95, 99, 100 };

        private readonly IRepository<Fish> FishData;
        private readonly List<Fisher> Fishers;
        private readonly AppSettings Settings;

        /// <summary>
        /// Event handler for events related to a specific user.
        /// </summary>
        /// <param name="fisher">The fisher object for the user.</param>
        public delegate void FisherEventHandler(Fisher fisher);
        /// <summary>
        /// Event handler for when a fisher catches a fish.
        /// </summary>
        /// <param name="fisher">The fisher object for the user.</param>
        /// <param name="catchData">The catch data for the fish caught.</param>
        public delegate void FishCatchEventHandler(Fisher fisher, Catch catchData);

        /// <summary>
        /// Event fired when a user hooks a fish.
        /// </summary>
        public event FisherEventHandler FishHooked;
        /// <summary>
        /// Event fired when a user's hooked fish gets away.
        /// </summary>
        public event FisherEventHandler FishGotAway;
        /// <summary>
        /// Event fired when a user catches a fish.
        /// </summary>
        public event FishCatchEventHandler FishCaught;

        public int CastTimeMinimum { get; set; }
        public int CastTimeMaximum { get; set; }

        /// <summary>
        /// The cost in wolfcoins for a user to gloat about their fishing.
        /// </summary>
        public int GloatCost { get { return Settings != null ? Settings.FishingGloatCost : -1; } }

        public FishingSystem(IRepositoryManager repositoryManager,
            IContentManager contentManager)
        {
            Fishers = repositoryManager.Users.Read().Select(x => new Fisher() { UserId = x.TwitchId }).ToList();
            FishData = contentManager.FishData;
            Settings = repositoryManager.AppSettings.Read().First();
            CastTimeMinimum = Settings.FishingCastMinimum;
            CastTimeMaximum = Settings.FishingCastMaximum;
        }

        private void OnFishHooked(Fisher fisher)
        {
            FishHooked?.Invoke(fisher);
        }

        private void OnFishGotAway(Fisher fisher)
        {
            FishGotAway?.Invoke(fisher);
        }

        private void OnFishCaught(Fisher fisher, Catch catchData)
        {
            FishCaught?.Invoke(fisher, catchData);
        }

        /// <summary>
        /// Gets or creates a fisher object for a given user id.
        /// </summary>
        /// <param name="userId">The twitch id for the user.</param>
        /// <returns>The fisher object for the user.</returns>
        public Fisher GetFisherById(string userId)
        {
            var fisher = Fishers.FirstOrDefault(x => x.UserId.Equals(userId));
            if (fisher == null)
            {
                fisher = new Fisher() { UserId = userId };
                Fishers.Add(fisher);
            }
            return fisher;
        }

        /// <summary>
        /// Resets all fishers by clearing any cast lines or hooked fish.
        /// </summary>
        public void ResetFishers()
        {
            foreach (var fisher in Fishers)
            {
                fisher.IsFishing = false;
                fisher.Hooked = null;
                fisher.HookedTime = null;
            }
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
            if (fisher == null || fisher.Hooked == null)
            {
                return null;
            }

            var fish = fisher.Hooked;
            var catchData = new Catch
            {
                UserId = fisher.UserId,
                Fish = fish
            };

            if (Settings.FishingUseNormalSizes)
            {
                catchData.Weight = (float)Random.NextNormalBounded(fish.MinimumWeight, fish.MaximumWeight);
                catchData.Length = (float)Random.NextNormalBounded(fish.MinimumLength, fish.MaximumLength);

                var weightRange = fish.MaximumWeight - fish.MinimumWeight;
                var lengthRange = fish.MaximumLength - fish.MinimumLength;
                catchData.Points = (int)Math.Round(
                    (catchData.Weight - fish.MinimumWeight) / weightRange * 50f +
                    (catchData.Length - fish.MinimumLength) / lengthRange * 50f);
            }
            else
            {
                var weightRange = (fish.MaximumWeight - fish.MinimumWeight) / 5;
                var lengthRange = (fish.MaximumLength - fish.MinimumLength) / 5;
                var weightVariance = Random.NextDouble() * weightRange;
                var lengthVariance = Random.NextDouble() * lengthRange;

                var size = Random.NextDouble() * 100;
                var category = Chances.Where(x => size > x).Count();
                catchData.Length = (float)Math.Round(fish.MinimumLength + lengthRange * category + lengthVariance, 2);
                catchData.Weight = (float)Math.Round(fish.MinimumWeight + weightRange * category + weightVariance, 2);
                catchData.Points = (int)Math.Max(Math.Round(size), 1);
            }

            return catchData;
        }

        /// <summary>
        /// Casts the line out for a user, starting the fishing process.
        /// </summary>
        /// <param name="userId">The twitch id of the user to begin fishing for.</param>
        public void Cast(string userId)
        {
            var fisher = GetFisherById(userId);
            var hookTime = DateTime.Now;
            hookTime = hookTime.AddSeconds(Random.Next(CastTimeMinimum, CastTimeMaximum + 1));
            if (fisher == null)
            {
                Fishers.Add(new Fisher()
                {
                    UserId = userId,
                    IsFishing = true,
                    HookedTime = hookTime
                });
            }
            else
            {
                fisher.IsFishing = true;
                fisher.HookedTime = hookTime;
            }
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
            if (Settings.FishingUseNormalRarity)
            {
                index = Random.NextNormalIndex(rarities.Count);
            }
            else
            {
                index = Random.WeightedRandom(rarities.Select(x => (double)x.Weight).ToList());
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
            if (fisher != null)
            {
                catchData = CalculateFishSizes(fisher);
                fisher.IsFishing = false;
                fisher.Hooked = null;
                fisher.HookedTime = null;
                if (catchData != null)
                {
                    OnFishCaught(fisher, catchData);
                }
            }
            return catchData;
        }

        /// <summary>
        /// Runs all active fishers to process hooking and releasing events.
        /// </summary>
        public void Process(bool broadcasting)
        {
            var messages = new Dictionary<string, IEnumerable<string>>();
            foreach (var fisher in Fishers.Where(x => x.IsFishing))
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
        }
    }
}
