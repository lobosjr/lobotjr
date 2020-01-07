namespace LobotJR.Shared.Common
{
    /// <summary>
    /// Object used for cursor-based pagination.
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// The cursor to pass to the next call to continue pagination.
        /// </summary>
        public string Cursor { get; set; }
    }
}
