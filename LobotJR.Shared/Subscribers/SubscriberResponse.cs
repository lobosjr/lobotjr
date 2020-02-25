using LobotJR.Shared.Common;
using System.Collections.Generic;

namespace LobotJR.Shared.Subscribers
{
    /// <summary>
    /// Response object provided by the twitch Get Broadcaster Subscriptions
    /// API. See https://dev.twitch.tv/docs/api/reference#get-broadcaster-subscriptions
    /// for more details.
    /// </summary>
    public class SubscriberResponse
    {
        /// <summary>
        /// Collection of subscriber data.
        /// </summary>
        public IEnumerable<SubscriberData> Data { get; set; }
        /// <summary>
        /// Pagination record for getting more than 100 results.
        /// </summary>
        public Pagination Pagination { get; set; }
    }
}
