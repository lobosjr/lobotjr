using LobotJR.Shared.Utility;
using NLog;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Net;
using System.Threading.Tasks;

namespace LobotJR.Shared.Chat
{
    /// <summary>
    /// Provides methods to access the twitch Send Whisper API.
    /// https://dev.twitch.tv/docs/api/reference/#send-whisper
    /// </summary>
    public class SendWhisper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Calls the twitch Send Whisper API to whisper a message to a user.
        /// </summary>
        /// <param name="token">The OAuth token for the user making the call.</param>
        /// <param name="clientId">The client id of the application.</param>
        /// <param name="senderId">The id of the user executing the ban (must be a moderator of the channel).</param>
        /// <param name="userId">The id of the user to ban.</param>
        /// <param name="reason">An optional string with the reason for the ban.</param>
        /// <returns>The http response code from the API.</returns>
        public static async Task<HttpStatusCode> Post(string token, string clientId, string senderId, string userId, string message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new RestClient("https://api.twitch.tv");
            client.UseNewtonsoftJson(SerializerSettings.Default);
            var request = new RestRequest("helix/whispers", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Client-ID", clientId);
            request.AddParameter("from_user_id", senderId, ParameterType.QueryString);
            request.AddParameter("to_user_id", userId, ParameterType.QueryString);
            request.AddBody(new Whisper(message));
            RestLogger.AddLogging(request, Logger);
            var response = await client.ExecuteAsync(request);
            return response.StatusCode;
        }
    }

    public class Whisper
    {
        public string Message { get; set; }

        public Whisper(string message)
        {
            Message = message;
        }
    }
}