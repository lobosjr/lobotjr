using System;
using System.Collections.Generic;

namespace LobotJR.Shared.Authentication
{
    /// <summary>
    /// The response object from the authorization code flow and refresh call.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// The oauth token to use when authenticating calls.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// The refresh token used to refresh the access token when it expires.
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// The number of seconds from the time of issuance until the token expires.
        /// </summary>
        public int ExpiresIn { get; set; }
        /// <summary>
        /// Calculated upon receipt using ExpiresIn
        /// </summary>
        public DateTime ExpirationDate { get; set; }
        /// <summary>
        /// The list of scopes the token has access to.
        /// </summary>
        public IEnumerable<string> Scope { get; set; }
        /// <summary>
        /// The type of token. This should always be "bearer"
        /// </summary>
        public string TokenType { get; set; }

        public void CopyFrom(TokenResponse other)
        {
            AccessToken = other.AccessToken;
            RefreshToken = other.RefreshToken;
            ExpiresIn = other.ExpiresIn;
            ExpirationDate = other.ExpirationDate;
        }
    }
}
