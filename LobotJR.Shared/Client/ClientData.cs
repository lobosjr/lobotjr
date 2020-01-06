namespace LobotJR.Shared.Client
{
    /// <summary>
    /// Holds the client data for the registered twitch app. This has all of
    /// the information necessary to obtain and maintain authentication. These
    /// data can all be found by clicking Manage on the app you want to use at
    /// https://dev.twitch.tv/console/apps.
    /// </summary>
    public class ClientData
    {
        /// <summary>
        /// The client id.
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// The client secret. This should be kept secure and secret.
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// The uri to redirect auth requests to.
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
