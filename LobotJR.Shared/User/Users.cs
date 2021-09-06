using LobotJR.Shared.Utility;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace LobotJR.Shared.User
{
    /// <summary>
    /// Provides methods to access the twitch Get Users API.
    /// https://dev.twitch.tv/docs/api/reference#get-users
    /// </summary>
    public class Users
    {
        /// <summary>
        /// Calls the twitch Get Users API with no parameters.
        /// </summary>
        /// <param name="token">A bearer token.</param>
        /// <param name="clientId">The client id the app is running under.</param>
        /// <returns>The user data of the authenticated user.</returns>
        public static UserResponse Get(string token, string clientId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestClient("https://api.twitch.tv");
            client.AddHandler("application/json", () => NewtonsoftDeserializer.Default);
            var request = new RestRequest("helix/users", Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Client-ID", clientId);
            var response = client.Execute<UserResponse>(request);
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
        /// Calls the twitch Get Users API with a list of usernames
        /// </summary>
        /// <param name="token">A bearer token.</param>
        /// <param name="clientId">The clied id the app is running under.</param>
        /// <param name="users">A collection of usernames.</param>
        /// <returns>The user data of the users in the collection.</returns>
        public static UserResponse Get(string token, string clientId, IEnumerable<string> users)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestClient("https://api.twitch.tv");
            client.AddHandler("application/json", () => NewtonsoftDeserializer.Default);
            var request = new RestRequest("helix/users", Method.GET);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Client-ID", clientId);
            foreach (var user in users)
            {
                request.AddParameter("login", user, ParameterType.QueryString);
            }
            var response = client.Execute<UserResponse>(request);
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
    }
}