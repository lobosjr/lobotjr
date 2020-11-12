using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Command
{
    /// <summary>
    /// Represents a user role, as well as the users who are members of it.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        public string Name;
        /// <summary>
        /// A list of users who are members of the role.
        /// </summary>
        public List<string> Users;
        /// <summary>
        /// A list of commands that require role access to execute.
        /// </summary>
        public List<string> Commands;
    }
}
