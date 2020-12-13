using System.Collections.Generic;

namespace LobotJR.Command
{
    /// <summary>
    /// Interface for designating an object to use as a compact response.
    /// </summary>
    public interface ICompactResponse
    {
        /// <summary>
        /// Converts the object to a collection of compact response strings.
        /// </summary>
        /// <returns>A collection of strings representing the response data.
        /// Each item in the collection should be a part of the response, with
        /// each field separated by a pipe (|), and ending in a semicolon (;).
        /// The collection is used to split the responses into messages that
        /// fit within the IRC message limit.</returns>
        IEnumerable<string> ToCompact();
    }
}
