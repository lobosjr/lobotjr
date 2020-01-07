using LobotJR.Shared.Utility;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace LobotJR.Shared.Subscribers
{
    /// <summary>
    /// Provides methods to call the twitch Get Broadcaster Subscriptions API.
    /// https://dev.twitch.tv/docs/api/reference#get-broadcaster-subscriptions
    /// </summary>
    public class Subscribers
    {
        /// <summary>
        /// Retrieves a list of users subscribed to a broadcaster. The
        /// broadcaster id must match the user in the token.
        /// </summary>
        /// <param name="token">A bearer token.</param>
        /// <param name="broadcasterId">The id of a broadcaster. This is not
        /// the same as the username, use Users.Get to retrieve the id for a
        /// specific username.</param>
        /// <param name="cursor">An optional pagination cursor.</param>
        /// <returns>A subscriber response containing a list of users. If there
        /// are more users available to retrieve, it will also contain a
        /// pagination cursor.</returns>
        public static SubscriberResponse Get(string token, string broadcasterId, string cursor = null)
        {
            var client = new RestClient("https://api.twitch.tv");
            client.AddHandler("application/json", () => NewtonsoftDeserializer.Default);
            var request = new RestRequest("helix/subscriptions", Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddQueryParameter("broadcaster_id", broadcasterId);
            if (!string.IsNullOrWhiteSpace(cursor))
            {
                request.AddQueryParameter("cursor", cursor);
            }
            var response = client.Execute<SubscriberResponse>(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return response.Data;
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieves a list of all of the users subscribed to a broadcaster.
        /// The broadcaster id must match the user in the token.
        /// </summary>
        /// <param name="token">A bearer token.</param>
        /// <param name="broadcasterId">The id of a broadcaster. This is not
        /// the same as the username, use Users.Get to retrieve the id for a
        /// specific username.</param>
        /// <returns>A list of all subscribers to the broadcaster.</returns>
        public static IEnumerable<SubscriberData> GetAll(string token, string broadcasterId)
        {
            var subscribers = new List<SubscriberData>();
            SubscriberResponse response;
            do
            {
                response = Get(token, broadcasterId);
                subscribers.AddRange(response.Data);

            }
            while (!string.IsNullOrWhiteSpace(response.Pagination.Cursor));
            return subscribers;
        }
    }
}
