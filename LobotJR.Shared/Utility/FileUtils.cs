using LobotJR.Shared.Authentication;
using LobotJR.Shared.Client;
using Newtonsoft.Json;
using System.IO;

namespace LobotJR.Shared.Utility
{
    /// <summary>
    /// A set of common utilities for dealing with the json files used to store
    /// client and authentication data.
    /// </summary>
    public class FileUtils
    {
        private const string _tokenJson = "token.json";
        private const string _clientJson = "client-data.json";

        private const string _legacyClientFile = "secret.txt";

        /// <summary>
        /// Returns the client secret from a legacy file if one exists.
        /// </summary>
        /// <returns>A client secret if one exists, otherwise an empty string.</returns>
        public static string ReadLegacySecret()
        {
            string output = "";
            if (File.Exists(_legacyClientFile))
            {
                output = File.ReadAllText(_legacyClientFile);
            }
            return output;
        }

        /// <summary>
        /// Determines if the client data file exists. Uses client-data.json in
        /// the executing folder unless a different value is specified.
        /// </summary>
        /// <returns>True if a file exists at the default location.</returns>
        public static bool HasClientData(string filename = _clientJson)
        {
            return File.Exists(filename);
        }

        /// <summary>
        /// Writes the client data to a json file. Uses client-data.json in the
        /// executing folder unless a different value is specified.
        /// </summary>
        /// <param name="clientData">The client data object to write.</param>
        /// <param name="filename">The name of the file to write to. Only
        /// specify this value if you need a non-default location.</param>
        public static void WriteClientData(ClientData clientData, string filename = _clientJson)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(clientData));
        }

        /// <summary>
        /// Reads the client data from a json file. Uses client-data.json in
        /// the executing folder unless a different value is specified.
        /// </summary>
        /// <param name="filename">The name of the file to read from. Only
        /// specify this value if you need a non-default location.</param>
        /// <returns>A client data object loaded from a file.</returns>
        public static ClientData ReadClientData(string filename = _clientJson)
        {
            return JsonConvert.DeserializeObject<ClientData>(File.ReadAllText(filename));
        }

        /// <summary>
        /// Determines if the token data file exists. Uses token.json in the
        /// executing folder unless a different value is specified.
        /// </summary>
        /// <returns>True if a file exists at the default location.</returns>
        public static bool HasTokenData(string filename = _tokenJson)
        {
            return File.Exists(filename);
        }

        /// <summary>
        /// Writes the results of the an authentication call to a json file.
        /// Uses token.json in the executing folder unless a different value is
        /// specified.
        /// </summary>
        /// <param name="tokenData">The token response to write out.</param>
        /// <param name="filename">The name of the file to write to. Only
        /// specify this value if you need a non-default location.</param>
        public static void WriteTokenData(TokenData tokenData, string filename = _tokenJson)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(tokenData));
        }

        /// <summary>
        /// Reads the authentication data from a json file. uses token.json in
        /// the executing folder unless a different file is specified.
        /// </summary>
        /// <param name="filename">The name of the file to read from. Only
        /// specify this value if you need a non-default location.</param>
        /// <returns>A token data object loaded from a file.</returns>
        public static TokenData ReadTokenData(string filename = _tokenJson)
        {
            try
            {
                return JsonConvert.DeserializeObject<TokenData>(File.ReadAllText(filename));
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }
    }
}
