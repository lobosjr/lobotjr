using LobotJR.Shared.Utility;
using System.Collections.Generic;

namespace LobotJR.Shared.Subscription
{
    /// <summary>
    /// Response object for the Get Broadcaster Subscription endpoint.
    /// </summary>
    public class SubscriptionResponse
    {
        /// <summary>
        /// The list of users that subscribe to the broadcaster. The list is
        /// empty if the broadcaster has no subscribers.
        /// </summary>
        public IEnumerable<SubscriptionResponseData> Data { get; set; }
        /// <summary>
        /// Contains the information used to page through the list of results.
        /// The object is empty if there are no more pages left to page
        /// through. Read More: https://dev.twitch.tv/docs/api/guide#pagination
        /// </summary>
        public Pagination Pagination { get; set; }
        /// <summary>
        /// The current number of subscriber points earned by this broadcaster.
        /// Points are based on the subscription tier of each user that
        /// subscribes to this broadcaster. For example, a Tier 1 subscription
        /// is worth 1 point, Tier 2 is worth 2 points, and Tier 3 is worth 6
        /// points. The number of points determines the number of emote slots
        /// that are unlocked for the broadcaster. See Subscriber Emote Slots:
        /// https://help.twitch.tv/s/article/subscriber-emote-guide#emoteslots
        /// </summary>
        public int Points { get; set; }
        /// <summary>
        /// The total number of users that subscribe to this broadcaster.
        /// </summary>
        public int Total { get; set; }
    }

    /// <summary>
    /// Users that subscribe to the broadcaster.
    /// </summary>
    public class SubscriptionResponseData
    {
        /// <summary>
        /// An ID that identifies the broadcaster.
        /// </summary>
        public string BroadcasterId { get; set; }
        /// <summary>
        /// The broadcaster’s login name.
        /// </summary>
        public string BroadcasterLogin { get; set; }
        /// <summary>
        /// The broadcaster’s display name.
        /// </summary>
        public string BroadcasterName { get; set; }
        /// <summary>
        /// The ID of the user that gifted the subscription to the user. Is an
        /// empty string if is_gift is false.
        /// </summary>
        public string GifterId { get; set; }
        /// <summary>
        /// The gifter’s login name. Is an empty string if is_gift is false.
        /// </summary>
        public string GifterLogin { get; set; }
        /// <summary>
        /// The gifter’s display name. Is an empty string if is_gift is false.
        /// </summary>
        public string GifterName { get; set; }
        /// <summary>
        /// A Boolean value that determines whether the subscription is a gift
        /// subscription. Is true if the subscription was gifted.
        /// </summary>
        public bool IsGift { get; set; }
        /// <summary>
        /// The type of subscription. Possible values are:
        /// 1000 — Tier 1
        /// 2000 — Tier 2
        /// 3000 — Tier 3
        /// </summary>
        public string Tier { get; set; }
        /// <summary>
        /// The name of the subscription.
        /// </summary>
        public string PlanName { get; set; }
        /// <summary>
        /// An ID that identifies the subscribing user.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The user’s login name.
        /// </summary>
        public string UserLogin { get; set; }
        /// <summary>
        /// The user’s display name.
        /// </summary>
        public string UserName { get; set; }
    }
}
