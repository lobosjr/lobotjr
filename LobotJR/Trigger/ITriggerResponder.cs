using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LobotJR.Trigger
{
    public interface ITriggerResponder
    {
        Regex Pattern { get; }
        IEnumerable<string> Process(Match match, string user);
    }
}
