using LobotJR.Shared.Utility;
using RestSharp;
using System.Net;
using System.Web;

namespace LobotJR.Shared.Authentication
{
    /// <summary>
    /// This class provides methods for getting and refreshing oauth tokens.
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// Acquires a user access token using the oauth authorization code flow.
        /// </summary>
        /// <param name="clientId">The client id of your registered twitch app.</param>
        /// <param name="clientSecret">The client secret of your registered twitch app.</param>
        /// <param name="code">The auth code provided when the user logged in.</param>
        /// <param name="redirectUri">The redirect uri of your registered twitch app.</param>
        /// <returns></returns>
        public static TokenResponse Fetch(string clientId, string clientSecret, string code, string redirectUri)
        {
            var client = new RestClient("https://id.twitch.tv");
            client.AddHandler("application/json", () => NewtonsoftDeserializer.Default);
            var request = new RestRequest("oauth2/token", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("client_id", clientId);
            request.AddQueryParameter("client_secret", clientSecret);
            request.AddQueryParameter("code", code);
            request.AddQueryParameter("grant_type", "authorization_code");
            request.AddQueryParameter("redirect_uri", redirectUri);
            var response = client.Execute<TokenResponse>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        /// <summary>
        /// Refreshes an expired user access token.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static TokenResponse Refresh(string clientId, string clientSecret, string refreshToken)
        {
            var client = new RestClient("https://id.twitch.tv");
            client.AddHandler("application/json", () => NewtonsoftDeserializer.Default);
            var request = new RestRequest("oauth2/token", Method.POST);
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("grant_type", "refresh_token");
            request.AddQueryParameter("refresh_token", HttpUtility.UrlEncode(refreshToken));
            request.AddQueryParameter("client_id", clientId);
            request.AddQueryParameter("client_secret", clientSecret);
            var response = client.Execute<TokenResponse>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }
    }
}
