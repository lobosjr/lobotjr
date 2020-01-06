namespace LobotJR.Shared.Authentication
{
    /// <summary>
    /// The request edata used to refresh auth tokens.
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>
        /// Must be "refresh_token" according to the documentation. This is the default value.
        /// </summary>
        public string GrantType { get; set; }
        /// <summary>
        /// The client id of your app registered with twitch.
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// The client secret of your app registered with twitch.
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// The refresh token provided with the original auth token.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
