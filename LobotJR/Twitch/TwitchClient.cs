using LobotJR.Data;
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
        private List<string> Blacklist = new List<string>();
        private List<WhisperRecord> History = new List<WhisperRecord>();
        private bool CanWhisper = true;

        private WhisperQueue Queue;
        private UserLookup UserLookup;
        private string ClientId;
        private string AccessToken;
        private string BroadcasterId;
        private string ChatId;

        public TwitchClient(IRepositoryManager repositoryManager, UserLookup userLookup, ClientData clientData, TokenData tokenData)
        {
            Queue = new WhisperQueue(repositoryManager);
            UserLookup = userLookup;
            ClientId = clientData.ClientId;
            AccessToken = tokenData.ChatToken.AccessToken;
            BroadcasterId = UserLookup.GetId(tokenData.BroadcastUser);
            ChatId = UserLookup.GetId(tokenData.ChatUser);
        }

        /// <summary>
        /// Sends a whisper to a user asynchronously.
        /// </summary>
        /// <param name="userId">The Twitch id of the user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the whisper was sent successfully.</returns>
        private async Task<HttpStatusCode> WhisperAsync(string userId, string message)
        {
            var result = await SendWhisper.Post(AccessToken, ClientId, ChatId, userId, message);
            return result;
        }

        /// <summary>
        /// Sends a whisper to a user synchronously.
        /// </summary>
        /// <param name="user">The name of the user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the whisper was sent successfully.</returns>
        public void QueueWhisper(string user, string message)
        {
            if (!Blacklist.Contains(user))
            {
                Queue.Enqueue(user, UserLookup.GetId(user), message, DateTime.Now);
            }
        }

        /// <summary>
        /// Sends a whisper to a group of users synchronously.
        /// </summary>
        /// <param name="users">A collection of users to message.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if all whispers were sent successfully.</returns>
        public void QueueWhisper(IEnumerable<string> users, string message)
        {
            foreach (var user in users)
            {
                QueueWhisper(user, message);
            }
        }

        /// <summary>
        /// Attempts to re-send all whispers that failed due to the id not being in the cache.
        /// </summary>
        public async void ProcessQueue(bool cacheUpdated)
        {
            var toSend = Queue.GetMessagesToSend(cacheUpdated, UserLookup).ToList();
            var results = await Task.WhenAll(toSend.Select(x => WhisperAsync(x.UserId, x.Message)));
            for (var i = 0; i < results.Length; i++)
            {
                var sent = toSend[i];
                var result = results[i];
                if (result == HttpStatusCode.NoContent)
                {
                    Queue.ReportSuccess(sent);
                }
                else if (result == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"User name {sent.Username} returned id {sent.UserId} from Twitch. Twitch says this user id doesn't exist. User {sent.Username} has been blacklisted from whispers.");
                    Blacklist.Add(sent.Username);
                }
                else if (result == (HttpStatusCode)429)
                {
                    Console.WriteLine($"ERROR: We sent too many whispers. Whispers have been turned off for one minute, and no more unique recipients will be allowed.");
                    Console.WriteLine("See below for details on the current state of the whisper queue.");
                    Console.WriteLine(Queue.Debug());
                    Queue.FreezeQueue();
                    Queue.NewRecipientsAllowed = false;
                }
                else
                {
                    Console.WriteLine($"ERROR: Something went wrong trying to send a whisper. Twitch response: {result}");
                }
            }
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
