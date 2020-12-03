namespace LobotJR.Data
{
    /// <summary>
    /// Interface for allowing access to data layer.
    /// <typeparam name="T">The type of data to read or write.</typeparam>
    /// </summary>
    public interface IDataAccess<T>
    {
        /// <summary>
        /// Reads data from a source and returns it in the form of type T.
        /// </summary>
        /// <typeparam name="T">The type of data to extract.</typeparam>
        /// <returns>The strongly-typed object containing the data in the source.</returns>
        T ReadData(string source);

        /// <summary>
        /// Writes data to a source from a typed object.
        /// </summary>
        /// <param name="source">The source to write to.</param>
        /// <param name="content">The object to write to the data source.</param>
        void WriteData(string source, T content);

        /// <summary>
        /// Checks a data source to see if it exists.
        /// </summary>
        /// <param name="source">The source to check.</param>
        /// <returns>True if the data source exists.</returns>
        bool Exists(string source);
    }
}
