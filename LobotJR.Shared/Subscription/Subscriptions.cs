using LobotJR.Shared.Utility;
using NLog;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LobotJR.Shared.Subscription
{
    /// <summary>
    /// Provides methods to access the twitch Ban User API.
    /// https://dev.twitch.tv/docs/api/reference/#get-broadcaster-subscriptions
    /// </summary>
    public class Subscriptions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Calls the Twitch Get Broadcaster Subscriptions API to get the list
        /// of all subscribers to a channel. Retrieves the first 100 users
        /// starting from the specified value.
        /// </summary>
        /// <param name="token">The OAuth token for the user making the call.</param>
        /// <param name="clientId">The client id of the application.</param>
        /// <param name="broadcasterId">The id of the channel to get subscribers for.</param>
        /// <param name="start">The pagination cursor value to start from. If
        /// this is the first request, set to null.</param>
        /// <returns>The response body from the API, or null if the response code is not 200 (OK).</returns>
        public static async Task<RestResponse<SubscriptionResponse>> Get(string token, string clientId, string broadcasterId, string start = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestClient("https://api.twitch.tv");
            client.UseNewtonsoftJson(SerializerSettings.Default);
            var request = new RestRequest("helix/subscriptions", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Client-ID", clientId);
            request.AddParameter("broadcaster_id", broadcasterId, ParameterType.QueryString);
            if (start != null)
            {
                request.AddParameter("after", start, ParameterType.QueryString);
            }
            request.AddParameter("first", 100, ParameterType.QueryString);
            RestLogger.AddLogging(request, Logger);
            return await client.ExecuteAsync<SubscriptionResponse>(request);
        }

        /// <summary>
        /// Gets all users subscribed to a given channel.
        /// </summary>
        /// <param name="token">The OAuth token for the user making the call.</param>
        /// <param name="clientId">The client id of the application.</param>
        /// <param name="broadcasterId">The id of the channel to ban the user from.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<RestResponse<SubscriptionResponse>>> GetAll(string token, string clientId, string broadcasterId)
        {
            List<RestResponse<SubscriptionResponse>> data = new List<RestResponse<SubscriptionResponse>>();
            string cursor = null;
            do
            {
                var response = await Get(token, clientId, broadcasterId, cursor);
                data.Add(response);
                cursor = response.Data?.Pagination?.Cursor;
            }
            while (cursor != null);
            return data;
        }
    }
}