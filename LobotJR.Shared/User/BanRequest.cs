namespace LobotJR.Shared.User
{
    /// <summary>
    /// Request object for the ban user endpoint.
    /// </summary>
    public class BanRequest
    {
        /// <summary>
        /// Identifies the user and type of ban.
        /// </summary>
        public BanRequestData Data { get; set; }

        public BanRequest(string userId, int? duration, string reason)
        {
            Data = new BanRequestData(userId, duration, reason);
        }
    }

    /// <summary>
    /// Identifies the user and type of ban.
    /// </summary>
    public class BanRequestData
    {
        /// <summary>
        /// The ID of the user to ban or put in a timeout.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// To ban a user indefinitely, don’t include this field.
        /// To put a user in a timeout, include this field and specify the
        /// timeout period, in seconds.The minimum timeout is 1 second and the
        /// maximum is 1,209,600 seconds(2 weeks).
        /// To end a user’s timeout early, set this field to 1, or use the
        /// Unban user endpoint.
        /// </summary>
        public int? Duration { get; set; }
        /// <summary>
        /// The reason the you’re banning the user or putting them in a
        /// timeout. The text is user defined and is limited to a maximum of
        /// 500 characters.
        /// </summary>
        public string Reason { get; set; }

        public BanRequestData(string userId, int? duration, string reason)
        {
            UserId = userId;
            Duration = duration;
            Reason = reason;
        }
    }
}
