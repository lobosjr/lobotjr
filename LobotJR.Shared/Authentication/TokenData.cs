﻿namespace LobotJR.Shared.Authentication
{
    /// <summary>
    /// Contains the pair of tokens required to access streamer data and act as
    /// a bot account in irc.
    /// </summary>
    public class TokenData
    {
        /// <summary>
        /// The user the chat token represents.
        /// </summary>
        public string ChatUser { get; set; }
        /// <summary>
        /// The token to use when connecting to chat.
        /// </summary>
        public TokenResponse ChatToken { get; set; }
        /// <summary>
        /// The user the broadcast token represents.
        /// </summary>
        public string BroadcastUser { get; set; }
        /// <summary>
        /// The token to use for accessing subscriber data APIs.
        /// </summary>
        public TokenResponse BroadcastToken { get; set; }
    }
}