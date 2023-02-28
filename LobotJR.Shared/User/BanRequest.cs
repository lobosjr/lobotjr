using System.Collections.Generic;

namespace LobotJR.Shared.User
{
    public class BanRequest
    {
        public IEnumerable<BanRequestData> Data { get; set; }

        public BanRequest(string userId, int? duration, string reason)
        {
            Data = new List<BanRequestData>()
            {
                new BanRequestData(userId, duration, reason)
            };
        }

        public BanRequest(params BanRequestData[] data)
        {
            Data = data;
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
