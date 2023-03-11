using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Shared.Authentication;
using LobotJR.Shared.Chat;
using LobotJR.Shared.Client;
using LobotJR.Shared.Subscription;
using LobotJR.Shared.User;
using LobotJR.Shared.Utility;
using RestSharp;
using System;
using System.Collections.Generic;
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

        private WhisperQueue Queue;
        private UserLookup UserLookup;
        private string BroadcasterId;
        private string ChatId;
        private ClientData ClientData;
        private TokenData TokenData;


        public TwitchClient(IRepositoryManager repositoryManager, UserLookup userLookup, ClientData clientData, TokenData tokenData)
        {
            Queue = new WhisperQueue(repositoryManager, 3, 100, 40);
            UserLookup = userLookup;
            ClientData = clientData;
            TokenData = tokenData;
        }

        /// <summary>
        /// Loads the Twitch id for the broadcast and chat users.
        /// </summary>
        /// <exception cref="Exception">Thrown if the id for either user cannot
        /// be retrieved from Twitch.</exception>
        public void GetBotIds()
        {
            BroadcasterId = UserLookup.GetId(TokenData.BroadcastUser);
            ChatId = UserLookup.GetId(TokenData.ChatUser);
            if (string.IsNullOrWhiteSpace(BroadcasterId) || string.IsNullOrWhiteSpace(ChatId))
            {
                UserLookup.UpdateCache(TokenData.ChatToken.AccessToken, ClientData.ClientId).GetAwaiter().GetResult();
                BroadcasterId = UserLookup.GetId(TokenData.BroadcastUser);
                ChatId = UserLookup.GetId(TokenData.ChatUser);
                if (string.IsNullOrWhiteSpace(BroadcasterId) || string.IsNullOrWhiteSpace(ChatId))
                {
                    throw new Exception($"Unable to retrieve required twitch ids: {TokenData.BroadcastUser}: {BroadcasterId}, {TokenData.ChatUser}: {ChatId}");
                }
            }
        }

        private async Task<RestResponse<TokenResponse>> RefreshToken(TokenResponse token)
        {
            if (token.ExpirationDate < DateTime.Now)
            {
                return await AuthToken.Refresh(ClientData.ClientId, ClientData.ClientSecret, token.RefreshToken);
            }
            return null;
        }

        /// <summary>
        /// Attempts to refresh the chat and broadcast tokens.
        /// </summary>
        /// <exception cref="Exception">Exception is thrown if the tokens fail to refresh.</exception>
        public async Task RefreshTokens()
        {
            var updated = false;
            var response = await RefreshToken(TokenData.ChatToken);
            if (response != null)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.ErrorException != null || response.ErrorMessage != null)
                    {
                        throw new Exception($"Encountered an exception trying to refresh the token for {TokenData.ChatUser}. {response.ErrorMessage}. {response.ErrorException.ToString()}");
                    }
                    throw new Exception($"Encountered an unexpected response trying to refresh the token for {TokenData.ChatUser}. {response.StatusCode}: {response.Content}");
                }
                TokenData.ChatToken.CopyFrom(response.Data);
                updated = true;
            }
            response = await RefreshToken(TokenData.BroadcastToken);
            if (response != null)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.ErrorException != null || response.ErrorMessage != null)
                    {
                        throw new Exception($"Encountered an exception trying to refresh the token for {TokenData.ChatUser}. {response.ErrorMessage}. {response.ErrorException.ToString()}");
                    }
                    throw new Exception($"Encountered an unexpected response trying to refresh the token for {TokenData.BroadcastUser}. {response.StatusCode}: {response.Content}");
                }
                TokenData.BroadcastToken.CopyFrom(response.Data);
                updated = true;
            }
            if (updated)
            {
                FileUtils.WriteTokenData(TokenData);
            }
        }

        /// <summary>
        /// Sends a whisper to a user asynchronously.
        /// </summary>
        /// <param name="userId">The Twitch id of the user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>True if the whisper was sent successfully.</returns>
        private async Task<HttpStatusCode> WhisperAsync(string userId, string message)
        {
            var result = await SendWhisper.Post(TokenData.ChatToken.AccessToken, ClientData.ClientId, ChatId, userId, message);
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
        public async Task ProcessQueue(bool cacheUpdated)
        {
            if (cacheUpdated)
            {
                Queue.UpdateUserIds(UserLookup);
            }
            var canSend = Queue.TryGetMessage(out var message);
            while (canSend)
            {
                var result = await WhisperAsync(message.UserId, message.Message);
                if (result == HttpStatusCode.NoContent)
                {
                    Queue.ReportSuccess(message);
                }
                else if (result == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"Token for chat user ({TokenData.ChatUser}) has expired. Refreshing token.");
                    Queue.ReportFailure(message);
                    await RefreshTokens();
                }
                else if (result == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"User name {message.Username} returned id {message.UserId} from Twitch. Twitch says this user id doesn't exist. User {message.Username} has been blacklisted from whispers.");
                    Blacklist.Add(message.Username);
                }
                else if (result == (HttpStatusCode)429)
                {
                    Console.WriteLine($"ERROR: We sent too many whispers. Whispers have been turned off for one minute, and no more unique recipients will be allowed.");
                    Console.WriteLine("See below for details on the current state of the whisper queue.");
                    Console.WriteLine(Queue.Debug());
                    Queue.FreezeQueue();
                    Queue.NewRecipientsAllowed = false;
                    Queue.ReportFailure(message);
                    break;
                }
                else
                {
                    Console.WriteLine($"ERROR: Something went wrong trying to send a whisper. Twitch response: {result}");
                }
                canSend = Queue.TryGetMessage(out message);
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
                await UserLookup.UpdateCache(TokenData.ChatToken.AccessToken, ClientData.ClientId);
                userId = UserLookup.GetId(user);
                if (userId == null)
                {
                    throw new Exception($"Failed to get user id for timeout of user {user} with reason \"{message ?? "null"}\"");
                }
            }
            var result = await BanUser.Post(TokenData.ChatToken.AccessToken, ClientData.ClientId, BroadcasterId, ChatId, userId, duration, message);
            if (result == HttpStatusCode.Unauthorized)
            {
                await RefreshTokens();
                result = await BanUser.Post(TokenData.ChatToken.AccessToken, ClientData.ClientId, BroadcasterId, ChatId, userId, duration, message);
            }
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

        /// <summary>
        /// Gets the details of all subscribers to the broadcast user.
        /// </summary>
        /// <returns>A collection of subscription responses from Twitch.</returns>
        public async Task<IEnumerable<SubscriptionResponseData>> GetSubscriberListAsync()
        {
            var result = await Subscriptions.GetAll(TokenData.ChatToken.AccessToken, ClientData.ClientId, BroadcasterId);
            if (result == null)
            {
                await RefreshTokens();
                result = await Subscriptions.GetAll(TokenData.ChatToken.AccessToken, ClientData.ClientId, BroadcasterId);
            }
            return result;
        }
    }
}
