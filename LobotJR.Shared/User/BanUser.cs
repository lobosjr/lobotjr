﻿using LobotJR.Shared.Utility;
using NLog;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Net;
using System.Threading.Tasks;

namespace LobotJR.Shared.User
{
    /// <summary>
    /// Provides methods to access the twitch Ban User API.
    /// https://dev.twitch.tv/docs/api/reference/#ban-user
    /// </summary>
    public class BanUser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Calls the twitch Ban User API to ban a user for a set duration.
        /// </summary>
        /// <param name="token">The OAuth token for the user making the call.</param>
        /// <param name="clientId">The client id of the application.</param>
        /// <param name="broadcasterId">The id of the channel to ban the user from.</param>
        /// <param name="moderatorId">The id of the user executing the ban (must be a moderator of the channel).</param>
        /// <param name="userId">The id of the user to ban.</param>
        /// <param name="duration">The duration of the ban. Set this to null for a permanent ban.</param>
        /// <param name="reason">An optional string with the reason for the ban.</param>
        /// <returns>The http response code from the API.</returns>
        public static async Task<HttpStatusCode> Post(string token, string clientId, string broadcasterId, string moderatorId, string userId, int? duration, string reason)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestClient("https://api.twitch.tv");
            client.UseNewtonsoftJson(SerializerSettings.Default);
            var request = new RestRequest("helix/moderation/bans", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Client-ID", clientId);
            request.AddParameter("broadcaster_id", broadcasterId, ParameterType.QueryString);
            request.AddParameter("moderator_id", moderatorId, ParameterType.QueryString);
            request.AddJsonBody(new BanRequest(userId, duration, reason));
            RestLogger.AddLogging(request, Logger);
            var response = await client.ExecuteAsync(request);
            return response.StatusCode;
        }
    }
}