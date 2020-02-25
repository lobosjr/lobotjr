using System.Collections.Generic;

namespace LobotJR.Shared.User
{
    /// <summary>
    /// Response object provided by twitch user API. See
    /// https://dev.twitch.tv/docs/api/reference#get-users for details.
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Collection of data on all requested users.
        /// </summary>
        public IEnumerable<UserData> Data { get; set; }
    }
}