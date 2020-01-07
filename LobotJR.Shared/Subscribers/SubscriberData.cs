namespace LobotJR.Shared.Subscribers
{
    /// <summary>
    /// A subscriber record as returned by the Get Broadcaster Subscriptions
    /// API. See https://dev.twitch.tv/docs/api/reference#get-broadcaster-subscriptions
    /// for more details.
    /// </summary>
    public class SubscriberData
    {
        /// <summary>
        /// User ID of the broadcaster. 
        /// </summary>
        public string BroadcasterId { get; set; }
        /// <summary>
        /// Display name of the broadcaster.
        /// </summary>
        public string BroadcasterName { get; set; }
        /// <summary>
        /// Determines if the subscription is a gift subscription.
        /// </summary>
        public bool IsGift { get; set; }
        /// <summary>
        /// Type of subscription (Tier 1, Tier 2, Tier 3).
        /// 1000 = Tier 1, 2000 = Tier 2, 3000 = Tier 3 subscriptions.
        /// </summary>
        public string Tier { get; set; }
        /// <summary>
        /// Name of the subscription.
        /// </summary>
        public string PlanName { get; set; }
        /// <summary>
        /// ID of the subscribed user.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Display name of the subscribed user. 
        /// </summary>
        public string UserName { get; set; }
    }
}
