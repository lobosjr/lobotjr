namespace LobotJR.Shared.Utility
{
    /// <summary>
    /// Contains the information used to page through the list of results. The
    /// object is empty if there are no more pages left to page through. Read
    /// More: https://dev.twitch.tv/docs/api/guide#pagination
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// The cursor used to get the next or previous page of results. Use
        /// the cursor to set the request’s after or before query parameter
        /// depending on whether you’re paging forwards or backwards.
        /// </summary>
        public string Cursor { get; set; }
    }
}
