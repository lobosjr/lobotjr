namespace LobotJR.Shared.User
{
    /// <summary>
    /// User data provided by twitch user API. See
    /// https://dev.twitch.tv/docs/api/reference#get-users for details.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// User’s broadcaster type: "partner", "affiliate", or "".
        /// </summary>
        public string BroadcasterType { get; set; }
        /// <summary>
        /// User’s channel description.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// User’s display name.
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// User’s email address. Returned if the request includes the user:read:email scope.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// User’s ID.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// User’s login name.
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// URL of the user’s offline image.
        /// </summary>
        public string OfflineImageUrl { get; set; }
        /// <summary>
        /// URL of the user’s profile image.
        /// </summary>
        public string ProfileImageUrl { get; set; }
        /// <summary>
        /// User’s type: "staff", "admin", "global_mod", or "".
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Total number of views of the user’s channel.
        /// </summary>
        public int ViewCount { get; set; }
    }
}