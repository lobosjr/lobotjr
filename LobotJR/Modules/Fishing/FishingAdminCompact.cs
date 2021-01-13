using LobotJR.Command;

namespace LobotJR.Modules.Fishing
{
    public class FishingAdminCompact
    {
        public ICompactResponse DebugTournament(string data, string user)
        {
            /*
            else if (whisperMessage == "!debugtournament")
            {
                // Enable a fishing tournament! 
                // set bool to check against | bool fishingTournament = TOURNAMENT_ACTIVE; 
                fishingTournamentActive = true;
                // set tourney start time i.e. tournamentStart = DateTime.Now 
                tournamentStart = DateTime.Now;
                tournamentDuration = 1;
                irc.sendChatMessage("A fishing tournament has begun! Participate at: https://tinyurl.com/PlayWolfpackRPG");

            }
             */
            return null;
        }

        public ICompactResponse DebugCast(string data, string user)
        {
            /*
            else if (whisperMessage == "!debugcatch")
            {
                if (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Fish randomFish = new Fish(WeightedRandomFish(ref fishDatabase));
                        Console.WriteLine(randomFish.name + " (Rarity " + randomFish.rarity + ") caught.");
                    }
                }
            }
             */
            return null;
        }
    }
}
