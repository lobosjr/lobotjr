namespace LobotJR.Data.User
{
    /// <summary>
    /// Maps the username to their twitch id.
    /// </summary>
    public class UserMap : TableObject
    {
        /// <summary>
        /// User's current username.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Id from twitch used to identify the user.
        /// </summary>
        public string TwitchId { get; set; }
    }
}
