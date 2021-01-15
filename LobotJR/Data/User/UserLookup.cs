using LobotJR.Shared.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Data.User
{
    /// <summary>
    /// Provides a lookup for users by id or username. If a username is not
    /// found, their id will be looked up from the twitch API, and either their
    /// username in the cache will be updated or an entry will be added for
    /// them.
    /// </summary>
    public class UserLookup
    {
        private readonly List<string> cacheMisses = new List<string>();

        public IRepository<UserMap> UserMap { get; private set; }

        /// <summary>
        /// Gets the username associated with a twitch id.
        /// </summary>
        /// <param name="id">The twitch id.</param>
        /// <returns>The username.</returns>
        public string GetUsername(string id)
        {
            var entry = UserMap.Read(x => x.Id.Equals(id)).FirstOrDefault();
            return entry?.Username;
        }

        /// <summary>
        /// Gets a twitch id from it's associated username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The twitch id.</returns>
        public string GetId(string username)
        {
            var entry = UserMap.Read(x => x.Username.Equals(username)).FirstOrDefault();
            if (entry == null)
            {
                if (!cacheMisses.Contains(username))
                {
                    cacheMisses.Add(username);
                }
            }
            return entry?.Id;
        }

        /// <summary>
        /// Updates the twitch id cache with every username requested that was
        /// not found in the cache.
        /// </summary>
        /// <param name="token">A valid twitch OAuth token.</param>
        /// <param name="clientId">The client id the app is running under.</param>
        public void UpdateCache(string token, string clientId)
        {
            while (cacheMisses.Count > 0)
            {
                var limit = Math.Min(cacheMisses.Count, 100);
                var removed = cacheMisses.GetRange(0, limit);
                cacheMisses.RemoveRange(0, limit);
                var response = Users.Get(token, clientId, removed);
                foreach (var entry in response.Data)
                {
                    var existing = UserMap.Read(x => x.Id.Equals(entry.Id)).FirstOrDefault();
                    if (existing != null)
                    {
                        existing.Username = entry.DisplayName;
                        UserMap.Update(existing);
                    }
                    else
                    {
                        UserMap.Create(new UserMap()
                        {
                            Username = entry.DisplayName,
                            Id = entry.Id
                        });
                    }
                }
            }
            UserMap.Commit();
        }
    }
}
