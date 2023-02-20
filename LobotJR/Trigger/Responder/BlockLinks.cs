using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wolfcoins;

namespace LobotJR.Trigger.Responder
{
    public class BlockLinks : ITriggerResponder
    {
        public Regex Pattern { get; private set; } = new Regex(@"([A-Za-z0-9])\.([A-Za-z])([A-Za-z0-9])", RegexOptions.IgnoreCase);
        private Currency UserList;

        public BlockLinks(Currency currency)
        {
            UserList = currency;
        }

        public IEnumerable<string> Process(Match match, string user)
        {
            var userHasXp = UserList.xpList.ContainsKey(user);
            if (!match.Groups[0].Value.Equals("d.va")
                && !UserList.subSet.Contains(user)
                && (userHasXp && (UserList.determineLevel(user) < 2) || !userHasXp))
            {
                return new List<string> { $"/timeout {user} 1", "Links may only be posted by viewers of Level 2 or above. (Message me '?' for more details)" };
            }
            return new List<string>();
        }
    }
}
