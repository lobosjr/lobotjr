using System.Text.RegularExpressions;

namespace LobotJR.Trigger.Responder
{
    public class NoceanMan : ITriggerResponder
    {
        // Regex doesn't like these emojis, so I had to convert to unicode values
        // OCEAN MAN 🌊  😍
        public Regex Pattern { get; private set; } = new Regex(@"OCEAN MAN \uD83C\uDF0A  \uD83D\uDE0D", RegexOptions.IgnoreCase);

        public TriggerResult Process(Match match, string user)
        {
            return new TriggerResult()
            {
                Messages = new string[] { "NOCEAN MAN" },
                TimeoutSender = true,
                TimeoutMessage = "NOCEAN MAN"
            };
        }
    }
}
