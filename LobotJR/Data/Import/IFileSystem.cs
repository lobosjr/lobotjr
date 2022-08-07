namespace LobotJR.Data.Import
{
    public interface IFileSystem
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">
        /// The file to check.
        /// </param>
        /// <returns>
        /// true if the caller has the required permissions and path contains the name of
        /// an existing file; otherwise, false. This method also returns false if path is
        /// null, an invalid path, or a zero-length string. If the caller does not have sufficient
        /// permissions to read the specified file, no exception is thrown and the method
        /// returns false regardless of the existence of path.
        /// </returns>
        bool Exists(string path);

        /// <summary>
        /// Opens a file, reads all lines of the file with UTF-8 encoding, and then
        /// closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// path is a zero-length string, contains only white space, or contains one or more
        /// invalid characters as defined by System.IO.Path.InvalidPathChars.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred while opening the file.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// path specified a file that is read-only.-or- This operation is not supported
        /// on the current platform.-or- path specified a directory.-or- The caller does
        /// not have the required permission.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// The file specified in path was not found.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path is in an invalid format.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        string[] ReadAllLines(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file with UTF-8 encoding, and then
        /// closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to open for reading.
        /// </param>
        /// <returns>
        /// A string containing all lines of the file.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// path is a zero-length string, contains only white space, or contains one or more
        /// invalid characters as defined by System.IO.Path.InvalidPathChars.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred while opening the file.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// path specified a file that is read-only.-or- This operation is not supported
        /// on the current platform.-or- path specified a directory.-or- The caller does
        /// not have the required permission.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// The file specified in path was not found.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path is in an invalid format.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        string ReadAllText(string path);

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is
        /// not allowed.
        /// </summary>
        /// <param name="sourceFileName">
        /// The file to copy.
        /// </param>
        /// <param name="destFileName">
        /// The name of the destination file. This cannot be a directory or an existing file.
        /// </param>
        /// <exception cref="System.UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// sourceFileName or destFileName is a zero-length string, contains only white space,
        /// or contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.-or-
        /// sourceFileName or destFileName specifies a directory.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// sourceFileName or destFileName is null.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The path specified in sourceFileName or destFileName is invalid (for example,
        /// it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// sourceFileName was not found.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// destFileName exists.-or- An I/O error has occurred.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// sourceFileName or destFileName is in an invalid format.
        /// </exception>
        void Copy(string sourceFileName, string destFileName);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">
        /// The name of the file to be deleted. Wildcard characters are not supported.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// path is a zero-length string, contains only white space, or contains one or more
        /// invalid characters as defined by System.IO.Path.InvalidPathChars.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The specified file is in use. -or-There is an open handle on the file, and the
        /// operating system is Windows XP or earlier. This open handle can result from enumerating
        /// directories and files. For more information, see How to: Enumerate Directories
        /// and Files.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path is in an invalid format.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// The caller does not have the required permission.-or- The file is an executable
        /// file that is in use.-or- path is a directory.-or- path specified a read-only
        /// file.
        /// </exception>
        void Delete(string path);

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new
        /// file name.
        /// </summary>
        /// <param name="sourceFileName">
        /// The name of the file to move. Can include a relative or absolute path.
        /// </param>
        /// <param name="destFileName">
        /// The new path and name for the file.
        /// </param>
        /// <exception cref="System.IO.IOException">
        /// The destination file already exists.-or-sourceFileName was not found.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// sourceFileName or destFileName is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// sourceFileName or destFileName is a zero-length string, contains only white space,
        /// or contains invalid characters as defined in System.IO.Path.InvalidPathChars.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The path specified in sourceFileName or destFileName is invalid, (for example,
        /// it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// sourceFileName or destFileName is in an invalid format.
        /// </exception>
        void Move(string sourceFileName, string destFileName);

        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file.
        /// If the file does not exist, this method creates a file, writes the specified
        /// string to the file, then closes the file.
        /// </summary>
        /// <param name="path">
        /// The file to append the specified string to.
        /// </param>
        /// <param name="contents">
        /// The string to append to the file.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// path is a zero-length string, contains only white space, or contains one or more
        /// invalid characters as defined by System.IO.Path.InvalidPathChars.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid (for example, the directory doesn’t exist or it
        /// is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred while opening the file.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// path specified a file that is read-only.-or- This operation is not supported
        /// on the current platform.-or- path specified a directory.-or- The caller does
        /// not have the required permission.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path is in an invalid format.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        void AppendAllText(string path, string contents);

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes
        /// the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">
        /// The file to write to.
        /// </param>
        /// <param name="bytes">
        /// The bytes to write to the file.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// path is a zero-length string, contains only white space, or contains one or more
        /// invalid characters as defined by System.IO.Path.InvalidPathChars.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null or the byte array is empty.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred while opening the file.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// path specified a file that is read-only.-or- This operation is not supported
        /// on the current platform.-or- path specified a directory.-or- The caller does
        /// not have the required permission.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path is in an invalid format.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        void WriteAllBytes(string path, byte[] bytes);
    }
}
