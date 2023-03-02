namespace LobotJR.Shared.User
{
    public class BanRequest
    {
        public BanRequestData Data { get; set; }

        public BanRequest(string userId, int? duration, string reason)
        {
            Data = new BanRequestData(userId, duration, reason);
        }
    }

    public class BanRequestData
    {
        public string UserId { get; set; }
        public int? Duration { get; set; }
        public string Reason { get; set; }

        public BanRequestData(string userId, int? duration, string reason)
        {
            UserId = userId;
            Duration = duration;
            Reason = reason;
        }
    }
}
