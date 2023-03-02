using LobotJR.Data.User;
using LobotJR.Shared.Authentication;
using LobotJR.Shared.Chat;
using LobotJR.Shared.Client;
using LobotJR.Shared.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LobotJR.Twitch
{
    /// <summary>
    /// Client that provide access to common Twitch API endpoints.
    /// </summary>
    public class TwitchClient
    {
        private Dictionary<string, IList<string>> PendingWhispers;
        private UserLookup UserLookup;
        private string ClientId;
        private string AccessToken;
        private string BroadcasterId;
        private string ChatId;

        public TwitchClient(UserLookup userLookup, ClientData clientData, TokenData tokenData)
        {
            PendingWhispers = new Dictionary<string, IList<string>>();
            UserLookup = userLookup;
            ClientId = clientData.ClientId;
            AccessToken = tokenData.ChatToken.AccessToken;
            BroadcasterId = UserLookup.GetId(tokenData.BroadcastUser);
            ChatId = UserLookup.GetId(tokenData.ChatUser);
        }

        /// <summary>
        /// Sends a whisper to a user asynchronously.
        /// </summary>
        /// <param name="user">The name of the user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the whisper was sent successfully.</returns>
        public async Task<bool> WhisperAsync(string user, string message)
        {
            var userId = UserLookup.GetId(user);
            if (userId == null)
            {
                if (PendingWhispers.TryGetValue(user, out var messages))
                {
                    messages.Add(message);
                }
                else
                {
                    PendingWhispers.Add(user, new List<string>() { message });
                }
                return false;
            }
            var result = await SendWhisper.Post(AccessToken, ClientId, ChatId, UserLookup.GetId(user), message);
            return result == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Sends a whisper to a user synchronously.
        /// </summary>
        /// <param name="user">The name of the user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the whisper was sent successfully.</returns>
        public bool Whisper(string user, string message)
        {
            return WhisperAsync(user, message).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a whisper to a group of users synchronously.
        /// </summary>
        /// <param name="users">A collection of users to message.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if all whispers were sent successfully.</returns>
        public bool Whisper(IEnumerable<string> users, string message)
        {
            var results = Task.WhenAll(users.Select(x => WhisperAsync(x, message))).GetAwaiter().GetResult();
            return results.All(x => x);
        }

        /// <summary>
        /// Attempts to re-send all whispers that failed due to the id not being in the cache.
        /// </summary>
        public async void ProcessQueue()
        {
            await Task.WhenAll(PendingWhispers.SelectMany(x => x.Value.Select(y => WhisperAsync(x.Key, y))));
        }

        /// <summary>
        /// Times out a user asynchronously.
        /// </summary>
        /// <param name="user">The user to timeout.</param>
        /// <param name="duration">The duration of the timeout. Null for a permanent ban.</param>
        /// <param name="message">The message to send along with the timeout.</param>
        /// <returns>True if the timeout was executed successfully.</returns>
        /// <exception cref="Exception">If the Twitch user id cannot be retrieved.</exception>
        public async Task<bool> TimeoutAsync(string user, int? duration, string message)
        {
            var userId = UserLookup.GetId(user);
            if (userId == null)
            {
                await UserLookup.UpdateCache(AccessToken, ClientId);
                userId = UserLookup.GetId(user);
                if (userId == null)
                {
                    throw new Exception($"Failed to get user id for timeout of user {user} with reason \"{message ?? "null"}\"");
                }
            }
            var result = await BanUser.Post(AccessToken, ClientId, BroadcasterId, ChatId, userId, duration, message);
            return result == HttpStatusCode.OK;
        }

        /// <summary>
        /// Times out a user synchronously.
        /// </summary>
        /// <param name="user">The user to timeout.</param>
        /// <param name="duration">The duration of the timeout. Null for a permanent ban.</param>
        /// <param name="message">The message to send along with the timeout.</param>
        /// <returns>True if the timeout was executed successfully.</returns>
        /// <exception cref="Exception">If the Twitch user id cannot be retrieved.</exception>
        public bool Timeout(string user, int? duration, string message)
        {
            return TimeoutAsync(user, duration, message).GetAwaiter().GetResult();
        }
    }
}
