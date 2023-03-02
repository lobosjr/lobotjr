using LobotJR.Trigger.Responder;
using System.Collections.Generic;
using Wolfcoins;

namespace LobotJR.Trigger
{
    /// <summary>
    /// Manages the responders that are automatically triggered on public messages.
    /// </summary>
    public class TriggerManager
    {
        private IEnumerable<ITriggerResponder> Responders;

        /// <summary>
        /// Loads all of the responders.
        /// </summary>
        /// <param name="currency">The currency object that contains user XP
        /// data.</param>
        public void LoadAllResponders(Currency currency)
        {
            var responders = new List<ITriggerResponder>();
            responders.Add(new BlockLinks(currency));
            responders.Add(new NoceanMan());
            Responders = responders;
        }

        /// <summary>
        /// Processes all trigger responders for a given message. If the
        /// message matches the pattern for a responder, that responder returns
        /// the messages that the bot should respond with. These messages are
        /// sent to the public channel. Each message can only trigger a single
        /// responder.
        /// </summary>
        /// <param name="message">A message sent by a user.</param>
        /// <param name="user">The name of the user who sent the message.</param>
        /// <returns>An object containing all actions resulting from the trigger.</returns>
        public TriggerResult ProcessTrigger(string message, string user)
        {
            foreach (var responder in Responders)
            {
                var match = responder.Pattern.Match(message);
                if (match.Success)
                {
                    return responder.Process(match, user);
                }
            }
            return new TriggerResult() { Processed = false };
        }
    }
}
