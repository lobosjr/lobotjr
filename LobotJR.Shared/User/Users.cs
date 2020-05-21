using LobotJR.Shared.Utility;
using RestSharp;
using System;
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
    }
}